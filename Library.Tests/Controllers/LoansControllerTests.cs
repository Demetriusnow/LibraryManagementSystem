using FluentAssertions;
using Library.Core.Interfaces;
using LibraryManagementSystem.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Library.Tests.Controllers;

public class LoansControllerTests
{
    [Fact]
    public async Task IssueLoan_WhenServiceSucceeds_ShouldReturnOk()
    {
        // Arrange
        var loanService = new Mock<ILoanService>();
        var controller = new LoansController(loanService.Object);
        var request = new IssueLoanRequest(BookCopyId: 1, ReaderId: 10);

        // Act
        var result = await controller.IssueLoan(request);

        // Assert
        result.Should().BeOfType<OkResult>();
        loanService.Verify(
            service => service.IssueLoanAsync(request.BookCopyId, request.ReaderId),
            Times.Once);
    }

    [Fact]
    public async Task IssueLoan_WhenServiceRejectsRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var loanService = new Mock<ILoanService>();
        loanService
            .Setup(service => service.IssueLoanAsync(1, 10))
            .ThrowsAsync(new InvalidOperationException("Book copy is not available."));

        var controller = new LoansController(loanService.Object);

        // Act
        var result = await controller.IssueLoan(
            new IssueLoanRequest(BookCopyId: 1, ReaderId: 10));

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}