using NUnit.Framework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinemaBooking.Application.DTOs.Rooms;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Infrastructure.Data;
using CinemaBooking.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Application.Tests.Services;

[TestFixture]
public class RoomServiceTests
{
    private ApplicationDbContext _dbContext = null!;
    private RoomService _roomService = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"RoomServiceTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning
            ))
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _roomService = new RoomService(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    #region === GetAllRoomsAsync - Tests ===

    [Test]
    public async Task GetAllRoomsAsync_WithNoRooms_ReturnsEmptyList()
    {
        // ========== ARRANGE ==========
        // No rooms in database

        // ========== ACT ==========
        var result = await _roomService.GetAllRoomsAsync();

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllRoomsAsync_WithMultipleRooms_ReturnsAllRooms()
    {
        // ========== ARRANGE ==========
        var rooms = new List<Room>
        {
            new Room { Id = 1, Name = "Phong 1", Capacity = 50, Type = "2D" },
            new Room { Id = 2, Name = "Phong 2", Capacity = 30, Type = "3D" },
            new Room { Id = 3, Name = "IMAX Hall", Capacity = 100, Type = "IMAX" }
        };
        _dbContext.Rooms.AddRange(rooms);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _roomService.GetAllRoomsAsync();

        // ========== ASSERT ==========
        result.Should().HaveCount(3);
        result.Select(r => r.Name).Should()
            .Contain(new[] { "Phong 1", "Phong 2", "IMAX Hall" });
    }

    [Test]
    public async Task GetAllRoomsAsync_ReturnsCorrectRoomData()
    {
        // ========== ARRANGE ==========
        var room = new Room
        {
            Id = 1,
            Name = "VIP Room",
            Capacity = 20,
            Type = "VIP"
        };
        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _roomService.GetAllRoomsAsync();

        // ========== ASSERT ==========
        result.Should().HaveCount(1);
        var dto = result[0];
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("VIP Room");
        dto.Capacity.Should().Be(20);
        dto.Type.Should().Be("VIP");
    }

    [Test]
    public async Task GetAllRoomsAsync_ReturnsCorrectDtoType()
    {
        // ========== ARRANGE ==========
        var room = new Room
        {
            Id = 1,
            Name = "Test Room",
            Capacity = 10,
            Type = "2D"
        };
        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _roomService.GetAllRoomsAsync();

        // ========== ASSERT ==========
        result.Should().AllBeOfType<RoomResponseDto>();
        result.Should().OnlyContain(r => r.GetType() == typeof(RoomResponseDto));
    }

    [Test]
    public async Task GetAllRoomsAsync_DifferentRoomTypes_ReturnsCorrectly()
    {
        // ========== ARRANGE ==========
        var rooms = new List<Room>
        {
            new Room { Id = 1, Name = "Standard 2D", Capacity = 50, Type = "2D" },
            new Room { Id = 2, Name = "Standard 3D", Capacity = 40, Type = "3D" },
            new Room { Id = 3, Name = "Premium IMAX", Capacity = 80, Type = "IMAX" },
            new Room { Id = 4, Name = "VIP Suite", Capacity = 15, Type = "VIP" }
        };
        _dbContext.Rooms.AddRange(rooms);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _roomService.GetAllRoomsAsync();

        // ========== ASSERT ==========
        result.Should().HaveCount(4);

        var standard2D = result.First(r => r.Type == "2D");
        standard2D.Name.Should().Be("Standard 2D");
        standard2D.Capacity.Should().Be(50);

        var imax = result.First(r => r.Type == "IMAX");
        imax.Name.Should().Be("Premium IMAX");
        imax.Capacity.Should().Be(80);
    }

    [Test]
    public async Task GetAllRoomsAsync_MapsEntityFieldsCorrectly()
    {
        // ========== ARRANGE ==========
        var room = new Room
        {
            Id = 42,
            Name = "Screen Alpha",
            Capacity = 75,
            Type = "Dolby Atmos"
        };
        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _roomService.GetAllRoomsAsync();

        // ========== ASSERT ==========
        result.Should().HaveCount(1);

        var dto = result[0];
        dto.Id.Should().Be(42);
        dto.Name.Should().Be("Screen Alpha");
        dto.Capacity.Should().Be(75);
        dto.Type.Should().Be("Dolby Atmos");
    }

    #endregion
}
