using Library.Core.Entities;
using Library.Core.Interfaces;
using Library.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.DAL.Repositories;

public class EfNotificationService : INotificationService
{
    private readonly LibraryDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public EfNotificationService(
        LibraryDbContext dbContext,
        TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task NotifyAsync(
        int readerId,
        int bookCopyId,
        string message,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Notifications.AddAsync(
            new NotificationRecord
            {
                ReaderId = readerId,
                BookCopyId = bookCopyId,
                Message = message,
                SentAt = _timeProvider.GetUtcNow().UtcDateTime
            },
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<NotificationRecord>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .AsNoTracking()
            .OrderByDescending(record => record.Id)
            .ToArrayAsync(cancellationToken);
    }
}
