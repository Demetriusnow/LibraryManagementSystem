using Library.Core.Entities;
using Library.Core.Enums;
using Library.Core.Interfaces;

namespace Library.BLL.Services;

public class ReservationService : IReservationService
{
    private readonly IBookCopyRepository _bookCopyRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly INotificationService _notificationService;
    private readonly TimeProvider _timeProvider;

    public ReservationService(
        IBookCopyRepository bookCopyRepository,
        IReservationRepository reservationRepository,
        INotificationService notificationService,
        TimeProvider? timeProvider = null)
    {
        _bookCopyRepository = bookCopyRepository;
        _reservationRepository = reservationRepository;
        _notificationService = notificationService;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<Reservation> ReserveBookCopyAsync(int bookCopyId, int readerId)
    {
        var bookCopy = await _bookCopyRepository.GetByIdAsync(bookCopyId);

        if (bookCopy is null)
        {
            throw new InvalidOperationException("Book copy was not found.");
        }

        if (bookCopy.Status == BookCopyStatus.Available)
        {
            throw new InvalidOperationException("Book copy is currently available and can be borrowed directly.");
        }

        var existingReservations = await _reservationRepository.GetByBookCopyIdAsync(bookCopyId);
        var alreadyQueued = existingReservations.Any(r =>
            r.ReaderId == readerId &&
            (r.Status == ReservationStatus.Waiting || r.Status == ReservationStatus.ReadyForPickup));

        if (alreadyQueued)
        {
            throw new InvalidOperationException("Reader is already in the reservation queue for this copy.");
        }

        var reservation = new Reservation
        {
            BookCopyId = bookCopyId,
            ReaderId = readerId,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            Status = ReservationStatus.Waiting
        };

        await _reservationRepository.AddAsync(reservation);

        return reservation;
    }

    public Task<IReadOnlyCollection<Reservation>> GetReservationsForCopyAsync(int bookCopyId)
    {
        return _reservationRepository.GetByBookCopyIdAsync(bookCopyId);
    }

    public async Task CancelReservationAsync(int reservationId)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId);

        if (reservation is null)
        {
            throw new InvalidOperationException("Reservation was not found.");
        }

        reservation.Status = ReservationStatus.Cancelled;
        await _reservationRepository.UpdateAsync(reservation);
    }

    public Task<IReadOnlyCollection<NotificationRecord>> GetNotificationsAsync()
    {
        return _notificationService.GetAllAsync();
    }
}
