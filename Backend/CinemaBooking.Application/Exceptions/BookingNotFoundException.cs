namespace CinemaBooking.Application.Exceptions;

/// <summary>
/// Exception khi booking không tồn tại.
/// </summary>
public class BookingNotFoundException : ApplicationException
{
    public BookingNotFoundException(int bookingId)
        : base("BOOKING_NOT_FOUND", $"Không tìm thấy đơn đặt vé với ID: {bookingId}.")
    {
    }
}
