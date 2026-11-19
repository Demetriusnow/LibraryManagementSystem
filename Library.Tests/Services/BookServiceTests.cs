using FluentAssertions;
using Library.BLL.Services;
using Library.Core.Entities;
using Library.Core.Enums;
using Library.Core.Interfaces;
using Moq;

namespace Library.Tests.Services;

public class BookServiceTests
{
    [Fact]
    public async Task GetBooksAsync_WhenSearchQueryIsEmpty_ShouldReturnAllBooks()
    {
        // Arrange
        IReadOnlyCollection<Book> books = new[]
        {
            new Book { Id = 1, Title = "Clean Architecture", ISBN = "978-0134494166", Author = new Author { Id = 1, Name = "Robert C. Martin" } },
            new Book { Id = 2, Title = "Domain-Driven Design", ISBN = "978-0321125217", Author = new Author { Id = 2, Name = "Eric Evans" } }
        };

        var bookRepository = new Mock<IBookRepository>();
        bookRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        var service = new BookService(bookRepository.Object, bookCopyRepository.Object);

        // Act
        var result = await service.GetBooksAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBooksAsync_WhenSearchQueryProvided_ShouldFilterByTitleAuthorOrIsbn()
    {
        // Arrange
        IReadOnlyCollection<Book> books = new[]
        {
            new Book { Id = 1, Title = "Clean Architecture", ISBN = "978-0134494166", Author = new Author { Id = 1, Name = "Robert C. Martin" } },
            new Book { Id = 2, Title = "Domain-Driven Design", ISBN = "978-0321125217", Author = new Author { Id = 2, Name = "Eric Evans" } }
        };

        var bookRepository = new Mock<IBookRepository>();
        bookRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        var service = new BookService(bookRepository.Object, bookCopyRepository.Object);

        // Act
        var byTitle = await service.GetBooksAsync("clean");
        var byAuthor = await service.GetBooksAsync("evans");

        // Assert
        byTitle.Should().ContainSingle(book => book.Id == 1);
        byAuthor.Should().ContainSingle(book => book.Id == 2);
    }

    [Fact]
    public async Task AddBookAsync_WhenInputIsValid_ShouldAddBookViaRepository()
    {
        // Arrange
        var bookRepository = new Mock<IBookRepository>();
        Book? addedBook = null;

        bookRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Callback<Book, CancellationToken>((book, _) =>
            {
                book.Id = 3;
                addedBook = book;
            })
            .Returns(Task.CompletedTask);

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        var service = new BookService(bookRepository.Object, bookCopyRepository.Object);

        // Act
        var result = await service.AddBookAsync("Refactoring", "978-0134757599", "Martin Fowler");

        // Assert
        result.Id.Should().Be(3);
        result.Title.Should().Be("Refactoring");
        result.ISBN.Should().Be("978-0134757599");
        result.Author.Name.Should().Be("Martin Fowler");
        addedBook.Should().NotBeNull();
    }

    [Fact]
    public async Task AddBookAsync_WhenTitleIsBlank_ShouldThrowArgumentException()
    {
        // Arrange
        var bookRepository = new Mock<IBookRepository>();
        var bookCopyRepository = new Mock<IBookCopyRepository>();
        var service = new BookService(bookRepository.Object, bookCopyRepository.Object);

        // Act
        Func<Task> act = () => service.AddBookAsync("   ", "978-0134757599", "Martin Fowler");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        bookRepository.Verify(
            repository => repository.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("Refactoring", "   ", "Martin Fowler")]
    [InlineData("Refactoring", "978-0134757599", "   ")]
    public async Task AddBookAsync_WhenIsbnOrAuthorIsBlank_ShouldThrowArgumentException(
        string title,
        string isbn,
        string authorName)
    {
        // Arrange
        var bookRepository = new Mock<IBookRepository>();
        var bookCopyRepository = new Mock<IBookCopyRepository>();
        var service = new BookService(bookRepository.Object, bookCopyRepository.Object);

        // Act
        Func<Task> act = () => service.AddBookAsync(title, isbn, authorName);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenBookExists_ShouldReturnBook()
    {
        // Arrange
        var expected = new Book { Id = 1, Title = "Clean Architecture", ISBN = "978-0134494166" };
        var bookRepository = new Mock<IBookRepository>();
        bookRepository
            .Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        var service = new BookService(bookRepository.Object, bookCopyRepository.Object);

        // Act
        var actual = await service.GetBookByIdAsync(1);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetBookCopiesAsync_ShouldReturnAllCopiesFromRepository()
    {
        // Arrange
        IReadOnlyCollection<BookCopy> copies = new[]
        {
            new BookCopy { Id = 1, BookId = 1, InventoryNumber = "INV-001", Status = BookCopyStatus.Available }
        };

        var bookRepository = new Mock<IBookRepository>();
        var bookCopyRepository = new Mock<IBookCopyRepository>();
        bookCopyRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(copies);

        var service = new BookService(bookRepository.Object, bookCopyRepository.Object);

        // Act
        var result = await service.GetBookCopiesAsync();

        // Assert
        result.Should().BeEquivalentTo(copies);
    }
}
