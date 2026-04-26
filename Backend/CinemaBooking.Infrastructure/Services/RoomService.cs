using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinemaBooking.Application.DTOs.Rooms;
using CinemaBooking.Application.Services.Interfaces;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Services;

public class RoomService : IRoomService
{
    private readonly ApplicationDbContext _context;

    public RoomService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<RoomResponseDto>> GetAllRoomsAsync()
    {
        var rooms = await _context.Rooms.ToListAsync();

        return rooms.Select(r => new RoomResponseDto
        {
            Id = r.Id,
            Name = r.Name,
            Capacity = r.Capacity,
            Type = r.Type
        }).ToList();
    }

    public async Task<RoomResponseDto?> GetRoomByIdAsync(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null) return null;

        return new RoomResponseDto
        {
            Id = room.Id,
            Name = room.Name,
            Capacity = room.Capacity,
            Type = room.Type
        };
    }

    public async Task<int> CreateRoomAsync(CreateRoomDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var existing = await _context.Rooms
            .FirstOrDefaultAsync(r => r.Name == dto.Name);
        if (existing != null)
        {
            throw new InvalidOperationException($"Phong chieu '{dto.Name}' da ton tai.");
        }

        var room = new Room
        {
            Name = dto.Name,
            Capacity = dto.Capacity,
            Type = dto.Type
        };

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        return room.Id;
    }

    public async Task<bool> UpdateRoomAsync(int id, UpdateRoomDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
            return false;

        if (!string.IsNullOrWhiteSpace(dto.Name))
            room.Name = dto.Name;
        if (dto.Capacity.HasValue)
            room.Capacity = dto.Capacity.Value;
        if (!string.IsNullOrWhiteSpace(dto.Type))
            room.Type = dto.Type;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteRoomAsync(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
            return false;

        var hasActiveShowtimes = await _context.Showtimes
            .AnyAsync(s => s.RoomId == id && s.IsActive);
        if (hasActiveShowtimes)
        {
            throw new InvalidOperationException(
                "Khong the xoa phong chieu nay vi co suat chieu dang hoat dong.");
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SeatTemplateDto>> GetSeatTemplatesByRoomIdAsync(int roomId)
    {
        var templates = await _context.SeatTemplates
            .Where(st => st.RoomId == roomId)
            .OrderBy(st => st.DisplayOrder)
            .AsNoTracking()
            .ToListAsync();

        return templates.Select(t => new SeatTemplateDto
        {
            Id = t.Id,
            RoomId = t.RoomId,
            Row = t.Row,
            Number = t.Number,
            Type = t.Type.ToString(),
            DisplayOrder = t.DisplayOrder
        }).ToList();
    }
}
