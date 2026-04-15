using CinemaBooking.Application.DTOs.Bookings;
using CinemaBooking.Application.Exceptions;
using CinemaBooking.Application.Helpers;
using CinemaBooking.Application.Services;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Domain.Exceptions;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Services;

/// <summary>
/// Service implementation quản lý các chức năng booking.
/// Xử lý logic tạo đơn đặt vé bao gồm:
/// - Pessimistic locking ghế
/// - Validation promo code
/// - Tính toán giá tiền
/// - Tạo Booking và Ticket entities
/// </summary>
public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;
    private readonly ISeatRepository _seatRepository;
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IPromotionRepository _promotionRepository;
    private readonly ITicketRepository _ticketRepository;

    private const int BOOKING_EXPIRATION_MINUTES = 5;

    public BookingService(
        ApplicationDbContext context,
        ISeatRepository seatRepository,
        IShowtimeRepository showtimeRepository,
        IPromotionRepository promotionRepository,
        ITicketRepository ticketRepository)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _seatRepository = seatRepository ?? throw new ArgumentNullException(nameof(seatRepository));
        _showtimeRepository = showtimeRepository ?? throw new ArgumentNullException(nameof(showtimeRepository));
        _promotionRepository = promotionRepository ?? throw new ArgumentNullException(nameof(promotionRepository));
        _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
    }

    /// <summary>
    /// Tạo đơn đặt vé mới với pessimistic locking.
    /// </summary>
    public async Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request)
    {
        // ============================================
        // BƯỚC 1: Validate input
        // ============================================
        ValidateInput(request);

        // ============================================
        // BƯỚC 2: Bắt đầu EF Core Transaction
        // ============================================
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // ============================================
                // BƯỚC 3: Verify suất chiếu tồn tại
                // ============================================
                var showtime = await _showtimeRepository.GetByIdAsync(request.ShowtimeId);
                if (showtime == null)
                {
                    throw new InvalidShowtimeException(request.ShowtimeId);
                }

                // ============================================
                // BƯỚC 4: Gọi ISeatRepository.LockSeatsAsync
                // (Pessimistic locking - FOR UPDATE NOWAIT)
                // ============================================
                var userId = request.SessionId ?? request.CustomerId?.ToString() ?? Guid.NewGuid().ToString();
                List<ShowtimeSeat> lockedSeats;
                
                try
                {
                    lockedSeats = await _seatRepository.LockSeatsAsync(
                        request.SeatIds,
                        request.ShowtimeId,
                        userId
                    );
                }
                catch (SeatAlreadyLockedException)
                {
                    // Re-throw domain exception
                    await transaction.RollbackAsync();
                    throw;
                }

                // ============================================
                // BƯỚC 5: Query giá các ghế + tính SubTotal
                // ============================================
                // Giá vé = Showtime.BasePrice (mặc định)
                var subtotal = lockedSeats.Count * showtime.BasePrice;

                // ============================================
                // BƯỚC 6: Tính tiền Combos (nếu có)
                // ============================================
                var comboAmount = 0m;
                if (request.CombosWithQuantity != null && request.CombosWithQuantity.Count > 0)
                {
                    // TODO: Query giá combo từ ComboRepository
                    // comboAmount = await CalculateComboAmount(request.CombosWithQuantity);
                    // Tạm thời set = 0
                    comboAmount = 0;
                }

                var totalBeforeDiscount = subtotal + comboAmount;

                // ============================================
                // BƯỚC 7: Kiểm tra PromoCode (nếu có)
                // ============================================
                decimal discountAmount = 0;
                string? appliedPromoCode = null;

                if (!string.IsNullOrWhiteSpace(request.PromoCode))
                {
                    var promotionInfo = await _promotionRepository.GetByCodeAsync(request.PromoCode);
                    
                    if (promotionInfo == null)
                    {
                        await transaction.RollbackAsync();
                        throw new InvalidPromoCodeException(request.PromoCode);
                    }

                    // Kiểm tra xem promo còn hợp lệ không
                    if (!promotionInfo.IsValid())
                    {
                        await transaction.RollbackAsync();
                        throw new PromoExpiredException(request.PromoCode);
                    }

                    // Tính toán discount amount
                    discountAmount = promotionInfo.CalculateDiscount(totalBeforeDiscount);
                    appliedPromoCode = request.PromoCode;
                }

                var totalAmount = totalBeforeDiscount - discountAmount;

                // ============================================
                // BƯỚC 8: Tạo entity Booking
                // ============================================
                var bookingCode = CodeGenerator.GenerateBookingCode();
                var expiresAt = DateTime.UtcNow.AddMinutes(BOOKING_EXPIRATION_MINUTES);

                var booking = new Booking
                {
                    BookingCode = bookingCode,
                    ShowtimeId = request.ShowtimeId,
                    CustomerId = request.CustomerId,
                    Status = BookingStatus.PendingPayment,
                    TotalTickets = lockedSeats.Count,
                    SubTotal = subtotal,
                    DiscountAmount = discountAmount,
                    TotalAmount = totalAmount,
                    PromotionCode = appliedPromoCode,
                    SessionId = userId,
                    BookedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    Notes = request.Notes
                };

                // ============================================
                // BƯỚC 9: Lưu Booking trước (để lấy ID)
                // ============================================
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // ============================================
                // BƯỚC 10: Tạo các entity Ticket cho từng ghế
                // ============================================
                var tickets = new List<Ticket>();

                foreach (var seat in lockedSeats)
                {
                    var ticketCode = CodeGenerator.GenerateTicketCode();
                    var ticket = new Ticket
                    {
                        TicketCode = ticketCode,
                        BookingId = booking.Id,
                        ShowtimeSeatId = seat.Id,
                        Price = showtime.BasePrice,
                        SeatType = "STANDARD",
                        IsActive = true,
                        IssuedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };
                    tickets.Add(ticket);
                }

                // ============================================
                // BƯỚC 11: Lưu Tickets vào DbContext
                // ============================================
                _context.Tickets.AddRange(tickets);
                await _context.SaveChangesAsync();

                // ============================================
                // BƯỚC 12: Cập nhật ShowtimeSeat để map với Ticket
                // ============================================
                foreach (var seat in lockedSeats)
                {
                    var ticket = tickets.FirstOrDefault(t => t.ShowtimeSeatId == seat.Id);
                    if (ticket != null)
                    {
                        seat.TicketId = ticket.Id;
                        seat.Status = SeatStatus.Booked;
                        seat.BookedByUserId = request.CustomerId;
                    }
                }
                _context.ShowtimeSeats.UpdateRange(lockedSeats);
                await _context.SaveChangesAsync();

                // ============================================
                // BƯỚC 13: Commit Transaction
                // ============================================
                await transaction.CommitAsync();

                // ============================================
                // BƯỚC 14: Trả về response DTO
                // ============================================
                return new BookingResponseDto
                {
                    BookingId = booking.Id,
                    BookingCode = booking.BookingCode,
                    Status = booking.Status.ToString(),
                    TotalTickets = booking.TotalTickets,
                    SubTotal = booking.SubTotal,
                    DiscountAmount = booking.DiscountAmount,
                    TotalAmount = booking.TotalAmount,
                    AppliedPromoCode = booking.PromotionCode,
                    ExpiresAt = booking.ExpiresAt,
                    BookedAt = booking.BookedAt,
                    TicketIds = tickets.Select(t => t.Id).ToList()
                };
            }
            catch (InvalidShowtimeException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (InvalidSeatsException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (InvalidPromoCodeException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (PromoExpiredException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (SeatAlreadyLockedException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (Exception)
            {
                // Rollback transaction nếu có lỗi bất ngờ
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    /// <summary>
    /// Validate input request.
    /// </summary>
    private static void ValidateInput(BookingRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.ShowtimeId <= 0)
            throw new InvalidShowtimeException("INVALID_SHOWTIME", "ID suất chiếu phải > 0.");

        if (request.SeatIds == null || request.SeatIds.Count == 0)
            throw new InvalidSeatsException("Danh sách ghế không thể trống. Vui lòng chọn ít nhất 1 ghế.");

        if (request.SeatIds.Any(id => id <= 0))
            throw new InvalidSeatsException("INVALID_SEAT_ID", "ID ghế phải > 0.");

        if (request.CombosWithQuantity != null)
        {
            foreach (var combo in request.CombosWithQuantity)
            {
                if (combo.Key <= 0 || combo.Value <= 0)
                    throw new InvalidSeatsException("INVALID_COMBO", "ID combo và số lượng phải > 0.");
            }
        }
    }
}
