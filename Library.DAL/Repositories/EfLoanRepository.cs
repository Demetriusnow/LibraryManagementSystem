using Library.Core.Entities;
using Library.Core.Interfaces;
using Library.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.DAL.Repositories;

public class EfLoanRepository : ILoanRepository
{
    private readonly LibraryDbContext _dbContext;

    public EfLoanRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Loan>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Loans
            .AsNoTracking()
            .OrderBy(loan => loan.Id)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddAsync(
        Loan loan,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Loans.AddAsync(loan, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Loan>> GetActiveByReaderIdAsync(
        int readerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Loans
            .AsNoTracking()
            .Where(loan => loan.ReaderId == readerId && loan.ReturnDate == null)
            .OrderBy(loan => loan.Id)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Loan?> GetByIdAsync(
        int loanId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Loans
            .AsNoTracking()
            .FirstOrDefaultAsync(loan => loan.Id == loanId, cancellationToken);
    }

    public async Task UpdateAsync(
        Loan loan,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Loans
            .FirstOrDefaultAsync(item => item.Id == loan.Id, cancellationToken);

        if (existing is null)
        {
            throw new InvalidOperationException(
                $"Loan with ID {loan.Id} was not found.");
        }

        _dbContext.Entry(existing).CurrentValues.SetValues(loan);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
