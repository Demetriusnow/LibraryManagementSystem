using System.Collections.Concurrent;
using Library.Core.Entities;
using Library.Core.Enums;
using Library.Core.Interfaces;

namespace Library.DAL.Repositories;

public class InMemoryBookCopyRepository : IBookCopyRepository
{
    private readonly ConcurrentDictionary<int, BookCopy> _bookCopies = new();
    private int _nextCopyId = 2;

    public InMemoryBookCopyRepository()
    {
        _bookCopies.TryAdd(1, new BookCopy
        {
            Id = 1,
            BookId = 1,
            InventoryNumber = "INV-001",
            Status = BookCopyStatus.Available
        });

        _bookCopies.TryAdd(2, new BookCopy
        {
            Id = 2,
            BookId = 2,
            InventoryNumber = "INV-002",
            Status = BookCopyStatus.Borrowed
        });
    }

    public Task<IReadOnlyCollection<BookCopy>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyCollection<BookCopy> copies = _bookCopies.Values
            .OrderBy(copy => copy.Id)
            .ToArray();

        return Task.FromResult(copies);
    }

    public Task<BookCopy?> GetByIdAsync(
        int bookCopyId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _bookCopies.TryGetValue(bookCopyId, out var bookCopy);

        return Task.FromResult(bookCopy);
    }

    public Task<IReadOnlyCollection<BookCopy>> GetByBookIdAsync(
        int bookId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyCollection<BookCopy> copies = _bookCopies.Values
            .Where(copy => copy.BookId == bookId)
            .OrderBy(copy => copy.Id)
            .ToArray();

        return Task.FromResult(copies);
    }

    public Task<BookCopy?> GetByInventoryNumberAsync(
        string inventoryNumber,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var copy = _bookCopies.Values.FirstOrDefault(item =>
            string.Equals(item.InventoryNumber, inventoryNumber.Trim(), StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(copy);
    }

    public Task AddAsync(
        BookCopy bookCopy,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        bookCopy.Id = Interlocked.Increment(ref _nextCopyId);
        _bookCopies.TryAdd(bookCopy.Id, bookCopy);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(
        BookCopy bookCopy,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_bookCopies.TryGetValue(bookCopy.Id, out var existingCopy) ||
            !_bookCopies.TryUpdate(bookCopy.Id, bookCopy, existingCopy))
        {
            throw new InvalidOperationException(
                $"Book copy with ID {bookCopy.Id} was not found.");
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        int bookCopyId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_bookCopies.TryRemove(bookCopyId, out _))
        {
            throw new InvalidOperationException(
                $"Book copy with ID {bookCopyId} was not found.");
        }

        return Task.CompletedTask;
    }
}