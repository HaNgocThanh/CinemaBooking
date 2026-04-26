using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Application.Tests.Helpers;

namespace CinemaBooking.Application.Tests.Fixtures;

/// <summary>
/// Fixture cung cấp test data cho User entity.
/// Sử dụng: Tạo dữ liệu test hợp lệ nhanh chóng.
/// LƯU Ý: ID được generate tự động để tránh tracking conflicts trong EF Core.
/// </summary>
public class UserFixture
{
    /// <summary>
    /// Tạo User hợp lệ với giá trị mặc định.
    /// ID được generate tự động nếu không chỉ định, tránh conflicts.
    /// </summary>
    public static User CreateValidUser(
        int? id = null,
        string username = "testuser",
        string email = "test@example.com",
        string fullName = "Test User",
        string password = "ValidPassword123",
        UserRole role = UserRole.Customer,
        bool isActive = true)
    {
        // Nếu không chỉ định ID, generate unique ID
        var userId = id ?? TestDataFactory.GenerateUserId();
        
        return new User
        {
            Id = userId,
            Username = username,
            Email = email,
            FullName = fullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            IsActive = isActive,
            IsEmailConfirmed = false,
            CreatedAt = DateTime.UtcNow,
            LastLogin = null
        };
    }

    /// <summary>
    /// Tạo User với dữ liệu custom.
    /// </summary>
    public static User CreateUserWithCustomData(
        string username,
        string email,
        string fullName,
        UserRole role = UserRole.Customer,
        int? id = null)
    {
        return CreateValidUser(
            id: id,
            username: username,
            email: email,
            fullName: fullName,
            role: role);
    }

    /// <summary>
    /// Tạo Admin User.
    /// </summary>
    public static User CreateAdminUser(
        string username = "admin",
        string email = "admin@example.com",
        int? id = null)
    {
        return CreateValidUser(
            id: id,
            username: username,
            email: email,
            role: UserRole.Admin);
    }

    /// <summary>
    /// Tạo inactive User (bị khóa).
    /// </summary>
    public static User CreateInactiveUser(
        string username = "inactive",
        string email = "inactive@example.com",
        string password = "ValidPassword123",
        int? id = null)
    {
        return CreateValidUser(
            id: id,
            username: username,
            email: email,
            password: password,
            isActive: false);
    }
}

/// <summary>
/// Fixture cung cấp test data cho Showtime entity.
/// </summary>
public class ShowtimeFixture
{
    /// <summary>
    /// Tạo Showtime hợp lệ (tương lai).
    /// </summary>
    public static Showtime CreateValidShowtime(
        int? id = null,
        int movieId = 1,
        int roomId = 1,
        decimal basePrice = 150000m,
        int totalSeats = 100)
    {
        var showtimeId = id ?? TestDataFactory.GenerateShowtimeId();
        var startTime = DateTime.UtcNow.AddHours(2);
        return new Showtime
        {
            Id = showtimeId,
            MovieId = movieId,
            RoomId = roomId,
            StartTime = startTime,
            EndTime = startTime.AddHours(2),
            BasePrice = basePrice,
            TotalSeats = totalSeats,
            BookedSeatsCount = 0,
            IsActive = true
        };
    }

    /// <summary>
    /// Tạo Showtime sắp diễn ra (trong 1 giờ).
    /// </summary>
    public static Showtime CreateUpcomingShowtime(int? id = null)
    {
        var showtimeId = id ?? TestDataFactory.GenerateShowtimeId();
        var startTime = DateTime.UtcNow.AddMinutes(30);
        return new Showtime
        {
            Id = showtimeId,
            MovieId = 1,
            RoomId = 1,
            StartTime = startTime,
            EndTime = startTime.AddHours(2.5),
            BasePrice = 120000m,
            TotalSeats = 100,
            BookedSeatsCount = 0,
            IsActive = true
        };
    }

    /// <summary>
    /// Tạo Showtime đã kết thúc (quá khứ).
    /// </summary>
    public static Showtime CreatePastShowtime(int? id = null)
    {
        var showtimeId = id ?? TestDataFactory.GenerateShowtimeId();
        var startTime = DateTime.UtcNow.AddHours(-3);
        return new Showtime
        {
            Id = showtimeId,
            MovieId = 1,
            RoomId = 1,
            StartTime = startTime,
            EndTime = startTime.AddHours(2),
            BasePrice = 150000m,
            TotalSeats = 100,
            BookedSeatsCount = 100,
            IsActive = false
        };
    }
}

/// <summary>
/// Fixture cung cấp test data cho Booking entity.
/// </summary>
public class BookingFixture
{
    /// <summary>
    /// Tạo Booking hợp lệ (pending payment).
    /// </summary>
    public static Booking CreateValidBooking(
        int? id = null,
        int showtimeId = 1,
        int? customerId = null,
        decimal totalAmount = 450000m,
        BookingStatus status = BookingStatus.PendingPayment)
    {
        var bookingId = id ?? TestDataFactory.GenerateBookingId();
        var now = DateTime.UtcNow;
        return new Booking
        {
            Id = bookingId,
            BookingCode = TestDataFactory.GenerateBookingCode(),
            ShowtimeId = showtimeId,
            CustomerId = customerId,
            SubTotal = totalAmount,
            TotalAmount = totalAmount,
            DiscountAmount = 0,
            Status = status,
            BookedAt = now,
            ExpiresAt = now.AddMinutes(5),
            PaidAt = status == BookingStatus.Confirmed ? now : null,
            Notes = "Test booking"
        };
    }

    /// <summary>
    /// Tạo Booking với discount (promo code).
    /// </summary>
    public static Booking CreateBookingWithDiscount(
        decimal totalAmount = 450000m,
        decimal discountAmount = 90000m,
        string promoCode = "SUMMER20",
        int? id = null)
    {
        var booking = CreateValidBooking(id: id, totalAmount: totalAmount);
        booking.DiscountAmount = discountAmount;
        booking.PromotionCode = promoCode;
        return booking;
    }

    /// <summary>
    /// Tạo Booking đã thanh toán.
    /// </summary>
    public static Booking CreatePaidBooking(
        int customerId = 1,
        decimal totalAmount = 450000m,
        int? id = null)
    {
        var bookingId = id ?? TestDataFactory.GenerateBookingId();
        var now = DateTime.UtcNow;
        return new Booking
        {
            Id = bookingId,
            BookingCode = TestDataFactory.GenerateBookingCode(),
            ShowtimeId = 1,
            CustomerId = customerId,
            SubTotal = totalAmount,
            TotalAmount = totalAmount,
            DiscountAmount = 0,
            Status = BookingStatus.Confirmed,
            BookedAt = now.AddHours(-1),
            ExpiresAt = now.AddMinutes(5),
            PaidAt = now,
            Notes = null
        };
    }

    /// <summary>
    /// Tạo Booking hết hạn (expired).
    /// </summary>
    public static Booking CreateExpiredBooking(int? id = null)
    {
        var bookingId = id ?? TestDataFactory.GenerateBookingId();
        var now = DateTime.UtcNow;
        return new Booking
        {
            Id = bookingId,
            BookingCode = TestDataFactory.GenerateBookingCode(),
            ShowtimeId = 1,
            CustomerId = null,
            SubTotal = 450000m,
            TotalAmount = 450000m,
            DiscountAmount = 0,
            Status = BookingStatus.Expired,
            BookedAt = now.AddMinutes(-10),
            ExpiresAt = now.AddMinutes(-5),  // Hết hạn
            PaidAt = null,
            Notes = null
        };
    }
}
