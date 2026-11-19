using FluentAssertions;
using Library.Core.Entities;
using Library.Core.Enums;
using Library.Core.Interfaces;
using LibraryManagementSystem.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Library.Tests.Controllers;

public class ReservationsControllerTests
{
    [Fact]
    public async Task ReserveBookCopy_WhenValid_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var reservation = new Reservation
        {
            Id = 1,
            BookCopyId = 2,
            ReaderId = 2,
            Status = ReservationStatus.Waiting
        };

        var reservationService = new Mock<IReservationService>();
        reservationService
            .Setup(service => service.ReserveBookCopyAsync(2, 2))
            .ReturnsAsync(reservation);

        var controller = new ReservationsController(reservationService.Object);

        // Act
        var result = await controller.ReserveBookCopy(new CreateReservationRequest(2, 2));

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().BeEquivalentTo(reservation);
    }

    [Fact]
    public async Task GetNotifications_ShouldReturnOkWithNotifications()
    {
        // Arrange
        IReadOnlyCollection<NotificationRecord> notifications = new[]
        {
            new NotificationRecord { Id = 1, ReaderId = 2, BookCopyId = 2, Message = "Reserved for 48 hours." }
        };

        var reservationService = new Mock<IReservationService>();
        reservationService
            .Setup(service => service.GetNotificationsAsync())
            .ReturnsAsync(notifications);

        var controller = new ReservationsController(reservationService.Object);

        // Act
        var result = await controller.GetNotifications();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(notifications);
    }
}
