using CinemaBooking.API.Responses;
using CinemaBooking.Application.DTOs.Bookings;
using CinemaBooking.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CinemaBooking.API.Controllers;

/// <summary>
/// Controller quản lý các endpoints liên quan đến booking (đặt vé).
/// ✅ Tất cả endpoints được bảo vệ bằng [Authorize] - yêu cầu JWT Token hợp lệ.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]  // ✅ Yêu cầu JWT Token cho tất cả endpoints
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

    /// <summary>
    /// ✅ Helper Method: Lấy UserId từ JWT Claims.
    /// 
    /// Cách sử dụng:
    /// ```csharp
    /// var userId = GetCurrentUserId();
    /// request.CustomerId = userId;
    /// ```
    /// 
    /// Dữ liệu JWT Claims có sẵn trong User.Claims:
    /// - "UserId" - Mã định danh user
    /// - "Email" - Email từ JWT Token
    /// - "Role" - Vai trò (Admin/Customer)
    /// - ClaimTypes.NameIdentifier - User ID (từ JWT)
    /// </summary>
    /// <returns>UserId (int) hoặc 0 nếu không tìm thấy.</returns>
    private int GetCurrentUserId()
    {
        // Cách 1: Lấy từ custom claim "UserId"
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        // Cách 2: Lấy từ standard claim NameIdentifier (fallback)
        var nameIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (nameIdClaim != null && int.TryParse(nameIdClaim.Value, out var nameId))
        {
            return nameId;
        }

        return 0;  // Return 0 nếu không tìm thấy (không nên xảy ra nếu [Authorize] hoạt động)
    }

    /// <summary>
    /// Helper Method: Lấy Email từ JWT Claims.
    /// </summary>
    private string GetCurrentUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("Email")?.Value ?? "unknown";
    }

    /// <summary>
    /// Helper Method: Lấy Role từ JWT Claims.
    /// </summary>
    private string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("Role")?.Value ?? "Customer";
    }

    /// <summary>
    /// Helper Method: Lấy Username từ JWT Claims.
    /// </summary>
    private string GetCurrentUsername()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("Username")?.Value ?? "unknown";
    }
}
