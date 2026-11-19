using System.Collections.Concurrent;
using Library.Core.Entities;
using Library.Core.Interfaces;

namespace Library.DAL.Repositories;

public class InMemoryBookRepository : IBookRepository
{
    private readonly ConcurrentDictionary<int, Book> _books = new();
    private int _nextBookId = 2;
    private int _nextAuthorId = 2;

    public InMemoryBookRepository()
    {
        _books.TryAdd(1, new Book
        {
            Id = 1,
            Title = "Clean Architecture",
            ISBN = "978-0134494166",
            AuthorId = 1,
            Author = new Author { Id = 1, Name = "Robert C. Martin" }
        });

        _books.TryAdd(2, new Book
        {
            Id = 2,
            Title = "Domain-Driven Design",
            ISBN = "978-0321125217",
            AuthorId = 2,
            Author = new Author { Id = 2, Name = "Eric Evans" }
        });
    }

    public Task<IReadOnlyCollection<Book>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyCollection<Book> books = _books.Values
            .OrderBy(book => book.Id)
            .ToArray();

        return Task.FromResult(books);
    }

    public Task<Book?> GetByIdAsync(
        int bookId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _books.TryGetValue(bookId, out var book);

        return Task.FromResult(book);
    }

    public Task AddAsync(
        Book book,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        book.Id = Interlocked.Increment(ref _nextBookId);

        if (book.Author is not null)
        {
            if (book.Author.Id == 0)
            {
                book.Author.Id = Interlocked.Increment(ref _nextAuthorId);
            }

            book.AuthorId = book.Author.Id;
        }

        _books.TryAdd(book.Id, book);

        return Task.CompletedTask;
    }
}
