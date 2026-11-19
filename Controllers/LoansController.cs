using Library.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers;

[ApiController]
[Route("api/loans")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpPost("issue")]
    public Task<IActionResult> IssueLoan([FromBody] IssueLoanRequest request)
    {
        return ExecuteAsync(() =>
            _loanService.IssueLoanAsync(request.BookCopyId, request.ReaderId));
    }

    [HttpPost("return")]
    public Task<IActionResult> ReturnLoan([FromBody] ReturnLoanRequest request)
    {
        return ExecuteAsync(() =>
            _loanService.ReturnLoanAsync(request.LoanId));
    }

    [HttpGet("reader/{readerId:int}")]
    public async Task<IActionResult> GetActiveLoansByReader(int readerId)
    {
        var loans = await _loanService.GetActiveLoansByReaderAsync(readerId);
        return Ok(loans);
    }

    private async Task<IActionResult> ExecuteAsync(Func<Task> action)
    {
        try
        {
            await action();
            return Ok();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }
}

public sealed record IssueLoanRequest(int BookCopyId, int ReaderId);

public sealed record ReturnLoanRequest(int LoanId);