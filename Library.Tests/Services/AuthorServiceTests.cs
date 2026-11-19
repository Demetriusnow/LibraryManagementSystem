using FluentAssertions;
using Library.BLL.Services;
using Library.Core.Entities;
using Library.Core.Interfaces;
using Moq;

namespace Library.Tests.Services;

public class AuthorServiceTests
{
    [Fact]
    public async Task AddAuthorAsync_WhenValidAndUnique_ShouldAddAuthor()
    {
        // Arrange
        var authorRepository = new Mock<IAuthorRepository>();
        authorRepository.Setup(r => r.GetByNameAsync("Martin Fowler", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Author?)null);
        authorRepository.Setup(r => r.AddAsync(It.IsAny<Author>(), It.IsAny<CancellationToken>()))
            .Callback<Author, CancellationToken>((a, _) => a.Id = 3)
            .Returns(Task.CompletedTask);

        var service = new AuthorService(authorRepository.Object);

        // Act
        var author = await service.AddAuthorAsync("Martin Fowler");

        // Assert
        author.Id.Should().Be(3);
        author.Name.Should().Be("Martin Fowler");
    }

    [Fact]
    public async Task AddAuthorAsync_WhenDuplicateName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var authorRepository = new Mock<IAuthorRepository>();
        authorRepository.Setup(r => r.GetByNameAsync("Eric Evans", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Author { Id = 2, Name = "Eric Evans" });

        var service = new AuthorService(authorRepository.Object);

        // Act
        Func<Task> act = () => service.AddAuthorAsync("Eric Evans");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
    }
}
