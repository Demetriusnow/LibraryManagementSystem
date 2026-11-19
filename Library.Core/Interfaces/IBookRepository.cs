using Library.Core.Entities;

namespace Library.Core.Interfaces;

public interface IBookRepository
{
    Task<IReadOnlyCollection<Book>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Book?> GetByIdAsync(
        int bookId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Book book,
        CancellationToken cancellationToken = default);
}
