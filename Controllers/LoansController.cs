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
    public async Task<IActionResult> IssueLoan([FromBody] IssueLoanRequest request)
    {
        try
        {
            await _loanService.IssueLoanAsync(request.BookCopyId, request.ReaderId);
            return Ok();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }
}

public sealed record IssueLoanRequest(int BookCopyId, int ReaderId);