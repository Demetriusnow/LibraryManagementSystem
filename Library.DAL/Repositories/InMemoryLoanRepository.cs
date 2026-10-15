using System.Collections.Concurrent;
using Library.Core.Entities;
using Library.Core.Interfaces;

namespace Library.DAL.Repositories;

public class InMemoryLoanRepository : ILoanRepository
{
    private readonly ConcurrentDictionary<int, Loan> _loans = new();
    private int _nextLoanId;

    public Task AddAsync(
        Loan loan,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        loan.Id = Interlocked.Increment(ref _nextLoanId);
        _loans.TryAdd(loan.Id, loan);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<Loan>> GetActiveByReaderIdAsync(
        int readerId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyCollection<Loan> activeLoans = _loans.Values
            .Where(loan => loan.ReaderId == readerId && loan.ReturnDate is null)
            .ToArray();

        return Task.FromResult(activeLoans);
    }

    public Task<Loan?> GetByIdAsync(
        int loanId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _loans.TryGetValue(loanId, out var loan);

        return Task.FromResult(loan);
    }

    public Task UpdateAsync(
        Loan loan,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_loans.TryGetValue(loan.Id, out var existingLoan) ||
            !_loans.TryUpdate(loan.Id, loan, existingLoan))
        {
            throw new InvalidOperationException(
                $"Loan with ID {loan.Id} was not found.");
        }

        return Task.CompletedTask;
    }
}