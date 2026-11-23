using Library.Core.Entities;
using Library.Core.Interfaces;
using Library.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.DAL.Repositories;

public class EfReservationRepository : IReservationRepository
{
    private readonly LibraryDbContext _dbContext;

    public EfReservationRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Reservation>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reservations
            .AsNoTracking()
            .OrderBy(reservation => reservation.Id)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Reservation?> GetByIdAsync(
        int reservationId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Reservations
            .AsNoTracking()
            .FirstOrDefaultAsync(reservation => reservation.Id == reservationId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Reservation>> GetByBookCopyIdAsync(
        int bookCopyId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reservations
            .AsNoTracking()
            .Where(reservation => reservation.BookCopyId == bookCopyId)
            .OrderBy(reservation => reservation.CreatedAt)
            .ThenBy(reservation => reservation.Id)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Reservations.AddAsync(reservation, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Reservations
            .FirstOrDefaultAsync(item => item.Id == reservation.Id, cancellationToken);

        if (existing is null)
        {
            throw new InvalidOperationException(
                $"Reservation with ID {reservation.Id} was not found.");
        }

        _dbContext.Entry(existing).CurrentValues.SetValues(reservation);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
