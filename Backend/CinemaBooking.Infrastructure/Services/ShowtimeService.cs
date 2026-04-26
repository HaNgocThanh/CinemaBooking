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

            var existingConflict = await _context.Showtimes
                .Where(s => s.RoomId == dto.RoomId && s.IsActive)
                .Where(s =>
                    (dto.StartTime >= s.StartTime && dto.StartTime < s.EndTime) ||
                    (dto.EndTime > s.StartTime && dto.EndTime <= s.EndTime) ||
                    (dto.StartTime <= s.StartTime && dto.EndTime >= s.EndTime))
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
                EndTime = dto.EndTime,
                BasePrice = dto.BasePrice,
                TotalSeats = room.Capacity,
                BookedSeatsCount = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
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
}
