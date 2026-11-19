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
}
