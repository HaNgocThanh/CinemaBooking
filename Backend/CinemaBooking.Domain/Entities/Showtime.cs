namespace CinemaBooking.Domain.Entities;

/// <summary>
/// Thực thể suất chiếu phim (một lần chiếu phim tại một rạp, một phòng, một thời gian cụ thể).
/// </summary>
public class Showtime
{
    /// <summary>Định danh duy nhất của suất chiếu.</summary>
    public int Id { get; set; }

    /// <summary>Định danh của phim liên quan (bắt buộc).</summary>
    public int MovieId { get; set; }

    /// <summary>Số hiệu phòng chiếu (vd: "101", "A1" - tối đa 10 ký tự).</summary>
    public required string RoomNumber { get; set; }

    /// <summary>Thời gian bắt đầu suất chiếu.</summary>
    public required DateTime StartTime { get; set; }

    /// <summary>Thời gian kết thúc suất chiếu dự kiến.</summary>
    public required DateTime EndTime { get; set; }

    /// <summary>Giá vé cơ bản cho suất chiếu này (tính bằng: VND, tối đa 10 chữ số với 2 số thập phân).</summary>
    public required decimal BasePrice { get; set; }

    /// <summary>Tổng số ghế trong phòng.</summary>
    public int TotalSeats { get; set; }

    /// <summary>Số ghế đã được đặt trong suất chiếu này.</summary>
    public int BookedSeatsCount { get; set; } = 0;

    /// <summary>Trạng thái suất chiếu (true = hoạt động, false = bị hủy/kết thúc).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Thời gian tạo bản ghi.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Thời gian cập nhật bản ghi lần cuối.</summary>
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Movie? Movie { get; set; }
    public ICollection<ShowtimeSeat> Seats { get; set; } = new List<ShowtimeSeat>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
