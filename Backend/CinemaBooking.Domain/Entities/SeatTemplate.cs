using System.Collections.Generic;
using CinemaBooking.Domain.Enums;

namespace CinemaBooking.Domain.Entities;

/// <summary>
/// Thực thể mẫu ghế - định nghĩa layout ghế cho mỗi phòng chiếu.
/// Mỗi phòng có N mẫu ghế (ví dụ: A1-D5 = 20 ghế).
/// Khi tạo Showtime, hệ thống sẽ sinh ShowtimeSeats từ SeatTemplates của phòng.
/// </summary>
public class SeatTemplate
{
    /// <summary>Định danh duy nhất của mẫu ghế.</summary>
    public int Id { get; set; }

    /// <summary>Định danh phòng chiếu chứa ghế này.</summary>
    public int RoomId { get; set; }

    /// <summary>Hàng ghế (ví dụ: 'A', 'B', 'C', 'D').</summary>
    public required string Row { get; set; }

    /// <summary>Số ghế trong hàng (ví dụ: 1, 2, 3, 4, 5).</summary>
    public int Number { get; set; }

    /// <summary>Loại ghế (Regular = thường, VIP).</summary>
    public SeatType Type { get; set; }

    /// <summary>Thứ tự sắp xếp trong phòng (để sort).</summary>
    public int DisplayOrder { get; set; }

    // Navigation property
    public virtual Room? Room { get; set; }
}
