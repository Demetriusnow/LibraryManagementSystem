using System.Collections.Concurrent;
using Library.Core.Entities;
using Library.Core.Interfaces;

namespace Library.DAL.Repositories;

public class InMemoryReaderRepository : IReaderRepository
{
    private readonly ConcurrentDictionary<int, Reader> _readers = new();
    private int _nextReaderId = 2;

    public InMemoryReaderRepository()
    {
        _readers.TryAdd(1, new Reader
        {
            Id = 1,
            FullName = "Ivan Petrenko",
            Email = "ivan.petrenko@example.com"
        });

        _readers.TryAdd(2, new Reader
        {
            Id = 2,
            FullName = "Olena Kovalenko",
            Email = "olena.kovalenko@example.com"
        });
    }

    public Task<IReadOnlyCollection<Reader>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyCollection<Reader> readers = _readers.Values
            .OrderBy(reader => reader.Id)
            .ToArray();

        return Task.FromResult(readers);
    }

    public Task<Reader?> GetByIdAsync(
        int readerId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _readers.TryGetValue(readerId, out var reader);

        return Task.FromResult(reader);
    }

    public Task AddAsync(
        Reader reader,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        reader.Id = Interlocked.Increment(ref _nextReaderId);
        _readers.TryAdd(reader.Id, reader);

        return Task.CompletedTask;
    }
}
