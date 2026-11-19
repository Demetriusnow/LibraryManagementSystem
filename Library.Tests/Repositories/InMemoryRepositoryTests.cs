using FluentAssertions;
using Library.Core.Enums;
using Library.DAL.Repositories;

namespace Library.Tests.Repositories;

public class InMemoryRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_WhenSeeded_ShouldReturnAvailableAndBorrowedCopies()
    {
        // Arrange
        var repository = new InMemoryBookCopyRepository();

        // Act
        var availableCopy = await repository.GetByIdAsync(1);
        var borrowedCopy = await repository.GetByIdAsync(2);

        // Assert
        availableCopy.Should().NotBeNull();
        availableCopy!.Status.Should().Be(BookCopyStatus.Available);

        borrowedCopy.Should().NotBeNull();
        borrowedCopy!.Status.Should().Be(BookCopyStatus.Borrowed);
    }

    [Fact]
    public async Task GetAllAsync_BookCopies_ShouldReturnSeededCopies()
    {
        // Arrange
        var repository = new InMemoryBookCopyRepository();

        // Act
        var copies = await repository.GetAllAsync();

        // Assert
        copies.Should().HaveCount(2);
    }

    [Fact]
    public async Task InMemoryLoanRepository_WhenSeeded_ShouldHaveActiveLoanForBorrowedCopy()
    {
        // Arrange
        var repository = new InMemoryLoanRepository();

        // Act
        var loan = await repository.GetByIdAsync(1);
        var activeLoans = await repository.GetActiveByReaderIdAsync(1);

        // Assert
        loan.Should().NotBeNull();
        loan!.BookCopyId.Should().Be(2);
        loan.ReturnDate.Should().BeNull();
        activeLoans.Should().ContainSingle(item => item.Id == 1);
    }

    [Fact]
    public async Task InMemoryBookRepository_WhenSeeded_ShouldReturnInitialBooksAndSupportAdding()
    {
        // Arrange
        var repository = new InMemoryBookRepository();

        // Act
        var initialBooks = await repository.GetAllAsync();
        await repository.AddAsync(new Library.Core.Entities.Book
        {
            Title = "Refactoring",
            ISBN = "978-0134757599",
            Author = new Library.Core.Entities.Author { Name = "Martin Fowler" }
        });
        var updatedBooks = await repository.GetAllAsync();

        // Assert
        initialBooks.Should().HaveCount(2);
        updatedBooks.Should().HaveCount(3);
    }
}