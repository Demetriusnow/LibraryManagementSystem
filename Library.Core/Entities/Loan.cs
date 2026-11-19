using Library.Core.Enums;

namespace Library.Core.Entities;

public class Loan
{
    public int Id { get; set; }

    public int BookCopyId { get; set; }

    public int ReaderId { get; set; }

    public DateTime BorrowDate { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public LoanStatus Status { get; set; } = LoanStatus.Active;
}