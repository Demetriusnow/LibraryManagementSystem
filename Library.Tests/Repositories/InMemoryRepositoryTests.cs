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
}