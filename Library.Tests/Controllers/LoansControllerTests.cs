using FluentAssertions;
using Library.Core.Entities;
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

    [Fact]
    public async Task ReturnLoan_WhenServiceSucceeds_ShouldReturnOk()
    {
        // Arrange
        var loanService = new Mock<ILoanService>();
        var controller = new LoansController(loanService.Object);
        var request = new ReturnLoanRequest(LoanId: 1);

        // Act
        var result = await controller.ReturnLoan(request);

        // Assert
        result.Should().BeOfType<OkResult>();
        loanService.Verify(service => service.ReturnLoanAsync(request.LoanId), Times.Once);
    }

    [Fact]
    public async Task ReturnLoan_WhenServiceRejectsRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var loanService = new Mock<ILoanService>();
        loanService
            .Setup(service => service.ReturnLoanAsync(999))
            .ThrowsAsync(new InvalidOperationException("Loan was not found."));

        var controller = new LoansController(loanService.Object);

        // Act
        var result = await controller.ReturnLoan(new ReturnLoanRequest(LoanId: 999));

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetActiveLoansByReader_ShouldReturnOkWithLoans()
    {
        // Arrange
        const int readerId = 1;
        IReadOnlyCollection<Loan> loans = new[]
        {
            new Loan { Id = 1, BookCopyId = 2, ReaderId = readerId }
        };

        var loanService = new Mock<ILoanService>();
        loanService
            .Setup(service => service.GetActiveLoansByReaderAsync(readerId))
            .ReturnsAsync(loans);

        var controller = new LoansController(loanService.Object);

        // Act
        var result = await controller.GetActiveLoansByReader(readerId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(loans);
    }
}