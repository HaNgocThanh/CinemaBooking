using CinemaBooking.Application.DTOs.Bookings;
using CinemaBooking.Application.Exceptions;
using CinemaBooking.Application.Helpers;
using CinemaBooking.Application.Services.Interfaces;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Services;

/// <summary>
/// Service quản lý thanh toán booking: submit, approve, reject, polling status.
/// Background worker BookingTimeoutWorker chịu trách nhiệm hủy đơn Pending quá hạn.
///
/// Vé (Ticket) được tạo KHI VÀ CHỈ KHI Admin duyệt đơn (ApproveBookingAsync).
/// Việc hoãn tạo vé tránh ORA-00001 khi đơn bị hủy và ghế nhả ra cho khách khác.
/// </summary>
public class BookingPaymentService : IBookingPaymentService
{
    private readonly ApplicationDbContext _context;

    public BookingPaymentService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<BookingStatusDto> SubmitPaymentAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            throw new BookingNotFoundException(bookingId);
        }

        if (booking.Status != BookingStatus.Pending)
        {
            throw new BookingInvalidStateException(
                bookingId,
                booking.Status,
                "Chỉ có thể xác nhận thanh toán khi đơn đang ở trạng thái Pending. Vui lòng tạo đơn mới.");
        }

        // Dùng ExecuteUpdateAsync để atomic update trạng thái, không cần track entity
        var rowsAffected = await _context.Bookings
            .Where(b => b.Id == bookingId && b.Status == BookingStatus.Pending)
            .ExecuteUpdateAsync(b => b
                .SetProperty(x => x.Status, BookingStatus.AwaitingConfirmation)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));

        if (rowsAffected == 0)
        {
            // Race condition: booking đã bị thay đổi (timeout worker xử lý trước)
            throw new BookingInvalidStateException(
                bookingId,
                booking.Status,
                "Đơn hàng đã bị hủy hoặc thay đổi. Vui lòng tạo đơn mới.");
        }

        // Trả về DTO với trạng thái mới
        return new BookingStatusDto
        {
            BookingId = booking.Id,
            BookingCode = booking.BookingCode,
            Status = BookingStatus.AwaitingConfirmation.ToString(),
            ExpiresAt = booking.ExpiresAt,
            CreatedAt = booking.CreatedAt,
            PaidAt = null,
            TotalAmount = booking.TotalAmount,
            RemainingSeconds = Math.Max(0, 300 - (int)(DateTime.UtcNow - booking.CreatedAt).TotalSeconds)
        };
    }

    /// <inheritdoc />
    public async Task<BookingStatusDto> GetBookingStatusAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            throw new BookingNotFoundException(bookingId);
        }

        return MapToStatusDto(booking);
    }

    /// <inheritdoc />
    public async Task<BookingStatusDto> ApproveBookingAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            throw new BookingNotFoundException(bookingId);
        }

        if (booking.Status != BookingStatus.AwaitingConfirmation)
        {
            throw new BookingInvalidStateException(
                bookingId,
                booking.Status,
                "Chỉ có thể duyệt đơn đang ở trạng thái AwaitingConfirmation.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            booking.Status = BookingStatus.Success;
            booking.PaidAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;

            // ============================================
            // BƯỚC 1: Lấy danh sách ghế thuộc booking qua BookingId (ShowtimeSeat.BookingId)
            // Không cần Ticket vì vé chưa được tạo ở bước CreateBooking
            // ============================================
            var showtimeSeats = await _context.ShowtimeSeats
                .Where(s => s.BookingId == bookingId)
                .ToListAsync();

            if (showtimeSeats.Count == 0)
            {
                throw new InvalidOperationException($"Không tìm thấy ghế nào cho booking {bookingId}.");
            }

            // ============================================
            // BƯỚC 2: Lấy giá ghế từ Showtime
            // ============================================
            var showtime = await _context.Showtimes
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == booking.ShowtimeId);

            if (showtime == null)
            {
                throw new InvalidShowtimeException(booking.ShowtimeId);
            }

            var basePrice = showtime.BasePrice;

            // ============================================
            // BƯỚC 3: Tạo Ticket cho từng ghế và cập nhật ShowtimeSeat
            // ============================================
            foreach (var seat in showtimeSeats)
            {
                var ticketCode = CodeGenerator.GenerateTicketCode();
                var ticket = new Ticket
                {
                    TicketCode = ticketCode,
                    BookingId = bookingId,
                    ShowtimeSeatId = seat.Id,
                    Price = basePrice,
                    SeatType = seat.Type == SeatType.VIP ? "VIP" : "STANDARD",
                    IsActive = true,
                    IssuedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Tickets.Add(ticket);
                // SaveChanges ngay để lấy Ticket.Id trước khi gán vào ShowtimeSeat
                await _context.SaveChangesAsync();

                // Cập nhật ShowtimeSeat: trạng thái Booked + gán TicketId
                seat.Status = SeatStatus.Booked;
                seat.TicketId = ticket.Id;
            }

            // ============================================
            // BƯỚC 4: SaveChanges để persist tất cả thay đổi trên ShowtimeSeats
            // KHÔNG dùng ExecuteUpdateAsync vì sẽ ghi đè TicketId
            // ============================================
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToStatusDto(booking);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BookingStatusDto> RejectBookingAsync(int bookingId, string? reason = null)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            throw new BookingNotFoundException(bookingId);
        }

        if (booking.Status != BookingStatus.AwaitingConfirmation && booking.Status != BookingStatus.Pending)
        {
            throw new BookingInvalidStateException(
                bookingId,
                booking.Status,
                "Không thể hủy đơn ở trạng thái này.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            booking.Status = BookingStatus.Cancelled;
            booking.Notes = string.IsNullOrEmpty(booking.Notes)
                ? reason ?? "Admin từ chối đơn hàng."
                : booking.Notes + " | " + (reason ?? "Admin từ chối đơn hàng.");
            booking.UpdatedAt = DateTime.UtcNow;

            // Giải phóng ghế dựa vào BookingId đã được gán lúc tạo Booking
            // (Không cần Ticket vì vé chỉ được tạo khi Admin duyệt)
            await _context.ShowtimeSeats
                .Where(s => s.BookingId == bookingId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, SeatStatus.Available)
                    .SetProperty(x => x.LockedBySessionId, (string?)null)
                    .SetProperty(x => x.LockedAt, (DateTime?)null)
                    .SetProperty(x => x.HoldExpiryTime, (DateTime?)null)
                    .SetProperty(x => x.TicketId, (int?)null)
                    .SetProperty(x => x.BookedByUserId, (int?)null)
                    .SetProperty(x => x.BookingId, (int?)null));

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToStatusDto(booking);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BookingAdminListDto> GetAllBookingsAsync(int? status = null, int page = 1, int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Bookings
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.Movie)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.Room)
            .Include(b => b.Customer)
            .Include(b => b.Tickets)
                .ThenInclude(t => t.ShowtimeSeat)
            .AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(b => (int)b.Status == status.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var bookingDtos = items.Select(b =>
        {
            var seats = b.Tickets
                .Where(t => t.ShowtimeSeat != null)
                .Select(t => t.ShowtimeSeat!.SeatNumber)
                .OrderBy(s => s)
                .ToList();

            return new BookingAdminDto
            {
                Id = b.Id,
                BookingCode = b.BookingCode,
                MovieTitle = b.Showtime?.Movie?.Title ?? "N/A",
                RoomName = b.Showtime?.Room?.Name ?? "N/A",
                ShowtimeStartTime = b.Showtime?.StartTime ?? DateTime.MinValue,
                CustomerName = b.Customer?.FullName,
                CustomerEmail = b.Customer?.Email,
                TotalAmount = b.TotalAmount,
                Status = b.Status.ToString(),
                StatusValue = (int)b.Status,
                CreatedAt = b.CreatedAt,
                PaidAt = b.PaidAt,
                Seats = string.Join(", ", seats),
                TotalTickets = b.TotalTickets
            };
        }).ToList();

        return new BookingAdminListDto
        {
            Items = bookingDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static BookingStatusDto MapToStatusDto(Domain.Entities.Booking booking)
    {
        return new BookingStatusDto
        {
            BookingId = booking.Id,
            BookingCode = booking.BookingCode,
            Status = booking.Status.ToString(),
            ExpiresAt = booking.ExpiresAt,
            CreatedAt = booking.CreatedAt,
            PaidAt = booking.PaidAt,
            TotalAmount = booking.TotalAmount,
            RemainingSeconds = Math.Max(0, 300 - (int)(DateTime.UtcNow - booking.CreatedAt).TotalSeconds)
        };
    }
}
