namespace CinemaBooking.Application.DTOs.Bookings;

/// <summary>
/// DTO cho trang Vé Điện Tử (E-Ticket).
/// Chứa toàn bộ thông tin hiển thị trên tấm vé: phim, rạp, ghế, QR code.
/// </summary>
public class ETicketDto
{
    /// <summary>ID đơn đặt vé.</summary>
    public int BookingId { get; set; }

    /// <summary>Mã đơn đặt vé.</summary>
    public required string BookingCode { get; set; }

    /// <summary>Tên phim.</summary>
    public required string MovieTitle { get; set; }

    /// <summary>URL poster phim.</summary>
    public string? PosterUrl { get; set; }

    /// <summary>Tên rạp chiếu phim.</summary>
    public required string CinemaName { get; set; }

    /// <summary>Tên phòng chiếu.</summary>
    public required string RoomName { get; set; }

    /// <summary>Thời gian bắt đầu chiếu (đã format).</summary>
    public required string StartTime { get; set; }

    /// <summary>Thời gian kết thúc chiếu (đã format).</summary>
    public required string EndTime { get; set; }

    /// <summary>Ngày chiếu (đã format).</summary>
    public required string ShowDate { get; set; }

    /// <summary>Chuỗi gộp tên các ghế (VD: "A1, A2, A3").</summary>
    public required string SeatNames { get; set; }

    /// <summary>Số lượng vé/ghế.</summary>
    public int TotalTickets { get; set; }

    /// <summary>Tổng tiền thanh toán.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Mã thanh toán.</summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Chuỗi dữ liệu QR code dùng để quét vé tại quầy.
    /// Format: "CINEMA_TICKET_{BookingId}_{UserId}_{BookingCode}_{Timestamp}"
    /// </summary>
    public required string QrCodeData { get; set; }

    /// <summary>Thời gian đặt vé (UTC).</summary>
    public DateTime BookedAt { get; set; }
}
