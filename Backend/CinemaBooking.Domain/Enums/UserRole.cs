namespace CinemaBooking.Domain.Enums;

/// <summary>
/// Vai trò của người dùng trong hệ thống.
/// </summary>
public enum UserRole
{
    /// <summary>Khách hàng bình thường - chỉ có thể đặt vé.</summary>
    Customer = 0,

    /// <summary>Quản trị viên - có quyền quản lý phim, suất chiếu, người dùng.</summary>
    Admin = 1
}
