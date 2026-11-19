using Library.Core.Entities;

namespace Library.Core.Interfaces;

public interface IAuthorRepository
{
    Task<IReadOnlyCollection<Author>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Author?> GetByIdAsync(
        int authorId,
        CancellationToken cancellationToken = default);

    Task<Author?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Author author,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Author author,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int authorId,
        CancellationToken cancellationToken = default);
}
