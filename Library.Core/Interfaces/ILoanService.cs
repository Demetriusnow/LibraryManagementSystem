using Library.Core.Entities;

namespace Library.Core.Interfaces;

public interface ILoanService
{
    Task IssueLoanAsync(int bookCopyId, int readerId);

    Task ReturnLoanAsync(int loanId);

    Task<IReadOnlyCollection<Loan>> GetActiveLoansByReaderAsync(int readerId);
}