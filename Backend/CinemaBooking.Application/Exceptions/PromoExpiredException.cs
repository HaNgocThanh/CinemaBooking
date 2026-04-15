namespace CinemaBooking.Application.Exceptions;

/// <summary>
/// Exception được ném khi mã khuyến mại đã hết hạn hoặc không còn hiệu lực.
/// </summary>
public class PromoExpiredException : ApplicationException
{
    public PromoExpiredException()
        : base("PROMO_EXPIRED", "Mã khuyến mại đã hết hạn. Vui lòng sử dụng mã khác.") { }

    public PromoExpiredException(string promoCode)
        : base("PROMO_EXPIRED", $"Mã khuyến mại '{promoCode}' đã hết hạn.") { }

    public PromoExpiredException(string code, string message)
        : base(code, message) { }
}
