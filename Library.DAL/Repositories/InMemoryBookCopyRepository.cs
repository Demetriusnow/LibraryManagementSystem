using System.Collections.Concurrent;
using Library.Core.Entities;
using Library.Core.Enums;
using Library.Core.Interfaces;

namespace Library.DAL.Repositories;

public class InMemoryBookCopyRepository : IBookCopyRepository
{
    private readonly ConcurrentDictionary<int, BookCopy> _bookCopies = new();

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

    public Task<BookCopy?> GetByIdAsync(
        int bookCopyId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _bookCopies.TryGetValue(bookCopyId, out var bookCopy);

        return Task.FromResult(bookCopy);
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
}