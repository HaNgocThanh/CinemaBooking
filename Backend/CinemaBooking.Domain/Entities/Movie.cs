namespace CinemaBooking.Domain.Entities;

using CinemaBooking.Domain.Enums;

/// <summary>
/// Thực thể phim.
/// </summary>
public class Movie
{
    /// <summary>Định danh duy nhất của phim.</summary>
    public int Id { get; set; }

    /// <summary>Tên phim (bắt buộc, tối đa 200 ký tự).</summary>
    public required string Title { get; set; }

    /// <summary>Mô tả chi tiết về phim (tối đa 1000 ký tự).</summary>
    public string? Description { get; set; }

    /// <summary>Tên đạo diễn (tối đa 100 ký tự).</summary>
    public string? Director { get; set; }

    /// <summary>Diễn viên chính (tối đa 500 ký tự, cách nhau bằng dấu phẩy).</summary>
    public string? Cast { get; set; }

    /// <summary>Thể loại phim (vd: Action, Drama, Comedy - tối đa 100 ký tự).</summary>
    public string? Genre { get; set; }

    /// <summary>Thời lượng phim tính bằng phút.</summary>
    public int? DurationMinutes { get; set; }

    /// <summary>Ngôn ngữ gốc của phim (vd: English, Vietnamese - tối đa 50 ký tự).</summary>
    public string? Language { get; set; }

    /// <summary>Mô tả xếp hạng phim (vd: PG, R, NC-17 - tối đa 10 ký tự).</summary>
    public string? RatingCode { get; set; }

    /// <summary>URL poster phim.</summary>
    public string? PosterUrl { get; set; }

    /// <summary>URL trailer phim.</summary>
    public string? TrailerUrl { get; set; }

    /// <summary>URL ảnh banner ngang (dùng cho Hero Section).</summary>
    public string? BannerUrl { get; set; }

    /// <summary>Phim nổi bật — Admin chọn để hiển thị trên Banner trang chủ.</summary>
    public bool IsFeatured { get; set; } = false;

    /// <summary>Ngày phim bắt đầu công chiếu.</summary>
    public DateTime? ReleaseDate { get; set; }

    /// <summary>Ngày phim kết thúc công chiếu.</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Trang thai phim do Admin quan ly truc tiep (ComingSoon, NowShowing, Stopped).</summary>
    public MovieStatus Status { get; set; } = MovieStatus.ComingSoon;

    /// <summary>Trạng thái phim (true = đang hoạt động, false = không).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Thời gian tạo bản ghi.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Thời gian cập nhật bản ghi lần cuối.</summary>
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
