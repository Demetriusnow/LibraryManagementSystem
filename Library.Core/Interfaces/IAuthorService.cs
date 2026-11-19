using Library.Core.Entities;

namespace Library.Core.Interfaces;

public interface IAuthorService
{
    Task<IReadOnlyCollection<Author>> GetAuthorsAsync();

    Task<Author?> GetAuthorByIdAsync(int authorId);

    Task<Author> AddAuthorAsync(string name);

    Task<Author> UpdateAuthorAsync(int authorId, string name);

    Task DeleteAuthorAsync(int authorId);
}
