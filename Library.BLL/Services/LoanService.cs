using Library.Core.DTOs;
using Library.Core.Entities;
using Library.Core.Enums;
using Library.Core.Interfaces;

namespace Library.BLL.Services;

public class LoanService : ILoanService
{
    private const int StandardLoanDays = 14;
    private const int ReservationHoldHours = 48;

    private readonly IBookCopyRepository _bookCopyRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IReservationRepository? _reservationRepository;
    private readonly INotificationService? _notificationService;
    private readonly IReaderRepository? _readerRepository;
    private readonly TimeProvider _timeProvider;

    public LoanService(
        IBookCopyRepository bookCopyRepository,
        ILoanRepository loanRepository,
        IReservationRepository? reservationRepository = null,
        INotificationService? notificationService = null,
        IReaderRepository? readerRepository = null,
        TimeProvider? timeProvider = null)
    {
        _bookCopyRepository = bookCopyRepository;
        _loanRepository = loanRepository;
        _reservationRepository = reservationRepository;
        _notificationService = notificationService;
        _readerRepository = readerRepository;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task IssueLoanAsync(int bookCopyId, int readerId)
    {
        var bookCopy = await _bookCopyRepository.GetByIdAsync(bookCopyId);

        if (bookCopy is null)
        {
            throw new InvalidOperationException("Book copy was not found.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        if (bookCopy.Status == BookCopyStatus.Reserved)
        {
            await ClaimOrExpireReservationAsync(bookCopy, readerId, utcNow);
        }
        else if (bookCopy.Status != BookCopyStatus.Available)
        {
            throw new InvalidOperationException("Book copy is not available.");
        }

        bookCopy.Status = BookCopyStatus.Borrowed;

        await _bookCopyRepository.UpdateAsync(bookCopy);
        await _loanRepository.AddAsync(new Loan
        {
            BookCopyId = bookCopyId,
            ReaderId = readerId,
            BorrowDate = utcNow,
            DueDate = utcNow.AddDays(StandardLoanDays),
            Status = LoanStatus.Active
        });
    }

    public async Task ReturnLoanAsync(int loanId)
    {
        var loan = await _loanRepository.GetByIdAsync(loanId);

        if (loan is null)
        {
            throw new InvalidOperationException("Loan was not found.");
        }

        if (loan.ReturnDate is not null)
        {
            throw new InvalidOperationException("Loan has already been returned.");
        }

        var bookCopy = await _bookCopyRepository.GetByIdAsync(loan.BookCopyId);

        if (bookCopy is null)
        {
            throw new InvalidOperationException("Book copy was not found.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        loan.ReturnDate = utcNow;
        loan.Status = LoanStatus.Returned;

        var reservedForNextReader = await TryHoldForNextReservationAsync(bookCopy, utcNow);
        if (!reservedForNextReader)
        {
            bookCopy.Status = BookCopyStatus.Available;
        }

        await _bookCopyRepository.UpdateAsync(bookCopy);
        await _loanRepository.UpdateAsync(loan);
    }

    public async Task<IReadOnlyCollection<Loan>> GetActiveLoansByReaderAsync(int readerId)
    {
        var activeLoans = await _loanRepository.GetActiveByReaderIdAsync(readerId);
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        foreach (var loan in activeLoans)
        {
            await MarkOverdueIfNeededAsync(loan, utcNow);
        }

        return activeLoans;
    }

    public async Task<IReadOnlyCollection<Loan>> GetOverdueLoansAsync()
    {
        var allLoans = await _loanRepository.GetAllAsync();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var overdueLoans = new List<Loan>();

        foreach (var loan in allLoans)
        {
            if (await MarkOverdueIfNeededAsync(loan, utcNow) || loan.Status == LoanStatus.Overdue)
            {
                overdueLoans.Add(loan);
            }
        }

        return overdueLoans;
    }

    public async Task<IReadOnlyCollection<OverdueReaderDto>> GetOverdueReadersAsync()
    {
        var overdueLoans = await GetOverdueLoansAsync();
        var groupedByReader = overdueLoans.GroupBy(loan => loan.ReaderId);
        var result = new List<OverdueReaderDto>();

        foreach (var group in groupedByReader)
        {
            var reader = _readerRepository is not null
                ? await _readerRepository.GetByIdAsync(group.Key)
                : null;

            result.Add(new OverdueReaderDto(
                group.Key,
                reader?.FullName ?? $"Reader #{group.Key}",
                reader?.Email ?? string.Empty,
                group.ToArray()));
        }

        return result;
    }

    private async Task<bool> MarkOverdueIfNeededAsync(Loan loan, DateTime utcNow)
    {
        if (loan.ReturnDate is not null || loan.DueDate >= utcNow)
        {
            return false;
        }

        if (loan.Status != LoanStatus.Overdue)
        {
            loan.Status = LoanStatus.Overdue;
            await _loanRepository.UpdateAsync(loan);
        }

        return true;
    }

    private async Task ClaimOrExpireReservationAsync(
        BookCopy bookCopy,
        int readerId,
        DateTime utcNow)
    {
        if (_reservationRepository is null)
        {
            throw new InvalidOperationException("Book copy is not available.");
        }

        var reservations = await _reservationRepository.GetByBookCopyIdAsync(bookCopy.Id);
        var readyHold = reservations.FirstOrDefault(r => r.Status == ReservationStatus.ReadyForPickup);

        if (readyHold is not null && readyHold.ReservedUntil > utcNow)
        {
            if (readyHold.ReaderId != readerId)
            {
                throw new InvalidOperationException("Book copy is not available.");
            }

            readyHold.Status = ReservationStatus.Fulfilled;
            await _reservationRepository.UpdateAsync(readyHold);
            return;
        }

        if (readyHold is null)
        {
            throw new InvalidOperationException("Book copy is not available.");
        }

        readyHold.Status = ReservationStatus.Expired;
        await _reservationRepository.UpdateAsync(readyHold);

        if (await TryHoldForNextReservationAsync(bookCopy, utcNow))
        {
            var promoted = (await _reservationRepository.GetByBookCopyIdAsync(bookCopy.Id))
                .FirstOrDefault(r => r.Status == ReservationStatus.ReadyForPickup);

            if (promoted is not null && promoted.ReaderId == readerId)
            {
                promoted.Status = ReservationStatus.Fulfilled;
                await _reservationRepository.UpdateAsync(promoted);
                return;
            }

            throw new InvalidOperationException("Book copy is not available.");
        }
    }

    private async Task<bool> TryHoldForNextReservationAsync(BookCopy bookCopy, DateTime utcNow)
    {
        if (_reservationRepository is null)
        {
            return false;
        }

        var reservations = await _reservationRepository.GetByBookCopyIdAsync(bookCopy.Id);
        var nextReservation = reservations
            .Where(reservation => reservation.Status == ReservationStatus.Waiting)
            .OrderBy(reservation => reservation.CreatedAt)
            .ThenBy(reservation => reservation.Id)
            .FirstOrDefault();

        if (nextReservation is null)
        {
            return false;
        }

        var reservedUntil = utcNow.AddHours(ReservationHoldHours);
        var message = $"Book copy {bookCopy.InventoryNumber} has been returned and reserved for you for {ReservationHoldHours} hours (until {reservedUntil:u}).";

        nextReservation.Status = ReservationStatus.ReadyForPickup;
        nextReservation.ReservedUntil = reservedUntil;
        nextReservation.NotificationSent = true;
        nextReservation.NotificationMessage = message;

        bookCopy.Status = BookCopyStatus.Reserved;

        await _reservationRepository.UpdateAsync(nextReservation);

        if (_notificationService is not null)
        {
            await _notificationService.NotifyAsync(nextReservation.ReaderId, bookCopy.Id, message);
        }

        return true;
    }
}
