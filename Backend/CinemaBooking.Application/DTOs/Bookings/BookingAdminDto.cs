namespace CinemaBooking.Application.DTOs.Bookings;

/// <summary>
/// DTO cho trang admin quản lý đơn đặt vé.
/// </summary>
public class BookingAdminDto
{
    /// <summary>ID của booking.</summary>
    public int Id { get; set; }

    /// <summary>Mã booking duy nhất.</summary>
    public required string BookingCode { get; set; }

    /// <summary>Tên phim.</summary>
    public required string MovieTitle { get; set; }

    /// <summary>Tên phòng chiếu.</summary>
    public required string RoomName { get; set; }

    /// <summary>Giờ bắt đầu suất chiếu.</summary>
    public DateTime ShowtimeStartTime { get; set; }

    /// <summary>Tên khách hàng (nếu có).</summary>
    public string? CustomerName { get; set; }

    /// <summary>Email khách hàng (nếu có).</summary>
    public string? CustomerEmail { get; set; }

    /// <summary>Tổng tiền thanh toán.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Trạng thái hiện tại.</summary>
    public required string Status { get; set; }

    /// <summary>Trạng thái hiện tại (number).</summary>
    public int StatusValue { get; set; }

    /// <summary>Thời gian tạo đơn.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Thời gian thanh toán (nếu có).</summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>Danh sách ghế đã đặt (format: "A1, A2, B3").</summary>
    public string Seats { get; set; } = string.Empty;

    /// <summary>Số lượng vé.</summary>
    public int TotalTickets { get; set; }
}

/// <summary>
/// DTO cho danh sách booking admin (phân trang).
/// </summary>
public class BookingAdminListDto
{
    /// <summary>Danh sách booking.</summary>
    public List<BookingAdminDto> Items { get; set; } = new();

    /// <summary>Tổng số booking.</summary>
    public int TotalCount { get; set; }

    /// <summary>Trang hiện tại.</summary>
    public int Page { get; set; }

    /// <summary>Kích thước trang.</summary>
    public int PageSize { get; set; }

    /// <summary>Tổng số trang.</summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
