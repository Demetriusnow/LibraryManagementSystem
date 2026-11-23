using Library.Core.Entities;
using Library.Core.Interfaces;
using Library.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.DAL.Repositories;

public class EfBookRepository : IBookRepository
{
    private readonly LibraryDbContext _dbContext;

    public EfBookRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Book>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Books
            .AsNoTracking()
            .Include(book => book.Author)
            .OrderBy(book => book.Id)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Book?> GetByIdAsync(
        int bookId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Books
            .AsNoTracking()
            .Include(book => book.Author)
            .FirstOrDefaultAsync(book => book.Id == bookId, cancellationToken);
    }

    public Task<Book?> GetByIsbnAsync(
        string isbn,
        CancellationToken cancellationToken = default)
    {
        var normalized = isbn.Trim().ToLower();

        return _dbContext.Books
            .AsNoTracking()
            .Include(book => book.Author)
            .FirstOrDefaultAsync(book => book.ISBN.ToLower() == normalized, cancellationToken);
    }

    public async Task AddAsync(
        Book book,
        CancellationToken cancellationToken = default)
    {
        await ResolveAuthorAsync(book, cancellationToken);
        await _dbContext.Books.AddAsync(book, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Book book,
        CancellationToken cancellationToken = default)
    {
        var existingBook = await _dbContext.Books
            .Include(item => item.Author)
            .FirstOrDefaultAsync(item => item.Id == book.Id, cancellationToken);

        if (existingBook is null)
        {
            throw new InvalidOperationException(
                $"Book with ID {book.Id} was not found.");
        }

        existingBook.Title = book.Title;
        existingBook.ISBN = book.ISBN;
        existingBook.Category = book.Category;
        existingBook.PublishedYear = book.PublishedYear;

        if (book.Author is not null && !string.IsNullOrWhiteSpace(book.Author.Name))
        {
            var author = await FindOrCreateAuthorAsync(book.Author.Name, cancellationToken);
            existingBook.Author = author;
            existingBook.AuthorId = author.Id;
            book.Author = author;
            book.AuthorId = author.Id;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        int bookId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Books
            .FirstOrDefaultAsync(book => book.Id == bookId, cancellationToken);

        if (existing is null)
        {
            throw new InvalidOperationException(
                $"Book with ID {bookId} was not found.");
        }

        _dbContext.Books.Remove(existing);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ResolveAuthorAsync(Book book, CancellationToken cancellationToken)
    {
        if (book.Author is null || string.IsNullOrWhiteSpace(book.Author.Name))
        {
            return;
        }

        var author = await FindOrCreateAuthorAsync(book.Author.Name, cancellationToken);
        book.Author = author;
        book.AuthorId = author.Id;
    }

    private async Task<Author> FindOrCreateAuthorAsync(
        string authorName,
        CancellationToken cancellationToken)
    {
        var trimmed = authorName.Trim();
        var normalized = trimmed.ToLower();

        var existingAuthor = await _dbContext.Authors
            .FirstOrDefaultAsync(author => author.Name.ToLower() == normalized, cancellationToken);

        if (existingAuthor is not null)
        {
            return existingAuthor;
        }

        var newAuthor = new Author { Name = trimmed };
        await _dbContext.Authors.AddAsync(newAuthor, cancellationToken);
        return newAuthor;
    }
}
