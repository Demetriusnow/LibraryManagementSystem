using Library.Core.Entities;

namespace Library.Core.Interfaces;

public interface INotificationService
{
    Task NotifyAsync(
        int readerId,
        int bookCopyId,
        string message,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<NotificationRecord>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
