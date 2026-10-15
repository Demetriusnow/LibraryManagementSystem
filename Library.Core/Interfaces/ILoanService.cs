namespace Library.Core.Interfaces;

public interface ILoanService
{
    Task IssueLoanAsync(int bookCopyId, int readerId);
}