using Library.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers;

[ApiController]
[Route("api/loans")]
public class LoansController : ApiControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpPost("issue")]
    public Task<IActionResult> IssueLoan([FromBody] IssueLoanRequest request)
    {
        return ExecuteActionAsync(() =>
            _loanService.IssueLoanAsync(request.BookCopyId, request.ReaderId));
    }

    [HttpPost("return")]
    public Task<IActionResult> ReturnLoan([FromBody] ReturnLoanRequest request)
    {
        return ExecuteActionAsync(() =>
            _loanService.ReturnLoanAsync(request.LoanId));
    }

    [HttpGet("reader/{readerId:int}")]
    public async Task<IActionResult> GetActiveLoansByReader(int readerId)
    {
        var loans = await _loanService.GetActiveLoansByReaderAsync(readerId);
        return Ok(loans);
    }

    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdueLoans()
    {
        var overdueLoans = await _loanService.GetOverdueLoansAsync();
        return Ok(overdueLoans);
    }

    [HttpGet("overdue-readers")]
    public async Task<IActionResult> GetOverdueReaders()
    {
        var overdueReaders = await _loanService.GetOverdueReadersAsync();
        return Ok(overdueReaders);
    }
}

public sealed record IssueLoanRequest(int BookCopyId, int ReaderId);

public sealed record ReturnLoanRequest(int LoanId);
