using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Application.Tests.Helpers;

/// <summary>
/// Helper tạo ApplicationDbContext test (in-memory).
/// Sử dụng: Khởi tạo DbContext nhanh & clean cho mỗi test.
/// </summary>
public class TestDbContextBuilder
{
    /// <summary>
    /// Tạo in-memory DbContext với unique database.
    /// </summary>
    public static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"CinemaBookingTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => 
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Tạo in-memory DbContext với database name cụ thể.
    /// Hữu ích khi cần chia sẻ data giữa multiple tests.
    /// </summary>
    public static ApplicationDbContext CreateInMemoryDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .ConfigureWarnings(w => 
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }
}

/// <summary>
/// Helper tạo mock IConfiguration cho JWT & App settings.
/// Sử dụng: Cấu hình mặc định cho AuthService tests.
/// </summary>
public class TestConfigurationBuilder
{
    /// <summary>
    /// Tạo Mock IConfiguration với JWT settings mặc định.
    /// </summary>
    public static Microsoft.Extensions.Configuration.IConfiguration CreateMockConfiguration(
        string jwtSecret = "this-is-a-very-long-secret-key-for-jwt-testing-purposes-32",
        string jwtIssuer = "CinemaBookingAPI",
        string jwtAudience = "CinemaBookingClients",
        string jwtExpiryMinutes = "60")
    {
        var config = new Moq.Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        
        config.Setup(c => c["Jwt:Secret"])
            .Returns(jwtSecret);
        config.Setup(c => c["Jwt:Issuer"])
            .Returns(jwtIssuer);
        config.Setup(c => c["Jwt:Audience"])
            .Returns(jwtAudience);
        config.Setup(c => c["Jwt:ExpiryMinutes"])
            .Returns(jwtExpiryMinutes);

        return config.Object;
    }

    /// <summary>
    /// Tạo Mock IConfiguration với setting custom.
    /// </summary>
    public static Microsoft.Extensions.Configuration.IConfiguration CreateMockConfigurationWithCustomValue(
        string key,
        string value)
    {
        var config = CreateMockConfiguration();
        var mockConfig = new Moq.Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        
        mockConfig.Setup(c => c[key])
            .Returns(value);

        return mockConfig.Object;
    }
}

/// <summary>
/// Helper factory để tạo test data objects.
/// Sử dụng: Tạo DTOs, entities nhanh chóng.
/// </summary>
public class TestDataFactory
{
    private static int _userId = 0;
    private static int _showtimeId = 0;
    private static int _bookingId = 0;

    /// <summary>
    /// Generate unique User ID.
    /// </summary>
    public static int GenerateUserId() => ++_userId;

    /// <summary>
    /// Generate unique Showtime ID.
    /// </summary>
    public static int GenerateShowtimeId() => ++_showtimeId;

    /// <summary>
    /// Generate unique Booking ID.
    /// </summary>
    public static int GenerateBookingId() => ++_bookingId;

    /// <summary>
    /// Generate Booking Code.
    /// </summary>
    public static string GenerateBookingCode()
    {
        var now = DateTime.UtcNow;
        return $"BK{now:yyMMddHHmm}{GenerateBookingId():D5}";
    }

    /// <summary>
    /// Reset counters (thường gọi giữa tests).
    /// </summary>
    public static void Reset()
    {
        _userId = 0;
        _showtimeId = 0;
        _bookingId = 0;
    }
}
