using Library.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationsController : ApiControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpPost]
    public Task<IActionResult> ReserveBookCopy([FromBody] CreateReservationRequest request)
    {
        return ExecuteActionAsync(async () =>
        {
            var reservation = await _reservationService.ReserveBookCopyAsync(
                request.BookCopyId,
                request.ReaderId);

            return CreatedAtAction(
                nameof(GetReservationsForCopy),
                new { bookCopyId = reservation.BookCopyId },
                reservation);
        });
    }

    [HttpGet("book-copy/{bookCopyId:int}")]
    public async Task<IActionResult> GetReservationsForCopy(int bookCopyId)
    {
        var reservations = await _reservationService.GetReservationsForCopyAsync(bookCopyId);
        return Ok(reservations);
    }

    [HttpDelete("{id:int}")]
    public Task<IActionResult> CancelReservation(int id)
    {
        return ExecuteActionAsync(async () =>
        {
            await _reservationService.CancelReservationAsync(id);
            return NoContent();
        });
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        var notifications = await _reservationService.GetNotificationsAsync();
        return Ok(notifications);
    }
}

public sealed record CreateReservationRequest(int BookCopyId, int ReaderId);
