using CinemaBooking.Domain.Enums;

namespace CinemaBooking.Domain.Entities;

/// <summary>
/// Thực thể User (Khách hàng/Admin) cho hệ thống đặt vé.
/// </summary>
public class User
{
    /// <summary>Định danh duy nhất của người dùng.</summary>
    public int Id { get; set; }

    /// <summary>Tên đăng nhập duy nhất (tối đa 50 ký tự), yêu cầu alphanumeric và underscore.</summary>
    public required string Username { get; set; }

    /// <summary>Email duy nhất, sử dụng làm tên đăng nhập thay thế (tối đa 255 ký tự).</summary>
    public required string Email { get; set; }

    /// <summary>Tên đầy đủ của người dùng (tối đa 200 ký tự).</summary>
    public required string FullName { get; set; }

    /// <summary>Mã hash mật khẩu (được mã hóa bằng BCrypt hoặc HMACSHA256, tối đa 255 ký tự).</summary>
    public required string PasswordHash { get; set; }

    /// <summary>Số điện thoại (tuỳ chọn, tối đa 20 ký tự).</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Vai trò của người dùng (Admin hoặc Customer).</summary>
    public UserRole Role { get; set; } = UserRole.Customer;

    /// <summary>Cho biết người dùng đã xác nhận email hay chưa.</summary>
    public bool IsEmailConfirmed { get; set; } = false;

    /// <summary>Cho biết tài khoản đã bị khoá hay chưa.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Thời gian tạo tài khoản.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Thời gian cập nhật tài khoản lần cuối.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Thời gian đăng nhập cuối cùng (tuỳ chọn).</summary>
    public DateTime? LastLogin { get; set; }

    // ============ Navigation Properties ============
    /// <summary>Danh sách đơn đặt vé của người dùng.</summary>
    public ICollection<Booking>? Bookings { get; set; }
}
