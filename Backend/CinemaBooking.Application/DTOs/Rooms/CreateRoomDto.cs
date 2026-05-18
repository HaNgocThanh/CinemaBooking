using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Application.DTOs.Rooms;

public class CreateRoomDto
{
    [StringLength(50, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; } // Auto-synced from seat layout; default 0

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Type { get; set; } = string.Empty;
}
