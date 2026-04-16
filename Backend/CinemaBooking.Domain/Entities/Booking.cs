using CinemaBooking.Domain.Enums;

namespace CinemaBooking.Domain.Entities;

/// <summary>
/// Thực thể đơn đặt vé (một lần đặt vé có thể chứa nhiều vé/ghế).
/// </summary>
public class Booking
{
    /// <summary>Định danh duy nhất của đơn đặt vé.</summary>
    public int Id { get; set; }

    /// <summary>Mã đơn đặt vé duy nhất, định dạng: "BK" + YYYYMMDDHHmmss + random 9 chữ số (tối đa 30 ký tự).</summary>
    public required string BookingCode { get; set; }

    /// <summary>Định danh của suất chiếu liên quan (bắt buộc).</summary>
    public int ShowtimeId { get; set; }

    /// <summary>Định danh khách hàng đặt vé (có thể null nếu đặt vé không cần đăng nhập).</summary>
    public int? CustomerId { get; set; }

    /// <summary>Trạng thái hiện tại của đơn đặt vé (PendingPayment, Confirmed, Cancelled, Expired, Refunded).</summary>
    public required BookingStatus Status { get; set; } = BookingStatus.PendingPayment;

    /// <summary>Tổng số vé/ghế được đặt trong đơn này.</summary>
    public int TotalTickets { get; set; }

    /// <summary>Tổng tiền của đơn đặt vé trước khi áp dụng giảm giá (tối đa 15 chữ số với 2 số thập phân).</summary>
    public required decimal SubTotal { get; set; }

    /// <summary>Tổng tiền giảm giá được áp dụng (tối đa 15 chữ số với 2 số thập phân).</summary>
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>Tổng tiền cuối cùng phải thanh toán (tối đa 15 chữ số với 2 số thập phân).</summary>
    public required decimal TotalAmount { get; set; }

    /// <summary>
    /// Mã khuyến mại được áp dụng (nếu có, tối đa 50 ký tự).
    /// </summary>
    public string? PromotionCode { get; set; }

    /// <summary>Session ID của người dùng để theo dõi session khi đặt vé.</summary>
    public string? SessionId { get; set; }

    /// <summary>Thời gian đặt vé.</summary>
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Thời gian hết hạn thanh toán (thường là 5 phút sau BookedAt, tính bằng phút).</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Thời gian thanh toán (khi status = Confirmed).</summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>Phương thức thanh toán được sử dụng (vd: "STRIPE", "MOMO", "VNPAY", "CASH").</summary>
    public string? PaymentMethod { get; set; }

    /// <summary>Mã giao dịch từ cổng thanh toán (tối đa 100 ký tự).</summary>
    public string? TransactionId { get; set; }

    /// <summary>Ghi chú thêm về đơn đặt vé (tối đa 500 ký tự).</summary>
    public string? Notes { get; set; }

    /// <summary>Thời gian cập nhật bản ghi lần cuối.</summary>
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Showtime? Showtime { get; set; }
    public virtual User? Customer { get; set; }
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    /// <summary>
    /// Kiểm tra xem đơn đặt vé có hết hạn hay không.
    /// </summary>
    /// <returns>true nếu hết hạn, false nếu chưa hết hạn.</returns>
    public bool IsExpired()
    {
        if (ExpiresAt is null)
            return false;

        return DateTime.UtcNow > ExpiresAt;
    }
}
