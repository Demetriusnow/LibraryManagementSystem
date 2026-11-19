using Library.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks([FromQuery] string? query = null)
    {
        var books = await _bookService.GetBooksAsync(query);
        return Ok(books);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBookById(int id)
    {
        var book = await _bookService.GetBookByIdAsync(id);

        if (book is null)
        {
            return NotFound(new { error = "Book was not found." });
        }

        return Ok(book);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBook([FromBody] CreateBookRequest request)
    {
        try
        {
            var createdBook = await _bookService.AddBookAsync(
                request.Title,
                request.Isbn,
                request.AuthorName);

            return CreatedAtAction(
                nameof(GetBookById),
                new { id = createdBook.Id },
                createdBook);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpGet("copies")]
    public async Task<IActionResult> GetBookCopies()
    {
        var copies = await _bookService.GetBookCopiesAsync();
        return Ok(copies);
    }
}

public sealed record CreateBookRequest(string Title, string Isbn, string AuthorName);
