namespace CinemaBooking.Application.DTOs.Movies;

/// <summary>
/// DTO cho chi tiet phim + cac suat chieu theo ngay.
/// </summary>
public class MovieDetailsResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DurationMinutes { get; set; }
    public string? PosterUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public string Status { get; set; } = "ComingSoon";

    /// <summary>Danh sach suat chieu nhom theo ngay.</summary>
    public List<ShowtimeGroupDto> ShowtimeGroups { get; set; } = new();
}

/// <summary>
/// Nhom suat chieu theo ngay (key = "dd/MM/yyyy").
/// </summary>
public class ShowtimeGroupDto
{
    /// <summary>Ngay hien thi (VD: "27/04/2026").</summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>Danh sach cac suat chieu trong ngay.</summary>
    public List<ShowtimeDto> Showtimes { get; set; } = new();
}

/// <summary>
/// Thong tin co ban mot suat chieu.
/// </summary>
public class ShowtimeDto
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal BasePrice { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public int AvailableSeats { get; set; }
}
