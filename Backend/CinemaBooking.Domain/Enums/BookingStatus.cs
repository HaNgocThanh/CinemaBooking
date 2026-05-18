namespace CinemaBooking.Domain.Enums;

/// <summary>
/// Trạng thái thanh toán của đơn đặt vé trong luồng QR thanh toán.
/// </summary>
public enum BookingStatus
{
    /// <summary>Chờ thanh toán - Khách đã chọn ghế, đang trong thời gian giữ ghế 5 phút.</summary>
    Pending = 0,

    /// <summary>Chờ xác nhận - Khách đã bấm "Tôi đã chuyển khoản", đang chờ Admin duyệt.</summary>
    AwaitingConfirmation = 1,

    /// <summary>Thành công - Admin đã xác nhận thanh toán, vé được xác nhận.</summary>
    Success = 2,

    /// <summary>Đã hủy - Admin từ chối hoặc đơn hàng bị hủy do quá hạn.</summary>
    Cancelled = 3,

    /// <summary>Hết hạn thanh toán (quá 5 phút mà không thanh toán).</summary>
    Expired = 4,

    /// <summary>Hoàn tiền (khách đã hủy sau khi thanh toán).</summary>
    Refunded = 5
}
