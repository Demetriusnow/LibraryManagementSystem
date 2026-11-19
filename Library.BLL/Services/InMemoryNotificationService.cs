using System.Collections.Concurrent;
using Library.Core.Entities;
using Library.Core.Interfaces;

namespace Library.BLL.Services;

public class InMemoryNotificationService : INotificationService
{
    private readonly ConcurrentQueue<NotificationRecord> _notifications = new();
    private readonly TimeProvider _timeProvider;
    private int _nextNotificationId;

    public InMemoryNotificationService(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public Task NotifyAsync(
        int readerId,
        int bookCopyId,
        string message,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _notifications.Enqueue(new NotificationRecord
        {
            Id = Interlocked.Increment(ref _nextNotificationId),
            ReaderId = readerId,
            BookCopyId = bookCopyId,
            Message = message,
            SentAt = _timeProvider.GetUtcNow().UtcDateTime
        });

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<NotificationRecord>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyCollection<NotificationRecord> records = _notifications
            .OrderByDescending(record => record.Id)
            .ToArray();

        return Task.FromResult(records);
    }
}
