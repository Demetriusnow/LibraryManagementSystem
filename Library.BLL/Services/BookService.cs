using Library.Core.Entities;
using Library.Core.Interfaces;

namespace Library.BLL.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IBookCopyRepository _bookCopyRepository;

    public BookService(
        IBookRepository bookRepository,
        IBookCopyRepository bookCopyRepository)
    {
        _bookRepository = bookRepository;
        _bookCopyRepository = bookCopyRepository;
    }

    public async Task<IReadOnlyCollection<Book>> GetBooksAsync(string? searchQuery = null)
    {
        var books = await _bookRepository.GetAllAsync();

        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            return books;
        }

        var normalizedQuery = searchQuery.Trim();

        return books
            .Where(book =>
                book.Title.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                book.ISBN.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                (book.Author?.Name.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToArray();
    }

    public Task<Book?> GetBookByIdAsync(int bookId)
    {
        return _bookRepository.GetByIdAsync(bookId);
    }

    public async Task<Book> AddBookAsync(string title, string isbn, string authorName)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Book title is required.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(isbn))
        {
            throw new ArgumentException("Book ISBN is required.", nameof(isbn));
        }

        if (string.IsNullOrWhiteSpace(authorName))
        {
            throw new ArgumentException("Author name is required.", nameof(authorName));
        }

        var book = new Book
        {
            Title = title.Trim(),
            ISBN = isbn.Trim(),
            Author = new Author
            {
                Name = authorName.Trim()
            }
        };

        await _bookRepository.AddAsync(book);

        return book;
    }

    public Task<IReadOnlyCollection<BookCopy>> GetBookCopiesAsync()
    {
        return _bookCopyRepository.GetAllAsync();
    }
}
