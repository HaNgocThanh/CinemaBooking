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

    /// <summary>
    /// Thoi gian ket thuc. Neu khong cung cap, se tu dong tinh = StartTime + DurationMinutes + 15 phut nghi.
    /// </summary>
    public DateTime? EndTime { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal BasePrice { get; set; }
}
