using FluentAssertions;
using Library.Core.DTOs;
using Library.Core.Entities;
using Library.Core.Enums;
using Library.Core.Interfaces;
using LibraryManagementSystem.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Library.Tests.Controllers;

public class BooksControllerTests
{
    [Fact]
    public async Task GetBooks_ShouldReturnOkWithCatalogItems()
    {
        // Arrange
        IReadOnlyCollection<BookCatalogItemDto> books = new[]
        {
            new BookCatalogItemDto(1, "Clean Architecture", "978-0134494166", "Software Engineering", 2017, 1, "Robert C. Martin", 1, 1)
        };

        var bookService = new Mock<IBookService>();
        bookService
            .Setup(service => service.SearchBooksAsync("clean", "Software Engineering", 1, 10))
            .ReturnsAsync(books);

        var controller = new BooksController(bookService.Object);

        // Act
        var result = await controller.GetBooks("clean", "Software Engineering", 1, 10);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(books);
    }

    [Fact]
    public async Task GetBookById_WhenBookDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var bookService = new Mock<IBookService>();
        bookService
            .Setup(service => service.GetBookByIdAsync(999))
            .ReturnsAsync((BookCatalogItemDto?)null);

        var controller = new BooksController(bookService.Object);

        // Act
        var result = await controller.GetBookById(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateBook_WhenValidRequest_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var createdBook = new Book
        {
            Id = 3,
            Title = "Refactoring",
            ISBN = "978-0134757599",
            Category = "Software Engineering",
            PublishedYear = 2018,
            Author = new Author { Id = 3, Name = "Martin Fowler" }
        };

        var bookService = new Mock<IBookService>();
        bookService
            .Setup(service => service.AddBookAsync("Refactoring", "978-0134757599", "Martin Fowler", "Software Engineering", 2018))
            .ReturnsAsync(createdBook);

        var controller = new BooksController(bookService.Object);

        // Act
        var result = await controller.CreateBook(
            new CreateBookRequest("Refactoring", "978-0134757599", "Martin Fowler", "Software Engineering", 2018));

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().BeEquivalentTo(createdBook);
    }

    [Fact]
    public async Task CreateBook_WhenDuplicateIsbn_ShouldReturnBadRequest()
    {
        // Arrange
        var bookService = new Mock<IBookService>();
        bookService
            .Setup(service => service.AddBookAsync("Clean", "978-0134494166", "Author", "Cat", 2020))
            .ThrowsAsync(new InvalidOperationException("Book with this ISBN already exists."));

        var controller = new BooksController(bookService.Object);

        // Act
        var result = await controller.CreateBook(
            new CreateBookRequest("Clean", "978-0134494166", "Author", "Cat", 2020));

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AddBookCopy_WhenValid_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var copy = new BookCopy { Id = 3, BookId = 1, InventoryNumber = "INV-003", Status = BookCopyStatus.Available };
        var bookService = new Mock<IBookService>();
        bookService
            .Setup(service => service.AddBookCopyAsync(1, "INV-003"))
            .ReturnsAsync(copy);

        var controller = new BooksController(bookService.Object);

        // Act
        var result = await controller.AddBookCopy(1, new CreateBookCopyRequest("INV-003"));

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().BeEquivalentTo(copy);
    }

    [Fact]
    public async Task DeleteBook_WhenBookExists_ShouldReturnNoContent()
    {
        // Arrange
        var bookService = new Mock<IBookService>();
        var controller = new BooksController(bookService.Object);

        // Act
        var result = await controller.DeleteBook(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        bookService.Verify(service => service.DeleteBookAsync(1), Times.Once);
    }
}
