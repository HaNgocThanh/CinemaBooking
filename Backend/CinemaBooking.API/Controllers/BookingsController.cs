using CinemaBooking.API.Responses;
using CinemaBooking.Application.DTOs.Bookings;
using CinemaBooking.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers;

/// <summary>
/// Controller quản lý các endpoints liên quan đến booking (đặt vé).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
    {
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Tạo đơn đặt vé mới.
    /// 
    /// Quy trình:
    /// - Validate request
    /// - Gọi IBookingService.CreateBookingAsync()
    /// - Nếu thành công: trả về 201 Created kèm booking info
    /// - Nếu lỗi: Middleware sẽ bắt exception và trả về error response
    /// 
    /// Không dùng try-catch - Middleware xử lý toàn cục.
    /// </summary>
    /// <param name="request">Request DTO chứa thông tin booking.</param>
    /// <returns>
    /// 201 Created - Booking thành công
    /// 400 Bad Request - Invalid request hoặc lỗi business logic
    /// 409 Conflict - Ghế bị khóa bởi user khác
    /// 500 Server Error - Lỗi unexpected
    /// </returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiSuccessResponse<BookingResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiSuccessResponse<BookingResponseDto>>> CreateBooking(
        [FromBody] BookingRequestDto request)
    {
        // ============================================
        // BƯỚC 1: Log incoming request
        // ============================================
        _logger.LogInformation(
            "Creating booking request. ShowtimeId: {ShowtimeId}, SeatCount: {SeatCount}, PromoCode: {PromoCode}",
            request.ShowtimeId,
            request.SeatIds?.Count ?? 0,
            request.PromoCode ?? "none"
        );

        // ============================================
        // BƯỚC 2: Gọi Service
        // (Exception từ Service sẽ được Middleware bắt)
        // ============================================
        var result = await _bookingService.CreateBookingAsync(request);

        // ============================================
        // BƯỚC 3: Log success
        // ============================================
        _logger.LogInformation(
            "Booking created successfully. BookingCode: {BookingCode}, BookingId: {BookingId}",
            result.BookingCode,
            result.BookingId
        );

        // ============================================
        // BƯỚC 4: Return 201 Created response
        // ============================================
        var response = new ApiSuccessResponse<BookingResponseDto>(
            result,
            "Đặt vé thành công. Vui lòng hoàn tất thanh toán trong 5 phút.",
            HttpContext.TraceIdentifier
        );

        return CreatedAtAction(
            actionName: nameof(CreateBooking),
            routeValues: new { id = result.BookingId },
            value: response
        );
    }
}
