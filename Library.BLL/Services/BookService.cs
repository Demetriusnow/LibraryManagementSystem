using System.Text.RegularExpressions;
using Library.Core.DTOs;
using Library.Core.Entities;
using Library.Core.Enums;
using Library.Core.Interfaces;

namespace Library.BLL.Services;

public partial class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IBookCopyRepository _bookCopyRepository;
    private readonly TimeProvider _timeProvider;

    public BookService(
        IBookRepository bookRepository,
        IBookCopyRepository bookCopyRepository,
        TimeProvider? timeProvider = null)
    {
        _bookRepository = bookRepository;
        _bookCopyRepository = bookCopyRepository;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<IReadOnlyCollection<BookCatalogItemDto>> SearchBooksAsync(
        string? query = null,
        string? category = null,
        int page = 1,
        int pageSize = 10)
    {
        var books = await _bookRepository.GetAllAsync();
        var copies = await _bookCopyRepository.GetAllAsync();
        var copiesByBookId = copies
            .GroupBy(copy => copy.BookId)
            .ToDictionary(group => group.Key, group => group.ToArray());

        IEnumerable<Book> filtered = books;

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.Trim();
            filtered = filtered.Where(book =>
                book.Title.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                book.ISBN.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                (book.Author?.Name.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalizedCategory = category.Trim();
            filtered = filtered.Where(book =>
                book.Category.Contains(normalizedCategory, StringComparison.OrdinalIgnoreCase));
        }

        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        return filtered
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(book => MapToCatalogDto(book, copiesByBookId))
            .ToArray();
    }

    public async Task<BookCatalogItemDto?> GetBookByIdAsync(int bookId)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);

        if (book is null)
        {
            return null;
        }

        var copies = await _bookCopyRepository.GetByBookIdAsync(bookId);
        var copiesByBookId = new Dictionary<int, BookCopy[]>
        {
            [bookId] = copies.ToArray()
        };

        return MapToCatalogDto(book, copiesByBookId);
    }

    public async Task<Book> AddBookAsync(
        string title,
        string isbn,
        string authorName,
        string category,
        int publishedYear)
    {
        ValidateBookInput(title, isbn, authorName, category, publishedYear);

        var normalizedIsbn = isbn.Trim();
        var existingByIsbn = await _bookRepository.GetByIsbnAsync(normalizedIsbn);

        if (existingByIsbn is not null)
        {
            throw new InvalidOperationException("A book with this ISBN already exists.");
        }

        var book = new Book
        {
            Title = title.Trim(),
            ISBN = normalizedIsbn,
            Category = category.Trim(),
            PublishedYear = publishedYear,
            Author = new Author
            {
                Name = authorName.Trim()
            }
        };

        await _bookRepository.AddAsync(book);

        return book;
    }

    public async Task<Book> UpdateBookAsync(
        int bookId,
        string title,
        string isbn,
        string authorName,
        string category,
        int publishedYear)
    {
        ValidateBookInput(title, isbn, authorName, category, publishedYear);

        var existingBook = await _bookRepository.GetByIdAsync(bookId);

        if (existingBook is null)
        {
            throw new InvalidOperationException("Book was not found.");
        }

        var normalizedIsbn = isbn.Trim();
        var bookWithSameIsbn = await _bookRepository.GetByIsbnAsync(normalizedIsbn);

        if (bookWithSameIsbn is not null && bookWithSameIsbn.Id != bookId)
        {
            throw new InvalidOperationException("A book with this ISBN already exists.");
        }

        existingBook.Title = title.Trim();
        existingBook.ISBN = normalizedIsbn;
        existingBook.Category = category.Trim();
        existingBook.PublishedYear = publishedYear;
        existingBook.Author = new Author
        {
            Id = existingBook.AuthorId,
            Name = authorName.Trim()
        };

        await _bookRepository.UpdateAsync(existingBook);

        return existingBook;
    }

    public Task DeleteBookAsync(int bookId)
    {
        return _bookRepository.DeleteAsync(bookId);
    }

    public Task<IReadOnlyCollection<BookCopy>> GetBookCopiesAsync()
    {
        return _bookCopyRepository.GetAllAsync();
    }

    public Task<IReadOnlyCollection<BookCopy>> GetCopiesByBookIdAsync(int bookId)
    {
        return _bookCopyRepository.GetByBookIdAsync(bookId);
    }

    public async Task<BookCopy> AddBookCopyAsync(int bookId, string inventoryNumber)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);

        if (book is null)
        {
            throw new InvalidOperationException("Book was not found.");
        }

        var normalizedInventory = ValidateInventoryNumber(inventoryNumber);
        var existingCopy = await _bookCopyRepository.GetByInventoryNumberAsync(normalizedInventory);

        if (existingCopy is not null)
        {
            throw new InvalidOperationException("A book copy with this inventory number already exists.");
        }

        var copy = new BookCopy
        {
            BookId = bookId,
            InventoryNumber = normalizedInventory,
            Status = BookCopyStatus.Available
        };

        await _bookCopyRepository.AddAsync(copy);

        return copy;
    }

    public async Task<BookCopy> UpdateBookCopyAsync(
        int copyId,
        string inventoryNumber,
        BookCopyStatus status)
    {
        var existingCopy = await _bookCopyRepository.GetByIdAsync(copyId);

        if (existingCopy is null)
        {
            throw new InvalidOperationException("Book copy was not found.");
        }

        var normalizedInventory = ValidateInventoryNumber(inventoryNumber);
        var copyWithSameInventory = await _bookCopyRepository.GetByInventoryNumberAsync(normalizedInventory);

        if (copyWithSameInventory is not null && copyWithSameInventory.Id != copyId)
        {
            throw new InvalidOperationException("A book copy with this inventory number already exists.");
        }

        existingCopy.InventoryNumber = normalizedInventory;
        existingCopy.Status = status;

        await _bookCopyRepository.UpdateAsync(existingCopy);

        return existingCopy;
    }

    public Task DeleteBookCopyAsync(int copyId)
    {
        return _bookCopyRepository.DeleteAsync(copyId);
    }

    private void ValidateBookInput(
        string title,
        string isbn,
        string authorName,
        string category,
        int publishedYear)
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

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Book category is required.", nameof(category));
        }

        var currentYear = _timeProvider.GetUtcNow().UtcDateTime.Year;

        if (publishedYear <= 0 || publishedYear > currentYear)
        {
            throw new ArgumentException(
                $"Published year cannot be in the future (must be between 1 and {currentYear}).",
                nameof(publishedYear));
        }
    }

    private static string ValidateInventoryNumber(string inventoryNumber)
    {
        if (string.IsNullOrWhiteSpace(inventoryNumber))
        {
            throw new ArgumentException("Inventory number is required.", nameof(inventoryNumber));
        }

        var normalized = inventoryNumber.Trim().ToUpperInvariant();

        if (!InventoryNumberPattern().IsMatch(normalized))
        {
            throw new ArgumentException(
                "Inventory number must follow the INV-xxx format.",
                nameof(inventoryNumber));
        }

        return normalized;
    }

    private static BookCatalogItemDto MapToCatalogDto(
        Book book,
        IReadOnlyDictionary<int, BookCopy[]> copiesByBookId)
    {
        var copies = copiesByBookId.TryGetValue(book.Id, out var matchedCopies)
            ? matchedCopies
            : Array.Empty<BookCopy>();

        var availableCount = copies.Count(copy => copy.Status == BookCopyStatus.Available);

        return new BookCatalogItemDto(
            book.Id,
            book.Title,
            book.ISBN,
            book.Category,
            book.PublishedYear,
            book.AuthorId,
            book.Author?.Name ?? string.Empty,
            copies.Length,
            availableCount);
    }

    [GeneratedRegex("^INV-[A-Z0-9]+$")]
    private static partial Regex InventoryNumberPattern();
}
