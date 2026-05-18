using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinemaBooking.Application.DTOs.Showtimes;
using CinemaBooking.Application.Services.Interfaces;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Services;

public class ShowtimeService : IShowtimeService
{
    private readonly ApplicationDbContext _context;
    private readonly ISeatTemplateService _seatTemplateService;

    public ShowtimeService(ApplicationDbContext context, ISeatTemplateService seatTemplateService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _seatTemplateService = seatTemplateService ?? throw new ArgumentNullException(nameof(seatTemplateService));
    }

    public async Task<List<ShowtimeResponseDto>> GetAllShowtimesAsync()
    {
        var showtimes = await _context.Showtimes
            .Include(s => s.Movie)
            .Include(s => s.Room)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        return showtimes.Select(s => new ShowtimeResponseDto
        {
            Id = s.Id,
            MovieId = s.MovieId,
            MovieTitle = s.Movie != null ? s.Movie.Title : "Unknown",
            RoomId = s.RoomId,
            RoomName = s.Room != null ? s.Room.Name : "Unknown",
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            BasePrice = s.BasePrice,
            TotalSeats = s.TotalSeats,
            BookedSeatsCount = s.BookedSeatsCount,
            AvailableSeats = s.TotalSeats - s.BookedSeatsCount,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt
        }).ToList();
    }

