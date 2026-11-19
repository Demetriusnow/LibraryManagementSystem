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
    public async Task SearchBooksAsync_WhenFiltersAreEmpty_ShouldReturnBooksWithAvailableCopiesCount()
    {
        // Arrange
        IReadOnlyCollection<Book> books = new[]
        {
            new Book
            {
                Id = 1,
                Title = "Clean Architecture",
                ISBN = "978-0134494166",
                Category = "Software Engineering",
                PublishedYear = 2017,
                AuthorId = 1,
                Author = new Author { Id = 1, Name = "Robert C. Martin" }
            },
            new Book
            {
                Id = 2,
                Title = "Domain-Driven Design",
                ISBN = "978-0321125217",
                Category = "Architecture",
                PublishedYear = 2003,
                AuthorId = 2,
                Author = new Author { Id = 2, Name = "Eric Evans" }
            }
        };

        IReadOnlyCollection<BookCopy> copies = new[]
        {
            new BookCopy { Id = 1, BookId = 1, InventoryNumber = "INV-001", Status = BookCopyStatus.Available },
            new BookCopy { Id = 2, BookId = 1, InventoryNumber = "INV-002", Status = BookCopyStatus.Borrowed },
            new BookCopy { Id = 3, BookId = 2, InventoryNumber = "INV-003", Status = BookCopyStatus.Borrowed }
        };

        var bookRepository = new Mock<IBookRepository>();
        bookRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        bookCopyRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(copies);

        var service = new BookService(bookRepository.Object, bookCopyRepository.Object);

        // Act
        var result = await service.SearchBooksAsync();

        // Assert
        result.Should().HaveCount(2);
        var cleanArch = result.Single(item => item.Id == 1);
        cleanArch.TotalCopiesCount.Should().Be(2);
        cleanArch.AvailableCopiesCount.Should().Be(1);

        var ddd = result.Single(item => item.Id == 2);
        ddd.AvailableCopiesCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchBooksAsync_WhenQueryCategoryAndPaginationProvided_ShouldFilterCaseInsensitively()
    {
        // Arrange
        IReadOnlyCollection<Book> books = new[]
        {
            new Book { Id = 1, Title = "Clean Architecture", ISBN = "978-0134494166", Category = "Architecture", Author = new Author { Id = 1, Name = "Robert C. Martin" } },
            new Book { Id = 2, Title = "Clean Code", ISBN = "978-0132350884", Category = "Clean Code", Author = new Author { Id = 1, Name = "Robert C. Martin" } },
            new Book { Id = 3, Title = "Domain-Driven Design", ISBN = "978-0321125217", Category = "Architecture", Author = new Author { Id = 2, Name = "Eric Evans" } }
        };

        var bookRepository = new Mock<IBookRepository>();
        bookRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        bookCopyRepository
            .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<BookCopy>());

        var service = new BookService(bookRepository.Object, bookCopyRepository.Object);

        // Act
        var filteredByCategoryAndAuthor = await service.SearchBooksAsync(query: "martin", category: "architecture");
        var pagedResult = await service.SearchBooksAsync(category: "architecture", page: 2, pageSize: 1);
        var emptyResult = await service.SearchBooksAsync(query: "nonexistent");

        // Assert
        filteredByCategoryAndAuthor.Should().ContainSingle(book => book.Id == 1);
        pagedResult.Should().ContainSingle(book => book.Id == 3);
        emptyResult.Should().BeEmpty();
    }

    [Fact]
    public async Task AddBookAsync_WhenInputIsValidAndUnique_ShouldCreateBook()
    {
        // Arrange
        var bookRepository = new Mock<IBookRepository>();
        bookRepository
            .Setup(repository => repository.GetByIsbnAsync("978-0134757599", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);
        bookRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Callback<Book, CancellationToken>((book, _) => book.Id = 3)
            .Returns(Task.CompletedTask);

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        var service = new BookService(bookRepository.Object, bookCopyRepository.Object);

        // Act
        var result = await service.AddBookAsync(
            "Refactoring",
            "978-0134757599",
            "Martin Fowler",
            "Software Engineering",
            2018);

        // Assert
        result.Id.Should().Be(3);
        result.Title.Should().Be("Refactoring");
        result.Category.Should().Be("Software Engineering");
        result.PublishedYear.Should().Be(2018);
    }

    [Fact]
    public async Task AddBookAsync_WhenIsbnAlreadyExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var existing = new Book { Id = 1, Title = "Clean Architecture", ISBN = "978-0134494166" };
        var bookRepository = new Mock<IBookRepository>();
        bookRepository
            .Setup(repository => repository.GetByIsbnAsync("978-0134494166", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        var service = new BookService(bookRepository.Object, bookCopyRepository.Object);

        // Act
        Func<Task> act = () => service.AddBookAsync(
            "Duplicate",
            "978-0134494166",
            "Author",
            "Category",
            2020);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*ISBN*exists*");
    }

    [Fact]
    public async Task AddBookAsync_WhenPublishedYearIsInFuture_ShouldThrowArgumentException()
    {
        // Arrange
        var futureYear = DateTime.UtcNow.Year + 1;
        var bookRepository = new Mock<IBookRepository>();
        var bookCopyRepository = new Mock<IBookCopyRepository>();
        var service = new BookService(bookRepository.Object, bookCopyRepository.Object);

        // Act
        Func<Task> act = () => service.AddBookAsync(
            "Future Book",
            "978-0000000001",
            "Author",
            "Sci-Fi",
            futureYear);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("*future*");
    }

    [Fact]
    public async Task AddBookCopyAsync_WhenValidBookAndUniqueInventoryNumber_ShouldBindMultipleCopiesToBook()
    {
        // Arrange
        var book = new Book { Id = 1, Title = "Clean Architecture" };
        var bookRepository = new Mock<IBookRepository>();
        bookRepository
            .Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        bookCopyRepository
            .Setup(repository => repository.GetByInventoryNumberAsync("INV-010", It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookCopy?)null);
        bookCopyRepository
            .Setup(repository => repository.AddAsync(It.IsAny<BookCopy>(), It.IsAny<CancellationToken>()))
            .Callback<BookCopy, CancellationToken>((copy, _) => copy.Id = 10)
            .Returns(Task.CompletedTask);

        var service = new BookService(bookRepository.Object, bookCopyRepository.Object);

        // Act
        var copy = await service.AddBookCopyAsync(1, "INV-010");

        // Assert
        copy.Id.Should().Be(10);
        copy.BookId.Should().Be(1);
        copy.InventoryNumber.Should().Be("INV-010");
        copy.Status.Should().Be(BookCopyStatus.Available);
    }

    [Fact]
    public async Task AddBookCopyAsync_WhenInventoryNumberIsDuplicateOrInvalidFormat_ShouldThrow()
    {
        // Arrange
        var book = new Book { Id = 1, Title = "Clean Architecture" };
        var bookRepository = new Mock<IBookRepository>();
        bookRepository
            .Setup(repository => repository.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        bookCopyRepository
            .Setup(repository => repository.GetByInventoryNumberAsync("INV-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookCopy { Id = 1, InventoryNumber = "INV-001" });

        var service = new BookService(bookRepository.Object, bookCopyRepository.Object);

        // Act
        Func<Task> duplicateAct = () => service.AddBookCopyAsync(1, "INV-001");
        Func<Task> badFormatAct = () => service.AddBookCopyAsync(1, "BAD-001");

        // Assert
        await duplicateAct.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
        await badFormatAct.Should().ThrowAsync<ArgumentException>().WithMessage("*INV-*");
    }

    [Fact]
    public async Task UpdateAndDeleteBook_WhenBookExists_ShouldUpdateAndDeleteViaRepository()
    {
        // Arrange
        var existing = new Book
        {
            Id = 1,
            Title = "Old Title",
            ISBN = "978-0134494166",
            Category = "Old",
            PublishedYear = 2010,
            Author = new Author { Id = 1, Name = "Author" }
        };

        var bookRepository = new Mock<IBookRepository>();
        bookRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        bookRepository.Setup(r => r.GetByIsbnAsync("978-0134494166", It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var bookCopyRepository = new Mock<IBookCopyRepository>();
        var service = new BookService(bookRepository.Object, bookCopyRepository.Object);

        // Act
        var updated = await service.UpdateBookAsync(1, "New Title", "978-0134494166", "Author", "Architecture", 2017);
        await service.DeleteBookAsync(1);

        // Assert
        updated.Title.Should().Be("New Title");
        updated.PublishedYear.Should().Be(2017);
        bookRepository.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        bookRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }
}
