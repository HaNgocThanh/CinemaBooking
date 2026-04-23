using CinemaBooking.Application.DTOs.Movies;

namespace CinemaBooking.Application.Services.Interfaces;

/// <summary>
/// Interface cho Movie Service (Application Layer).
/// Xử lý logic nghiệp vụ liên quan đến phim.
/// </summary>
public interface IMovieService
{
    /// <summary>
    /// Lấy danh sách tất cả phim đang hoạt động.
    /// </summary>
    /// <param name="onlyActive">Chỉ lấy phim đang hoạt động (IsActive = true). Mặc định: true.</param>
    /// <param name="status">Trạng thái phim ("now-showing" hoặc "coming-soon"). Mặc định: null.</param>
    /// <returns>Danh sách MovieResponseDto.</returns>
    Task<List<MovieResponseDto>> GetAllMoviesAsync(bool onlyActive = true, string? status = null);

    /// <summary>
    /// Lấy chi tiết phim theo ID.
    /// </summary>
    /// <param name="movieId">ID phim cần lấy.</param>
    /// <returns>MovieResponseDto nếu tìm thấy, null nếu không.</returns>
    Task<MovieResponseDto?> GetMovieByIdAsync(int movieId);

    /// <summary>
    /// Tạo phim mới (dành cho Admin).
    /// </summary>
    /// <param name="request">CreateMovieDto chứa thông tin phim.</param>
    /// <returns>MovieResponseDto của phim vừa tạo.</returns>
    Task<MovieResponseDto> CreateMovieAsync(CreateMovieDto request);

    /// <summary>
    /// Cập nhật thông tin phim (dành cho Admin).
    /// </summary>
    /// <param name="movieId">ID phim cần cập nhật.</param>
    /// <param name="request">UpdateMovieDto chứa thông tin cần cập nhật.</param>
    /// <returns>MovieResponseDto của phim sau khi cập nhật.</returns>
    /// <exception cref="KeyNotFoundException">Nếu phim không tồn tại.</exception>
    Task<MovieResponseDto> UpdateMovieAsync(int movieId, UpdateMovieDto request);

    /// <summary>
    /// Xóa phim (dành cho Admin).
    /// </summary>
    /// <param name="movieId">ID phim cần xóa.</param>
    /// <exception cref="KeyNotFoundException">Nếu phim không tồn tại.</exception>
    Task DeleteMovieAsync(int movieId);
}
