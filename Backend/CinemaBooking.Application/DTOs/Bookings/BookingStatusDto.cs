namespace CinemaBooking.Application.DTOs.Bookings;

/// <summary>
/// DTO cho trạng thái booking - dùng cho polling ở trang thanh toán.
/// </summary>
public class BookingStatusDto
{
    /// <summary>ID của booking.</summary>
    public int BookingId { get; set; }

    /// <summary>Mã booking.</summary>
    public required string BookingCode { get; set; }

    /// <summary>Trạng thái hiện tại (Pending, AwaitingConfirmation, Success, Cancelled, Expired).</summary>
    public required string Status { get; set; }

    /// <summary>Thời gian hết hạn (UTC).</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Thời gian tạo (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Thời gian thanh toán thành công (UTC).</summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>Tổng tiền.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Số giây còn lại để thanh toán (tính tại server, tránh lỗi timezone client).</summary>
    public int RemainingSeconds { get; set; }
}
