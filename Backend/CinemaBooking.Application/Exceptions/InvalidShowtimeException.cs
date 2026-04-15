namespace CinemaBooking.Application.Exceptions;

/// <summary>
/// Exception được ném khi showtime không hợp lệ hoặc không tồn tại.
/// </summary>
public class InvalidShowtimeException : ApplicationException
{
    public InvalidShowtimeException()
        : base("INVALID_SHOWTIME", "Suất chiếu không hợp lệ hoặc không tồn tại.") { }

    public InvalidShowtimeException(int showtimeId)
        : base("INVALID_SHOWTIME", $"Suất chiếu với ID {showtimeId} không tồn tại.") { }

    public InvalidShowtimeException(string code, string message)
        : base(code, message) { }
}
