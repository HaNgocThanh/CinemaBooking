using CinemaBooking.Application.DTOs.Auth;
using CinemaBooking.API.Responses;
using CinemaBooking.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
    /// Đăng nhập với email và password.
    /// </summary>
    /// <param name="request">LoginRequestDto chứa email và password.</param>
    /// <returns>AuthResponseDto chứa JWT Token nếu thành công.</returns>
    /// <response code="200">Đăng nhập thành công, trả về JWT Token.</response>
    /// <response code="400">Email hoặc mật khẩu không chính xác, hoặc tài khoản bị khóa.</response>
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
            // Gọi AuthService để xác thực
            var result = await _authService.LoginAsync(request.Email, request.Password);

            if (result == null)
            {
                return BadRequest(new ApiErrorResponse(
                    "INVALID_CREDENTIALS",
                    "Email hoặc mật khẩu không chính xác.",
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
}
