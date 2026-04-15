using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho thực thể Showtime.
/// </summary>
public class ShowtimeRepository : IShowtimeRepository
{
    private readonly ApplicationDbContext _context;

    public ShowtimeRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Lấy suất chiếu theo ID.
    /// </summary>
    public async Task<Showtime?> GetByIdAsync(int showtimeId)
    {
        return await _context.Showtimes
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == showtimeId);
    }

    /// <summary>
    /// Lấy suất chiếu kèm thông tin phim.
    /// </summary>
    public async Task<Showtime?> GetWithMovieAsync(int showtimeId)
    {
        return await _context.Showtimes
            .Include(s => s.Movie)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == showtimeId);
    }

    /// <summary>
    /// Lấy danh sách suất chiếu theo ID phim.
    /// </summary>
    public async Task<List<Showtime>> GetByMovieIdAsync(int movieId)
    {
        return await _context.Showtimes
            .Where(s => s.MovieId == movieId && s.IsActive)
            .OrderBy(s => s.StartTime)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Thêm suất chiếu mới.
    /// </summary>
    public async Task AddAsync(Showtime showtime)
    {
        if (showtime == null)
            throw new ArgumentNullException(nameof(showtime));

        _context.Showtimes.Add(showtime);
        await SaveChangesAsync();
    }

    /// <summary>
    /// Cập nhật suất chiếu.
    /// </summary>
    public async Task UpdateAsync(Showtime showtime)
    {
        if (showtime == null)
            throw new ArgumentNullException(nameof(showtime));

        showtime.UpdatedAt = DateTime.UtcNow;
        _context.Showtimes.Update(showtime);
        await SaveChangesAsync();
    }

    /// <summary>
    /// Lưu thay đổi vào database.
    /// </summary>
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
