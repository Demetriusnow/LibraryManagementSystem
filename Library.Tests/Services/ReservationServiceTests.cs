using FluentAssertions;
using Library.BLL.Services;
using Library.Core.Entities;
using Library.Core.Enums;
using Library.Core.Interfaces;
using Moq;

namespace Library.Tests.Services;

public class ReservationServiceTests
{
    [Fact]
    public async Task ReserveBookCopyAsync_WhenBookCopyIsBorrowed_ShouldAddToWaitingQueue()
    {
        // Arrange
        var copy = new BookCopy { Id = 2, BookId = 2, InventoryNumber = "INV-002", Status = BookCopyStatus.Borrowed };
        var bookCopyRepository = new Mock<IBookCopyRepository>();
        bookCopyRepository
            .Setup(repository => repository.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(copy);

        var reservationRepository = new Mock<IReservationRepository>();
        reservationRepository
            .Setup(repository => repository.GetByBookCopyIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Reservation>());
        reservationRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .Callback<Reservation, CancellationToken>((reservation, _) => reservation.Id = 1)
            .Returns(Task.CompletedTask);

        var notificationService = new Mock<INotificationService>();
        var service = new ReservationService(bookCopyRepository.Object, reservationRepository.Object, notificationService.Object);

        // Act
        var reservation = await service.ReserveBookCopyAsync(bookCopyId: 2, readerId: 2);

        // Assert
        reservation.Id.Should().Be(1);
        reservation.BookCopyId.Should().Be(2);
        reservation.ReaderId.Should().Be(2);
        reservation.Status.Should().Be(ReservationStatus.Waiting);
    }

    [Fact]
    public async Task ReserveBookCopyAsync_WhenBookCopyIsAvailable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var copy = new BookCopy { Id = 1, BookId = 1, InventoryNumber = "INV-001", Status = BookCopyStatus.Available };
        var bookCopyRepository = new Mock<IBookCopyRepository>();
        bookCopyRepository
            .Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(copy);

        var reservationRepository = new Mock<IReservationRepository>();
        var notificationService = new Mock<INotificationService>();
        var service = new ReservationService(bookCopyRepository.Object, reservationRepository.Object, notificationService.Object);

        // Act
        Func<Task> act = () => service.ReserveBookCopyAsync(bookCopyId: 1, readerId: 2);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*currently available*");
    }
}
