using System;
using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Application.DTOs.Showtimes;

public class CreateShowtimeDto
{
    [Required]
    public int MovieId { get; set; }

    [Required]
    public int RoomId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal BasePrice { get; set; }
}
