using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinemaBooking.Application.Services.Interfaces;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Services;

/// <summary>
/// Service for managing seat templates and generating showtime seats.
/// Implements ISeatTemplateService interface for Clean Architecture.
/// </summary>
public class SeatTemplateService : ISeatTemplateService
{
    private readonly ApplicationDbContext _context;

    public SeatTemplateService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<SeatTemplate>> GetSeatTemplatesByRoomIdAsync(int roomId)
    {
        return await _context.SeatTemplates
            .Where(st => st.RoomId == roomId)
            .OrderBy(st => st.DisplayOrder)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<ShowtimeSeat>> GenerateShowtimeSeatsAsync(int showtimeId, int roomId)
    {
        var templates = await GetSeatTemplatesByRoomIdAsync(roomId);

        return templates.Select(template => new ShowtimeSeat
        {
            ShowtimeId = showtimeId,
            SeatNumber = $"{template.Row}{template.Number}",
            RowLetter = template.Row,
            ColumnNumber = template.Number,
            Type = template.Type,
            Status = SeatStatus.Available,
            CreatedAt = DateTime.UtcNow
        }).ToList();
    }
}
