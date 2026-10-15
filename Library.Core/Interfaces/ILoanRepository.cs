using Library.Core.Entities;

namespace Library.Core.Interfaces;

public interface ILoanRepository
{
    Task AddAsync(
        Loan loan,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Loan>> GetActiveByReaderIdAsync(
        int readerId,
        CancellationToken cancellationToken = default);

    Task<Loan?> GetByIdAsync(
        int loanId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Loan loan,
        CancellationToken cancellationToken = default);
}