using Library.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers;

[ApiController]
[Route("api/authors")]
public class AuthorsController : ApiControllerBase
{
    private readonly IAuthorService _authorService;

    public AuthorsController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuthors()
    {
        var authors = await _authorService.GetAuthorsAsync();
        return Ok(authors);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAuthorById(int id)
    {
        var author = await _authorService.GetAuthorByIdAsync(id);

        if (author is null)
        {
            return NotFound(new { error = "Author was not found." });
        }

        return Ok(author);
    }

    [HttpPost]
    public Task<IActionResult> CreateAuthor([FromBody] AuthorRequest request)
    {
        return ExecuteActionAsync(async () =>
        {
            var createdAuthor = await _authorService.AddAuthorAsync(request.Name);
            return CreatedAtAction(
                nameof(GetAuthorById),
                new { id = createdAuthor.Id },
                createdAuthor);
        });
    }

    [HttpPut("{id:int}")]
    public Task<IActionResult> UpdateAuthor(int id, [FromBody] AuthorRequest request)
    {
        return ExecuteActionAsync(async () =>
        {
            var updatedAuthor = await _authorService.UpdateAuthorAsync(id, request.Name);
            return Ok(updatedAuthor);
        });
    }

    [HttpDelete("{id:int}")]
    public Task<IActionResult> DeleteAuthor(int id)
    {
        return ExecuteActionAsync(async () =>
        {
            await _authorService.DeleteAuthorAsync(id);
            return NoContent();
        });
    }
}

public sealed record AuthorRequest(string Name);
