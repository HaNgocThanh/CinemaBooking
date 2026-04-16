using CinemaBooking.Application.DTOs.Auth;

namespace CinemaBooking.Application.Services.Interfaces;

/// <summary>
/// Interface cho Authentication Service.
/// Xử lý logic đăng nhập, đăng ký, và sinh JWT Token.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Xác thực người dùng bằng email và password.
    /// </summary>
    /// <param name="email">Email của người dùng.</param>
    /// <param name="password">Mật khẩu (plaintext, sẽ được so sánh với hash).</param>
    /// <returns>AuthResponseDto chứa JWT Token nếu thành công, null nếu thất bại.</returns>
    Task<AuthResponseDto?> LoginAsync(string email, string password);

    /// <summary>
    /// Đăng ký tài khoản mới cho người dùng.
    /// </summary>
    /// <param name="request">RegisterRequestDto chứa thông tin đăng ký.</param>
    /// <returns>AuthResponseDto chứa JWT Token nếu thành công.</returns>
    /// <exception cref="InvalidOperationException">Khi email đã tồn tại hoặc password không hợp lệ.</exception>
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);

    /// <summary>
    /// Sinh JWT Token cho người dùng.
    /// </summary>
    /// <param name="userId">ID của người dùng.</param>
    /// <param name="username">Tên đăng nhập của người dùng (để làm claim).</param>
    /// <param name="email">Email của người dùng (để làm claim).</param>
    /// <param name="role">Vai trò của người dùng (Admin/Customer).</param>
    /// <returns>JWT Token string.</returns>
    string GenerateJwtToken(int userId, string username, string email, string role);

    /// <summary>
    /// Kiểm tra email đã tồn tại hay chưa.
    /// </summary>
    /// <param name="email">Email cần kiểm tra.</param>
    /// <returns>true nếu email tồn tại, false nếu chưa.</returns>
    Task<bool> EmailExistsAsync(string email);
}
