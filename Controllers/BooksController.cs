using Library.Core.Enums;
using Library.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ApiControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks(
        [FromQuery] string? query = null,
        [FromQuery] string? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var books = await _bookService.SearchBooksAsync(query, category, page, pageSize);
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
    public Task<IActionResult> CreateBook([FromBody] CreateBookRequest request)
    {
        return ExecuteActionAsync(async () =>
        {
            var createdBook = await _bookService.AddBookAsync(
                request.Title,
                request.Isbn,
                request.AuthorName,
                request.Category,
                request.PublishedYear);

            return CreatedAtAction(
                nameof(GetBookById),
                new { id = createdBook.Id },
                createdBook);
        });
    }

    [HttpPut("{id:int}")]
    public Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookRequest request)
    {
        return ExecuteActionAsync(async () =>
        {
            var updatedBook = await _bookService.UpdateBookAsync(
                id,
                request.Title,
                request.Isbn,
                request.AuthorName,
                request.Category,
                request.PublishedYear);

            return Ok(updatedBook);
        });
    }

    [HttpDelete("{id:int}")]
    public Task<IActionResult> DeleteBook(int id)
    {
        return ExecuteActionAsync(async () =>
        {
            await _bookService.DeleteBookAsync(id);
            return NoContent();
        });
    }

    [HttpGet("copies")]
    public async Task<IActionResult> GetBookCopies()
    {
        var copies = await _bookService.GetBookCopiesAsync();
        return Ok(copies);
    }

    [HttpGet("{id:int}/copies")]
    public async Task<IActionResult> GetCopiesByBookId(int id)
    {
        var copies = await _bookService.GetCopiesByBookIdAsync(id);
        return Ok(copies);
    }

    [HttpPost("{id:int}/copies")]
    public Task<IActionResult> AddBookCopy(int id, [FromBody] CreateBookCopyRequest request)
    {
        return ExecuteActionAsync(async () =>
        {
            var createdCopy = await _bookService.AddBookCopyAsync(id, request.InventoryNumber);

            return CreatedAtAction(
                nameof(GetCopiesByBookId),
                new { id = createdCopy.BookId },
                createdCopy);
        });
    }

    [HttpPut("copies/{copyId:int}")]
    public Task<IActionResult> UpdateBookCopy(
        int copyId,
        [FromBody] UpdateBookCopyRequest request)
    {
        return ExecuteActionAsync(async () =>
        {
            var updatedCopy = await _bookService.UpdateBookCopyAsync(
                copyId,
                request.InventoryNumber,
                request.Status);

            return Ok(updatedCopy);
        });
    }

    [HttpDelete("copies/{copyId:int}")]
    public Task<IActionResult> DeleteBookCopy(int copyId)
    {
        return ExecuteActionAsync(async () =>
        {
            await _bookService.DeleteBookCopyAsync(copyId);
            return NoContent();
        });
    }
}

public sealed record CreateBookRequest(
    string Title,
    string Isbn,
    string AuthorName,
    string Category,
    int PublishedYear);

public sealed record UpdateBookRequest(
    string Title,
    string Isbn,
    string AuthorName,
    string Category,
    int PublishedYear);

public sealed record CreateBookCopyRequest(string InventoryNumber);

public sealed record UpdateBookCopyRequest(string InventoryNumber, BookCopyStatus Status);
