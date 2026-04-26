using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Application.DTOs.Rooms;

public class CreateRoomDto
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, 1000)]
    public int Capacity { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Type { get; set; } = string.Empty;
}
