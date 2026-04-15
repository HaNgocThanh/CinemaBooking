namespace CinemaBooking.Application.Exceptions;

/// <summary>
/// Exception được ném khi danh sách ghế cần đặt không hợp lệ.
/// </summary>
public class InvalidSeatsException : ApplicationException
{
    public InvalidSeatsException()
        : base("INVALID_SEATS", "Danh sách ghế không hợp lệ. Vui lòng chọn ít nhất 1 ghế.") { }

    public InvalidSeatsException(string message)
        : base("INVALID_SEATS", message) { }

    public InvalidSeatsException(string code, string message)
        : base(code, message) { }
}
