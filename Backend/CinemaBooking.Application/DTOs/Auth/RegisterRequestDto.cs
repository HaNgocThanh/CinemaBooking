namespace CinemaBooking.Application.DTOs.Auth;

/// <summary>
/// DTO cho request đăng ký tài khoản.
/// </summary>
public class RegisterRequestDto
{
    /// <summary>Tên đăng nhập duy nhất (tối đa 50 ký tự, alphanumeric + underscore).</summary>
    public required string Username { get; set; }

    /// <summary>Email duy nhất cho tài khoản (tối đa 255 ký tự).</summary>
    public required string Email { get; set; }

    /// <summary>Mật khẩu (sẽ được mã hóa bằng BCrypt).</summary>
    public required string Password { get; set; }

    /// <summary>Xác nhận mật khẩu (phải giống Password).</summary>
    public required string ConfirmPassword { get; set; }

    /// <summary>Tên đầy đủ của người dùng (tối đa 200 ký tự).</summary>
    public required string FullName { get; set; }

    /// <summary>Số điện thoại (tuỳ chọn, tối đa 20 ký tự).</summary>
    public string? PhoneNumber { get; set; }
}