    public async Task<int> CreateShowtimeAsync(CreateShowtimeDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == dto.RoomId);
            if (room == null)
            {
                throw new InvalidOperationException($"Khong tim thay phong chieu voi ID: {dto.RoomId}");
            }

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == dto.MovieId);
            if (movie == null)
            {
                throw new InvalidOperationException($"Khong tim thay phim voi ID: {dto.MovieId}");
            }

            if (!movie.DurationMinutes.HasValue || movie.DurationMinutes.Value <= 0)
            {
                throw new InvalidOperationException(
                    $"Phim '{movie.Title}' chua co thoi luong (DurationMinutes). Vui long cap nhat DurationMinutes truoc.");
            }

            int breakMinutes = 15;
            int durationMinutes = movie.DurationMinutes.Value;
            DateTime endTime = dto.EndTime ?? dto.StartTime.AddMinutes(durationMinutes + breakMinutes);

            if (dto.EndTime.HasValue && dto.EndTime.Value <= dto.StartTime)
            {
                throw new InvalidOperationException(
                    $"Thoi gian ket thuc phai lon hon thoi gian bat dau.");
            }

            var existingConflict = await _context.Showtimes
                .Where(s => s.RoomId == dto.RoomId && s.IsActive)
                .Where(s =>
                    (dto.StartTime >= s.StartTime && dto.StartTime < s.EndTime) ||
                    (endTime > s.StartTime && endTime <= s.EndTime) ||
                    (dto.StartTime <= s.StartTime && endTime >= s.EndTime))
                .FirstOrDefaultAsync();

            if (existingConflict != null)
            {
                throw new InvalidOperationException(
                    $"Phong nay da co suat chieu trung gio. "
                    + $"Suat chieu trung tam bat dau luc {existingConflict.StartTime:HH:mm} - {existingConflict.EndTime:HH:mm}");
            }

            var showtime = new Showtime
            {
                MovieId = dto.MovieId,
                RoomId = dto.RoomId,
                StartTime = dto.StartTime,
                EndTime = endTime,
                BasePrice = dto.BasePrice,
                TotalSeats = room.Capacity,
                BookedSeatsCount = 0,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _context.Showtimes.AddAsync(showtime);
            await _context.SaveChangesAsync();

            var showtimeSeats = await _seatTemplateService.GenerateShowtimeSeatsAsync(showtime.Id, dto.RoomId);

            if (showtimeSeats.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Phong chieu {room.Name} khong co mau ghe nao. "
                    + $"Vui long kiem tra bang SeatTemplates.");
            }

            await _context.ShowtimeSeats.AddRangeAsync(showtimeSeats);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return showtime.Id;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Lấy chi tiết một suất chiếu theo ID.
    /// Trả về null nếu không tìm thấy.
    /// </summary>
    public async Task<ShowtimeResponseDto?> GetByIdAsync(int id)
    {
        var showtime = await _context.Showtimes
            .Include(s => s.Movie)
            .Include(s => s.Room)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (showtime == null)
            return null;

        return new ShowtimeResponseDto
        {
            Id = showtime.Id,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.Movie?.Title ?? "Unknown",
            RoomId = showtime.RoomId,
            RoomName = showtime.Room?.Name ?? "Unknown",
            StartTime = showtime.StartTime,
            EndTime = showtime.EndTime,
            BasePrice = showtime.BasePrice,
            TotalSeats = showtime.TotalSeats,
            BookedSeatsCount = showtime.BookedSeatsCount,
            AvailableSeats = showtime.TotalSeats - showtime.BookedSeatsCount,
            IsActive = showtime.IsActive,
            CreatedAt = showtime.CreatedAt
        };
    }

    public async Task<List<ShowtimeSeatDto>> GetSeatsByShowtimeAsync(int showtimeId)
    {
        var showtime = await _context.Showtimes
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == showtimeId);

        if (showtime == null)
            return new List<ShowtimeSeatDto>();

        var seats = await _context.ShowtimeSeats
            .Where(s => s.ShowtimeId == showtimeId)
            .OrderBy(s => s.RowLetter)
            .ThenBy(s => s.ColumnNumber)
            .AsNoTracking()
            .ToListAsync();

        return seats.Select(s => new ShowtimeSeatDto
        {
            Id = s.Id,
            ShowtimeId = s.ShowtimeId,
            SeatNumber = s.SeatNumber,
            RowLetter = s.RowLetter,
            ColumnNumber = s.ColumnNumber,
            GridRow = s.GridRow,
            GridColumn = s.GridColumn,
            Type = s.Type.ToString(),
            Status = s.Status.ToString(),
            Price = showtime.BasePrice
        }).ToList();
    }

    /// <summary>
    /// Cập nhật suất chiếu theo ID.
    ///
    /// Logic tự động hóa:
    ///   - Khi StartTime được cung cấp, EndTime được tính lại = StartTime + Movie.DurationMinutes + 15 phút nghỉ.
    ///   - Khi MovieId hoặc RoomId thay đổi, đều phải kiểm tra đụng độ (collision check).
    ///   - Nếu có ghế đã được đặt (BookedSeatsCount > 0), chỉ cho phép sửa: StartTime (miễn là không gây
    ///     collision), EndTime, BasePrice. Không cho sửa MovieId/RoomId khi đã có vé được đặt.
    ///
    /// Kiểm tra đụng độ:
    ///   - Với RoomId mới (hoặc hiện tại), kiểm tra xem khoảng [StartTime, EndTime] có chồng lấn
    ///     với bất kỳ suất chiếu active nào khác trong cùng phòng hay không.
    ///   - Nếu có, ném InvalidOperationException.
    /// </summary>
    public async Task UpdateShowtimeAsync(int id, UpdateShowtimeDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (showtime == null)
                throw new KeyNotFoundException($"Khong tim thay suat chieu voi ID: {id}");

            var hasBookings = showtime.BookedSeatsCount > 0;

            // --- Validate MovieId ---
            int targetMovieId = dto.MovieId ?? showtime.MovieId;
            if (dto.MovieId.HasValue && dto.MovieId.Value != showtime.MovieId)
            {
                if (hasBookings)
                    throw new InvalidOperationException(
                        "Khong the doi phim vi da co ve duoc dat cho suat chieu nay.");

                var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == dto.MovieId.Value);
                if (movie == null)
                    throw new InvalidOperationException($"Khong tim thay phim voi ID: {dto.MovieId}");

                showtime.MovieId = dto.MovieId.Value;
                showtime.Movie = movie;
            }

            // --- Validate RoomId ---
            int targetRoomId = dto.RoomId ?? showtime.RoomId;
            if (dto.RoomId.HasValue && dto.RoomId.Value != showtime.RoomId)
            {
                if (hasBookings)
                    throw new InvalidOperationException(
                        "Khong the doi phong chieu vi da co ve duoc dat cho suat chieu nay.");

                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == dto.RoomId.Value);
                if (room == null)
                    throw new InvalidOperationException($"Khong tim thay phong chieu voi ID: {dto.RoomId}");

                showtime.RoomId = dto.RoomId.Value;
                showtime.Room = room;
            }

            // --- Resolve final StartTime and compute EndTime ---
            DateTime startTime = dto.StartTime ?? showtime.StartTime;

            if (dto.StartTime.HasValue)
            {
                var targetMovie = await _context.Movies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == targetMovieId);

                if (targetMovie == null)
                    throw new InvalidOperationException($"Khong tim thay phim voi ID: {targetMovieId}");

                if (!targetMovie.DurationMinutes.HasValue || targetMovie.DurationMinutes.Value <= 0)
                    throw new InvalidOperationException(
                        $"Phim '{targetMovie.Title}' chua co thoi luong. Vui long cap nhat DurationMinutes truoc.");

                int breakMinutes = 15;
                int durationMinutes = targetMovie.DurationMinutes.Value;
                showtime.StartTime = startTime;
                showtime.EndTime = startTime.AddMinutes(durationMinutes + breakMinutes);
            }

            // --- Collision check (always needed when room or start changes) ---
            if (dto.StartTime.HasValue || dto.RoomId.HasValue || dto.MovieId.HasValue)
            {
                var conflict = await _context.Showtimes
                    .Where(s => s.Id != id && s.RoomId == targetRoomId && s.IsActive)
                    .Where(s =>
                        (showtime.StartTime >= s.StartTime && showtime.StartTime < s.EndTime) ||
                        (showtime.EndTime > s.StartTime && showtime.EndTime <= s.EndTime) ||
                        (showtime.StartTime <= s.StartTime && showtime.EndTime >= s.EndTime))
                    .FirstOrDefaultAsync();

                if (conflict != null)
                {
                    throw new InvalidOperationException(
                        $"Phong nay da co suat chieu trung gio. "
                        + $"Suat chieu trung tam bat dau luc {conflict.StartTime:HH:mm} - {conflict.EndTime:HH:mm}");
                }
            }

            // --- Update remaining fields ---
            if (dto.BasePrice.HasValue)
            {
                if (dto.BasePrice.Value < 0)
                    throw new InvalidOperationException("Gia ve khong duoc am.");

                showtime.BasePrice = dto.BasePrice.Value;
            }

            if (dto.IsActive.HasValue)
            {
                showtime.IsActive = dto.IsActive.Value;
            }

            showtime.UpdatedAt = DateTime.Now;

            _context.Showtimes.Update(showtime);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
