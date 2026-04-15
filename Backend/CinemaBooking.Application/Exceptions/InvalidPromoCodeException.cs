namespace CinemaBooking.Application.Exceptions;

/// <summary>
/// Exception được ném khi mã khuyến mại không hợp lệ hoặc không tồn tại.
/// </summary>
public class InvalidPromoCodeException : ApplicationException
{
    public InvalidPromoCodeException()
        : base("INVALID_PROMO_CODE", "Mã khuyến mại không hợp lệ. Vui lòng kiểm tra lại.") { }

    public InvalidPromoCodeException(string promoCode)
        : base("INVALID_PROMO_CODE", $"Mã khuyến mại '{promoCode}' không tồn tại.") { }

    public InvalidPromoCodeException(string code, string message)
        : base(code, message) { }
}
