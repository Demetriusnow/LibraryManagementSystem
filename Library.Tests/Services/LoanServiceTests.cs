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
}
