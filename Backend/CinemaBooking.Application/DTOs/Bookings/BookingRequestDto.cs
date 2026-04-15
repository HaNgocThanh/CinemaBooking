namespace CinemaBooking.Application.DTOs.Bookings;

/// <summary>
/// DTO cho yêu cầu tạo booking mới.
/// </summary>
public class BookingRequestDto
{
    /// <summary>
    /// ID của suất chiếu cần đặt vé.
    /// </summary>
    public required int ShowtimeId { get; set; }

    /// <summary>
    /// Danh sách ID của các ghế cần đặt (bắt buộc, ít nhất 1 ghế).
    /// </summary>
    public required List<int> SeatIds { get; set; } = new();

    /// <summary>
    /// Danh sách combo kèm số lượng (tuỳ chọn).
    /// Key: ComboId, Value: Số lượng
    /// </summary>
    public Dictionary<int, int>? CombosWithQuantity { get; set; }

    /// <summary>
    /// Mã khuyến mại / mã giảm giá (tuỳ chọn).
    /// Định dạng: tối đa 50 ký tự (vd: "SUMMER2024", "FIRST10", "VNPAY50K").
    /// </summary>
    public string? PromoCode { get; set; }

    /// <summary>
    /// Session ID của khách hàng (dùng để theo dõi lock ghế).
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// ID của khách hàng (tuỳ chọn, có thể null nếu đặt vé không cần đăng nhập).
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Ghi chú thêm về đơn đặt vé (tuỳ chọn).
    /// </summary>
    public string? Notes { get; set; }
}
