using Library.Core.Entities;
using Library.Core.Enums;
using Library.Core.Interfaces;

namespace Library.BLL.Services;

public class LoanService : ILoanService
{
    private readonly IBookCopyRepository _bookCopyRepository;
    private readonly ILoanRepository _loanRepository;

    public LoanService(
        IBookCopyRepository bookCopyRepository,
        ILoanRepository loanRepository)
    {
        _bookCopyRepository = bookCopyRepository;
        _loanRepository = loanRepository;
    }

    public async Task IssueLoanAsync(int bookCopyId, int readerId)
    {
        var bookCopy = await _bookCopyRepository.GetByIdAsync(bookCopyId);

        if (bookCopy is null)
        {
            throw new InvalidOperationException("Book copy was not found.");
        }

        if (bookCopy.Status != BookCopyStatus.Available)
        {
            throw new InvalidOperationException("Book copy is not available.");
        }

        var borrowDate = DateTime.UtcNow;

        bookCopy.Status = BookCopyStatus.Borrowed;

        await _bookCopyRepository.UpdateAsync(bookCopy);
        await _loanRepository.AddAsync(new Loan
        {
            BookCopyId = bookCopyId,
            ReaderId = readerId,
            BorrowDate = borrowDate,
            DueDate = borrowDate.AddDays(14)
        });
    }
}