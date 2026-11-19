using FluentAssertions;
using Library.BLL.Services;
using Library.Core.Entities;
using Library.Core.Enums;
using Library.Core.Interfaces;
using Moq;

namespace Library.Tests.Services;

public class LoanServiceTests
{
    [Fact]
    public async Task IssueLoanAsync_WhenBookCopyIsBorrowed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        const int bookCopyId = 1;
        const int readerId = 10;

        var bookCopy = new BookCopy
        {
            Id = bookCopyId,
            BookId = 100,
            InventoryNumber = "INV-001",
            Status = BookCopyStatus.Borrowed
        };

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        bookCopyRepository
            .Setup(repository => repository.GetByIdAsync(
                bookCopyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookCopy);

        var loanRepository = new Mock<ILoanRepository>();

        var service = new LoanService(
            bookCopyRepository.Object,
            loanRepository.Object);

        // Act
        Func<Task> act = () => service.IssueLoanAsync(bookCopyId, readerId);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*not available*");

        bookCopyRepository.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<BookCopy>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        loanRepository.Verify(
            repository => repository.AddAsync(
                It.IsAny<Loan>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task IssueLoanAsync_WhenBookCopyIsAvailable_ShouldCreateLoanAndSetStatusToBorrowed()
    {
        // Arrange
        const int bookCopyId = 2;
        const int readerId = 20;

        var bookCopy = new BookCopy
        {
            Id = bookCopyId,
            BookId = 200,
            InventoryNumber = "INV-002",
            Status = BookCopyStatus.Available
        };

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        bookCopyRepository
            .Setup(repository => repository.GetByIdAsync(
                bookCopyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookCopy);

        var loanRepository = new Mock<ILoanRepository>();
        Loan? createdLoan = null;

        loanRepository
            .Setup(repository => repository.AddAsync(
                It.IsAny<Loan>(),
                It.IsAny<CancellationToken>()))
            .Callback<Loan, CancellationToken>((loan, _) => createdLoan = loan)
            .Returns(Task.CompletedTask);

        var service = new LoanService(
            bookCopyRepository.Object,
            loanRepository.Object);

        // Act
        await service.IssueLoanAsync(bookCopyId, readerId);

        // Assert
        bookCopy.Status.Should().Be(BookCopyStatus.Borrowed);

        bookCopyRepository.Verify(
            repository => repository.UpdateAsync(
                It.Is<BookCopy>(copy =>
                    copy.Id == bookCopyId &&
                    copy.Status == BookCopyStatus.Borrowed),
                It.IsAny<CancellationToken>()),
            Times.Once);

        loanRepository.Verify(
            repository => repository.AddAsync(
                It.IsAny<Loan>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        createdLoan.Should().NotBeNull();
        createdLoan!.BookCopyId.Should().Be(bookCopyId);
        createdLoan.ReaderId.Should().Be(readerId);
        createdLoan.DueDate.Should().Be(createdLoan.BorrowDate.AddDays(14));
    }

    [Fact]
    public async Task ReturnLoanAsync_WhenLoanIsActive_ShouldSetReturnDateAndMakeCopyAvailable()
    {
        // Arrange
        const int loanId = 5;
        const int bookCopyId = 2;

        var loan = new Loan
        {
            Id = loanId,
            BookCopyId = bookCopyId,
            ReaderId = 10,
            BorrowDate = DateTime.UtcNow.AddDays(-3),
            DueDate = DateTime.UtcNow.AddDays(11),
            ReturnDate = null
        };

        var bookCopy = new BookCopy
        {
            Id = bookCopyId,
            BookId = 100,
            InventoryNumber = "INV-002",
            Status = BookCopyStatus.Borrowed
        };

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        bookCopyRepository
            .Setup(repository => repository.GetByIdAsync(bookCopyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookCopy);

        var loanRepository = new Mock<ILoanRepository>();
        loanRepository
            .Setup(repository => repository.GetByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        var service = new LoanService(bookCopyRepository.Object, loanRepository.Object);

        // Act
        await service.ReturnLoanAsync(loanId);

        // Assert
        loan.ReturnDate.Should().NotBeNull();
        bookCopy.Status.Should().Be(BookCopyStatus.Available);

        loanRepository.Verify(
            repository => repository.UpdateAsync(
                It.Is<Loan>(updated => updated.Id == loanId && updated.ReturnDate != null),
                It.IsAny<CancellationToken>()),
            Times.Once);

        bookCopyRepository.Verify(
            repository => repository.UpdateAsync(
                It.Is<BookCopy>(copy => copy.Id == bookCopyId && copy.Status == BookCopyStatus.Available),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReturnLoanAsync_WhenLoanDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var bookCopyRepository = new Mock<IBookCopyRepository>();
        var loanRepository = new Mock<ILoanRepository>();
        loanRepository
            .Setup(repository => repository.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Loan?)null);

        var service = new LoanService(bookCopyRepository.Object, loanRepository.Object);

        // Act
        Func<Task> act = () => service.ReturnLoanAsync(999);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task ReturnLoanAsync_WhenLoanAlreadyReturned_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var loan = new Loan
        {
            Id = 5,
            BookCopyId = 2,
            ReaderId = 10,
            BorrowDate = DateTime.UtcNow.AddDays(-5),
            DueDate = DateTime.UtcNow.AddDays(9),
            ReturnDate = DateTime.UtcNow.AddDays(-1)
        };

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        var loanRepository = new Mock<ILoanRepository>();
        loanRepository
            .Setup(repository => repository.GetByIdAsync(loan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        var service = new LoanService(bookCopyRepository.Object, loanRepository.Object);

        // Act
        Func<Task> act = () => service.ReturnLoanAsync(loan.Id);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*already been returned*");

        bookCopyRepository.Verify(
            repository => repository.UpdateAsync(It.IsAny<BookCopy>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ReturnLoanAsync_WhenBookCopyDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var loan = new Loan
        {
            Id = 5,
            BookCopyId = 999,
            ReaderId = 10,
            BorrowDate = DateTime.UtcNow.AddDays(-2),
            DueDate = DateTime.UtcNow.AddDays(12)
        };

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        bookCopyRepository
            .Setup(repository => repository.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookCopy?)null);

        var loanRepository = new Mock<ILoanRepository>();
        loanRepository
            .Setup(repository => repository.GetByIdAsync(loan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        var service = new LoanService(bookCopyRepository.Object, loanRepository.Object);

        // Act
        Func<Task> act = () => service.ReturnLoanAsync(loan.Id);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*Book copy was not found*");
    }

    [Fact]
    public async Task GetActiveLoansByReaderAsync_ShouldReturnLoansFromRepository()
    {
        // Arrange
        const int readerId = 10;
        IReadOnlyCollection<Loan> expectedLoans = new[]
        {
            new Loan { Id = 1, BookCopyId = 2, ReaderId = readerId }
        };

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        var loanRepository = new Mock<ILoanRepository>();
        loanRepository
            .Setup(repository => repository.GetActiveByReaderIdAsync(readerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedLoans);

        var service = new LoanService(bookCopyRepository.Object, loanRepository.Object);

        // Act
        var actualLoans = await service.GetActiveLoansByReaderAsync(readerId);

        // Assert
        actualLoans.Should().BeEquivalentTo(expectedLoans);
    }

    [Fact]
    public async Task GetOverdueLoansAsync_WhenActiveLoanIsPastDueDate_ShouldMarkAsOverdueAndReturnIt()
    {
        // Arrange
        var now = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);
        var timeProvider = new FixedTimeProvider(now);

        var overdueLoan = new Loan
        {
            Id = 1,
            BookCopyId = 2,
            ReaderId = 1,
            BorrowDate = now.AddDays(-16),
            DueDate = now.AddDays(-2),
            ReturnDate = null,
            Status = LoanStatus.Active
        };

        var currentLoan = new Loan
        {
            Id = 2,
            BookCopyId = 3,
            ReaderId = 2,
            BorrowDate = now.AddDays(-2),
            DueDate = now.AddDays(12),
            ReturnDate = null,
            Status = LoanStatus.Active
        };

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        var loanRepository = new Mock<ILoanRepository>();
        loanRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { overdueLoan, currentLoan });

        var service = new LoanService(
            bookCopyRepository.Object,
            loanRepository.Object,
            timeProvider: timeProvider);

        // Act
        var result = await service.GetOverdueLoansAsync();

        // Assert
        result.Should().ContainSingle(loan => loan.Id == 1);
        overdueLoan.Status.Should().Be(LoanStatus.Overdue);
        currentLoan.Status.Should().Be(LoanStatus.Active);

        loanRepository.Verify(
            repository => repository.UpdateAsync(
                It.Is<Loan>(loan => loan.Id == 1 && loan.Status == LoanStatus.Overdue),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOverdueReadersAsync_ShouldGroupOverdueLoansByReader()
    {
        // Arrange
        var now = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);
        var timeProvider = new FixedTimeProvider(now);

        var overdueLoan = new Loan
        {
            Id = 1,
            BookCopyId = 2,
            ReaderId = 1,
            BorrowDate = now.AddDays(-16),
            DueDate = now.AddDays(-2),
            Status = LoanStatus.Active
        };

        var reader = new Reader { Id = 1, FullName = "Ivan Petrenko", Email = "ivan@example.com" };

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        var loanRepository = new Mock<ILoanRepository>();
        loanRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { overdueLoan });

        var readerRepository = new Mock<IReaderRepository>();
        readerRepository
            .Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reader);

        var service = new LoanService(
            bookCopyRepository.Object,
            loanRepository.Object,
            readerRepository: readerRepository.Object,
            timeProvider: timeProvider);

        // Act
        var overdueReaders = await service.GetOverdueReadersAsync();

        // Assert
        var dto = overdueReaders.Should().ContainSingle().Subject;
        dto.ReaderId.Should().Be(1);
        dto.FullName.Should().Be("Ivan Petrenko");
        dto.OverdueLoans.Should().ContainSingle(loan => loan.Id == 1);
    }

    [Fact]
    public async Task ReturnLoanAsync_WhenWaitingReservationExists_ShouldReserveCopyFor48HoursAndNotifyFirstReader()
    {
        // Arrange
        var now = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);
        var timeProvider = new FixedTimeProvider(now);

        var loan = new Loan
        {
            Id = 1,
            BookCopyId = 2,
            ReaderId = 1,
            BorrowDate = now.AddDays(-5),
            DueDate = now.AddDays(9)
        };

        var bookCopy = new BookCopy
        {
            Id = 2,
            BookId = 2,
            InventoryNumber = "INV-002",
            Status = BookCopyStatus.Borrowed
        };

        var firstReservation = new Reservation
        {
            Id = 10,
            BookCopyId = 2,
            ReaderId = 2,
            CreatedAt = now.AddHours(-3),
            Status = ReservationStatus.Waiting
        };

        var secondReservation = new Reservation
        {
            Id = 11,
            BookCopyId = 2,
            ReaderId = 3,
            CreatedAt = now.AddHours(-1),
            Status = ReservationStatus.Waiting
        };

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        bookCopyRepository
            .Setup(repository => repository.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookCopy);

        var loanRepository = new Mock<ILoanRepository>();
        loanRepository
            .Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        var reservationRepository = new Mock<IReservationRepository>();
        reservationRepository
            .Setup(repository => repository.GetByBookCopyIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { secondReservation, firstReservation });

        var notificationService = new Mock<INotificationService>();

        var service = new LoanService(
            bookCopyRepository.Object,
            loanRepository.Object,
            reservationRepository: reservationRepository.Object,
            notificationService: notificationService.Object,
            timeProvider: timeProvider);

        // Act
        await service.ReturnLoanAsync(1);

        // Assert
        loan.Status.Should().Be(LoanStatus.Returned);
        bookCopy.Status.Should().Be(BookCopyStatus.Reserved);
        firstReservation.Status.Should().Be(ReservationStatus.ReadyForPickup);
        firstReservation.ReservedUntil.Should().Be(now.AddHours(48));
        firstReservation.NotificationSent.Should().BeTrue();
        secondReservation.Status.Should().Be(ReservationStatus.Waiting);

        notificationService.Verify(
            notifier => notifier.NotifyAsync(
                2,
                2,
                It.Is<string>(msg => msg.Contains("48")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IssueLoanAsync_WhenCopyIsReservedForAnotherReaderWithin48Hours_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var now = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);
        var timeProvider = new FixedTimeProvider(now);

        var bookCopy = new BookCopy
        {
            Id = 2,
            BookId = 2,
            InventoryNumber = "INV-002",
            Status = BookCopyStatus.Reserved
        };

        var activeHold = new Reservation
        {
            Id = 10,
            BookCopyId = 2,
            ReaderId = 2,
            Status = ReservationStatus.ReadyForPickup,
            ReservedUntil = now.AddHours(24)
        };

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        bookCopyRepository
            .Setup(repository => repository.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookCopy);

        var loanRepository = new Mock<ILoanRepository>();
        var reservationRepository = new Mock<IReservationRepository>();
        reservationRepository
            .Setup(repository => repository.GetByBookCopyIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { activeHold });

        var service = new LoanService(
            bookCopyRepository.Object,
            loanRepository.Object,
            reservationRepository: reservationRepository.Object,
            timeProvider: timeProvider);

        // Act
        Func<Task> act = () => service.IssueLoanAsync(2, readerId: 99);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*not available*");
    }

    [Fact]
    public async Task IssueLoanAsync_WhenCopyIsReservedForSameReaderWithin48Hours_ShouldFulfillReservationAndIssueLoan()
    {
        // Arrange
        var now = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);
        var timeProvider = new FixedTimeProvider(now);

        var bookCopy = new BookCopy
        {
            Id = 2,
            BookId = 2,
            InventoryNumber = "INV-002",
            Status = BookCopyStatus.Reserved
        };

        var activeHold = new Reservation
        {
            Id = 10,
            BookCopyId = 2,
            ReaderId = 2,
            Status = ReservationStatus.ReadyForPickup,
            ReservedUntil = now.AddHours(24)
        };

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        bookCopyRepository
            .Setup(repository => repository.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookCopy);

        var loanRepository = new Mock<ILoanRepository>();
        var reservationRepository = new Mock<IReservationRepository>();
        reservationRepository
            .Setup(repository => repository.GetByBookCopyIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { activeHold });

        var service = new LoanService(
            bookCopyRepository.Object,
            loanRepository.Object,
            reservationRepository: reservationRepository.Object,
            timeProvider: timeProvider);

        // Act
        await service.IssueLoanAsync(2, readerId: 2);

        // Assert
        bookCopy.Status.Should().Be(BookCopyStatus.Borrowed);
        activeHold.Status.Should().Be(ReservationStatus.Fulfilled);
        loanRepository.Verify(
            repository => repository.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IssueLoanAsync_When48HourReservationHoldExpiredAndQueueIsEmpty_ShouldExpireHoldAndIssueLoan()
    {
        // Arrange
        var now = new DateTime(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);
        var timeProvider = new FixedTimeProvider(now);

        var bookCopy = new BookCopy
        {
            Id = 2,
            BookId = 2,
            InventoryNumber = "INV-002",
            Status = BookCopyStatus.Reserved
        };

        var expiredHold = new Reservation
        {
            Id = 10,
            BookCopyId = 2,
            ReaderId = 2,
            Status = ReservationStatus.ReadyForPickup,
            ReservedUntil = now.AddHours(-1)
        };

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        bookCopyRepository
            .Setup(repository => repository.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookCopy);

        var loanRepository = new Mock<ILoanRepository>();
        var reservationRepository = new Mock<IReservationRepository>();
        reservationRepository
            .Setup(repository => repository.GetByBookCopyIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { expiredHold });

        var service = new LoanService(
            bookCopyRepository.Object,
            loanRepository.Object,
            reservationRepository: reservationRepository.Object,
            timeProvider: timeProvider);

        // Act
        await service.IssueLoanAsync(2, readerId: 99);

        // Assert
        expiredHold.Status.Should().Be(ReservationStatus.Expired);
        bookCopy.Status.Should().Be(BookCopyStatus.Borrowed);
        loanRepository.Verify(
            repository => repository.AddAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }
}
