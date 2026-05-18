namespace CinemaBooking.Domain.Exceptions;

/// <summary>
/// Exception thrown when a booking violates the VIP single seat gap rule:
/// a VIP seat is left empty while flanked by booked/selected seats on both sides.
/// </summary>
public class VipSingleGapException : DomainException
{
    public VipSingleGapException()
        : base("VIP_SEAT_GAP_VIOLATION",
               "Khu vực VIP không được để lại một ghế trống đơn lẻ ở giữa hai ghế đã chọn hoặc đã bán.") { }

    public VipSingleGapException(string code, string message)
        : base(code, message) { }

    public VipSingleGapException(string code, string message, Exception innerException)
        : base(code, message, innerException) { }
}
