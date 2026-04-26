using Moq;
using NUnit.Framework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinemaBooking.Application.DTOs.Bookings;
using CinemaBooking.Application.Exceptions;
using CinemaBooking.Application.Services;
using CinemaBooking.Application.Tests.Helpers;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Domain.Exceptions;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Infrastructure.Data;
using CinemaBooking.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CinemaBooking.Application.Tests;

[TestFixture]
public class BookingServiceTests
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<ISeatRepository> _mockSeatRepository = null!;
    private Mock<IShowtimeRepository> _mockShowtimeRepository = null!;
    private Mock<IPromotionRepository> _mockPromotionRepository = null!;
    private Mock<ITicketRepository> _mockTicketRepository = null!;
    private BookingService _bookingService = null!;

    [SetUp]
    public void Setup()
    {
        // Reset TestDataFactory để tránh ID conflicts giữa các tests
        TestDataFactory.Reset();
        
        // Create real in-memory DbContext for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"CinemaBookingTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new ApplicationDbContext(options);
        
        _mockSeatRepository = new Mock<ISeatRepository>();
        _mockShowtimeRepository = new Mock<IShowtimeRepository>();
        _mockPromotionRepository = new Mock<IPromotionRepository>();
        _mockTicketRepository = new Mock<ITicketRepository>();

        // Create service instance with real DbContext
        _bookingService = new BookingService(
            _dbContext,
            _mockSeatRepository.Object,
            _mockShowtimeRepository.Object,
            _mockPromotionRepository.Object,
            _mockTicketRepository.Object
        );
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    /// <summary>
    /// Helper method to seed ShowtimeSeats into DbContext so they can be updated
    /// </summary>
    private void SeedShowtimeSeats(List<ShowtimeSeat> seats)
    {
        foreach (var seat in seats)
        {
            _dbContext.ShowtimeSeats.Add(seat);
        }
        _dbContext.SaveChanges();
    }

    #region Success Cases

    [Test]
    public async Task CreateBookingAsync_ValidRequest_ReturnsSuccessfulBookingResponse()
    {
        // ========== ARRANGE ==========
        const int showtimeId = 1;
        var seatIds = new List<int> { 1, 2, 3 };
        const decimal basePrice = 150000m; // 150k VND
        const string sessionId = "session-user-123";

        var request = new BookingRequestDto
        {
            ShowtimeId = showtimeId,
            SeatIds = seatIds,
            SessionId = sessionId,
            CustomerId = null,
            PromoCode = null,
            Notes = "Test booking"
        };

        // Mock Showtime
        var showtime = new Showtime
        {
            Id = showtimeId,
            MovieId = 1,
            RoomId = 1,
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(4),
            BasePrice = basePrice,
            TotalSeats = 100,
            BookedSeatsCount = 0,
            IsActive = true
        };

        // Mock Locked Seats
        var lockedSeats = new List<ShowtimeSeat>
        {
            new() { Id = 1, ShowtimeId = showtimeId, SeatNumber = "A1", RowLetter = "A", ColumnNumber = 1, Status = SeatStatus.Locked, LockedAt = DateTime.UtcNow },
            new() { Id = 2, ShowtimeId = showtimeId, SeatNumber = "A2", RowLetter = "A", ColumnNumber = 2, Status = SeatStatus.Locked, LockedAt = DateTime.UtcNow },
            new() { Id = 3, ShowtimeId = showtimeId, SeatNumber = "A3", RowLetter = "A", ColumnNumber = 3, Status = SeatStatus.Locked, LockedAt = DateTime.UtcNow }
        };

        // Seed ShowtimeSeats to DbContext so they can be updated
        SeedShowtimeSeats(lockedSeats);

        // Setup repository mocks
        _mockShowtimeRepository.Setup(x => x.GetByIdAsync(showtimeId))
            .ReturnsAsync(showtime);

        _mockSeatRepository.Setup(x => x.LockSeatsAsync(seatIds, showtimeId, sessionId))
            .ReturnsAsync(lockedSeats);

        _mockPromotionRepository.Setup(x => x.GetByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((PromotionInfo?)null);

        // ========== ACT ==========
        var result = await _bookingService.CreateBookingAsync(request);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result.BookingId.Should().BeGreaterThan(0);
        result.BookingCode.Should().NotBeNullOrEmpty();
        result.Status.Should().Be(BookingStatus.PendingPayment.ToString());
        result.TotalTickets.Should().Be(3);
        result.SubTotal.Should().Be(basePrice * 3);
        result.DiscountAmount.Should().Be(0);
        result.TotalAmount.Should().Be(basePrice * 3);
        result.AppliedPromoCode.Should().BeNull();
        result.ExpiresAt.Should().NotBeNull();
        result.BookedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        result.TicketIds.Should().HaveCount(3);

        // Verify repository calls
        _mockShowtimeRepository.Verify(x => x.GetByIdAsync(showtimeId), Times.Once);
        _mockSeatRepository.Verify(x => x.LockSeatsAsync(seatIds, showtimeId, sessionId), Times.Once);
    }

    [Test]
    public async Task CreateBookingAsync_ValidRequestWithValidPromoCode_AppliesDiscountCorrectly()
    {
        // ========== ARRANGE ==========
        const int showtimeId = 1;
        var seatIds = new List<int> { 1, 2 };
        const decimal basePrice = 100000m; // 100k VND
        const string promoCode = "SUMMER2024";
        const decimal discountPercent = 0.2m; // 20% discount
        const string sessionId = "session-user-456";

        var request = new BookingRequestDto
        {
            ShowtimeId = showtimeId,
            SeatIds = seatIds,
            PromoCode = promoCode,
            SessionId = sessionId,
            CustomerId = null
        };

        // Mock Showtime
        var showtime = new Showtime
        {
            Id = showtimeId,
            MovieId = 1,
            RoomId = 1,
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(4),
            BasePrice = basePrice,
            TotalSeats = 100,
            BookedSeatsCount = 0,
            IsActive = true
        };

        // Mock Locked Seats
        var lockedSeats = new List<ShowtimeSeat>
        {
            new() { Id = 1, ShowtimeId = showtimeId, SeatNumber = "B1", RowLetter = "B", ColumnNumber = 1, Status = SeatStatus.Locked, LockedAt = DateTime.UtcNow },
            new() { Id = 2, ShowtimeId = showtimeId, SeatNumber = "B2", RowLetter = "B", ColumnNumber = 2, Status = SeatStatus.Locked, LockedAt = DateTime.UtcNow }
        };

        // Mock Promotion (valid, not expired)
        var promotion = new PromotionInfo
        {
            Code = promoCode,
            DiscountType = "PERCENTAGE",
            DiscountValue = discountPercent * 100, // Convert 0.2 to 20%
            MaxDiscountAmount = null,
            RemainingQuantity = 100,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30)
        };

        // Seed ShowtimeSeats to DbContext so they can be updated
        SeedShowtimeSeats(lockedSeats);

        // Setup repository mocks
        _mockShowtimeRepository.Setup(x => x.GetByIdAsync(showtimeId))
            .ReturnsAsync(showtime);

        _mockSeatRepository.Setup(x => x.LockSeatsAsync(seatIds, showtimeId, sessionId))
            .ReturnsAsync(lockedSeats);

        _mockPromotionRepository.Setup(x => x.GetByCodeAsync(promoCode))
            .ReturnsAsync(promotion);

        // ========== ACT ==========
        var result = await _bookingService.CreateBookingAsync(request);

        // ========== ASSERT ==========
        var subtotal = basePrice * 2;
        var expectedDiscount = subtotal * discountPercent;
        var expectedTotal = subtotal - expectedDiscount;

        result.Should().NotBeNull();
        result.TotalTickets.Should().Be(2);
        result.SubTotal.Should().Be(subtotal);
        result.DiscountAmount.Should().Be(expectedDiscount);
        result.TotalAmount.Should().Be(expectedTotal);
        result.AppliedPromoCode.Should().Be(promoCode);
        result.BookingCode.Should().NotBeNullOrEmpty();
        result.Status.Should().Be(BookingStatus.PendingPayment.ToString());

        // Verify promotion was retrieved
        _mockPromotionRepository.Verify(x => x.GetByCodeAsync(promoCode), Times.Once);
    }

    #endregion

    #region Failure Cases - Concurrency

    [Test]
    public async Task CreateBookingAsync_SeatAlreadyLocked_ThrowsSeatAlreadyLockedException()
    {
        // ========== ARRANGE ==========
        const int showtimeId = 1;
        var seatIds = new List<int> { 1, 2, 3 };
        const string sessionId = "session-user-789";

        var request = new BookingRequestDto
        {
            ShowtimeId = showtimeId,
            SeatIds = seatIds,
            SessionId = sessionId,
            CustomerId = null
        };

        // Mock Showtime
        var showtime = new Showtime
        {
            Id = showtimeId,
            MovieId = 1,
            RoomId = 1,
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(4),
            BasePrice = 150000m,
            TotalSeats = 100,
            BookedSeatsCount = 0,
            IsActive = true
        };

        // Setup repository mocks
        _mockShowtimeRepository.Setup(x => x.GetByIdAsync(showtimeId))
            .ReturnsAsync(showtime);

        // Mock SeatRepository to throw SeatAlreadyLockedException
        _mockSeatRepository.Setup(x => x.LockSeatsAsync(seatIds, showtimeId, sessionId))
            .ThrowsAsync(new SeatAlreadyLockedException("SEAT_LOCKED", "Ghế C1 đã được khóa bởi user khác. Vui lòng chọn ghế khác."));

        // ========== ACT & ASSERT ==========
        var exception = Assert.ThrowsAsync<SeatAlreadyLockedException>(
            async () => await _bookingService.CreateBookingAsync(request)
        );

        exception.Should().NotBeNull();
        exception.Code.Should().Be("SEAT_LOCKED");

        // Verify that seat locking was attempted
        _mockSeatRepository.Verify(x => x.LockSeatsAsync(seatIds, showtimeId, sessionId), Times.Once);
    }

    #endregion

    #region Failure Cases - Promotion

    [Test]
    public async Task CreateBookingAsync_PromoCodeExpired_ThrowsPromoExpiredException()
    {
        // ========== ARRANGE ==========
        const int showtimeId = 1;
        var seatIds = new List<int> { 1, 2 };
        const decimal basePrice = 100000m;
        const string expiredPromoCode = "OLD_PROMO";
        const string sessionId = "session-user-101";

        var request = new BookingRequestDto
        {
            ShowtimeId = showtimeId,
            SeatIds = seatIds,
            PromoCode = expiredPromoCode,
            SessionId = sessionId,
            CustomerId = null
        };

        // Mock Showtime
        var showtime = new Showtime
        {
            Id = showtimeId,
            MovieId = 1,
            RoomId = 1,
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(4),
            BasePrice = basePrice,
            TotalSeats = 100,
            BookedSeatsCount = 0,
            IsActive = true
        };

        // Mock Locked Seats
        var lockedSeats = new List<ShowtimeSeat>
        {
            new() { Id = 1, ShowtimeId = showtimeId, SeatNumber = "D1", RowLetter = "D", ColumnNumber = 1, Status = SeatStatus.Locked, LockedAt = DateTime.UtcNow },
            new() { Id = 2, ShowtimeId = showtimeId, SeatNumber = "D2", RowLetter = "D", ColumnNumber = 2, Status = SeatStatus.Locked, LockedAt = DateTime.UtcNow }
        };

        // Mock Expired Promotion
        var expiredPromotion = new PromotionInfo
        {
            Code = expiredPromoCode,
            DiscountType = "FIXED",
            DiscountValue = 50000m,
            MaxDiscountAmount = null,
            RemainingQuantity = 100,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(-1) // Expired 1 day ago
        };

        // Setup repository mocks
        _mockShowtimeRepository.Setup(x => x.GetByIdAsync(showtimeId))
            .ReturnsAsync(showtime);

        _mockSeatRepository.Setup(x => x.LockSeatsAsync(seatIds, showtimeId, sessionId))
            .ReturnsAsync(lockedSeats);

        _mockPromotionRepository.Setup(x => x.GetByCodeAsync(expiredPromoCode))
            .ReturnsAsync(expiredPromotion);

        // ========== ACT & ASSERT ==========
        var exception = Assert.ThrowsAsync<PromoExpiredException>(
            async () => await _bookingService.CreateBookingAsync(request)
        );

        exception.Should().NotBeNull();
        exception.Code.Should().Be("PROMO_EXPIRED");
        exception.UserMessage.Should().Contain("hết hạn");

        // Verify that seat locking occurred and promo was checked
        _mockSeatRepository.Verify(x => x.LockSeatsAsync(seatIds, showtimeId, sessionId), Times.Once);
        _mockPromotionRepository.Verify(x => x.GetByCodeAsync(expiredPromoCode), Times.Once);
    }

    #endregion

    #region Failure Cases - Validation

    [Test]
    public async Task CreateBookingAsync_EmptySeatIdList_ThrowsInvalidSeatsException()
    {
        // ========== ARRANGE ==========
        var request = new BookingRequestDto
        {
            ShowtimeId = 1,
            SeatIds = new List<int>(), // Empty list
            SessionId = "session-user-102",
            CustomerId = null
        };

        // ========== ACT & ASSERT ==========
        var exception = Assert.ThrowsAsync<InvalidSeatsException>(
            async () => await _bookingService.CreateBookingAsync(request)
        );

        exception.Should().NotBeNull();
        exception.Code.Should().Be("INVALID_SEATS");
        exception.UserMessage.Should().Contain("không thể trống");

        // Verify that no repository calls were made (validation failed early)
        _mockSeatRepository.Verify(x => x.LockSeatsAsync(It.IsAny<List<int>>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CreateBookingAsync_NullRequest_ThrowsArgumentNullException()
    {
        // ========== ARRANGE ==========
        BookingRequestDto? request = null;

        // ========== ACT & ASSERT ==========
        var exception = Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _bookingService.CreateBookingAsync(request!)
        );

        exception.Should().NotBeNull();
        exception.ParamName.Should().Be("request");

        // Verify that no repository calls were made
        _mockSeatRepository.Verify(x => x.LockSeatsAsync(It.IsAny<List<int>>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CreateBookingAsync_InvalidShowtimeId_ThrowsInvalidShowtimeException()
    {
        // ========== ARRANGE ==========
        var request = new BookingRequestDto
        {
            ShowtimeId = -1, // Invalid: must be > 0
            SeatIds = new List<int> { 1, 2 },
            SessionId = "session-user-103",
            CustomerId = null
        };

        // ========== ACT & ASSERT ==========
        var exception = Assert.ThrowsAsync<InvalidShowtimeException>(
            async () => await _bookingService.CreateBookingAsync(request)
        );

        exception.Should().NotBeNull();
        exception.Code.Should().Be("INVALID_SHOWTIME");
        exception.UserMessage.Should().Contain("phải > 0");

        // Verify that no repository calls were made
        _mockShowtimeRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region Transaction & Rollback Cases

    [Test]
    public async Task CreateBookingAsync_UnexpectedDatabaseException_RollsBackTransactionAndThrows()
    {
        // ========== ARRANGE ==========
        const int showtimeId = 1;
        var seatIds = new List<int> { 1, 2 };
        const string sessionId = "session-user-104";

        var request = new BookingRequestDto
        {
            ShowtimeId = showtimeId,
            SeatIds = seatIds,
            SessionId = sessionId,
            CustomerId = null
        };

        // Mock Showtime
        var showtime = new Showtime
        {
            Id = showtimeId,
            MovieId = 1,
            RoomId = 1,
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(4),
            BasePrice = 100000m,
            TotalSeats = 100,
            BookedSeatsCount = 0,
            IsActive = true
        };

        // Mock Locked Seats
        var lockedSeats = new List<ShowtimeSeat>
        {
            new() { Id = 1, ShowtimeId = showtimeId, SeatNumber = "E1", RowLetter = "E", ColumnNumber = 1, Status = SeatStatus.Locked, LockedAt = DateTime.UtcNow },
            new() { Id = 2, ShowtimeId = showtimeId, SeatNumber = "E2", RowLetter = "E", ColumnNumber = 2, Status = SeatStatus.Locked, LockedAt = DateTime.UtcNow }
        };

        // Setup repository mocks
        _mockShowtimeRepository.Setup(x => x.GetByIdAsync(showtimeId))
            .ReturnsAsync(showtime);

        _mockSeatRepository.Setup(x => x.LockSeatsAsync(seatIds, showtimeId, sessionId))
            .ThrowsAsync(new SeatAlreadyLockedException("SEAT_LOCKED", "Seat already locked"));

        // ========== ACT & ASSERT ==========
        // When seat repository throws exception, service should propagate it
        var ex = Assert.ThrowsAsync<SeatAlreadyLockedException>(
            async () => await _bookingService.CreateBookingAsync(request)
        );

        ex.Should().NotBeNull();
    }

    #endregion
}
