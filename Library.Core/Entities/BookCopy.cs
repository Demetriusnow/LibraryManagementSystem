using Library.Core.Enums;

namespace Library.Core.Entities;

public class BookCopy
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public string InventoryNumber { get; set; } = string.Empty;

    public BookCopyStatus Status { get; set; } = BookCopyStatus.Available;
}