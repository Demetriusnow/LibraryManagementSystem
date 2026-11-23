using Library.Core.Entities;
using Library.Core.Interfaces;
using Library.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.DAL.Repositories;

public class EfReaderRepository : IReaderRepository
{
    private readonly LibraryDbContext _dbContext;

    public EfReaderRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Reader>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Readers
            .AsNoTracking()
            .OrderBy(reader => reader.Id)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Reader?> GetByIdAsync(
        int readerId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Readers
            .AsNoTracking()
            .FirstOrDefaultAsync(reader => reader.Id == readerId, cancellationToken);
    }

    public async Task AddAsync(
        Reader reader,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Readers.AddAsync(reader, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
