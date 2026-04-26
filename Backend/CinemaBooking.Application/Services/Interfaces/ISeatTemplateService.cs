using System.Collections.Generic;
using System.Threading.Tasks;
using CinemaBooking.Domain.Entities;

namespace CinemaBooking.Application.Services.Interfaces;

/// <summary>
/// Service for managing seat templates and generating showtime seats.
/// </summary>
public interface ISeatTemplateService
{
    /// <summary>
    /// Gets all seat templates for a specific room.
    /// </summary>
    Task<List<SeatTemplate>> GetSeatTemplatesByRoomIdAsync(int roomId);

    /// <summary>
    /// Generates ShowtimeSeat entities from seat templates for a given showtime.
    /// Maps each SeatTemplate to a ShowtimeSeat with status = Available.
    /// </summary>
    Task<List<ShowtimeSeat>> GenerateShowtimeSeatsAsync(int showtimeId, int roomId);
}
