using Library.Core.Entities;

namespace Library.Core.Interfaces;

public interface IReservationService
{
    Task<Reservation> ReserveBookCopyAsync(int bookCopyId, int readerId);

    Task<IReadOnlyCollection<Reservation>> GetReservationsForCopyAsync(int bookCopyId);

    Task CancelReservationAsync(int reservationId);

    Task<IReadOnlyCollection<NotificationRecord>> GetNotificationsAsync();
}
