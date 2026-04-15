namespace CinemaBooking.Application.DTOs.Bookings;

/// <summary>
/// DTO cho response sau khi tạo booking thành công.
/// </summary>
public class BookingResponseDto
{
    /// <summary>ID của booking vừa tạo.</summary>
    public int BookingId { get; set; }

    /// <summary>Mã booking duy nhất (vd: "BK20260416160000000123456789").</summary>
    public required string BookingCode { get; set; }

    /// <summary>Trạng thái booking (PendingPayment, Confirmed, etc.).</summary>
    public required string Status { get; set; }

    /// <summary>Tổng số vé được đặt.</summary>
    public int TotalTickets { get; set; }

    /// <summary>Tổng tiền trước giảm giá (VND).</summary>
    public decimal SubTotal { get; set; }

    /// <summary>Số tiền giảm giá được áp dụng (VND).</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>Tổng tiền cuối cùng phải thanh toán (VND).</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Mã khuyến mại được áp dụng (nếu có).</summary>
    public string? AppliedPromoCode { get; set; }

    /// <summary>Hạn cuối cùng để thanh toán (UTC).</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Thời gian tạo booking (UTC).</summary>
    public DateTime BookedAt { get; set; }

    /// <summary>Danh sách ID vé được tạo.</summary>
    public List<int> TicketIds { get; set; } = new();
}
