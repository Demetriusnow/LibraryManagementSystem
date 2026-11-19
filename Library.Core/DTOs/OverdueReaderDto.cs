using Library.Core.Entities;

namespace Library.Core.DTOs;

public sealed record OverdueReaderDto(
    int ReaderId,
    string FullName,
    string Email,
    IReadOnlyCollection<Loan> OverdueLoans);
