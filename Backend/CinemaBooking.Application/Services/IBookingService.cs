using CinemaBooking.Application.DTOs.Bookings;

namespace CinemaBooking.Application.Services;

/// <summary>
/// Service interface quản lý các chức năng liên quan đến booking (đặt vé).
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Tạo một đơn đặt vé mới.
    /// 
    /// Quy trình:
    /// 1. Validate input (suất chiếu, danh sách ghế, promo code)
    /// 2. Bắt đầu EF Core transaction
    /// 3. Gọi ISeatRepository.LockSeatsAsync để khóa ghế (pessimistic locking)
    /// 4. Query giá các ghế từ ShowtimeSeat
    /// 5. Tính tiền combo (nếu có)
    /// 6. Kiểm tra promo code (nếu có):
    ///    - Kiểm tra tồn tại
    ///    - Kiểm tra hết hạn (throw PromoExpiredException)
    ///    - Tính discount và áp dụng
    /// 7. Tạo entity Booking với trạng thái PendingPayment
    /// 8. Tạo các entity Ticket cho từng ghế
    /// 9. Lưu changes vào DbContext
    /// 10. Commit transaction
    /// 
    /// Nếu bất kỳ lỗi nào xảy ra, transaction sẽ tự động rollback.
    /// </summary>
    /// <param name="request">Request DTO chứa thông tin booking.</param>
    /// <returns>Response DTO chứa thông tin booking vừa tạo.</returns>
    /// <exception cref="InvalidShowtimeException">Khi suất chiếu không tồn tại.</exception>
    /// <exception cref="InvalidSeatsException">Khi danh sách ghế không hợp lệ.</exception>
    /// <exception cref="InvalidPromoCodeException">Khi mã promo không hợp lệ.</exception>
    /// <exception cref="PromoExpiredException">Khi mã promo đã hết hạn.</exception>
    /// <exception cref="Domain.Exceptions.SeatAlreadyLockedException">Khi ghế đã bị khóa bởi user khác.</exception>
    Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request);
}
