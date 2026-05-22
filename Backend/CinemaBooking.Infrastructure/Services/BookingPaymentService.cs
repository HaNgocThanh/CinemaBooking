using CinemaBooking.Application.DTOs.Bookings;
using CinemaBooking.Application.Exceptions;
using CinemaBooking.Application.Helpers;
using CinemaBooking.Application.Services.Interfaces;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<BookingPaymentService> _logger;

    public BookingPaymentService(ApplicationDbContext context, ILogger<BookingPaymentService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        // Load booking cùng với navigation property ShowtimeSeats (được gán lúc CreateBooking)
        var booking = await _context.Bookings
            .Include(b => b.ShowtimeSeats)
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

        // Đổi trạng thái đơn hàng
        booking.Status = BookingStatus.Cancelled;
        booking.Notes = string.IsNullOrEmpty(booking.Notes)
            ? reason ?? "Admin từ chối đơn hàng."
            : booking.Notes + " | " + (reason ?? "Admin từ chối đơn hàng.");
        booking.UpdatedAt = DateTime.UtcNow;

        // Giải phóng từng ghế qua entity (KHÔNG dùng ExecuteUpdateAsync để tránh
        // conflict với pessimistic lock đang active trên ghế từ LockSeatsAsync)
        foreach (var seat in booking.ShowtimeSeats)
        {
            seat.Status = SeatStatus.Available;
            seat.LockedBySessionId = null;
            seat.LockedAt = null;
            seat.HoldExpiryTime = null;
            seat.TicketId = null;
            seat.BookedByUserId = null;
            seat.BookingId = null;
            seat.UpdatedAt = DateTime.UtcNow;
        }

        // Lưu toàn bộ thay đổi xuống Oracle
        await _context.SaveChangesAsync();

        return MapToStatusDto(booking);
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

    /// <inheritdoc />
    public async Task<ETicketDto> GetETicketAsync(int bookingId, int? customerId = null)
    {
        var query = _context.Bookings
            .Include(b => b.Tickets)
                .ThenInclude(t => t.ShowtimeSeat)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.Movie)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.Room)
            .AsQueryable();

        if (customerId.HasValue && customerId.Value > 0)
        {
            query = query.Where(b => b.CustomerId == customerId.Value);
        }

        var booking = await query.FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            throw new BookingNotFoundException(bookingId);
        }

        if (booking.Status != BookingStatus.Success)
        {
            throw new BookingInvalidStateException(
                bookingId,
                booking.Status,
                "Vé chỉ có thể xem khi đơn đặt vé đã được thanh toán thành công.");
        }

        var showtime = booking.Showtime!;
        var movie = showtime.Movie!;
        var room = showtime.Room!;

        // Gom ten cac ghe thanh mot chuoi VD: "A1, A2, A3"
        var seatNames = string.Join(", ", booking.Tickets
            .Select(t => t.ShowtimeSeat?.SeatNumber)
            .Where(n => n != null)
            .OrderBy(n => n)
            .Select(n => n!));

        // Tao chuoi QR code data
        var qrData = $"CINEMA_TICKET_{booking.Id}_{booking.CustomerId ?? 0}_{booking.BookingCode}_{DateTime.UtcNow:yyyyMMddHHmmss}";

        // Format thoi gian
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(showtime.StartTime, timeZone);
        var localEnd = TimeZoneInfo.ConvertTimeFromUtc(showtime.EndTime, timeZone);
        var localBookedAt = TimeZoneInfo.ConvertTimeFromUtc(booking.BookedAt, timeZone);

        return new ETicketDto
        {
            BookingId = booking.Id,
            BookingCode = booking.BookingCode,
            MovieTitle = movie.Title,
            PosterUrl = movie.PosterUrl,
            CinemaName = "CGV Cinema", // Lay tu config neu can
            RoomName = room.Name,
            StartTime = localStart.ToString("HH:mm"),
            EndTime = localEnd.ToString("HH:mm"),
            ShowDate = localStart.ToString("dddd, dd/MM/yyyy", new System.Globalization.CultureInfo("vi-VN")),
            SeatNames = seatNames,
            TotalTickets = booking.TotalTickets,
            TotalAmount = booking.TotalAmount,
            PaymentMethod = booking.PaymentMethod ?? "QR Code",
            QrCodeData = qrData,
            BookedAt = localBookedAt
        };
    }

    /// <inheritdoc />
    public async Task<BookingHistoryListDto> GetMyHistoryAsync(int customerId)
    {
        _logger.LogInformation("User {UserId} is requesting ticket history (GetMyHistoryAsync)", customerId);

        // Lấy các đơn đặt vé đã hoàn tất (Success), đã hủy (Cancelled), hoặc hết hạn (Expired)
        // Không bao gồm Pending/AwaitingConfirmation (vé chưa được duyệt)
        var bookings = await _context.Bookings
            .Include(b => b.Tickets)
                .ThenInclude(t => t.ShowtimeSeat)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.Movie)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.Room)
            .Where(b => b.CustomerId == customerId)
            .Where(b =>
                b.Status == BookingStatus.Success ||
                b.Status == BookingStatus.Cancelled ||
                b.Status == BookingStatus.Expired)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        _logger.LogInformation(
            "GetMyHistoryAsync: UserId={UserId}, Found {Count} completed bookings (Success/Cancelled/Expired). " +
            "Note: Pending/AwaitingConfirmation bookings are excluded from ticket history.",
            customerId, bookings.Count);

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        var items = bookings.Select(b =>
        {
            var showtime = b.Showtime!;
            var localStart = TimeZoneInfo.ConvertTimeFromUtc(showtime.StartTime, timeZone);
            var localBookedAt = TimeZoneInfo.ConvertTimeFromUtc(b.BookedAt, timeZone);

            var seatNames = string.Join(", ",
                b.Tickets
                    .Select(t => t.ShowtimeSeat?.SeatNumber)
                    .Where(n => n != null)
                    .OrderBy(n => n)
                    .Select(n => n!));

            return new BookingHistoryDto
            {
                BookingId = b.Id,
                BookingCode = b.BookingCode,
                MovieTitle = showtime.Movie?.Title ?? "Unknown",
                PosterUrl = showtime.Movie?.PosterUrl,
                RoomName = showtime.Room?.Name ?? "Unknown",
                ShowDate = localStart.ToString("dd/MM/yyyy"),
                StartTime = localStart.ToString("HH:mm"),
                SeatNames = seatNames,
                TotalTickets = b.TotalTickets,
                TotalAmount = b.TotalAmount,
                Status = b.Status.ToString(),
                StatusValue = (int)b.Status,
                BookedAt = localBookedAt.ToString("dd/MM/yyyy HH:mm")
            };
        }).ToList();

        return new BookingHistoryListDto
        {
            Items = items,
            TotalCount = items.Count
        };
    }

    /// <inheritdoc />
    public async Task<BookingHistoryListDto> GetAllMyBookingsAsync(int customerId)
    {
        _logger.LogInformation("User {UserId} is requesting all bookings (GetAllMyBookingsAsync)", customerId);

        // Lấy TẤT CẢ booking của user (bao gồm Pending, AwaitingConfirmation, Success, Cancelled, Expired)
        var bookings = await _context.Bookings
            .Include(b => b.Tickets)
                .ThenInclude(t => t.ShowtimeSeat)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.Movie)
            .Include(b => b.Showtime)
                .ThenInclude(s => s!.Room)
            .Where(b => b.CustomerId == customerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        _logger.LogInformation(
            "GetAllMyBookingsAsync: UserId={UserId}, Found {Count} total bookings (all statuses)",
            customerId, bookings.Count);

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        var items = bookings.Select(b =>
        {
            var showtime = b.Showtime!;
            var localStart = TimeZoneInfo.ConvertTimeFromUtc(showtime.StartTime, timeZone);
            var localBookedAt = TimeZoneInfo.ConvertTimeFromUtc(b.BookedAt, timeZone);

            var seatNames = string.Join(", ",
                b.Tickets
                    .Select(t => t.ShowtimeSeat?.SeatNumber)
                    .Where(n => n != null)
                    .OrderBy(n => n)
                    .Select(n => n!));

            return new BookingHistoryDto
            {
                BookingId = b.Id,
                BookingCode = b.BookingCode,
                MovieTitle = showtime.Movie?.Title ?? "Unknown",
                PosterUrl = showtime.Movie?.PosterUrl,
                RoomName = showtime.Room?.Name ?? "Unknown",
                ShowDate = localStart.ToString("dd/MM/yyyy"),
                StartTime = localStart.ToString("HH:mm"),
                SeatNames = seatNames,
                TotalTickets = b.TotalTickets,
                TotalAmount = b.TotalAmount,
                Status = b.Status.ToString(),
                StatusValue = (int)b.Status,
                BookedAt = localBookedAt.ToString("dd/MM/yyyy HH:mm")
            };
        }).ToList();

        return new BookingHistoryListDto
        {
            Items = items,
            TotalCount = items.Count
        };
    }
}
