using Library.Core.Entities;
using Library.Core.Interfaces;

namespace Library.BLL.Services;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _authorRepository;

    public AuthorService(IAuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }

    public Task<IReadOnlyCollection<Author>> GetAuthorsAsync()
    {
        return _authorRepository.GetAllAsync();
    }

    public Task<Author?> GetAuthorByIdAsync(int authorId)
    {
        return _authorRepository.GetByIdAsync(authorId);
    }

    public async Task<Author> AddAuthorAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Author name is required.", nameof(name));
        }

        var normalized = name.Trim();
        var existing = await _authorRepository.GetByNameAsync(normalized);

        if (existing is not null)
        {
            throw new InvalidOperationException("Author with this name already exists.");
        }

        var author = new Author { Name = normalized };
        await _authorRepository.AddAsync(author);

        return author;
    }

    public async Task<Author> UpdateAuthorAsync(int authorId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Author name is required.", nameof(name));
        }

        var existing = await _authorRepository.GetByIdAsync(authorId);

        if (existing is null)
        {
            throw new InvalidOperationException("Author was not found.");
        }

        existing.Name = name.Trim();
        await _authorRepository.UpdateAsync(existing);

        return existing;
    }

    public Task DeleteAuthorAsync(int authorId)
    {
        return _authorRepository.DeleteAsync(authorId);
    }
}
