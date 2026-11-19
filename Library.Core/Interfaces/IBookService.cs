using Library.Core.Entities;

namespace Library.Core.Interfaces;

public interface IBookService
{
    Task<IReadOnlyCollection<Book>> GetBooksAsync(string? searchQuery = null);

    Task<Book?> GetBookByIdAsync(int bookId);

    Task<Book> AddBookAsync(string title, string isbn, string authorName);

    Task<IReadOnlyCollection<BookCopy>> GetBookCopiesAsync();
}
