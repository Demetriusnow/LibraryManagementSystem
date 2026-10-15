using Library.Core.Entities;

namespace Library.Core.Interfaces;

public interface IBookCopyRepository
{
    Task<BookCopy?> GetByIdAsync(
        int bookCopyId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        BookCopy bookCopy,
        CancellationToken cancellationToken = default);
}