namespace CinemaBooking.Domain.Exceptions;

/// <summary>
/// Exception được ném khi cố gắng khóa ghế nhưng ghế đã bị khóa bởi user khác.
/// Xảy ra khi gặp lỗi ORA-00054 (Resource busy) từ Oracle Database.
/// </summary>
public class SeatAlreadyLockedException : DomainException
{
    public SeatAlreadyLockedException() 
        : base("SEAT_ALREADY_LOCKED", "Ghế này đã bị khóa bởi người dùng khác. Vui lòng chọn ghế khác.") { }

    public SeatAlreadyLockedException(string code, string message) 
        : base(code, message) { }

    public SeatAlreadyLockedException(string code, string message, Exception innerException) 
        : base(code, message, innerException) { }
}
