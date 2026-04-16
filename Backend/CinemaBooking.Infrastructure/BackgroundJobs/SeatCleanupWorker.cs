using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CinemaBooking.Infrastructure.BackgroundJobs;

/// <summary>
/// Background service để tự động nhả ghế bị khóa quá hạn (5 phút).
/// Chạy lặp mỗi 1 phút để kiểm tra và cập nhật trạng thái ghế.
/// </summary>
public class SeatCleanupWorker : BackgroundService
{
    private const int CleanupIntervalMinutes = 1;
    private const int LockTimeoutMinutes = 5;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SeatCleanupWorker> _logger;

    public SeatCleanupWorker(
        IServiceProvider serviceProvider,
        ILogger<SeatCleanupWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 SeatCleanupWorker started. Cleanup interval: {IntervalMinutes} minute(s)", CleanupIntervalMinutes);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(CleanupIntervalMinutes));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await CleanupExpiredLocksAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("🛑 SeatCleanupWorker is stopping...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Fatal error in SeatCleanupWorker");
            throw;
        }
        finally
        {
            timer?.Dispose();
        }
    }

    private async Task CleanupExpiredLocksAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔄 Starting cleanup of expired seat locks at {Time:yyyy-MM-dd HH:mm:ss}Z", DateTime.UtcNow);

            using var scope = _serviceProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;
            var expiryThreshold = now;

            // ⚠️ CRITICAL: Dùng ExecuteSqlRawAsync để UPDATE status và clear lock fields
            var updatedCount = await context.Database.ExecuteSqlRawAsync(
                @"UPDATE ShowtimeSeats 
                  SET Status = 0, 
                      LockedBySessionId = NULL, 
                      LockedAt = NULL, 
                      HoldExpiryTime = NULL,
                      UpdatedAt = SYSDATE
                  WHERE Status = 1 
                    AND HoldExpiryTime < {0}",
                now,
                cancellationToken);

            if (updatedCount > 0)
            {
                _logger.LogInformation(
                    "✅ Cleanup completed. Released {Count} expired seats at {Time:yyyy-MM-dd HH:mm:ss}Z",
                    updatedCount, now);
            }
            else
            {
                _logger.LogDebug(
                    "ℹ️ Cleanup completed. No expired seats found at {Time:yyyy-MM-dd HH:mm:ss}Z",
                    now);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("🛑 Cleanup task was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Error during seat cleanup at {Time:yyyy-MM-dd HH:mm:ss}Z. " +
                "Will retry in {IntervalMinutes} minute(s)",
                DateTime.UtcNow, CleanupIntervalMinutes);
            // ⚠️ Không throw - để job tiếp tục chạy lần sau
        }
    }
}
