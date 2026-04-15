namespace CinemaBooking.Domain.Entities;

/// <summary>
/// Thực thể vé (một vé tương ứng với một ghế trong một suất chiếu).
/// </summary>
public class Ticket
{
    /// <summary>Định danh duy nhất của vé.</summary>
    public int Id { get; set; }

    /// <summary>Mã vé duy nhất, định dạng: "TK" + YYYYMMDDHHmmss + random 9 chữ số (tối đa 30 ký tự).</summary>
    public required string TicketCode { get; set; }

    /// <summary>Định danh của đơn đặt vé liên quan (bắt buộc).</summary>
    public int BookingId { get; set; }

    /// <summary>Định danh của ghế trong suất chiếu liên quan (bắt buộc).</summary>
    public int ShowtimeSeatId { get; set; }

    /// <summary>Giá vé (tính bằng VND, tối đa 10 chữ số với 2 số thập phân).</summary>
    public required decimal Price { get; set; }

    /// <summary>Loại vé (vd: "STANDARD", "VIP", "COUPLE" - tối đa 20 ký tự).</summary>
    public string? SeatType { get; set; } = "STANDARD";

    /// <summary>Trạng thái vé (true = active/có hiệu lực, false = vé bị hủy/não sử dụng được).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Thời gian vé được phát hành/in.</summary>
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Thời gian người dùng sử dụng vé (check-in lúc vào rạp), null nếu chưa sử dụng.</summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>Thứ tự in vé (cho phép in lại vé nếu mất).</summary>
    public int PrintCount { get; set; } = 0;

    /// <summary>Lần in cuối cùng.</summary>
    public DateTime? LastPrintedAt { get; set; }

    /// <summary>Ghi chú về vé (tối đa 500 ký tự).</summary>
    public string? Notes { get; set; }

    /// <summary>Thời gian tạo bản ghi.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Thời gian cập nhật bản ghi lần cuối.</summary>
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Booking? Booking { get; set; }
    public virtual ShowtimeSeat? ShowtimeSeat { get; set; }

    /// <summary>
    /// Kiểm tra xem vé đã được sử dụng chưa.
    /// </summary>
    /// <returns>true nếu vé đã được check-in, false nếu chưa.</returns>
    public bool IsUsed()
    {
        return UsedAt.HasValue;
    }

    /// <summary>
    /// Đánh dấu vé đã được sử dụng (check-in).
    /// </summary>
    public void MarkAsUsed()
    {
        UsedAt = DateTime.UtcNow;
    }
}
