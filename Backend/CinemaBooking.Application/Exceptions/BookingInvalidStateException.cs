using CinemaBooking.Domain.Enums;

namespace CinemaBooking.Application.Exceptions;

/// <summary>
/// Exception khi booking không ở trạng thái hợp lệ để thực hiện thao tác.
/// </summary>
public class BookingInvalidStateException : ApplicationException
{
    public BookingInvalidStateException(int bookingId, BookingStatus currentStatus, string message)
        : base("BOOKING_INVALID_STATE", $"{message} (BookingId: {bookingId}, CurrentStatus: {currentStatus})")
    {
    }
}
