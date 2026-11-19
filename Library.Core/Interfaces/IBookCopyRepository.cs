using Library.Core.Entities;

namespace Library.Core.Interfaces;

public interface IBookCopyRepository
{
    Task<IReadOnlyCollection<BookCopy>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<BookCopy?> GetByIdAsync(
        int bookCopyId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<BookCopy>> GetByBookIdAsync(
        int bookId,
        CancellationToken cancellationToken = default);

    Task<BookCopy?> GetByInventoryNumberAsync(
        string inventoryNumber,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        BookCopy bookCopy,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        BookCopy bookCopy,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int bookCopyId,
        CancellationToken cancellationToken = default);
}