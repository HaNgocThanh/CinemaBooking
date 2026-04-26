using CinemaBooking.Domain.Enums;

namespace CinemaBooking.Domain.Entities;

/// <summary>
/// Thực thể ghế trong một suất chiếu (tính toán từng ghế cụ thể cho từng suất chiếu).
/// </summary>
public class ShowtimeSeat
{
    /// <summary>Định danh duy nhất của ghế trong suất chiếu.</summary>
    public int Id { get; set; }

    /// <summary>Định danh của suất chiếu liên quan (bắt buộc).</summary>
    public int ShowtimeId { get; set; }

    /// <summary>Số hiệu ghế trong rạp (vd: "A1", "B5" - tối đa 10 ký tự).</summary>
    public required string SeatNumber { get; set; }

    /// <summary>Hàng ghế (vd: "A", "B" - tối đa 5 ký tự).</summary>
    public required string RowLetter { get; set; }

    /// <summary>Số cột ghế (vd: 1, 2, 3).</summary>
    public int ColumnNumber { get; set; }

    /// <summary>Trạng thái hiện tại của ghế (Available, Locked, Booked, Unavailable).</summary>
    public required SeatStatus Status { get; set; } = SeatStatus.Available;

    /// <summary>Loại ghế trong suất chiếu này (Regular hoặc VIP), kế thừa từ SeatTemplate.</summary>
    public SeatType Type { get; set; }

    /// <summary>Thời gian ghế được khóa (dùng cho pessimistic locking, 5 phút timeout).</summary>
    public DateTime? LockedAt { get; set; }

    /// <summary>Thời gian hết hạn giữ ghế (5 phút sau khi khóa, auto-unlock sau thời điểm này).</summary>
    public DateTime? HoldExpiryTime { get; set; }

    /// <summary>Định danh session của người dùng đang khóa ghế này (nếu có).</summary>
    public string? LockedBySessionId { get; set; }

    /// <summary>Định danh người dùng đã đặt ghế này (nếu có).</summary>
    public int? BookedByUserId { get; set; }

    /// <summary>Định danh vé liên quan đến ghế này (nếu ghế đã được đặt).</summary>
    public int? TicketId { get; set; }

    /// <summary>Thời gian tạo bản ghi.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Thời gian cập nhật bản ghi lần cuối.</summary>
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Showtime? Showtime { get; set; }
    public virtual Ticket? Ticket { get; set; }
}
