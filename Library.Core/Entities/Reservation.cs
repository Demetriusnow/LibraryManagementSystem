using Library.Core.Enums;

namespace Library.Core.Entities;

public class Reservation
{
    public int Id { get; set; }

    public int BookCopyId { get; set; }

    public int ReaderId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReservedUntil { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Waiting;

    public bool NotificationSent { get; set; }

    public string? NotificationMessage { get; set; }
}
