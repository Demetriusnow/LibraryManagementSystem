using Library.Core.Entities;
using Library.Core.Interfaces;
using Library.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.DAL.Repositories;

public class EfAuthorRepository : IAuthorRepository
{
    private readonly LibraryDbContext _dbContext;

    public EfAuthorRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Author>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Authors
            .AsNoTracking()
            .OrderBy(author => author.Id)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Author?> GetByIdAsync(
        int authorId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Authors
            .AsNoTracking()
            .FirstOrDefaultAsync(author => author.Id == authorId, cancellationToken);
    }

    public Task<Author?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLower();

        return _dbContext.Authors
            .AsNoTracking()
            .FirstOrDefaultAsync(author => author.Name.ToLower() == normalized, cancellationToken);
    }

    public async Task AddAsync(
        Author author,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Authors.AddAsync(author, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Author author,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Authors
            .FirstOrDefaultAsync(item => item.Id == author.Id, cancellationToken);

        if (existing is null)
        {
            throw new InvalidOperationException(
                $"Author with ID {author.Id} was not found.");
        }

        existing.Name = author.Name;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        int authorId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Authors
            .FirstOrDefaultAsync(author => author.Id == authorId, cancellationToken);

        if (existing is null)
        {
            throw new InvalidOperationException(
                $"Author with ID {authorId} was not found.");
        }

        _dbContext.Authors.Remove(existing);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
