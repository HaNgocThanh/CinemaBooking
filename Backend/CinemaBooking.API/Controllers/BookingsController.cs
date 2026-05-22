using CinemaBooking.Application.DTOs.Common;
using CinemaBooking.Application.DTOs.Bookings;
using CinemaBooking.Application.Services.Interfaces;
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
    private readonly IBookingPaymentService _bookingPaymentService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(
        IBookingService bookingService,
        IBookingPaymentService bookingPaymentService,
        ILogger<BookingsController> logger)
    {
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        _bookingPaymentService = bookingPaymentService ?? throw new ArgumentNullException(nameof(bookingPaymentService));
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
        // BƯỚC 0: Gán CustomerId từ JWT claims (nếu chưa có)
        // ============================================
        if (request.CustomerId == null || request.CustomerId == 0)
        {
            var jwtUserId = GetCurrentUserId();
            if (jwtUserId > 0)
            {
                request.CustomerId = jwtUserId;
                _logger.LogDebug("CreateBooking: auto-assigned CustomerId={CustomerId} from JWT", jwtUserId);
            }
        }

        // ============================================
        // BƯỚC 1: Log incoming request
        // ============================================
        _logger.LogInformation(
            "Creating booking request. ShowtimeId: {ShowtimeId}, SeatCount: {SeatCount}, PromoCode: {PromoCode}, CustomerId: {CustomerId}",
            request.ShowtimeId,
            request.SeatIds?.Count ?? 0,
            request.PromoCode ?? "none",
            request.CustomerId ?? 0
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

    /// <summary>
    /// Khách bấm "Tôi đã chuyển khoản" - chuyển trạng thái sang AwaitingConfirmation.
    /// Đơn hàng sẽ bị đóng băng, không bị hủy ngầm bởi background worker.
    /// </summary>
    /// <param name="id">Booking ID.</param>
    /// <returns>Thông tin trạng thái booking sau khi submit.</returns>
    [HttpPut("{id}/submit")]
    [ProducesResponseType(typeof(ApiSuccessResponse<BookingStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiSuccessResponse<BookingStatusDto>>> SubmitPayment(int id)
    {
        _logger.LogInformation("SubmitPayment called for BookingId: {BookingId}", id);

        var result = await _bookingPaymentService.SubmitPaymentAsync(id);

        _logger.LogInformation(
            "Payment submitted for BookingId: {BookingId}, Status: {Status}",
            id, result.Status);

        return Ok(new ApiSuccessResponse<BookingStatusDto>(
            result,
            "Đã gửi yêu cầu xác nhận thanh toán. Vui lòng chờ Admin duyệt.",
            HttpContext.TraceIdentifier
        ));
    }

    /// <summary>
    /// Lấy trạng thái hiện tại của booking (dùng cho polling ở trang thanh toán).
    /// </summary>
    /// <param name="id">Booking ID.</param>
    /// <returns>Thông tin trạng thái booking.</returns>
    [HttpGet("{id}/status")]
    [ProducesResponseType(typeof(ApiSuccessResponse<BookingStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiSuccessResponse<BookingStatusDto>>> GetBookingStatus(int id)
    {
        var result = await _bookingPaymentService.GetBookingStatusAsync(id);
        return Ok(new ApiSuccessResponse<BookingStatusDto>(result, string.Empty, HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Lấy lịch sử đặt vé của khách hàng đang đăng nhập.
    /// Chỉ trả về các đơn đã hoàn tất (Success) hoặc đã hủy (Cancelled/Expired).
    /// KHÔNG bao gồm các đơn đang chờ thanh toán (Pending) hoặc chờ xác nhận (AwaitingConfirmation).
    /// </summary>
    [HttpGet("my-history")]
    [ProducesResponseType(typeof(ApiSuccessResponse<BookingHistoryListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiSuccessResponse<BookingHistoryListDto>>> GetMyHistory()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("GetMyHistory called for UserId: {UserId} (from JWT)", userId);

        var result = await _bookingPaymentService.GetMyHistoryAsync(userId);

        _logger.LogInformation(
            "MyHistory retrieved for UserId: {UserId}. Total bookings: {Count}",
            userId, result.TotalCount);

        return Ok(new ApiSuccessResponse<BookingHistoryListDto>(
            result,
            $"Tìm thấy {result.TotalCount} đơn đặt vé.",
            HttpContext.TraceIdentifier
        ));
    }

    /// <summary>
    /// Lấy TẤT CẢ booking của khách hàng đang đăng nhập (bao gồm cả Pending/AwaitingConfirmation).
    /// Dùng khi khách hàng muốn xem các đơn vé chưa hoàn tất (chờ thanh toán, chờ xác nhận).
    /// </summary>
    [HttpGet("my-all-bookings")]
    [ProducesResponseType(typeof(ApiSuccessResponse<BookingHistoryListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiSuccessResponse<BookingHistoryListDto>>> GetAllMyBookings()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("GetAllMyBookings called for UserId: {UserId} (from JWT)", userId);

        var result = await _bookingPaymentService.GetAllMyBookingsAsync(userId);

        _logger.LogInformation(
            "AllMyBookings retrieved for UserId: {UserId}. Total bookings: {Count}",
            userId, result.TotalCount);

        return Ok(new ApiSuccessResponse<BookingHistoryListDto>(
            result,
            $"Tìm thấy {result.TotalCount} đơn đặt vé.",
            HttpContext.TraceIdentifier
        ));
    }

    /// <summary>
    /// Lấy chi tiết vé điện tử (E-Ticket) cho khách hàng.
    /// Chỉ hoạt động khi booking đã được thanh toán thành công (Status = Success).
    /// </summary>
    /// <param name="id">Booking ID.</param>
    /// <returns>Thông tin E-Ticket gồm phim, ghế, thời gian và QR code.</returns>
    [HttpGet("{id}/ticket")]
    [ProducesResponseType(typeof(ApiSuccessResponse<ETicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiSuccessResponse<ETicketDto>>> GetETicket(int id)
    {
        var userId = GetCurrentUserId();

        _logger.LogInformation(
            "GetETicket called. BookingId: {BookingId}, UserId: {UserId}",
            id, userId);

        var result = await _bookingPaymentService.GetETicketAsync(id, userId);

        _logger.LogInformation(
            "ETicket retrieved for BookingId: {BookingId}. Tickets: {TicketCount}",
            id, result.TotalTickets);

        return Ok(new ApiSuccessResponse<ETicketDto>(
            result,
            "Lấy thông tin vé thành công.",
            HttpContext.TraceIdentifier
        ));
    }
}

/// <summary>
/// Controller quản lý các endpoints admin liên quan đến booking.
/// </summary>
[ApiController]
[Route("api/admin/bookings")]
[Authorize(Roles = "Admin")]
public class AdminBookingsController : ControllerBase
{
    private readonly IBookingPaymentService _bookingPaymentService;
    private readonly ILogger<AdminBookingsController> _logger;

    public AdminBookingsController(
        IBookingPaymentService bookingPaymentService,
        ILogger<AdminBookingsController> logger)
    {
        _bookingPaymentService = bookingPaymentService ?? throw new ArgumentNullException(nameof(bookingPaymentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lấy danh sách tất cả booking cho trang admin (phân trang, lọc theo trạng thái).
    /// </summary>
    /// <param name="status">Lọc theo trạng thái (0=Pending, 1=AwaitingConfirmation, 2=Success, 3=Cancelled, 4=Expired).</param>
    /// <param name="page">Trang hiện tại.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <returns>Danh sách booking kèm phân trang.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResponse<BookingAdminListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiSuccessResponse<BookingAdminListDto>>> GetAllBookings(
        [FromQuery] int? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation(
            "GetAllBookings called. Status: {Status}, Page: {Page}, PageSize: {PageSize}",
            status, page, pageSize);

        var result = await _bookingPaymentService.GetAllBookingsAsync(status, page, pageSize);

        return Ok(new ApiSuccessResponse<BookingAdminListDto>(
            result,
            $"Tìm thấy {result.TotalCount} đơn đặt vé.",
            HttpContext.TraceIdentifier
        ));
    }

    /// <summary>
    /// Admin xác nhận đã nhận tiền - chuyển trạng thái sang Success.
    /// Đồng thời đổi các ghế liên quan sang Booked (bán vĩnh viễn).
    /// </summary>
    /// <param name="id">Booking ID.</param>
    /// <returns>Thông tin booking sau khi approve.</returns>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(ApiSuccessResponse<BookingStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiSuccessResponse<BookingStatusDto>>> ApproveBooking(int id)
    {
        _logger.LogInformation("ApproveBooking called for BookingId: {BookingId}", id);

        var adminUsername = User.FindFirst(ClaimTypes.Name)?.Value
            ?? User.FindFirst("Username")?.Value
            ?? "Admin";

        _logger.LogInformation(
            "Admin {AdminUsername} approving booking {BookingId}",
            adminUsername, id);

        var result = await _bookingPaymentService.ApproveBookingAsync(id);

        _logger.LogInformation(
            "Booking {BookingId} approved successfully by {AdminUsername}. Status: {Status}",
            id, adminUsername, result.Status);

        return Ok(new ApiSuccessResponse<BookingStatusDto>(
            result,
            "Xác nhận thanh toán thành công. Đơn đặt vé đã được xác nhận.",
            HttpContext.TraceIdentifier
        ));
    }

    /// <summary>
    /// Admin từ chối / hủy đơn - chuyển trạng thái sang Cancelled.
    /// Đồng thời giải phóng các ghế liên quan về Available.
    /// </summary>
    /// <param name="id">Booking ID.</param>
    /// <param name="reason">Lý do từ chối (tuỳ chọn).</param>
    /// <returns>Thông tin booking sau khi reject.</returns>
    [HttpPost("{id}/reject")]
    [ProducesResponseType(typeof(ApiSuccessResponse<BookingStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiSuccessResponse<BookingStatusDto>>> RejectBooking(
        int id,
        [FromBody] RejectBookingRequest? request = null)
    {
        _logger.LogInformation("RejectBooking called for BookingId: {BookingId}, Reason: {Reason}",
            id, request?.Reason ?? "Không có lý do");

        var adminUsername = User.FindFirst(ClaimTypes.Name)?.Value
            ?? User.FindFirst("Username")?.Value
            ?? "Admin";

        _logger.LogInformation(
            "Admin {AdminUsername} rejecting booking {BookingId}",
            adminUsername, id);

        var result = await _bookingPaymentService.RejectBookingAsync(id, request?.Reason);

        _logger.LogInformation(
            "Booking {BookingId} rejected by {AdminUsername}. Status: {Status}",
            id, adminUsername, result.Status);

        return Ok(new ApiSuccessResponse<BookingStatusDto>(
            result,
            "Đơn đặt vé đã bị từ chối. Các ghế đã được giải phóng.",
            HttpContext.TraceIdentifier
        ));
    }
}

/// <summary>
/// Request DTO cho việc từ chối booking.
/// </summary>
public class RejectBookingRequest
{
    public string? Reason { get; set; }
}
