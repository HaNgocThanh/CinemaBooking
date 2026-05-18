using System;
using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Application.DTOs.Showtimes;

/// <summary>
/// DTO cho việc cập nhật suất chiếu.
/// Tất cả trường nullable — nếu không cung cấp, giá trị hiện tại được giữ nguyên.
/// Khi StartTime được cung cấp, EndTime sẽ được tự động tính lại:
/// EndTime = StartTime + Movie.DurationMinutes + 15 phút nghỉ giữa các suất.
/// </summary>
public class UpdateShowtimeDto
{
    /// <summary>ID phim (tùy chọn). Nếu null, giữ nguyên phim hiện tại.</summary>
    public int? MovieId { get; set; }

    /// <summary>ID phòng chiếu (tùy chọn). Nếu null, giữ nguyên phòng hiện tại.</summary>
    public int? RoomId { get; set; }

    /// <summary>
    /// Thời gian bắt đầu mới (tùy chọn).
    /// Nếu được cung cấp, EndTime sẽ được tự động tính lại từ DurationMinutes của phim + 15 phút nghỉ.
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>Giá vé cơ bản mới (tùy chọn). Nếu null, giữ nguyên giá hiện tại.</summary>
    [Range(0, double.MaxValue, ErrorMessage = "Giá vé không được âm.")]
    public decimal? BasePrice { get; set; }

    /// <summary>Trạng thái hoạt động (tùy chọn). Nếu null, giữ nguyên trạng thái hiện tại.</summary>
    public bool? IsActive { get; set; }
}
