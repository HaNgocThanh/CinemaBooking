namespace CinemaBooking.Application.Helpers;

/// <summary>
/// Helper class để generate mã unique cho booking và ticket.
/// </summary>
public static class CodeGenerator
{
    /// <summary>
    /// Tạo mã booking duy nhất.
    /// Format: "BK" + YYYYMMDDHHmmss + random 9 chữ số.
    /// Ví dụ: "BK20260416160000000123456789"
    /// </summary>
    public static string GenerateBookingCode()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var randomPart = Random.Shared.Next(100000000, 999999999);
        return $"BK{timestamp}{randomPart}";
    }

    /// <summary>
    /// Tạo mã vé duy nhất.
    /// Format: "TK" + YYYYMMDDHHmmss + random 9 chữ số.
    /// Ví dụ: "TK20260416160000000123456789"
    /// </summary>
    public static string GenerateTicketCode()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var randomPart = Random.Shared.Next(100000000, 999999999);
        return $"TK{timestamp}{randomPart}";
    }
}
