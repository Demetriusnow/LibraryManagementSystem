namespace Library.Core.DTOs;

public sealed record BookCatalogItemDto(
    int Id,
    string Title,
    string ISBN,
    string Category,
    int PublishedYear,
    int AuthorId,
    string AuthorName,
    int TotalCopiesCount,
    int AvailableCopiesCount);
