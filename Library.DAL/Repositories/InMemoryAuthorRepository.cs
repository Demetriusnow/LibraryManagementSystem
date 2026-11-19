using System.Collections.Concurrent;
using Library.Core.Entities;
using Library.Core.Interfaces;

namespace Library.DAL.Repositories;

public class InMemoryAuthorRepository : IAuthorRepository
{
    private readonly ConcurrentDictionary<int, Author> _authors = new();
    private int _nextAuthorId = 2;

    public InMemoryAuthorRepository()
    {
        _authors.TryAdd(1, new Author { Id = 1, Name = "Robert C. Martin" });
        _authors.TryAdd(2, new Author { Id = 2, Name = "Eric Evans" });
    }

    public Task<IReadOnlyCollection<Author>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyCollection<Author> authors = _authors.Values
            .OrderBy(author => author.Id)
            .ToArray();

        return Task.FromResult(authors);
    }

    public Task<Author?> GetByIdAsync(
        int authorId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _authors.TryGetValue(authorId, out var author);

        return Task.FromResult(author);
    }

    public Task<Author?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var author = _authors.Values.FirstOrDefault(item =>
            string.Equals(item.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(author);
    }

    public Task AddAsync(
        Author author,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        author.Id = Interlocked.Increment(ref _nextAuthorId);
        _authors.TryAdd(author.Id, author);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(
        Author author,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_authors.TryGetValue(author.Id, out var existingAuthor) ||
            !_authors.TryUpdate(author.Id, author, existingAuthor))
        {
            throw new InvalidOperationException(
                $"Author with ID {author.Id} was not found.");
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        int authorId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_authors.TryRemove(authorId, out _))
        {
            throw new InvalidOperationException(
                $"Author with ID {authorId} was not found.");
        }

        return Task.CompletedTask;
    }
}
