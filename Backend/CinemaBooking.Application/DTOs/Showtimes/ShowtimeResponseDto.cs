using System;

namespace CinemaBooking.Application.DTOs.Showtimes;

public class ShowtimeResponseDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal BasePrice { get; set; }
    public int TotalSeats { get; set; }
    public int BookedSeatsCount { get; set; }
    public int AvailableSeats { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
