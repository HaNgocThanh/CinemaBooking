using System.Collections.Generic;

namespace CinemaBooking.Domain.Entities;

/// <summary>
/// Thực thể phòng chiếu phim.
/// </summary>
public class Room
{
    /// <summary>Định danh duy nhất của phòng chiếu.</summary>
    public int Id { get; set; }

    /// <summary>Tên phòng chiếu (vd: 'Phòng 01').</summary>
    public required string Name { get; set; }

    /// <summary>Tổng số ghế trong phòng.</summary>
    public int Capacity { get; set; }

    /// <summary>Loại phòng chiếu (vd: '2D', '3D', 'IMAX').</summary>
    public required string Type { get; set; }

    // Navigation properties
    public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
    public ICollection<SeatTemplate> SeatTemplates { get; set; } = new List<SeatTemplate>();
}
