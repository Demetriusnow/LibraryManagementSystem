using Library.Core.Entities;

namespace Library.Core.Interfaces;

public interface IReaderRepository
{
    Task<IReadOnlyCollection<Reader>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Reader?> GetByIdAsync(
        int readerId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Reader reader,
        CancellationToken cancellationToken = default);
}
