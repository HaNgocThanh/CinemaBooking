using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho thực thể Ticket.
/// </summary>
public class TicketRepository : ITicketRepository
{
    private readonly ApplicationDbContext _context;

    public TicketRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Lấy vé theo ID.
    /// </summary>
    public async Task<Ticket?> GetByIdAsync(int ticketId)
    {
        return await _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == ticketId);
    }

    /// <summary>
    /// Lấy danh sách vé theo booking ID.
    /// </summary>
    public async Task<List<Ticket>> GetByBookingIdAsync(int bookingId)
    {
        return await _context.Tickets
            .Where(t => t.BookingId == bookingId)
            .OrderBy(t => t.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Thêm vé mới.
    /// </summary>
    public async Task AddAsync(Ticket ticket)
    {
        if (ticket == null)
            throw new ArgumentNullException(nameof(ticket));

        _context.Tickets.Add(ticket);
        await SaveChangesAsync();
    }

    /// <summary>
    /// Thêm nhiều vé.
    /// </summary>
    public async Task AddRangeAsync(List<Ticket> tickets)
    {
        if (tickets == null || tickets.Count == 0)
            throw new ArgumentException("Danh sách vé không thể trống.", nameof(tickets));

        _context.Tickets.AddRange(tickets);
        await SaveChangesAsync();
    }

    /// <summary>
    /// Cập nhật vé.
    /// </summary>
    public async Task UpdateAsync(Ticket ticket)
    {
        if (ticket == null)
            throw new ArgumentNullException(nameof(ticket));

        ticket.UpdatedAt = DateTime.UtcNow;
        _context.Tickets.Update(ticket);
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
