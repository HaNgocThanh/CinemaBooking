namespace CinemaBooking.Domain.Enums;

/// <summary>
/// Trạng thái của ghế trong một suất chiếu.
/// </summary>
public enum SeatStatus
{
    /// <summary>Ghế còn trống, có thể đặt.</summary>
    Available = 0,

    /// <summary>Ghế đang bị khóa (người dùng đang trong quá trình thanh toán).</summary>
    Locked = 1,

    /// <summary>Ghế đã được đặt và thanh toán thành công.</summary>
    Booked = 2,

    /// <summary>Ghế bị khóa vĩnh viễn (vd: ghế bị hỏng, không sử dụng được).</summary>
    Unavailable = 3
}
