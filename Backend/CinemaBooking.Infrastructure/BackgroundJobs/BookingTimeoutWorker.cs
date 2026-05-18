using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CinemaBooking.Infrastructure.BackgroundJobs;

/// <summary>
/// Background service quét và hủy các đơn đặt vé ở trạng thái Pending quá hạn (5 phút).
/// Chạy định kỳ mỗi 1 phút.
/// CRITICAL: Chỉ hủy đơn Pending - không đụng đơn AwaitingConfirmation (đang chờ admin duyệt).
/// </summary>
public class BookingTimeoutWorker : BackgroundService
{
    private const int ScanIntervalSeconds = 60;
    private const int BookingTimeoutMinutes = 5;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingTimeoutWorker> _logger;

    public BookingTimeoutWorker(
        IServiceProvider serviceProvider,
        ILogger<BookingTimeoutWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "BookingTimeoutWorker started. Scan interval: {IntervalSeconds}s, Timeout: {TimeoutMinutes}m",
            ScanIntervalSeconds, BookingTimeoutMinutes);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(ScanIntervalSeconds));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ScanAndCancelExpiredBookingsAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("BookingTimeoutWorker is stopping...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in BookingTimeoutWorker");
            throw;
        }
        finally
        {
            timer?.Dispose();
        }
    }

    private async Task ScanAndCancelExpiredBookingsAsync(CancellationToken cancellationToken)
    {
        List<int> expiredBookingIds;

        try
        {
            var now = DateTime.UtcNow;
            var expiryThreshold = now.AddMinutes(-BookingTimeoutMinutes);

            _logger.LogDebug(
                "Scanning for expired bookings at {Time}. Threshold: {Threshold}",
                now, expiryThreshold);

            // QUÉT: Tạo scope riêng, quét xong đóng ngay — không giữ kết nối
            using (var scanScope = _serviceProvider.CreateAsyncScope())
            {
                var scanContext = scanScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                expiredBookingIds = await scanContext.Bookings
                    .AsNoTracking()
                    .Where(b => b.Status == BookingStatus.Pending && b.CreatedAt < expiryThreshold)
                    .Select(b => b.Id)
                    .ToListAsync(cancellationToken);

                // Context disposed ngay khi ra khỏi using — kết nối trả về pool
            }

            if (expiredBookingIds.Count == 0)
            {
                _logger.LogDebug("No expired bookings found.");
                return;
            }

            _logger.LogWarning(
                "Found {Count} expired bookings to cancel: {BookingIds}",
                expiredBookingIds.Count, string.Join(", ", expiredBookingIds));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Scan task was cancelled");
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error scanning for expired bookings at {Time}. Will retry next cycle.",
                DateTime.UtcNow);
            return;
        }

        // THỰC THI: Mỗi booking dùng scope riêng → context đóng ngay sau khi hoàn tất
        foreach (var bookingId in expiredBookingIds)
        {
            await CancelExpiredBookingAsync(bookingId, cancellationToken);
        }

        _logger.LogInformation(
            "Processed {Count} expired bookings",
            expiredBookingIds.Count);
    }

    private async Task CancelExpiredBookingAsync(int bookingId, CancellationToken cancellationToken)
    {
        // Mỗi booking: scope mới → transaction mới → đóng ngay
        using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Đổi trạng thái booking sang Cancelled
            await context.Bookings
                .Where(b => b.Id == bookingId)
                .ExecuteUpdateAsync(
                    b => b.SetProperty(x => x.Status, BookingStatus.Cancelled)
                          .SetProperty(x => x.UpdatedAt, DateTime.UtcNow),
                    cancellationToken);

            // Giải phóng ghế dựa vào ShowtimeSeat.BookingId (được gán khi tạo Booking)
            // Không cần Ticket vì vé chưa được tạo ở bước CreateBooking
            await context.ShowtimeSeats
                .Where(s => s.BookingId == bookingId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, SeatStatus.Available)
                    .SetProperty(x => x.LockedBySessionId, (string?)null)
                    .SetProperty(x => x.LockedAt, (DateTime?)null)
                    .SetProperty(x => x.HoldExpiryTime, (DateTime?)null)
                    .SetProperty(x => x.TicketId, (int?)null)
                    .SetProperty(x => x.BookedByUserId, (int?)null)
                    .SetProperty(x => x.BookingId, (int?)null),
                    cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Cancelled expired booking {BookingId}",
                bookingId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to cancel booking {BookingId}", bookingId);
        }
        // Context + transaction tự dispose khi ra khỏi using
    }
}
