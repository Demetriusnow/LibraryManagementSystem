namespace Library.Core.Entities;

public class NotificationRecord
{
    public int Id { get; set; }

    public int ReaderId { get; set; }

    public int BookCopyId { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }
}
