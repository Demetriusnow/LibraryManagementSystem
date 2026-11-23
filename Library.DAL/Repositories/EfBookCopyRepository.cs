using Library.Core.Entities;
using Library.Core.Interfaces;
using Library.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.DAL.Repositories;

public class EfBookCopyRepository : IBookCopyRepository
{
    private readonly LibraryDbContext _dbContext;

    public EfBookCopyRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<BookCopy>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.BookCopies
            .AsNoTracking()
            .OrderBy(copy => copy.Id)
            .ToArrayAsync(cancellationToken);
    }

    public Task<BookCopy?> GetByIdAsync(
        int bookCopyId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.BookCopies
            .AsNoTracking()
            .FirstOrDefaultAsync(copy => copy.Id == bookCopyId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<BookCopy>> GetByBookIdAsync(
        int bookId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.BookCopies
            .AsNoTracking()
            .Where(copy => copy.BookId == bookId)
            .OrderBy(copy => copy.Id)
            .ToArrayAsync(cancellationToken);
    }

    public Task<BookCopy?> GetByInventoryNumberAsync(
        string inventoryNumber,
        CancellationToken cancellationToken = default)
    {
        var normalized = inventoryNumber.Trim().ToLower();

        return _dbContext.BookCopies
            .AsNoTracking()
            .FirstOrDefaultAsync(copy => copy.InventoryNumber.ToLower() == normalized, cancellationToken);
    }

    public async Task AddAsync(
        BookCopy bookCopy,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.BookCopies.AddAsync(bookCopy, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        BookCopy bookCopy,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.BookCopies
            .FirstOrDefaultAsync(copy => copy.Id == bookCopy.Id, cancellationToken);

        if (existing is null)
        {
            throw new InvalidOperationException(
                $"Book copy with ID {bookCopy.Id} was not found.");
        }

        _dbContext.Entry(existing).CurrentValues.SetValues(bookCopy);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        int bookCopyId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.BookCopies
            .FirstOrDefaultAsync(copy => copy.Id == bookCopyId, cancellationToken);

        if (existing is null)
        {
            throw new InvalidOperationException(
                $"Book copy with ID {bookCopyId} was not found.");
        }

        _dbContext.BookCopies.Remove(existing);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
