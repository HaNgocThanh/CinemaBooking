using CinemaBooking.Application.DTOs.Movies;
using CinemaBooking.Application.Services.Interfaces;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Services;

/// <summary>
/// Service implementation cho Movie (Infrastructure Layer).
/// Xử lý business logic liên quan đến phim.
/// 
/// ⚠️ Nằm ở Infrastructure layer vì phụ thuộc vào ApplicationDbContext.
/// </summary>
public class MovieService : IMovieService
{
    private readonly ApplicationDbContext _context;

    public MovieService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Ánh xạ Movie entity → MovieResponseDto
    /// </summary>
    private static MovieResponseDto MapToDto(Movie movie)
    {
        return new MovieResponseDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            Director = movie.Director,
            Cast = movie.Cast,
            Genre = movie.Genre,
            DurationMinutes = movie.DurationMinutes,
            Language = movie.Language,
            RatingCode = movie.RatingCode,
            PosterUrl = movie.PosterUrl,
            TrailerUrl = movie.TrailerUrl,
            BannerUrl = movie.BannerUrl,
            IsFeatured = movie.IsFeatured,
            Status = movie.Status.ToString(),
            ReleaseDate = movie.ReleaseDate,
            EndDate = movie.EndDate,
            IsActive = movie.IsActive,
            CreatedAt = movie.CreatedAt,
            UpdatedAt = movie.UpdatedAt,
        };
    }

    /// <summary>
    /// Lấy danh sách tất cả phim đang hoạt động.
    /// </summary>
    public async Task<List<MovieResponseDto>> GetAllMoviesAsync(bool onlyActive = true, string? status = null)
    {
        var query = _context.Movies.AsQueryable();

        if (onlyActive)
        {
            query = query.Where(m => m.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<MovieStatus>(status, true, out var movieStatus))
            {
                query = query.Where(m => m.Status == movieStatus);
            }
        }

        var movies = await query
            .OrderByDescending(m => m.ReleaseDate)
            .ToListAsync();

        return movies.Select(m => MapToDto(m)).ToList();
    }

    /// <summary>
    /// Lấy chi tiết phim theo ID.
    /// </summary>
    public async Task<MovieResponseDto?> GetMovieByIdAsync(int movieId)
    {
        var movie = await _context.Movies
            .Where(m => m.Id == movieId)
            .FirstOrDefaultAsync();

        if (movie == null)
        {
            return null;
        }

        return MapToDto(movie);
    }

    /// <summary>
    /// Tạo phim mới.
    /// </summary>
    public async Task<MovieResponseDto> CreateMovieAsync(CreateMovieDto request)
    {
        var movieStatus = Enum.TryParse<MovieStatus>(request.Status, true, out var s)
            ? s
            : MovieStatus.ComingSoon;

        var movie = new Movie
        {
            Title = request.Title,
            Description = request.Description,
            Director = request.Director,
            Cast = request.Cast,
            Genre = request.Genre,
            DurationMinutes = request.DurationMinutes,
            Language = request.Language,
            RatingCode = request.RatingCode,
            PosterUrl = request.PosterUrl,
            TrailerUrl = request.TrailerUrl,
            BannerUrl = request.BannerUrl,
            ReleaseDate = request.ReleaseDate,
            EndDate = request.EndDate,
            Status = movieStatus,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            IsFeatured = request.IsFeatured,
        };

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();

        return MapToDto(movie);
    }

    /// <summary>
    /// Cập nhật thông tin phim.
    /// </summary>
    public async Task<MovieResponseDto> UpdateMovieAsync(int movieId, UpdateMovieDto request)
    {
        var movie = await _context.Movies
            .FirstOrDefaultAsync(m => m.Id == movieId);

        if (movie == null)
        {
            throw new KeyNotFoundException($"Phim với ID {movieId} không tồn tại.");
        }

        // Cập nhật các thuộc tính không null từ request
        if (!string.IsNullOrEmpty(request.Title))
            movie.Title = request.Title;
        if (!string.IsNullOrEmpty(request.Description))
            movie.Description = request.Description;
        if (!string.IsNullOrEmpty(request.Director))
            movie.Director = request.Director;
        if (!string.IsNullOrEmpty(request.Cast))
            movie.Cast = request.Cast;
        if (!string.IsNullOrEmpty(request.Genre))
            movie.Genre = request.Genre;
        if (request.DurationMinutes.HasValue)
            movie.DurationMinutes = request.DurationMinutes;
        if (!string.IsNullOrEmpty(request.Language))
            movie.Language = request.Language;
        if (!string.IsNullOrEmpty(request.RatingCode))
            movie.RatingCode = request.RatingCode;
        if (!string.IsNullOrEmpty(request.PosterUrl))
            movie.PosterUrl = request.PosterUrl;
        if (!string.IsNullOrEmpty(request.TrailerUrl))
            movie.TrailerUrl = request.TrailerUrl;
        if (!string.IsNullOrEmpty(request.BannerUrl))
            movie.BannerUrl = request.BannerUrl;
        if (request.ReleaseDate.HasValue)
            movie.ReleaseDate = request.ReleaseDate;
        if (request.EndDate.HasValue)
            movie.EndDate = request.EndDate;
        if (request.IsActive.HasValue)
            movie.IsActive = request.IsActive.Value;
        if (request.IsFeatured.HasValue)
            movie.IsFeatured = request.IsFeatured.Value;
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<MovieStatus>(request.Status, true, out var movieStatus))
            movie.Status = movieStatus;

        movie.UpdatedAt = DateTime.UtcNow;

        _context.Movies.Update(movie);
        await _context.SaveChangesAsync();

        return MapToDto(movie);
    }

    /// <summary>
    /// Xóa phim.
    /// </summary>
    public async Task DeleteMovieAsync(int movieId)
    {
        var movie = await _context.Movies
            .FirstOrDefaultAsync(m => m.Id == movieId);

        if (movie == null)
        {
            throw new KeyNotFoundException($"Phim với ID {movieId} không tồn tại.");
        }

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();
    }
}
