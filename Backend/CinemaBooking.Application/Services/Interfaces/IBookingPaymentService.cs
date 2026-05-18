using CinemaBooking.Application.DTOs.Bookings;

namespace CinemaBooking.Application.Services.Interfaces;

/// <summary>
/// Service interface quản lý các thao tác thanh toán và trạng thái booking.
/// Bao gồm: submit thanh toán, approve/reject bởi admin, lấy trạng thái.
/// </summary>
public interface IBookingPaymentService
{
    /// <summary>
    /// Khách bấm "Tôi đã chuyển khoản" - chuyển trạng thái sang AwaitingConfirmation.
    /// Đơn hàng sẽ bị đóng băng, không bị hủy ngầm bởi background worker.
    /// </summary>
    /// <param name="bookingId">ID của booking.</param>
    /// <returns>Thông tin trạng thái booking sau khi submit.</returns>
    Task<BookingStatusDto> SubmitPaymentAsync(int bookingId);

    /// <summary>
    /// Lấy trạng thái hiện tại của booking (dùng cho polling ở trang thanh toán).
    /// </summary>
    /// <param name="bookingId">ID của booking.</param>
    /// <returns>Thông tin trạng thái booking.</returns>
    Task<BookingStatusDto> GetBookingStatusAsync(int bookingId);

    /// <summary>
    /// Admin xác nhận đã nhận tiền - chuyển trạng thái sang Success.
    /// Đồng thời đổi các ghế liên quan sang Booked (vĩnh viễn bán).
    /// </summary>
    /// <param name="bookingId">ID của booking.</param>
    /// <returns>Thông tin booking sau khi approve.</returns>
    Task<BookingStatusDto> ApproveBookingAsync(int bookingId);

    /// <summary>
    /// Admin từ chối / hủy đơn - chuyển trạng thái sang Cancelled.
    /// Đồng thời giải phóng các ghế liên quan về Available.
    /// </summary>
    /// <param name="bookingId">ID của booking.</param>
    /// <param name="reason">Lý do từ chối (tuỳ chọn).</param>
    /// <returns>Thông tin booking sau khi reject.</returns>
    Task<BookingStatusDto> RejectBookingAsync(int bookingId, string? reason = null);

    /// <summary>
    /// Lấy danh sách tất cả booking cho trang admin (phân trang, lọc theo trạng thái).
    /// </summary>
    /// <param name="status">Lọc theo trạng thái (null = tất cả).</param>
    /// <param name="page">Trang hiện tại.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <returns>Danh sách booking kèm phân trang.</returns>
    Task<BookingAdminListDto> GetAllBookingsAsync(int? status = null, int page = 1, int pageSize = 20);
}
