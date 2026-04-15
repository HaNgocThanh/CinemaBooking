using CinemaBooking.Domain.Entities;

namespace CinemaBooking.Domain.Interfaces;

/// <summary>
/// Repository interface cho thực thể Showtime.
/// </summary>
public interface IShowtimeRepository
{
    /// <summary>
    /// Lấy suất chiếu theo ID.
    /// </summary>
    Task<Showtime?> GetByIdAsync(int showtimeId);

    /// <summary>
    /// Lấy suất chiếu kèm thông tin phim.
    /// </summary>
    Task<Showtime?> GetWithMovieAsync(int showtimeId);

    /// <summary>
    /// Lấy danh sách suất chiếu theo ID phim.
    /// </summary>
    Task<List<Showtime>> GetByMovieIdAsync(int movieId);

    /// <summary>
    /// Thêm suất chiếu mới.
    /// </summary>
    Task AddAsync(Showtime showtime);

    /// <summary>
    /// Cập nhật suất chiếu.
    /// </summary>
    Task UpdateAsync(Showtime showtime);

    /// <summary>
    /// Lưu thay đổi vào database.
    /// </summary>
    Task SaveChangesAsync();
}
