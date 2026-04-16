namespace CinemaBooking.Application.DTOs.Auth;

/// <summary>
/// DTO cho request đăng nhập.
/// Hỗ trợ đăng nhập bằng Email HOẶC Username.
/// </summary>
public class LoginRequestDto
{
    /// <summary>Tên đăng nhập hoặc Email của người dùng.</summary>
    public required string UsernameOrEmail { get; set; }

    /// <summary>Mật khẩu của người dùng (sẽ được so sánh với password hash).</summary>
    public required string Password { get; set; }
}
