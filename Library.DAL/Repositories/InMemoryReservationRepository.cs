using System.Collections.Concurrent;
using Library.Core.Entities;
using Library.Core.Interfaces;

namespace Library.DAL.Repositories;

public class InMemoryReservationRepository : IReservationRepository
{
    private readonly ConcurrentDictionary<int, Reservation> _reservations = new();
    private int _nextReservationId;

    public Task<IReadOnlyCollection<Reservation>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyCollection<Reservation> reservations = _reservations.Values
            .OrderBy(reservation => reservation.Id)
            .ToArray();

        return Task.FromResult(reservations);
    }

    public Task<Reservation?> GetByIdAsync(
        int reservationId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _reservations.TryGetValue(reservationId, out var reservation);

        return Task.FromResult(reservation);
    }

    public Task<IReadOnlyCollection<Reservation>> GetByBookCopyIdAsync(
        int bookCopyId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyCollection<Reservation> reservations = _reservations.Values
            .Where(reservation => reservation.BookCopyId == bookCopyId)
            .OrderBy(reservation => reservation.CreatedAt)
            .ThenBy(reservation => reservation.Id)
            .ToArray();

        return Task.FromResult(reservations);
    }

    public Task AddAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        reservation.Id = Interlocked.Increment(ref _nextReservationId);
        _reservations.TryAdd(reservation.Id, reservation);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_reservations.TryGetValue(reservation.Id, out var existing) ||
            !_reservations.TryUpdate(reservation.Id, reservation, existing))
        {
            throw new InvalidOperationException(
                $"Reservation with ID {reservation.Id} was not found.");
        }

        return Task.CompletedTask;
    }
}
