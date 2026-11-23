using FluentAssertions;
using Library.BLL.Services;
using Library.Core.Entities;
using Library.Core.Interfaces;
using LibraryManagementSystem.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Library.Tests.Controllers;

public class AuthorsControllerTests
{
    [Fact]
    public async Task GetAuthors_ShouldReturnOkWithAuthors()
    {
        // Arrange
        IReadOnlyCollection<Author> authors = new[]
        {
            new Author { Id = 1, Name = "Robert C. Martin" }
        };

        var authorService = new Mock<IAuthorService>();
        authorService.Setup(service => service.GetAuthorsAsync()).ReturnsAsync(authors);
        var controller = new AuthorsController(authorService.Object);

        // Act
        var result = await controller.GetAuthors();

        // Assert
        //var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        //okResult.Value.Should().BeEquivalentTo(authors);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(authors, okResult.Value);

    }

    [Fact]
    public async Task CreateAuthor_WhenValid_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var author = new Author { Id = 3, Name = "Martin Fowler" };
        var authorService = new Mock<IAuthorService>();
        authorService.Setup(service => service.AddAuthorAsync("Martin Fowler")).ReturnsAsync(author);
        var controller = new AuthorsController(authorService.Object);

        // Act
        var result = await controller.CreateAuthor(new AuthorRequest("Martin Fowler"));

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().BeEquivalentTo(author);
    }
}
