using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho thực thể Promotion (Mã khuyến mại).
/// 
/// MOCK IMPLEMENTATION - Trong production cần:
/// - Tạo Promotion entity trong Domain
/// - Tạo Promotions DbSet trong ApplicationDbContext
/// - Implement full database query logic
/// </summary>
public class PromotionRepository : IPromotionRepository
{
    private readonly ApplicationDbContext _context;

    public PromotionRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Lấy thông tin khuyến mại theo mã promo.
    /// 
    /// TODO: Implement actual database query
    /// var promotion = await _context.Promotions
    ///     .AsNoTracking()
    ///     .FirstOrDefaultAsync(p => p.Code.ToUpper() == promoCode.ToUpper());
    /// </summary>
    public async Task<PromotionInfo?> GetByCodeAsync(string promoCode)
    {
        if (string.IsNullOrWhiteSpace(promoCode))
            return null;

        // TODO: Replace with actual database query
        // Tạm thời return mock data để test
        if (promoCode.Equals("SUMMER2024", StringComparison.OrdinalIgnoreCase))
        {
            return new PromotionInfo
            {
                Code = "SUMMER2026",
                DiscountType = "PERCENTAGE",
                DiscountValue = 15, // 15% off
                MaxDiscountAmount = 500000, // Max 500k VND
                RemainingQuantity = 100,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(30)
            };
        }

        if (promoCode.Equals("FIRST10", StringComparison.OrdinalIgnoreCase))
        {
            return new PromotionInfo
            {
                Code = "NGOCTHANH",
                DiscountType = "FIXED",
                DiscountValue = 700000, // 700k VND off
                RemainingQuantity = 50,
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow.AddDays(15)
            };
        }

        // EXPIRED promo for testing
        if (promoCode.Equals("EXPIRED123", StringComparison.OrdinalIgnoreCase))
        {
            return new PromotionInfo
            {
                Code = "EXPIRED123",
                DiscountType = "PERCENTAGE",
                DiscountValue = 20,
                EndDate = DateTime.UtcNow.AddDays(-1) // Already expired
            };
        }

        return null;
    }

    /// <summary>
    /// Kiểm tra xem mã promo còn hợp lệ không.
    /// </summary>
    public async Task<bool> IsValidAsync(string promoCode)
    {
        var promo = await GetByCodeAsync(promoCode);
        return promo != null && promo.IsValid();
    }

    /// <summary>
    /// Kiểm tra xem mã promo đã hết hạn chưa.
    /// </summary>
    public async Task<bool> IsExpiredAsync(string promoCode)
    {
        var promo = await GetByCodeAsync(promoCode);
        if (promo == null)
            return true;

        return !promo.IsValid();
    }
}
