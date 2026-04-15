namespace CinemaBooking.Domain.Interfaces;

/// <summary>
/// Repository interface cho thực thể Promotion (Mã khuyến mại).
/// </summary>
public interface IPromotionRepository
{
    /// <summary>
    /// Lấy thông tin khuyến mại theo mã promo.
    /// </summary>
    /// <param name="promoCode">Mã khuyến mại (case-insensitive).</param>
    /// <returns>Thông tin promotion nếu tồn tại, null nếu không tìm thấy.</returns>
    Task<PromotionInfo?> GetByCodeAsync(string promoCode);

    /// <summary>
    /// Kiểm tra xem mã promo còn hợp lệ không (chưa hết hạn và còn số lượng).
    /// </summary>
    /// <param name="promoCode">Mã khuyến mại.</param>
    /// <returns>true nếu còn hợp lệ, false nếu hết hạn hoặc hết số lượng.</returns>
    Task<bool> IsValidAsync(string promoCode);

    /// <summary>
    /// Kiểm tra xem mã promo đã hết hạn chưa.
    /// </summary>
    /// <param name="promoCode">Mã khuyến mại.</param>
    /// <returns>true nếu đã hết hạn, false nếu còn hiệu lực.</returns>
    Task<bool> IsExpiredAsync(string promoCode);
}

/// <summary>
/// DTO chứa thông tin khuyến mại từ database.
/// </summary>
public class PromotionInfo
{
    /// <summary>Mã khuyến mại.</summary>
    public required string Code { get; set; }

    /// <summary>Loại giảm giá: "PERCENTAGE" (%) hoặc "FIXED" (VND).</summary>
    public required string DiscountType { get; set; }

    /// <summary>Giá trị giảm giá (% nếu PERCENTAGE, VND nếu FIXED).</summary>
    public required decimal DiscountValue { get; set; }

    /// <summary>Giá trị tối đa giảm giá (nếu là PERCENTAGE).</summary>
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>Số lượng mã còn lại (null = không giới hạn).</summary>
    public int? RemainingQuantity { get; set; }

    /// <summary>Ngày bắt đầu áp dụng (UTC).</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Ngày hết hạn (UTC).</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Kiểm tra xem promotion này còn hợp lệ không.
    /// </summary>
    public bool IsValid()
    {
        var now = DateTime.UtcNow;

        // Kiểm tra ngày bắt đầu
        if (StartDate.HasValue && now < StartDate)
            return false;

        // Kiểm tra ngày hết hạn
        if (EndDate.HasValue && now > EndDate)
            return false;

        // Kiểm tra số lượng còn lại
        if (RemainingQuantity.HasValue && RemainingQuantity <= 0)
            return false;

        return true;
    }

    /// <summary>
    /// Tính toán số tiền giảm giá dựa trên giá trị gốc.
    /// </summary>
    public decimal CalculateDiscount(decimal originalAmount)
    {
        if (DiscountType == "PERCENTAGE")
        {
            var discount = (originalAmount * DiscountValue) / 100;
            return MaxDiscountAmount.HasValue ? Math.Min(discount, MaxDiscountAmount.Value) : discount;
        }

        if (DiscountType == "FIXED")
        {
            return Math.Min(DiscountValue, originalAmount);
        }

        return 0;
    }
}
