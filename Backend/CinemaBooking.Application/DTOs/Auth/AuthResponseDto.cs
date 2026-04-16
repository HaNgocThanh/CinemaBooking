namespace CinemaBooking.Application.DTOs.Auth;

/// <summary>
/// DTO cho response xác thực (login/register).
/// Chứa JWT Token và thông tin người dùng.
/// </summary>
public class AuthResponseDto
{
    /// <summary>Mã định danh duy nhất của người dùng.</summary>
    public int UserId { get; set; }

    /// <summary>Tên đăng nhập của người dùng.</summary>
    public required string Username { get; set; }

    /// <summary>Email của người dùng.</summary>
    public required string Email { get; set; }

    /// <summary>Tên đầy đủ của người dùng.</summary>
    public required string FullName { get; set; }

    /// <summary>Vai trò của người dùng (Admin/Customer).</summary>
    public required string Role { get; set; }

    /// <summary>JWT Token để dùng cho các request tiếp theo.</summary>
    public required string Token { get; set; }

    /// <summary>Thời gian hết hạn của Token (Unix timestamp).</summary>
    public long ExpiresAt { get; set; }

    /// <summary>Loại token (Bearer).</summary>
    public string TokenType { get; set; } = "Bearer";
}
