namespace CinemaBooking.Application.DTOs.Movies;

/// <summary>
/// DTO cho phim - dùng để trả về danh sách phim hoặc chi tiết phim.
/// </summary>
public class MovieResponseDto
{
    /// <summary>ID phim.</summary>
    public int Id { get; set; }

    /// <summary>Tên phim.</summary>
    public string Title { get; set; } = string.Empty;

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

    /// <summary>Mã xếp hạng (PG, R, ...).</summary>
    public string? RatingCode { get; set; }

    /// <summary>URL poster phim (dùng cho MovieCard).</summary>
    public string? PosterUrl { get; set; }

    /// <summary>URL trailer.</summary>
    public string? TrailerUrl { get; set; }

    /// <summary>URL banner ngang.</summary>
    public string? BannerUrl { get; set; }

    /// <summary>Phim nổi bật.</summary>
    public bool IsFeatured { get; set; }

    /// <summary>Trạng thái phim (now-showing / coming-soon).</summary>
    public string? Status { get; set; }

    /// <summary>Ngày bắt đầu công chiếu.</summary>
    public DateTime? ReleaseDate { get; set; }

    /// <summary>Ngày kết thúc công chiếu.</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Trạng thái hoạt động.</summary>
    public bool IsActive { get; set; }

    /// <summary>Thời gian tạo.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Thời gian cập nhật lần cuối.</summary>
    public DateTime? UpdatedAt { get; set; }
}
