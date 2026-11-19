using Library.Core.DTOs;
using Library.Core.Entities;
using Library.Core.Enums;

namespace Library.Core.Interfaces;

public interface IBookService
{
    Task<IReadOnlyCollection<BookCatalogItemDto>> SearchBooksAsync(
        string? query = null,
        string? category = null,
        int page = 1,
        int pageSize = 10);

    Task<BookCatalogItemDto?> GetBookByIdAsync(int bookId);

    Task<Book> AddBookAsync(
        string title,
        string isbn,
        string authorName,
        string category,
        int publishedYear);

    Task<Book> UpdateBookAsync(
        int bookId,
        string title,
        string isbn,
        string authorName,
        string category,
        int publishedYear);

    Task DeleteBookAsync(int bookId);

    Task<IReadOnlyCollection<BookCopy>> GetBookCopiesAsync();

    Task<IReadOnlyCollection<BookCopy>> GetCopiesByBookIdAsync(int bookId);

    Task<BookCopy> AddBookCopyAsync(int bookId, string inventoryNumber);

    Task<BookCopy> UpdateBookCopyAsync(int copyId, string inventoryNumber, BookCopyStatus status);

    Task DeleteBookCopyAsync(int copyId);
}
