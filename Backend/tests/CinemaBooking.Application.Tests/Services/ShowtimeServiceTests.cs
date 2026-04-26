using NUnit.Framework;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using CinemaBooking.Application.DTOs.Showtimes;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using CinemaBooking.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Application.Tests.Services;

[TestFixture]
public class ShowtimeServiceTests
{
    private ApplicationDbContext _dbContext = null!;
    private ShowtimeService _showtimeService = null!;
    private SeatTemplateService _seatTemplateService = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"ShowtimeServiceTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning
            ))
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _seatTemplateService = new SeatTemplateService(_dbContext);
        _showtimeService = new ShowtimeService(_dbContext, _seatTemplateService);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    #region === CreateShowtimeAsync - Tests ===

    [Test]
    public async Task CreateShowtimeAsync_ValidDto_CreatesShowtimeAndSeats()
    {
        // ========== ARRANGE ==========
        var room = new Room
        {
            Id = 1,
            Name = "Phong 1",
            Capacity = 20,
            Type = "2D"
        };
        _dbContext.Rooms.Add(room);

        var movie = new Movie
        {
            Id = 1,
            Title = "Phim Test",
            Genre = "Action",
            DurationMinutes = 120,
            Language = "English",
            RatingCode = "PG-13",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(10),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Movies.Add(movie);

        // Seed 20 SeatTemplates (A-D, 5 per row) matching real database seed
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

        var dto = new CreateShowtimeDto
        {
            MovieId = 1,
            RoomId = 1,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            BasePrice = 120000m
        };

        // ========== ACT ==========
        var showtimeId = await _showtimeService.CreateShowtimeAsync(dto);

        // ========== ASSERT ==========
        showtimeId.Should().BeGreaterThan(0);

        var showtime = await _dbContext.Showtimes.FirstOrDefaultAsync(s => s.Id == showtimeId);
        showtime.Should().NotBeNull();
        showtime!.MovieId.Should().Be(1);
        showtime.RoomId.Should().Be(1);
        showtime.BasePrice.Should().Be(120000m);
        showtime.TotalSeats.Should().Be(20);
        showtime.BookedSeatsCount.Should().Be(0);
        showtime.IsActive.Should().BeTrue();

        var seats = await _dbContext.ShowtimeSeats
            .Where(s => s.ShowtimeId == showtimeId)
            .OrderBy(s => s.RowLetter)
            .ThenBy(s => s.ColumnNumber)
            .ToListAsync();
        seats.Should().HaveCount(20);
        seats.Should().OnlyContain(s => s.Status == SeatStatus.Available);

        seats[0].SeatNumber.Should().Be("A1");
        seats[0].RowLetter.Should().Be("A");
        seats[0].ColumnNumber.Should().Be(1);
        seats[0].Type.Should().Be(SeatType.Regular);

        // VIP seats (C, D rows)
        seats[10].SeatNumber.Should().Be("C1");
        seats[10].Type.Should().Be(SeatType.VIP);
    }

    [Test]
    public async Task CreateShowtimeAsync_InvalidRoom_ThrowsException()
    {
        // ========== ARRANGE ==========
        var dto = new CreateShowtimeDto
        {
            MovieId = 1,
            RoomId = 999,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            BasePrice = 100000m
        };

        // ========== ACT ==========
        Func<Task> act = async () => await _showtimeService.CreateShowtimeAsync(dto);

        // ========== ASSERT ==========
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    [Test]
    public async Task CreateShowtimeAsync_NullDto_ThrowsArgumentNullException()
    {
        // ========== ACT ==========
        Func<Task> act = async () => await _showtimeService.CreateShowtimeAsync(null!);

        // ========== ASSERT ==========
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task CreateShowtimeAsync_InvalidMovie_ThrowsException()
    {
        // ========== ARRANGE ==========
        var room = new Room
        {
            Id = 1,
            Name = "Phong 1",
            Capacity = 20,
            Type = "2D"
        };
        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync();

        var dto = new CreateShowtimeDto
        {
            MovieId = 999,
            RoomId = 1,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            BasePrice = 100000m
        };

        // ========== ACT ==========
        Func<Task> act = async () => await _showtimeService.CreateShowtimeAsync(dto);

        // ========== ASSERT ==========
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    [Test]
    public async Task CreateShowtimeAsync_RoomHasNoSeatTemplates_ThrowsException()
    {
        // ========== ARRANGE ==========
        var room = new Room
        {
            Id = 1,
            Name = "Phong Rong",
            Capacity = 20,
            Type = "2D"
        };
        _dbContext.Rooms.Add(room);

        var movie = new Movie
        {
            Id = 1,
            Title = "Phim Test",
            Genre = "Action",
            DurationMinutes = 120,
            Language = "English",
            RatingCode = "PG-13",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(10),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        var dto = new CreateShowtimeDto
        {
            MovieId = movie.Id,
            RoomId = room.Id,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            BasePrice = 100000m
        };

        // ========== ACT ==========
        Func<Task> act = async () => await _showtimeService.CreateShowtimeAsync(dto);

        // ========== ASSERT ==========
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*SeatTemplates*");
    }

    [Test]
    public async Task CreateShowtimeAsync_GeneratesCorrectSeatLayout()
    {
        // ========== ARRANGE ==========
        var room = new Room
        {
            Id = 1,
            Name = "Phong Test",
            Capacity = 20,
            Type = "IMAX"
        };
        _dbContext.Rooms.Add(room);

        var movie = new Movie
        {
            Id = 1,
            Title = "Phim Test",
            Genre = "Action",
            DurationMinutes = 120,
            Language = "English",
            RatingCode = "PG-13",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(10),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Movies.Add(movie);

        // Seed seat templates matching the real layout
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

        var dto = new CreateShowtimeDto
        {
            MovieId = 1,
            RoomId = 1,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(3),
            BasePrice = 200000m
        };

        // ========== ACT ==========
        var showtimeId = await _showtimeService.CreateShowtimeAsync(dto);

        // ========== ASSERT ==========
        var seats = await _dbContext.ShowtimeSeats
            .Where(s => s.ShowtimeId == showtimeId)
            .OrderBy(s => s.RowLetter)
            .ThenBy(s => s.ColumnNumber)
            .ToListAsync();

        seats.Should().HaveCount(20);

        seats[0].SeatNumber.Should().Be("A1");
        seats[4].SeatNumber.Should().Be("A5");
        seats[5].SeatNumber.Should().Be("B1");
        seats[9].SeatNumber.Should().Be("B5");
        seats[10].SeatNumber.Should().Be("C1");
        seats[14].SeatNumber.Should().Be("C5");
        seats[15].SeatNumber.Should().Be("D1");
        seats[19].SeatNumber.Should().Be("D5");
    }

    [Test]
    public async Task CreateShowtimeAsync_SetsCreatedAtTimestamp()
    {
        // ========== ARRANGE ==========
        var room = new Room
        {
            Id = 1,
            Name = "Phong 1",
            Capacity = 10,
            Type = "2D"
        };
        _dbContext.Rooms.Add(room);

        var movie = new Movie
        {
            Id = 1,
            Title = "Phim Test",
            Genre = "Action",
            DurationMinutes = 120,
            Language = "English",
            RatingCode = "PG-13",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(10),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Movies.Add(movie);

        for (int i = 1; i <= 10; i++)
        {
            _dbContext.SeatTemplates.Add(new SeatTemplate
            {
                Id = i,
                RoomId = 1,
                Row = ((char)('A' + (i - 1) / 5)).ToString(),
                Number = ((i - 1) % 5) + 1,
                Type = SeatType.Regular,
                DisplayOrder = i
            });
        }

        await _dbContext.SaveChangesAsync();

        var beforeCreate = DateTime.UtcNow;

        var dto = new CreateShowtimeDto
        {
            MovieId = 1,
            RoomId = 1,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            BasePrice = 100000m
        };

        // ========== ACT ==========
        var showtimeId = await _showtimeService.CreateShowtimeAsync(dto);
        var afterCreate = DateTime.UtcNow;

        // ========== ASSERT ==========
        var showtime = await _dbContext.Showtimes.FindAsync(showtimeId);
        showtime!.CreatedAt.Should().BeOnOrAfter(beforeCreate);
        showtime.CreatedAt.Should().BeOnOrBefore(afterCreate);
    }

    #endregion

    #region === GetAllShowtimesAsync - Tests ===

    [Test]
    public async Task GetAllShowtimesAsync_WithNoShowtimes_ReturnsEmptyList()
    {
        // ========== ARRANGE ==========
        // No showtimes in database

        // ========== ACT ==========
        var result = await _showtimeService.GetAllShowtimesAsync();

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllShowtimesAsync_ReturnsShowtimesOrderedByStartTimeDescending()
    {
        // ========== ARRANGE ==========
        var room = new Room { Name = "Phong 1", Capacity = 10, Type = "2D" };
        _dbContext.Rooms.Add(room);

        var movie = new Movie
        {
            Title = "Phim 1",
            Genre = "Action",
            DurationMinutes = 120,
            Language = "English",
            RatingCode = "PG-13",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(10),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        var roomId = room.Id;
        var movieId = movie.Id;
        var now = DateTime.UtcNow;

        var showtime1 = new Showtime
        {
            MovieId = movieId,
            RoomId = roomId,
            StartTime = now.AddHours(-2),
            EndTime = now.AddHours(0),
            BasePrice = 100000m,
            TotalSeats = 10,
            BookedSeatsCount = 0,
            IsActive = true,
            CreatedAt = now
        };
        var showtime2 = new Showtime
        {
            MovieId = movieId,
            RoomId = roomId,
            StartTime = now.AddHours(5),
            EndTime = now.AddHours(7),
            BasePrice = 120000m,
            TotalSeats = 10,
            BookedSeatsCount = 5,
            IsActive = true,
            CreatedAt = now
        };
        _dbContext.Showtimes.AddRange(showtime1, showtime2);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _showtimeService.GetAllShowtimesAsync();

        // ========== ASSERT ==========
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(showtime2.Id);
        result[0].StartTime.Should().BeOnOrAfter(result[1].StartTime);
    }

    [Test]
    public async Task GetAllShowtimesAsync_IncludesMovieAndRoomInfo()
    {
        // ========== ARRANGE ==========
        var room = new Room { Name = "IMAX Hall", Capacity = 50, Type = "IMAX" };
        _dbContext.Rooms.Add(room);

        var movie = new Movie
        {
            Title = "Avatar 2",
            Genre = "Sci-Fi",
            DurationMinutes = 180,
            Language = "English",
            RatingCode = "PG-13",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        var showtime = new Showtime
        {
            MovieId = movie.Id,
            RoomId = room.Id,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(3),
            BasePrice = 250000m,
            TotalSeats = 50,
            BookedSeatsCount = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Showtimes.Add(showtime);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _showtimeService.GetAllShowtimesAsync();

        // ========== ASSERT ==========
        result.Should().HaveCount(1);
        var dto = result[0];
        dto.MovieTitle.Should().Be("Avatar 2");
        dto.RoomName.Should().Be("IMAX Hall");
        dto.BasePrice.Should().Be(250000m);
        dto.TotalSeats.Should().Be(50);
        dto.BookedSeatsCount.Should().Be(10);
        dto.AvailableSeats.Should().Be(40);
        dto.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task GetAllShowtimesAsync_MultipleShowtimes_ReturnsCorrectAvailableSeats()
    {
        // ========== ARRANGE ==========
        var room = new Room { Name = "Phong 1", Capacity = 100, Type = "2D" };
        _dbContext.Rooms.Add(room);

        var movie = new Movie
        {
            Title = "Phim Test",
            Genre = "Action",
            DurationMinutes = 120,
            Language = "English",
            RatingCode = "PG-13",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(10),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        var showtime1 = new Showtime
        {
            MovieId = movie.Id,
            RoomId = room.Id,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            BasePrice = 100000m,
            TotalSeats = 100,
            BookedSeatsCount = 25,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var showtime2 = new Showtime
        {
            MovieId = movie.Id,
            RoomId = room.Id,
            StartTime = DateTime.UtcNow.AddDays(2),
            EndTime = DateTime.UtcNow.AddDays(2).AddHours(2),
            BasePrice = 100000m,
            TotalSeats = 100,
            BookedSeatsCount = 50,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Showtimes.AddRange(showtime1, showtime2);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _showtimeService.GetAllShowtimesAsync();

        // ========== ASSERT ==========
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.AvailableSeats == 75);
        result.Should().Contain(s => s.AvailableSeats == 50);
    }

    [Test]
    public async Task GetAllShowtimesAsync_IncludesEndTime()
    {
        // ========== ARRANGE ==========
        var room = new Room { Name = "Phong 1", Capacity = 10, Type = "2D" };
        _dbContext.Rooms.Add(room);

        var movie = new Movie
        {
            Title = "Phim EndTime",
            Genre = "Drama",
            DurationMinutes = 120,
            Language = "English",
            RatingCode = "PG",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(10),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(2);

        var showtime = new Showtime
        {
            MovieId = movie.Id,
            RoomId = room.Id,
            StartTime = startTime,
            EndTime = endTime,
            BasePrice = 100000m,
            TotalSeats = 10,
            BookedSeatsCount = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Showtimes.Add(showtime);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _showtimeService.GetAllShowtimesAsync();

        // ========== ASSERT ==========
        result.Should().HaveCount(1);
        result[0].StartTime.Should().Be(startTime);
        result[0].EndTime.Should().Be(endTime);
    }

    #endregion
}
