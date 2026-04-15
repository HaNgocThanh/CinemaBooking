using CinemaBooking.Domain.Entities;

namespace CinemaBooking.Domain.Interfaces;

/// <summary>
/// Repository interface cho thực thể Ticket.
/// </summary>
public interface ITicketRepository
{
    /// <summary>
    /// Lấy vé theo ID.
    /// </summary>
    Task<Ticket?> GetByIdAsync(int ticketId);

    /// <summary>
    /// Lấy danh sách vé theo booking ID.
    /// </summary>
    Task<List<Ticket>> GetByBookingIdAsync(int bookingId);

    /// <summary>
    /// Thêm vé mới.
    /// </summary>
    Task AddAsync(Ticket ticket);

    /// <summary>
    /// Thêm nhiều vé.
    /// </summary>
    Task AddRangeAsync(List<Ticket> tickets);

    /// <summary>
    /// Cập nhật vé.
    /// </summary>
    Task UpdateAsync(Ticket ticket);

    /// <summary>
    /// Lưu thay đổi vào database.
    /// </summary>
    Task SaveChangesAsync();
}
