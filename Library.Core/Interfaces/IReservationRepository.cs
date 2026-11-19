using Library.Core.Entities;

namespace Library.Core.Interfaces;

public interface IReservationRepository
{
    Task<IReadOnlyCollection<Reservation>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Reservation?> GetByIdAsync(
        int reservationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Reservation>> GetByBookCopyIdAsync(
        int bookCopyId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default);
}
