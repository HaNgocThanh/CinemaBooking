using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Application.DTOs.Rooms;

public class UpdateRoomDto
{
    [StringLength(50, MinimumLength = 1)]
    public string? Name { get; set; }

    [Range(1, 1000)]
    public int? Capacity { get; set; }

    [StringLength(50, MinimumLength = 1)]
    public string? Type { get; set; }
}
