using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using CinemaBooking.Application.DTOs.Auth;
using CinemaBooking.Application.Services.Interfaces;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CinemaBooking.Infrastructure.Services;

/// <summary>
/// Service xử lý Authentication: Login, Register, JWT Token Generation.
/// Mã hóa mật khẩu bằng BCrypt.Net-Next.
/// </summary>
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Đăng nhập: Xác thực bằng Email hoặc Username kèm Password và sinh JWT Token.
    /// </summary>
    /// <param name="usernameOrEmail">Tên đăng nhập hoặc Email của người dùng.</param>
    /// <param name="password">Mật khẩu (plaintext, sẽ được so sánh với hash).</param>
    /// <returns>AuthResponseDto nếu thành công, null nếu thất bại.</returns>
    public async Task<AuthResponseDto?> LoginAsync(string usernameOrEmail, string password)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
            return null;

        // Tìm user theo Username HOẶC Email (case-insensitive for email)
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => 
                u.Username == usernameOrEmail || 
                u.Email == usernameOrEmail.ToLower());

        if (user == null)
            return null;

        // Kiểm tra mật khẩu (so sánh plaintext với hash)
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        // Kiểm tra tài khoản còn active không
        if (!user.IsActive)
            return null;

        // Cập nhật LastLogin
        user.LastLogin = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        // Sinh JWT Token
        var token = GenerateJwtToken(user.Id, user.Username, user.Email, user.Role.ToString());

        // Tính toán ExpiresAt (từ config)
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes).ToUnixTimeSeconds();

        return new AuthResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            Token = token,
            ExpiresAt = expiresAt,
            TokenType = "Bearer"
        };
    }

    /// <summary>
    /// Đăng ký: Tạo user mới với mật khẩu được mã hóa bằng BCrypt.
    /// </summary>
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password) || 
            string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new InvalidOperationException("Username, Email, Password, và FullName không thể trống.");
        }

        // Kiểm tra password matches confirm password
        if (request.Password != request.ConfirmPassword)
        {
            throw new InvalidOperationException("Mật khẩu xác nhận không khớp.");
        }

        // Kiểm tra username đã tồn tại
        var existingUsername = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == request.Username.Trim());

        if (existingUsername != null)
        {
            throw new InvalidOperationException($"Tên đăng nhập '{request.Username}' đã được sử dụng. Vui lòng chọn tên khác.");
        }

        // Kiểm tra email đã tồn tại
        var existingEmail = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email.Trim());

        if (existingEmail != null)
        {
            throw new InvalidOperationException($"Email '{request.Email}' đã được sử dụng.");
        }

        // Validate password strength (tối thiểu 6 ký tự)
        if (request.Password.Length < 6)
        {
            throw new InvalidOperationException("Mật khẩu phải có ít nhất 6 ký tự.");
        }

        // Validate username length (3-50 ký tự)
        if (request.Username.Length < 3 || request.Username.Length > 50)
        {
            throw new InvalidOperationException("Tên đăng nhập phải từ 3 đến 50 ký tự.");
        }

        // Mã hóa mật khẩu bằng BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Tạo user mới
        var user = new User
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim(),
            FullName = request.FullName.Trim(),
            PasswordHash = passwordHash,
            PhoneNumber = request.PhoneNumber?.Trim(),
            Role = UserRole.Customer,  // Mặc định là Customer
            IsEmailConfirmed = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Lưu vào database
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Sinh JWT Token
        var token = GenerateJwtToken(user.Id, user.Username, user.Email, user.Role.ToString());

        // Tính toán ExpiresAt
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes).ToUnixTimeSeconds();

        return new AuthResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            Token = token,
            ExpiresAt = expiresAt,
            TokenType = "Bearer"
        };
    }

    /// <summary>
    /// Sinh JWT Token chứa Claims: UserID, Username, Email, Role.
    /// </summary>
    public string GenerateJwtToken(int userId, string username, string email, string role)
    {
        var jwtSecret = _configuration["Jwt:Secret"];
        var jwtIssuer = _configuration["Jwt:Issuer"];
        var jwtAudience = _configuration["Jwt:Audience"];
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

        if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Length < 32)
        {
            throw new InvalidOperationException("JWT Secret phải có ít nhất 32 ký tự.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Tạo Claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim("UserId", userId.ToString()),      // Custom claim
            new Claim("Username", username),             // Custom claim
            new Claim("Email", email),                   // Custom claim
            new Claim("Role", role)                      // Custom claim
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Kiểm tra email đã tồn tại hay chưa.
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == email);
    }

    /// <summary>
    /// Thay đổi mật khẩu cho người dùng.
    /// Kiểm tra mật khẩu cũ, xác thực mật khẩu mới, và lưu vào database.
    /// </summary>
    /// <param name="userId">ID của người dùng muốn đổi mật khẩu.</param>
    /// <param name="request">ChangePasswordRequestDto chứa mật khẩu cũ, mật khẩu mới, và xác nhận.</param>
    /// <returns>true nếu đổi mật khẩu thành công.</returns>
    /// <exception cref="InvalidOperationException">Khi user không tồn tại, mật khẩu cũ sai, hoặc mật khẩu mới không hợp lệ.</exception>
    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
    {
        // Validate input
        if (request == null || 
            string.IsNullOrWhiteSpace(request.OldPassword) ||
            string.IsNullOrWhiteSpace(request.NewPassword) ||
            string.IsNullOrWhiteSpace(request.ConfirmNewPassword))
        {
            throw new InvalidOperationException("Mật khẩu cũ, mật khẩu mới, và xác nhận không thể trống.");
        }

        // Kiểm tra mật khẩu mới == xác nhận
        if (request.NewPassword != request.ConfirmNewPassword)
        {
            throw new InvalidOperationException("Mật khẩu mới và xác nhận không khớp.");
        }

        // Kiểm tra mật khẩu mới khác với mật khẩu cũ
        if (request.OldPassword == request.NewPassword)
        {
            throw new InvalidOperationException("Mật khẩu mới phải khác với mật khẩu cũ.");
        }

        // Tìm user
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new InvalidOperationException("Người dùng không tồn tại.");
        }

        // Kiểm tra mật khẩu cũ có đúng không (dùng BCrypt.Verify)
        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Mật khẩu cũ không chính xác.");
        }

        // Mã hóa mật khẩu mới
        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        // Cập nhật mật khẩu và UpdatedAt timestamp
        user.PasswordHash = newPasswordHash;
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return true;
    }
}
