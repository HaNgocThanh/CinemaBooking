using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Application.DTOs.Auth;

/// <summary>
/// DTO for changing user password.
/// Includes validation for old password, new password, and confirmation.
/// </summary>
public class ChangePasswordRequestDto
{
    /// <summary>
    /// Current password (must be correct).
    /// </summary>
    [Required(ErrorMessage = "Mật khẩu cũ không thể trống.")]
    [MinLength(6, ErrorMessage = "Mật khẩu cũ phải có ít nhất 6 ký tự.")]
    public required string OldPassword { get; set; }

    /// <summary>
    /// New password to set (must be different from old password).
    /// </summary>
    [Required(ErrorMessage = "Mật khẩu mới không thể trống.")]
    [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[a-zA-Z\d@$!%*?&]{6,}$",
        ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt (@$!%*?&)."
    )]
    public required string NewPassword { get; set; }

    /// <summary>
    /// Confirmation of new password (must match NewPassword).
    /// </summary>
    [Required(ErrorMessage = "Xác nhận mật khẩu mới không thể trống.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu mới và xác nhận không khớp.")]
    public required string ConfirmNewPassword { get; set; }
}
