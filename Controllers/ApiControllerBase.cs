using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected async Task<IActionResult> ExecuteActionAsync(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    protected Task<IActionResult> ExecuteActionAsync(Func<Task> action)
    {
        return ExecuteActionAsync(async () =>
        {
            await action();
            return Ok();
        });
    }
}
