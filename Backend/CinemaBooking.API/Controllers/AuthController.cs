using CinemaBooking.Application.DTOs.Auth;
using CinemaBooking.Application.DTOs.Common;
using CinemaBooking.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CinemaBooking.API.Controllers;

/// <summary>
/// API Controller cho xác thực (Authentication).
/// Cung cấp endpoints: Login, Register.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Đăng nhập bằng Username hoặc Email kèm Password.
    /// </summary>
    /// <param name="request">LoginRequestDto chứa UsernameOrEmail và Password.</param>
    /// <returns>AuthResponseDto chứa JWT Token nếu thành công.</returns>
    /// <response code="200">Đăng nhập thành công, trả về JWT Token.</response>
    /// <response code="400">Tên đăng nhập/Email hoặc mật khẩu không chính xác, hoặc tài khoản bị khóa.</response>
    /// <response code="422">Request không hợp lệ.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiSuccessResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        // Validate input
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiErrorResponse(
                "VALIDATION_ERROR",
                "Request không hợp lệ.",
                400,
                null,
                HttpContext.TraceIdentifier
            ));
        }

        try
        {
            // Gọi AuthService để xác thực bằng Username hoặc Email
            var result = await _authService.LoginAsync(request.UsernameOrEmail, request.Password);

            if (result == null)
            {
                return BadRequest(new ApiErrorResponse(
                    "INVALID_CREDENTIALS",
                    "Tên đăng nhập/Email hoặc mật khẩu không chính xác.",
                    400,
                    null,
                    HttpContext.TraceIdentifier
                ));
            }

            // Return success response
            return Ok(new ApiSuccessResponse<AuthResponseDto>(
                result,
                $"Đăng nhập thành công. Xin chào {result.FullName}!",
                HttpContext.TraceIdentifier
            ));
        }
        catch (Exception)
        {
            return BadRequest(new ApiErrorResponse(
                "LOGIN_ERROR",
                "Có lỗi xảy ra khi đăng nhập.",
                400,
                null,
                HttpContext.TraceIdentifier
            ));
        }
    }

    /// <summary>
    /// Đăng ký tài khoản mới.
    /// </summary>
    /// <param name="request">RegisterRequestDto chứa email, password, fullName, phoneNumber.</param>
    /// <returns>AuthResponseDto chứa JWT Token nếu thành công.</returns>
    /// <response code="201">Đăng ký thành công, trả về JWT Token.</response>
    /// <response code="400">Email đã tồn tại hoặc mật khẩu không hợp lệ.</response>
    /// <response code="422">Request không hợp lệ.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiSuccessResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        // Validate input
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiErrorResponse(
                "VALIDATION_ERROR",
                "Request không hợp lệ.",
                400,
                null,
                HttpContext.TraceIdentifier
            ));
        }

        try
        {
            // Gọi AuthService để đăng ký
            var result = await _authService.RegisterAsync(request);

            return CreatedAtAction(
                nameof(Register),
                new ApiSuccessResponse<AuthResponseDto>(
                    result,
                    $"Đăng ký thành công. Xin chào {result.FullName}!",
                    HttpContext.TraceIdentifier
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiErrorResponse(
                "REGISTER_ERROR",
                ex.Message,
                400,
                null,
                HttpContext.TraceIdentifier
            ));
        }
        catch (Exception)
        {
            return BadRequest(new ApiErrorResponse(
                "REGISTER_ERROR",
                "Có lỗi xảy ra khi đăng ký.",
                400,
                null,
                HttpContext.TraceIdentifier
            ));
        }
    }

    /// <summary>
    /// Đổi mật khẩu cho người dùng đã đăng nhập.
    /// ✅ SECURITY: Yêu cầu JWT Token hợp lệ, không cho phép IDOR (Insecure Direct Object Reference).
    /// 
    /// Quy trình:
    /// - Lấy UserId từ JWT Token (KHÔNG từ request body để chống IDOR).
    /// - Xác thực mật khẩu cũ.
    /// - Kiểm tra mật khẩu mới == xác nhận.
    /// - Hash mật khẩu mới và lưu vào database.
    /// </summary>
    /// <param name="request">ChangePasswordRequestDto chứa OldPassword, NewPassword, ConfirmNewPassword.</param>
    /// <returns>Message thành công nếu đổi mật khẩu thành công.</returns>
    /// <response code="200">Đổi mật khẩu thành công.</response>
    /// <response code="400">Mật khẩu cũ sai hoặc mật khẩu mới không hợp lệ.</response>
    /// <response code="401">Không có quyền truy cập (thiếu JWT Token).</response>
    /// <response code="422">Request không hợp lệ.</response>
    [HttpPut("change-password")]
    [Authorize]  // ✅ SECURITY: Yêu cầu JWT Token
    [ProducesResponseType(typeof(ApiSuccessResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        // Validate input
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiErrorResponse(
                "VALIDATION_ERROR",
                "Request không hợp lệ.",
                400,
                null,
                HttpContext.TraceIdentifier
            ));
        }

        try
        {
            // ✅ SECURITY: Lấy UserId từ JWT Token (KHÔNG từ request body)
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse(
                    "UNAUTHORIZED",
                    "Không thể xác định người dùng từ JWT Token.",
                    401,
                    null,
                    HttpContext.TraceIdentifier
                ));
            }

            // Gọi AuthService để đổi mật khẩu
            var result = await _authService.ChangePasswordAsync(userId, request);

            if (result)
            {
                return Ok(new ApiSuccessResponse<string>(
                    "Mật khẩu đã được thay đổi thành công.",
                    "Đổi mật khẩu thành công. Vui lòng sử dụng mật khẩu mới để đăng nhập lần sau.",
                    HttpContext.TraceIdentifier
                ));
            }

            return BadRequest(new ApiErrorResponse(
                "CHANGE_PASSWORD_ERROR",
                "Không thể thay đổi mật khẩu.",
                400,
                null,
                HttpContext.TraceIdentifier
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiErrorResponse(
                "CHANGE_PASSWORD_ERROR",
                ex.Message,
                400,
                null,
                HttpContext.TraceIdentifier
            ));
        }
        catch (Exception)
        {
            return BadRequest(new ApiErrorResponse(
                "CHANGE_PASSWORD_ERROR",
                "Có lỗi xảy ra khi đổi mật khẩu.",
                400,
                null,
                HttpContext.TraceIdentifier
            ));
        }
    }

    /// <summary>
    /// ✅ Helper Method: Lấy UserId từ JWT Claims.
    /// 
    /// Dữ liệu JWT Claims có sẵn trong User.Claims:
    /// - "UserId" - Mã định danh user
    /// - ClaimTypes.NameIdentifier - User ID (fallback)
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

        return 0;  // Return 0 nếu không tìm thấy
    }

    /// <summary>
    /// ✅ Helper Method: Lấy Email từ JWT Claims.
    /// </summary>
    private string GetCurrentUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("Email")?.Value ?? "unknown";
    }

    /// <summary>
    /// ✅ Helper Method: Lấy Username từ JWT Claims.
    /// </summary>
    private string GetCurrentUsername()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("Username")?.Value ?? "unknown";
    }
}
