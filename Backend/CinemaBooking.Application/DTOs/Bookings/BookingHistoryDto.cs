namespace CinemaBooking.Application.DTOs.Bookings;

/// <summary>
/// DTO cho danh sách lịch sử đặt vé của khách hàng.
/// </summary>
public class BookingHistoryDto
{
    /// <summary>ID đơn đặt vé.</summary>
    public int BookingId { get; set; }

    /// <summary>Mã đơn đặt vé.</summary>
    public required string BookingCode { get; set; }

    /// <summary>Tên phim.</summary>
    public required string MovieTitle { get; set; }

    /// <summary>URL poster phim.</summary>
    public string? PosterUrl { get; set; }

    /// <summary>Phòng chiếu.</summary>
    public required string RoomName { get; set; }

    /// <summary>Ngày chiếu (đã format).</summary>
    public required string ShowDate { get; set; }

    /// <summary>Giờ bắt đầu chiếu (đã format).</summary>
    public required string StartTime { get; set; }

    /// <summary>Tên các ghế (gộp thành chuỗi).</summary>
    public required string SeatNames { get; set; }

    /// <summary>Số lượng vé.</summary>
    public int TotalTickets { get; set; }

    /// <summary>Tổng tiền.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Trạng thái (Pending, AwaitingConfirmation, Success, Cancelled, Expired).</summary>
    public required string Status { get; set; }

    /// <summary>Giá trị số của trạng thái (0-4).</summary>
    public int StatusValue { get; set; }

    /// <summary>Ngày đặt vé (đã format).</summary>
    public required string BookedAt { get; set; }
}

/// <summary>
/// DTO chứa danh sách lịch sử đặt vé có phân trang.
/// </summary>
public class BookingHistoryListDto
{
    /// <summary>Danh sách đơn đặt vé.</summary>
    public List<BookingHistoryDto> Items { get; set; } = new();

    /// <summary>Tổng số đơn.</summary>
    public int TotalCount { get; set; }
}
