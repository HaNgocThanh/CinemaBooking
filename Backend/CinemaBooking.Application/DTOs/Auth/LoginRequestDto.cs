namespace CinemaBooking.Application.DTOs.Auth;

/// <summary>
/// DTO cho request đăng nhập.
/// </summary>
public class LoginRequestDto
{
    /// <summary>Email của người dùng.</summary>
    public required string Email { get; set; }

    /// <summary>Mật khẩu của người dùng (sẽ được so sánh với password hash).</summary>
    public required string Password { get; set; }
}
