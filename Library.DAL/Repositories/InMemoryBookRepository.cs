using System.Collections.Concurrent;
using Library.Core.Entities;
using Library.Core.Interfaces;

namespace Library.DAL.Repositories;

public class InMemoryBookRepository : IBookRepository
{
    private readonly ConcurrentDictionary<int, Book> _books = new();
    private readonly IAuthorRepository? _authorRepository;
    private int _nextBookId = 2;
    private int _nextAuthorId = 2;

    public InMemoryBookRepository(IAuthorRepository? authorRepository = null)
    {
        _authorRepository = authorRepository;

        _books.TryAdd(1, new Book
        {
            Id = 1,
            Title = "Clean Architecture",
            ISBN = "978-0134494166",
            Category = "Software Engineering",
            PublishedYear = 2017,
            AuthorId = 1,
            Author = new Author { Id = 1, Name = "Robert C. Martin" }
        });

        _books.TryAdd(2, new Book
        {
            Id = 2,
            Title = "Domain-Driven Design",
            ISBN = "978-0321125217",
            Category = "Architecture",
            PublishedYear = 2003,
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

    public Task<Book?> GetByIsbnAsync(
        string isbn,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var book = _books.Values.FirstOrDefault(item =>
            string.Equals(item.ISBN, isbn.Trim(), StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(book);
    }

    public async Task AddAsync(
        Book book,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        book.Id = Interlocked.Increment(ref _nextBookId);
        await ResolveAuthorAsync(book, cancellationToken);

        _books.TryAdd(book.Id, book);
    }

    public async Task UpdateAsync(
        Book book,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_books.TryGetValue(book.Id, out var existingBook))
        {
            throw new InvalidOperationException(
                $"Book with ID {book.Id} was not found.");
        }

        await ResolveAuthorAsync(book, cancellationToken);

        if (!_books.TryUpdate(book.Id, book, existingBook))
        {
            throw new InvalidOperationException(
                $"Book with ID {book.Id} was not found.");
        }
    }

    private async Task ResolveAuthorAsync(Book book, CancellationToken cancellationToken)
    {
        if (book.Author is null)
        {
            return;
        }

        if (_authorRepository is not null && !string.IsNullOrWhiteSpace(book.Author.Name))
        {
            var existingAuthor = await _authorRepository.GetByNameAsync(book.Author.Name, cancellationToken);
            if (existingAuthor is null)
            {
                await _authorRepository.AddAsync(book.Author, cancellationToken);
            }
            else
            {
                book.Author = existingAuthor;
            }
        }
        else if (book.Author.Id == 0)
        {
            book.Author.Id = Interlocked.Increment(ref _nextAuthorId);
        }

        book.AuthorId = book.Author.Id;
    }

    public Task DeleteAsync(
        int bookId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_books.TryRemove(bookId, out _))
        {
            throw new InvalidOperationException(
                $"Book with ID {bookId} was not found.");
        }

        return Task.CompletedTask;
    }
}
