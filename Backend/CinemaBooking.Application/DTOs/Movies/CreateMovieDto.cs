namespace CinemaBooking.Application.DTOs.Movies;

/// <summary>
/// DTO cho tạo phim mới (dành cho Admin).
/// </summary>
public class CreateMovieDto
{
    /// <summary>Tên phim (bắt buộc).</summary>
    public required string Title { get; set; }

    /// <summary>Mô tả phim.</summary>
    public string? Description { get; set; }

    /// <summary>Tên đạo diễn.</summary>
    public string? Director { get; set; }

    /// <summary>Diễn viên chính.</summary>
    public string? Cast { get; set; }

    /// <summary>Thể loại phim.</summary>
    public string? Genre { get; set; }

    /// <summary>Thời lượng phim (phút).</summary>
    public int? DurationMinutes { get; set; }

    /// <summary>Ngôn ngữ gốc.</summary>
    public string? Language { get; set; }

    /// <summary>Mã xếp hạng.</summary>
    public string? RatingCode { get; set; }

    /// <summary>URL poster phim.</summary>
    public string? PosterUrl { get; set; }

    /// <summary>URL trailer.</summary>
    public string? TrailerUrl { get; set; }

    /// <summary>URL banner ngang.</summary>
    public string? BannerUrl { get; set; }

    /// <summary>Phim nổi bật.</summary>
    public bool IsFeatured { get; set; }

    /// <summary>Ngày bắt đầu công chiếu.</summary>
    public DateTime? ReleaseDate { get; set; }

    /// <summary>Ngày kết thúc công chiếu.</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Trạng thái phim (ComingSoon, NowShowing, Stopped). Mặc định: ComingSoon.</summary>
    public string Status { get; set; } = "ComingSoon";
}
