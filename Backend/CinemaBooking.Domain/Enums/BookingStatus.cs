namespace CinemaBooking.Domain.Enums;

/// <summary>
/// Trạng thái của một đơn đặt vé.
/// </summary>
public enum BookingStatus
{
    /// <summary>Chờ thanh toán (người dùng đã chọn ghế nhưng chưa thanh toán).</summary>
    PendingPayment = 0,

    /// <summary>Đã thanh toán và xác nhận (đặt vé thành công).</summary>
    Confirmed = 1,

    /// <summary>Hủy đơn đặt vé (do người dùng hoặc tự động do timeout).</summary>
    Cancelled = 2,

    /// <summary>Hết hạn thanh toán (quá 5 phút mà người dùng không thanh toán).</summary>
    Expired = 3,

    /// <summary>Hoàn tiền (người dùng đã hủy sau khi thanh toán).</summary>
    Refunded = 4
}
