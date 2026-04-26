using NUnit.Framework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using CinemaBooking.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Application.Tests.Services;

[TestFixture]
public class SeatTemplateServiceTests
{
    private ApplicationDbContext _dbContext = null!;
    private SeatTemplateService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"SeatTemplateTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning
            ))
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _service = new SeatTemplateService(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    #region === GetSeatTemplatesByRoomIdAsync - Tests ===

    [Test]
    public async Task GetSeatTemplatesByRoomIdAsync_RoomHasTemplates_ReturnsAllOrderedByDisplayOrder()
    {
        // ========== ARRANGE ==========
        var room = new Room { Id = 1, Name = "Phong 01", Capacity = 20, Type = "2D" };
        _dbContext.Rooms.Add(room);

        var templates = new List<SeatTemplate>
        {
            new SeatTemplate { Id = 3, RoomId = 1, Row = "C", Number = 1, Type = SeatType.VIP, DisplayOrder = 3 },
            new SeatTemplate { Id = 1, RoomId = 1, Row = "A", Number = 1, Type = SeatType.Regular, DisplayOrder = 1 },
            new SeatTemplate { Id = 2, RoomId = 1, Row = "B", Number = 1, Type = SeatType.Regular, DisplayOrder = 2 }
        };
        _dbContext.SeatTemplates.AddRange(templates);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _service.GetSeatTemplatesByRoomIdAsync(1);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result[0].Row.Should().Be("A");
        result[0].DisplayOrder.Should().Be(1);
        result[1].Row.Should().Be("B");
        result[1].DisplayOrder.Should().Be(2);
        result[2].Row.Should().Be("C");
        result[2].DisplayOrder.Should().Be(3);
    }

    [Test]
    public async Task GetSeatTemplatesByRoomIdAsync_RoomHasNoTemplates_ReturnsEmptyList()
    {
        // ========== ARRANGE ==========
        var room = new Room { Id = 1, Name = "Phong Rong", Capacity = 20, Type = "2D" };
        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _service.GetSeatTemplatesByRoomIdAsync(1);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetSeatTemplatesByRoomIdAsync_RoomDoesNotExist_ReturnsEmptyList()
    {
        // ========== ARRANGE ==========
        // No rooms in database

        // ========== ACT ==========
        var result = await _service.GetSeatTemplatesByRoomIdAsync(999);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetSeatTemplatesByRoomIdAsync_MultipleRooms_ReturnsOnlyRequestedRoomTemplates()
    {
        // ========== ARRANGE ==========
        var room1 = new Room { Id = 1, Name = "Phong 01", Capacity = 20, Type = "2D" };
        var room2 = new Room { Id = 2, Name = "Phong 02", Capacity = 20, Type = "3D" };
        _dbContext.Rooms.AddRange(room1, room2);

        _dbContext.SeatTemplates.AddRange(new[]
        {
            new SeatTemplate { Id = 1, RoomId = 1, Row = "A", Number = 1, Type = SeatType.Regular, DisplayOrder = 1 },
            new SeatTemplate { Id = 2, RoomId = 1, Row = "A", Number = 2, Type = SeatType.Regular, DisplayOrder = 2 },
            new SeatTemplate { Id = 3, RoomId = 2, Row = "A", Number = 1, Type = SeatType.Regular, DisplayOrder = 1 },
            new SeatTemplate { Id = 4, RoomId = 2, Row = "A", Number = 2, Type = SeatType.VIP, DisplayOrder = 2 }
        });
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var resultRoom1 = await _service.GetSeatTemplatesByRoomIdAsync(1);
        var resultRoom2 = await _service.GetSeatTemplatesByRoomIdAsync(2);

        // ========== ASSERT ==========
        resultRoom1.Should().HaveCount(2);
        resultRoom1.Should().OnlyContain(t => t.RoomId == 1);

        resultRoom2.Should().HaveCount(2);
        resultRoom2.Should().OnlyContain(t => t.RoomId == 2);
    }

    [Test]
    public async Task GetSeatTemplatesByRoomIdAsync_ReturnsCorrectSeatTypeInfo()
    {
        // ========== ARRANGE ==========
        var room = new Room { Id = 1, Name = "Phong 01", Capacity = 4, Type = "IMAX" };
        _dbContext.Rooms.Add(room);

        _dbContext.SeatTemplates.AddRange(new[]
        {
            new SeatTemplate { Id = 1, RoomId = 1, Row = "A", Number = 1, Type = SeatType.Regular, DisplayOrder = 1 },
            new SeatTemplate { Id = 2, RoomId = 1, Row = "A", Number = 2, Type = SeatType.VIP, DisplayOrder = 2 },
            new SeatTemplate { Id = 3, RoomId = 1, Row = "B", Number = 1, Type = SeatType.VIP, DisplayOrder = 3 },
            new SeatTemplate { Id = 4, RoomId = 1, Row = "B", Number = 2, Type = SeatType.Regular, DisplayOrder = 4 }
        });
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _service.GetSeatTemplatesByRoomIdAsync(1);

        // ========== ASSERT ==========
        result.Should().HaveCount(4);
        result.Should().Contain(t => t.Row == "A" && t.Number == 1 && t.Type == SeatType.Regular);
        result.Should().Contain(t => t.Row == "A" && t.Number == 2 && t.Type == SeatType.VIP);
        result.Should().Contain(t => t.Row == "B" && t.Number == 1 && t.Type == SeatType.VIP);
        result.Should().Contain(t => t.Row == "B" && t.Number == 2 && t.Type == SeatType.Regular);
    }

    #endregion

    #region === GenerateShowtimeSeatsAsync - Tests ===

    [Test]
    public async Task GenerateShowtimeSeatsAsync_ValidTemplates_GeneratesCorrectNumberOfSeats()
    {
        // ========== ARRANGE ==========
        var room = new Room { Id = 1, Name = "Phong 01", Capacity = 20, Type = "2D" };
        _dbContext.Rooms.Add(room);

        var rows = new[] { "A", "B", "C", "D" };
        int seatId = 1;
        for (int r = 0; r < rows.Length; r++)
        {
            for (int n = 1; n <= 5; n++)
            {
                _dbContext.SeatTemplates.Add(new SeatTemplate
                {
                    Id = seatId,
                    RoomId = 1,
                    Row = rows[r],
                    Number = n,
                    Type = r < 2 ? SeatType.Regular : SeatType.VIP,
                    DisplayOrder = seatId
                });
                seatId++;
            }
        }
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _service.GenerateShowtimeSeatsAsync(showtimeId: 100, roomId: 1);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result.Should().HaveCount(20);
    }

    [Test]
    public async Task GenerateShowtimeSeatsAsync_CorrectSeatNumberMapping()
    {
        // ========== ARRANGE ==========
        var room = new Room { Id = 1, Name = "Phong Test", Capacity = 10, Type = "2D" };
        _dbContext.Rooms.Add(room);

        _dbContext.SeatTemplates.AddRange(new[]
        {
            new SeatTemplate { Id = 1, RoomId = 1, Row = "A", Number = 1, Type = SeatType.Regular, DisplayOrder = 1 },
            new SeatTemplate { Id = 2, RoomId = 1, Row = "A", Number = 5, Type = SeatType.Regular, DisplayOrder = 2 },
            new SeatTemplate { Id = 3, RoomId = 1, Row = "B", Number = 3, Type = SeatType.Regular, DisplayOrder = 3 }
        });
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _service.GenerateShowtimeSeatsAsync(showtimeId: 1, roomId: 1);

        // ========== ASSERT ==========
        result.Should().HaveCount(3);

        var seatA1 = result.First(s => s.RowLetter == "A" && s.ColumnNumber == 1);
        seatA1.SeatNumber.Should().Be("A1");
        seatA1.ShowtimeId.Should().Be(1);

        var seatA5 = result.First(s => s.RowLetter == "A" && s.ColumnNumber == 5);
        seatA5.SeatNumber.Should().Be("A5");

        var seatB3 = result.First(s => s.RowLetter == "B" && s.ColumnNumber == 3);
        seatB3.SeatNumber.Should().Be("B3");
    }

    [Test]
    public async Task GenerateShowtimeSeatsAsync_AllSeatsHaveAvailableStatus()
    {
        // ========== ARRANGE ==========
        var room = new Room { Id = 1, Name = "Phong 01", Capacity = 5, Type = "2D" };
        _dbContext.Rooms.Add(room);

        for (int i = 1; i <= 5; i++)
        {
            _dbContext.SeatTemplates.Add(new SeatTemplate
            {
                Id = i,
                RoomId = 1,
                Row = "A",
                Number = i,
                Type = SeatType.Regular,
                DisplayOrder = i
            });
        }
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _service.GenerateShowtimeSeatsAsync(showtimeId: 1, roomId: 1);

        // ========== ASSERT ==========
        result.Should().HaveCount(5);
        result.Should().OnlyContain(s => s.Status == SeatStatus.Available);
    }

    [Test]
    public async Task GenerateShowtimeSeatsAsync_AllSeatsHaveCreatedAtSet()
    {
        // ========== ARRANGE ==========
        var room = new Room { Id = 1, Name = "Phong 01", Capacity = 3, Type = "2D" };
        _dbContext.Rooms.Add(room);

        for (int i = 1; i <= 3; i++)
        {
            _dbContext.SeatTemplates.Add(new SeatTemplate
            {
                Id = i,
                RoomId = 1,
                Row = "A",
                Number = i,
                Type = SeatType.Regular,
                DisplayOrder = i
            });
        }
        await _dbContext.SaveChangesAsync();

        var beforeGenerate = DateTime.UtcNow;

        // ========== ACT ==========
        var result = await _service.GenerateShowtimeSeatsAsync(showtimeId: 1, roomId: 1);
        var afterGenerate = DateTime.UtcNow;

        // ========== ASSERT ==========
        result.Should().HaveCount(3);
        result.Should().OnlyContain(s => s.CreatedAt >= beforeGenerate && s.CreatedAt <= afterGenerate);
    }

    [Test]
    public async Task GenerateShowtimeSeatsAsync_RoomWithNoTemplates_ReturnsEmptyList()
    {
        // ========== ARRANGE ==========
        var room = new Room { Id = 1, Name = "Phong Rong", Capacity = 20, Type = "2D" };
        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _service.GenerateShowtimeSeatsAsync(showtimeId: 1, roomId: 1);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GenerateShowtimeSeatsAsync_ShowtimeIdIsCorrectlyAssigned()
    {
        // ========== ARRANGE ==========
        var room = new Room { Id = 1, Name = "Phong 01", Capacity = 2, Type = "2D" };
        _dbContext.Rooms.Add(room);

        _dbContext.SeatTemplates.AddRange(new[]
        {
            new SeatTemplate { Id = 1, RoomId = 1, Row = "A", Number = 1, Type = SeatType.Regular, DisplayOrder = 1 },
            new SeatTemplate { Id = 2, RoomId = 1, Row = "A", Number = 2, Type = SeatType.Regular, DisplayOrder = 2 }
        });
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _service.GenerateShowtimeSeatsAsync(showtimeId: 42, roomId: 1);

        // ========== ASSERT ==========
        result.Should().HaveCount(2);
        result.Should().OnlyContain(s => s.ShowtimeId == 42);
    }

    [Test]
    public async Task GenerateShowtimeSeatsAsync_TypeInheritedFromSeatTemplate()
    {
        // ========== ARRANGE ==========
        var room = new Room { Id = 1, Name = "Phong 01", Capacity = 4, Type = "2D" };
        _dbContext.Rooms.Add(room);

        _dbContext.SeatTemplates.AddRange(new[]
        {
            new SeatTemplate { Id = 1, RoomId = 1, Row = "A", Number = 1, Type = SeatType.Regular, DisplayOrder = 1 },
            new SeatTemplate { Id = 2, RoomId = 1, Row = "A", Number = 2, Type = SeatType.VIP, DisplayOrder = 2 },
            new SeatTemplate { Id = 3, RoomId = 1, Row = "B", Number = 1, Type = SeatType.VIP, DisplayOrder = 3 },
            new SeatTemplate { Id = 4, RoomId = 1, Row = "B", Number = 2, Type = SeatType.Regular, DisplayOrder = 4 }
        });
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _service.GenerateShowtimeSeatsAsync(showtimeId: 1, roomId: 1);

        // ========== ASSERT ==========
        result.Should().HaveCount(4);

        var regularSeats = result.Where(s => s.Type == SeatType.Regular).ToList();
        regularSeats.Should().HaveCount(2);
        regularSeats.Should().Contain(s => s.SeatNumber == "A1");
        regularSeats.Should().Contain(s => s.SeatNumber == "B2");

        var vipSeats = result.Where(s => s.Type == SeatType.VIP).ToList();
        vipSeats.Should().HaveCount(2);
        vipSeats.Should().Contain(s => s.SeatNumber == "A2");
        vipSeats.Should().Contain(s => s.SeatNumber == "B1");
    }

    [Test]
    public async Task GenerateShowtimeSeatsAsync_SeatNumberFormat_Correct()
    {
        // ========== ARRANGE ==========
        var room = new Room { Id = 1, Name = "Phong Test", Capacity = 2, Type = "IMAX" };
        _dbContext.Rooms.Add(room);

        _dbContext.SeatTemplates.AddRange(new[]
        {
            new SeatTemplate { Id = 1, RoomId = 1, Row = "D", Number = 10, Type = SeatType.VIP, DisplayOrder = 1 },
            new SeatTemplate { Id = 2, RoomId = 1, Row = "Z", Number = 1, Type = SeatType.Regular, DisplayOrder = 2 }
        });
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _service.GenerateShowtimeSeatsAsync(showtimeId: 1, roomId: 1);

        // ========== ASSERT ==========
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.SeatNumber == "D10");
        result.Should().Contain(s => s.SeatNumber == "Z1");
    }

    #endregion
}
