using FluentAssertions;
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
    public async Task GetBooks_ShouldReturnOkWithBooks()
    {
        // Arrange
        IReadOnlyCollection<Book> books = new[]
        {
            new Book { Id = 1, Title = "Clean Architecture", ISBN = "978-0134494166" }
        };

        var bookService = new Mock<IBookService>();
        bookService
            .Setup(service => service.GetBooksAsync("clean"))
            .ReturnsAsync(books);

        var controller = new BooksController(bookService.Object);

        // Act
        var result = await controller.GetBooks("clean");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(books);
    }

    [Fact]
    public async Task GetBookById_WhenBookExists_ShouldReturnOkWithBook()
    {
        // Arrange
        var book = new Book { Id = 1, Title = "Clean Architecture", ISBN = "978-0134494166" };
        var bookService = new Mock<IBookService>();
        bookService
            .Setup(service => service.GetBookByIdAsync(1))
            .ReturnsAsync(book);

        var controller = new BooksController(bookService.Object);

        // Act
        var result = await controller.GetBookById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(book);
    }

    [Fact]
    public async Task GetBookById_WhenBookDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var bookService = new Mock<IBookService>();
        bookService
            .Setup(service => service.GetBookByIdAsync(999))
            .ReturnsAsync((Book?)null);

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
            Author = new Author { Id = 3, Name = "Martin Fowler" }
        };

        var bookService = new Mock<IBookService>();
        bookService
            .Setup(service => service.AddBookAsync("Refactoring", "978-0134757599", "Martin Fowler"))
            .ReturnsAsync(createdBook);

        var controller = new BooksController(bookService.Object);

        // Act
        var result = await controller.CreateBook(
            new CreateBookRequest("Refactoring", "978-0134757599", "Martin Fowler"));

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().BeEquivalentTo(createdBook);
    }

    [Fact]
    public async Task CreateBook_WhenServiceThrowsArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        var bookService = new Mock<IBookService>();
        bookService
            .Setup(service => service.AddBookAsync("", "123", "Author"))
            .ThrowsAsync(new ArgumentException("Title is required."));

        var controller = new BooksController(bookService.Object);

        // Act
        var result = await controller.CreateBook(new CreateBookRequest("", "123", "Author"));

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetBookCopies_ShouldReturnOkWithCopies()
    {
        // Arrange
        IReadOnlyCollection<BookCopy> copies = new[]
        {
            new BookCopy { Id = 1, BookId = 1, InventoryNumber = "INV-001", Status = BookCopyStatus.Available }
        };

        var bookService = new Mock<IBookService>();
        bookService
            .Setup(service => service.GetBookCopiesAsync())
            .ReturnsAsync(copies);

        var controller = new BooksController(bookService.Object);

        // Act
        var result = await controller.GetBookCopies();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(copies);
    }
}
