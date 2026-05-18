using CinemaBooking.Application.DTOs.Bookings;
using CinemaBooking.Application.Exceptions;
using CinemaBooking.Application.Helpers;
using CinemaBooking.Application.Services.Interfaces;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Domain.Exceptions;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace CinemaBooking.Infrastructure.Services;

/// <summary>
/// Service implementation quản lý các chức năng booking.
/// Xử lý logic tạo đơn đặt vé bao gồm:
/// - Pessimistic locking ghế
/// - Validation promo code
/// - Tính toán giá tiền
/// - Tạo Booking entity (trạng thái Pending)
/// - Chuyển ghế sang trạng thái Reserved
///
/// Vé (Ticket) được tạo KHI VÀ CHỈ KHI Admin duyệt đơn (ApproveBookingAsync).
/// Việc hoãn tạo vé tránh ORA-00001 khi đơn bị hủy và ghế nhả ra cho khách khác.
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
                // BƯỚC 4: Validate VIP single seat gap rule
                // ============================================
                var allSeats = await _context.ShowtimeSeats
                    .Where(s => s.ShowtimeId == request.ShowtimeId)
                    .AsNoTracking()
                    .ToListAsync();
                ValidateVipSingleGapRule(allSeats, request.SeatIds);

                // ============================================
                // BƯỚC 5: Gọi ISeatRepository.LockSeatsAsync
                // (Pessimistic locking - FOR UPDATE NOWAIT)
                // Transaction do BookingService quản lý
                // ============================================
                var userId = request.SessionId ?? request.CustomerId?.ToString() ?? Guid.NewGuid().ToString();
                var lockedSeats = await _seatRepository.LockSeatsAsync(
                    request.SeatIds,
                    request.ShowtimeId,
                    userId
                );

                // ============================================
                // BƯỚC 6: Query giá các ghế + tính SubTotal
                // ============================================
                // Giá vé = Showtime.BasePrice (mặc định)
                var subtotal = lockedSeats.Count * showtime.BasePrice;

                // ============================================
                // BƯỚC 7: Tính tiền Combos (nếu có)
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
                // BƯỚC 8: Kiểm tra PromoCode (nếu có)
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
                // BƯỚC 9: Tạo entity Booking
                // ============================================
                var bookingCode = CodeGenerator.GenerateBookingCode();
                var expiresAt = DateTime.UtcNow.AddMinutes(BOOKING_EXPIRATION_MINUTES);

                var booking = new Booking
                {
                    BookingCode = bookingCode,
                    ShowtimeId = request.ShowtimeId,
                    CustomerId = request.CustomerId,
                    Status = BookingStatus.Pending,
                    TotalTickets = lockedSeats.Count,
                    SubTotal = subtotal,
                    DiscountAmount = discountAmount,
                    TotalAmount = totalAmount,
                    PromotionCode = appliedPromoCode,
                    SessionId = userId,
                    BookedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    Notes = request.Notes
                };

                // ============================================
                // BƯỚC 10: Lưu Booking
                // ============================================
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // ============================================
                // BƯỚC 11: Gán BookingId vào ShowtimeSeats để Admin duyệt biết ghế nào
                // ============================================
                var lockedSeatIds = lockedSeats.Select(s => s.Id).ToList();
                await _context.ShowtimeSeats
                    .Where(s => lockedSeatIds.Contains(s.Id))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.BookingId, booking.Id)
                        .SetProperty(x => x.BookedByUserId, booking.CustomerId));

                // ============================================
                // BƯỚC 12: Commit Transaction
                // ============================================
                await transaction.CommitAsync();

                // ============================================
                // BƯỚC 12: Trả về response DTO
                // ============================================
                // TicketIds sẽ được tạo khi Admin duyệt đơn
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
                    CreatedAt = booking.CreatedAt,
                    RemainingSeconds = Math.Max(0, 300 - (int)(DateTime.UtcNow - booking.CreatedAt).TotalSeconds),
                    TicketIds = new List<int>()
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
            catch (VipSingleGapException)
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

    /// <summary>
    /// Validates the VIP single seat gap rule.
    /// A VIP seat that is currently Available cannot be left empty while flanked
    /// by TAKEN seats (Booked or Locked) on both sides.
    /// Boundary seats (first and last in a row) are always exempt.
    ///
    /// @param allSeats - full seat list for the showtime from the database
    /// @param newSeatIds - IDs of the seats being booked in this request
    /// @throws VipSingleGapException if the rule is violated
    /// </summary>
    private void ValidateVipSingleGapRule(List<ShowtimeSeat> allSeats, List<int> newSeatIds)
    {
        var newSeatSet = new HashSet<int>(newSeatIds);

        // Group seats by row and sort by column
        var byRow = allSeats
            .GroupBy(s => s.RowLetter)
            .ToDictionary(g => g.Key, g => g.OrderBy(s => s.ColumnNumber).ToList());

        foreach (var kvp in byRow)
        {
            var row = kvp.Value;

            // Skip boundary seats: index 0 (left edge) and last index (right edge)
            for (int i = 1; i < row.Count - 1; i++)
            {
                var seat = row[i];

                // Only apply to VIP seats that are currently empty (Available) in the database
                if (seat.Type != SeatType.VIP || seat.Status != SeatStatus.Available)
                    continue;

                var prev = row[i - 1];
                var next = row[i + 1];

                var prevTaken = prev.Status == SeatStatus.Booked
                    || prev.Status == SeatStatus.Locked
                    || newSeatSet.Contains(prev.Id);

                var nextTaken = next.Status == SeatStatus.Booked
                    || next.Status == SeatStatus.Locked
                    || newSeatSet.Contains(next.Id);

                // Violation: flanked by TAKEN seats on both sides
                if (prevTaken && nextTaken)
                {
                    throw new VipSingleGapException();
                }
            }
        }
    }
}
