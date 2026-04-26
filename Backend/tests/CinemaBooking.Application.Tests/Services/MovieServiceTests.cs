using NUnit.Framework;
using Moq;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CinemaBooking.Application.DTOs.Movies;
using CinemaBooking.Application.Exceptions;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using CinemaBooking.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Application.Tests.Services;

/// <summary>
/// Unit Tests cho MovieService - tầng Application
/// 
/// Kiểm tra các tính năng:
/// - Lấy danh sách phim (active/inactive)
/// - Lấy chi tiết phim theo ID
/// - Tạo phim mới
/// - Cập nhật phim
/// - Xóa phim
/// 
/// Tech: NUnit, Moq, FluentAssertions
/// </summary>
[TestFixture]
public class MovieServiceTests
{
    private ApplicationDbContext _dbContext = null!;
    private MovieService _movieService = null!;

    [SetUp]
    public void Setup()
    {
        InitializeDbContext();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    private void InitializeDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"MovieServiceTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning
            ))
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _movieService = new MovieService(_dbContext);
    }

    #region === GetAllMoviesAsync - Tests ===

    [Test]
    public async Task GetAllMoviesAsync_WithActiveMovies_ReturnsOnlyActiveMovies()
    {
        // ========== ARRANGE ==========
        var activeMovie1 = new Movie
        {
            Title = "Inception",
            Description = "A thief who steals corporate secrets",
            Director = "Christopher Nolan",
            Genre = "Sci-Fi",
            DurationMinutes = 148,
            Language = "English",
            RatingCode = "PG-13",
            PosterUrl = "https://example.com/inception.jpg",
            TrailerUrl = "https://youtube.com/trailer/inception",
            ReleaseDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var activeMovie2 = new Movie
        {
            Title = "The Matrix",
            Description = "A computer hacker learns about reality",
            Director = "The Wachowskis",
            Genre = "Sci-Fi",
            DurationMinutes = 136,
            Language = "English",
            RatingCode = "R",
            PosterUrl = "https://example.com/matrix.jpg",
            TrailerUrl = "https://youtube.com/trailer/matrix",
            ReleaseDate = DateTime.UtcNow.AddDays(-20),
            EndDate = DateTime.UtcNow.AddDays(40),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var inactiveMovie = new Movie
        {
            Title = "Old Movie",
            Description = "An old inactive movie",
            Director = "Old Director",
            Genre = "Drama",
            DurationMinutes = 120,
            Language = "English",
            RatingCode = "PG",
            PosterUrl = "https://example.com/old.jpg",
            TrailerUrl = "https://youtube.com/trailer/old",
            ReleaseDate = DateTime.UtcNow.AddYears(-5),
            EndDate = DateTime.UtcNow.AddYears(-4),
            IsActive = false,
            CreatedAt = DateTime.UtcNow.AddYears(-5)
        };

        _dbContext.Movies.AddRange(activeMovie1, activeMovie2, inactiveMovie);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _movieService.GetAllMoviesAsync(onlyActive: true);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(m => m.IsActive.Should().BeTrue());
        result.Select(m => m.Title).Should().Contain(new[] { "Inception", "The Matrix" });
        result.Select(m => m.Title).Should().NotContain("Old Movie");
    }

    [Test]
    public async Task GetAllMoviesAsync_WithInactiveFilter_ReturnsAllMovies()
    {
        // ========== ARRANGE ==========
        var activeMovie = new Movie
        {
            Title = "Active Movie",
            Description = "Active",
            Director = "Director",
            Genre = "Drama",
            DurationMinutes = 120,
            Language = "English",
            RatingCode = "PG",
            PosterUrl = "https://example.com/active.jpg",
            TrailerUrl = "https://youtube.com/trailer/active",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(10),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var inactiveMovie = new Movie
        {
            Title = "Inactive Movie",
            Description = "Inactive",
            Director = "Director",
            Genre = "Drama",
            DurationMinutes = 110,
            Language = "English",
            RatingCode = "PG",
            PosterUrl = "https://example.com/inactive.jpg",
            TrailerUrl = "https://youtube.com/trailer/inactive",
            ReleaseDate = DateTime.UtcNow.AddYears(-2),
            EndDate = DateTime.UtcNow.AddYears(-1),
            IsActive = false,
            CreatedAt = DateTime.UtcNow.AddYears(-2)
        };

        _dbContext.Movies.AddRange(activeMovie, inactiveMovie);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _movieService.GetAllMoviesAsync(onlyActive: false);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(m => m.Title == "Active Movie");
        result.Should().Contain(m => m.Title == "Inactive Movie");
    }

    [Test]
    public async Task GetAllMoviesAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // ========== ARRANGE ==========
        // Database is empty

        // ========== ACT ==========
        var result = await _movieService.GetAllMoviesAsync(onlyActive: true);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllMoviesAsync_OrdersMoviesByReleaseDateDescending()
    {
        // ========== ARRANGE ==========
        var movie1 = new Movie
        {
            Title = "Old Release",
            Director = "Director",
            Genre = "Drama",
            DurationMinutes = 120,
            Language = "English",
            RatingCode = "PG",
            PosterUrl = "https://example.com/old.jpg",
            TrailerUrl = "https://youtube.com/trailer/old",
            ReleaseDate = DateTime.UtcNow.AddDays(-100),
            EndDate = DateTime.UtcNow.AddDays(100),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var movie2 = new Movie
        {
            Title = "Recent Release",
            Director = "Director",
            Genre = "Drama",
            DurationMinutes = 120,
            Language = "English",
            RatingCode = "PG",
            PosterUrl = "https://example.com/recent.jpg",
            TrailerUrl = "https://youtube.com/trailer/recent",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(100),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Movies.AddRange(movie1, movie2);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _movieService.GetAllMoviesAsync(onlyActive: true);

        // ========== ASSERT ==========
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Recent Release"); // Newest first
        result[1].Title.Should().Be("Old Release");    // Oldest second
    }

    [Test]
    public async Task GetAllMoviesAsync_StatusNowShowing_ReturnsValidMovies()
    {
        // ========== ARRANGE ==========
        var movie1 = new Movie
        {
            Title = "Now Showing Movie",
            ReleaseDate = DateTime.UtcNow.AddDays(-5),
            IsActive = true,
            Status = MovieStatus.NowShowing
        };

        var movie2 = new Movie
        {
            Title = "Coming Soon Movie",
            ReleaseDate = DateTime.UtcNow.AddDays(5),
            IsActive = true,
            Status = MovieStatus.ComingSoon
        };

        var movie3 = new Movie
        {
            Title = "Stopped Movie",
            ReleaseDate = DateTime.UtcNow.AddDays(-30),
            IsActive = true,
            Status = MovieStatus.Stopped
        };

        _dbContext.Movies.AddRange(movie1, movie2, movie3);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _movieService.GetAllMoviesAsync(onlyActive: true, status: "NowShowing");

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Now Showing Movie");
        result[0].Status.Should().Be("NowShowing");
    }

    [Test]
    public async Task GetAllMoviesAsync_StatusComingSoon_ReturnsValidMovies()
    {
        // ========== ARRANGE ==========
        var movie1 = new Movie
        {
            Title = "Now Showing Movie",
            ReleaseDate = DateTime.UtcNow.AddDays(-5),
            IsActive = true,
            Status = MovieStatus.NowShowing
        };

        var movie2 = new Movie
        {
            Title = "Coming Soon Movie 1",
            ReleaseDate = DateTime.UtcNow.AddDays(5),
            IsActive = true,
            Status = MovieStatus.ComingSoon
        };

        var movie3 = new Movie
        {
            Title = "Coming Soon Movie 2",
            ReleaseDate = DateTime.UtcNow.AddDays(10),
            IsActive = true,
            Status = MovieStatus.ComingSoon
        };

        _dbContext.Movies.AddRange(movie1, movie2, movie3);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _movieService.GetAllMoviesAsync(onlyActive: true, status: "ComingSoon");

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Select(m => m.Title).Should().Contain(new[] { "Coming Soon Movie 1", "Coming Soon Movie 2" });
        result.Should().AllSatisfy(m => m.Status.Should().Be("ComingSoon"));
    }

    #endregion

    #region === GetMovieByIdAsync - Tests ===

    [Test]
    public async Task GetMovieByIdAsync_ExistingMovie_ReturnsMovieDetails()
    {
        // ========== ARRANGE ==========
        var movie = new Movie
        {
            Title = "Inception",
            Description = "A thief who steals corporate secrets",
            Director = "Christopher Nolan",
            Cast = "Leonardo DiCaprio, Ellen Page",
            Genre = "Sci-Fi",
            DurationMinutes = 148,
            Language = "English",
            RatingCode = "PG-13",
            PosterUrl = "https://example.com/inception.jpg",
            TrailerUrl = "https://youtube.com/trailer/inception",
            ReleaseDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        var movieId = movie.Id;

        // ========== ACT ==========
        var result = await _movieService.GetMovieByIdAsync(movieId);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result!.Title.Should().Be("Inception");
        result.Director.Should().Be("Christopher Nolan");
        result.DurationMinutes.Should().Be(148);
        result.IsActive.Should().BeTrue();
        result.Id.Should().Be(movieId);
    }

    [Test]
    public async Task GetMovieByIdAsync_NonExistentMovie_ReturnsNull()
    {
        // ========== ARRANGE ==========
        const int nonExistentId = 999;

        // ========== ACT ==========
        var result = await _movieService.GetMovieByIdAsync(nonExistentId);

        // ========== ASSERT ==========
        result.Should().BeNull();
    }

    [Test]
    public async Task GetMovieByIdAsync_WithInactiveMovie_ReturnsMovie()
    {
        // ========== ARRANGE ==========
        var inactiveMovie = new Movie
        {
            Title = "Old Movie",
            Description = "An old movie",
            Director = "Old Director",
            Genre = "Drama",
            DurationMinutes = 120,
            Language = "English",
            RatingCode = "PG",
            PosterUrl = "https://example.com/old.jpg",
            TrailerUrl = "https://youtube.com/trailer/old",
            ReleaseDate = DateTime.UtcNow.AddYears(-5),
            EndDate = DateTime.UtcNow.AddYears(-4),
            IsActive = false,
            CreatedAt = DateTime.UtcNow.AddYears(-5)
        };

        _dbContext.Movies.Add(inactiveMovie);
        await _dbContext.SaveChangesAsync();

        var movieId = inactiveMovie.Id;

        // ========== ACT ==========
        var result = await _movieService.GetMovieByIdAsync(movieId);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result!.Title.Should().Be("Old Movie");
        result.IsActive.Should().BeFalse();
    }

    #endregion

    #region === CreateMovieAsync - Tests ===

    [Test]
    public async Task CreateMovieAsync_ValidRequest_CreatesAndReturnsMovie()
    {
        // ========== ARRANGE ==========
        var request = new CreateMovieDto
        {
            Title = "Dune",
            Description = "An epic science fiction film",
            Director = "Denis Villeneuve",
            Cast = "Timothée Chalamet, Zendaya",
            Genre = "Sci-Fi",
            DurationMinutes = 166,
            Language = "English",
            RatingCode = "PG-13",
            PosterUrl = "https://example.com/dune.jpg",
            TrailerUrl = "https://youtube.com/trailer/dune",
            ReleaseDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(60)
        };

        // ========== ACT ==========
        var result = await _movieService.CreateMovieAsync(request);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result.Title.Should().Be("Dune");
        result.Director.Should().Be("Denis Villeneuve");
        result.DurationMinutes.Should().Be(166);
        result.IsActive.Should().BeTrue();
        result.Id.Should().BeGreaterThan(0);

        // Verify it was persisted to database
        var savedMovie = await _dbContext.Movies.FindAsync(result.Id);
        savedMovie.Should().NotBeNull();
        savedMovie!.Title.Should().Be("Dune");
    }

    [Test]
    public async Task CreateMovieAsync_WithOptionalFields_CreatesSuccessfully()
    {
        // ========== ARRANGE ==========
        var request = new CreateMovieDto
        {
            Title = "Minimal Movie",
            Description = null,    // Optional
            Director = null,       // Optional
            Cast = null,           // Optional
            Genre = "Drama",
            DurationMinutes = 90,
            Language = "English",
            RatingCode = "PG",
            PosterUrl = null,      // Optional
            TrailerUrl = null,     // Optional
            ReleaseDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(40)
        };

        // ========== ACT ==========
        var result = await _movieService.CreateMovieAsync(request);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result.Title.Should().Be("Minimal Movie");
        result.Director.Should().BeNull();
        result.Cast.Should().BeNull();
        result.PosterUrl.Should().BeNull();
        result.Genre.Should().Be("Drama");
    }

    [Test]
    public async Task CreateMovieAsync_SetsCreatedAtToUtcNow()
    {
        // ========== ARRANGE ==========
        var beforeCreation = DateTime.UtcNow;

        var request = new CreateMovieDto
        {
            Title = "Time Test Movie",
            Genre = "Drama",
            DurationMinutes = 100,
            Language = "English",
            RatingCode = "PG",
            ReleaseDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(40)
        };

        // ========== ACT ==========
        var result = await _movieService.CreateMovieAsync(request);
        var afterCreation = DateTime.UtcNow;

        // ========== ASSERT ==========
        result.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        result.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    #endregion

    #region === UpdateMovieAsync - Tests ===

    [Test]
    public async Task UpdateMovieAsync_PartialUpdate_UpdatesOnlyProvidedFields()
    {
        // ========== ARRANGE ==========
        var originalMovie = new Movie
        {
            Title = "Original Title",
            Description = "Original Description",
            Director = "Original Director",
            Genre = "Action",
            DurationMinutes = 120,
            Language = "English",
            RatingCode = "PG-13",
            PosterUrl = "https://example.com/original.jpg",
            TrailerUrl = "https://youtube.com/trailer/original",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(20),
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        _dbContext.Movies.Add(originalMovie);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new UpdateMovieDto
        {
            Title = "Updated Title",
            Director = "Updated Director",
            DurationMinutes = 150,
            Description = null,   // Keep original
            Genre = null,         // Keep original
            Language = null,      // Keep original
            RatingCode = null,    // Keep original
            PosterUrl = null,     // Keep original
            TrailerUrl = null,    // Keep original
            ReleaseDate = null,   // Keep original
            EndDate = null        // Keep original
        };

        // ========== ACT ==========
        var result = await _movieService.UpdateMovieAsync(originalMovie.Id, updateRequest);

        // ========== ASSERT ==========
        result.Title.Should().Be("Updated Title");
        result.Director.Should().Be("Updated Director");
        result.DurationMinutes.Should().Be(150);
        result.Description.Should().Be("Original Description"); // Unchanged
        result.Genre.Should().Be("Action");                     // Unchanged
        result.RatingCode.Should().Be("PG-13");               // Unchanged
        result.UpdatedAt.Should().NotBeNull();
    }

    [Test]
    public async Task UpdateMovieAsync_NonExistentMovie_ThrowsKeyNotFoundException()
    {
        // ========== ARRANGE ==========
        const int nonExistentId = 999;

        var updateRequest = new UpdateMovieDto
        {
            Title = "Updated Title"
        };

        // ========== ACT ==========
        var action = () => _movieService.UpdateMovieAsync(nonExistentId, updateRequest);

        // ========== ASSERT ==========
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{nonExistentId}*");
    }

    [Test]
    public async Task UpdateMovieAsync_SetsUpdatedAtTimestamp()
    {
        // ========== ARRANGE ==========
        var movie = new Movie
        {
            Title = "Original Title",
            Genre = "Drama",
            DurationMinutes = 100,
            Language = "English",
            RatingCode = "PG",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(20),
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        var beforeUpdate = DateTime.UtcNow;

        var updateRequest = new UpdateMovieDto
        {
            Title = "New Title"
        };

        // ========== ACT ==========
        var result = await _movieService.UpdateMovieAsync(movie.Id, updateRequest);
        var afterUpdate = DateTime.UtcNow;

        // ========== ASSERT ==========
        result.UpdatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
        result.UpdatedAt.Should().BeOnOrBefore(afterUpdate);
    }

    [Test]
    public async Task UpdateMovieAsync_FullUpdate_UpdatesAllFields()
    {
        // ========== ARRANGE ==========
        var movie = new Movie
        {
            Title = "Original",
            Description = "Original Description",
            Director = "Original Director",
            Cast = "Original Cast",
            Genre = "Action",
            DurationMinutes = 120,
            Language = "English",
            RatingCode = "PG-13",
            PosterUrl = "https://example.com/original.jpg",
            TrailerUrl = "https://youtube.com/trailer/original",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(20),
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new UpdateMovieDto
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Director = "Updated Director",
            Cast = "Updated Cast",
            Genre = "Drama",
            DurationMinutes = 150,
            Language = "Vietnamese",
            RatingCode = "R",
            PosterUrl = "https://example.com/updated.jpg",
            TrailerUrl = "https://youtube.com/trailer/updated",
            ReleaseDate = DateTime.UtcNow.AddDays(-20),
            EndDate = DateTime.UtcNow.AddDays(30)
        };

        // ========== ACT ==========
        var result = await _movieService.UpdateMovieAsync(movie.Id, updateRequest);

        // ========== ASSERT ==========
        result.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated Description");
        result.Director.Should().Be("Updated Director");
        result.Cast.Should().Be("Updated Cast");
        result.Genre.Should().Be("Drama");
        result.DurationMinutes.Should().Be(150);
        result.Language.Should().Be("Vietnamese");
        result.RatingCode.Should().Be("R");
        result.PosterUrl.Should().Be("https://example.com/updated.jpg");
        result.TrailerUrl.Should().Be("https://youtube.com/trailer/updated");
    }

    #endregion

    #region === DeleteMovieAsync - Tests ===

    [Test]
    public async Task DeleteMovieAsync_ExistingMovie_DeletesSuccessfully()
    {
        // ========== ARRANGE ==========
        var movie = new Movie
        {
            Title = "Movie to Delete",
            Genre = "Drama",
            DurationMinutes = 100,
            Language = "English",
            RatingCode = "PG",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(20),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        var movieId = movie.Id;

        // Verify movie exists
        var movieBeforeDelete = await _dbContext.Movies.FindAsync(movieId);
        movieBeforeDelete.Should().NotBeNull();

        // ========== ACT ==========
        await _movieService.DeleteMovieAsync(movieId);

        // ========== ASSERT ==========
        var deletedMovie = await _dbContext.Movies.FindAsync(movieId);
        deletedMovie.Should().BeNull();
    }

    [Test]
    public async Task DeleteMovieAsync_NonExistentMovie_ThrowsKeyNotFoundException()
    {
        // ========== ARRANGE ==========
        const int nonExistentId = 999;

        // ========== ACT ==========
        var action = () => _movieService.DeleteMovieAsync(nonExistentId);

        // ========== ASSERT ==========
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{nonExistentId}*");
    }

    [Test]
    public async Task DeleteMovieAsync_RemovesMovieFromDatabase()
    {
        // ========== ARRANGE ==========
        var movie1 = new Movie
        {
            Title = "Movie 1",
            Genre = "Action",
            DurationMinutes = 120,
            Language = "English",
            RatingCode = "PG",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(20),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var movie2 = new Movie
        {
            Title = "Movie 2",
            Genre = "Drama",
            DurationMinutes = 110,
            Language = "English",
            RatingCode = "PG",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(20),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Movies.AddRange(movie1, movie2);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        await _movieService.DeleteMovieAsync(movie1.Id);

        // ========== ASSERT ==========
        var remainingMovies = await _dbContext.Movies.ToListAsync();
        remainingMovies.Should().HaveCount(1);
        remainingMovies[0].Title.Should().Be("Movie 2");
    }

    #endregion

    #region === Edge Cases ===

    [Test]
    public async Task GetAllMoviesAsync_WithSpecialCharactersInTitle_ReturnsCorrectly()
    {
        // ========== ARRANGE ==========
        var movie = new Movie
        {
            Title = "L'Amour: A Romantic Story & Drama",
            Description = "Test special chars: @#$%^&*()",
            Genre = "Romance",
            DurationMinutes = 120,
            Language = "French",
            RatingCode = "PG",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(20),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _movieService.GetAllMoviesAsync(onlyActive: true);

        // ========== ASSERT ==========
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("L'Amour: A Romantic Story & Drama");
        result[0].Description.Should().Contain("@#$%^&*()");
    }

    [Test]
    public async Task CreateMovieAsync_WithMinimalDuration_CreatesSuccessfully()
    {
        // ========== ARRANGE ==========
        var request = new CreateMovieDto
        {
            Title = "Short Film",
            Genre = "Short",
            DurationMinutes = 1,  // Minimal duration
            Language = "English",
            RatingCode = "G",
            ReleaseDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(20)
        };

        // ========== ACT ==========
        var result = await _movieService.CreateMovieAsync(request);

        // ========== ASSERT ==========
        result.DurationMinutes.Should().Be(1);
    }

    [Test]
    public async Task CreateMovieAsync_WithLongDuration_CreatesSuccessfully()
    {
        // ========== ARRANGE ==========
        var request = new CreateMovieDto
        {
            Title = "Extended Cut",
            Genre = "Epic",
            DurationMinutes = 480,  // 8 hours
            Language = "English",
            RatingCode = "PG-13",
            ReleaseDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(20)
        };

        // ========== ACT ==========
        var result = await _movieService.CreateMovieAsync(request);

        // ========== ASSERT ==========
        result.DurationMinutes.Should().Be(480);
    }

    #endregion

    #region === GetMovieDetailsWithShowtimesAsync - Tests ===

    [Test]
    public async Task GetMovieDetailsWithShowtimesAsync_ExistingMovie_ReturnsMovieWithShowtimeGroups()
    {
        // ========== ARRANGE ==========
        var room = new Room { Name = "Phòng 01", Capacity = 20, Type = "2D" };
        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync();

        var movie = new Movie
        {
            Title = "Avengers: Endgame",
            Description = "Epic superhero film",
            Director = "Russo Brothers",
            DurationMinutes = 180,
            PosterUrl = "https://example.com/avengers.jpg",
            BannerUrl = "https://example.com/avengers-banner.jpg",
            TrailerUrl = "https://youtube.com/avengers",
            Status = MovieStatus.NowShowing,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        // Navigation property must be set for in-memory to load it
        var showtimes = new[]
        {
            new Showtime { MovieId = movie.Id, RoomId = room.Id, StartTime = DateTime.UtcNow.AddHours(2), EndTime = DateTime.UtcNow.AddHours(4), BasePrice = 80000, TotalSeats = 20, BookedSeatsCount = 5, IsActive = true, Room = room },
            new Showtime { MovieId = movie.Id, RoomId = room.Id, StartTime = DateTime.UtcNow.AddHours(5), EndTime = DateTime.UtcNow.AddHours(7), BasePrice = 80000, TotalSeats = 20, BookedSeatsCount = 0, IsActive = true, Room = room }
        };
        _dbContext.Showtimes.AddRange(showtimes);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _movieService.GetMovieDetailsWithShowtimesAsync(movie.Id);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result!.Title.Should().Be("Avengers: Endgame");
        result.Description.Should().Be("Epic superhero film");
        result.DurationMinutes.Should().Be(180);
        result.Status.Should().Be("NowShowing");
        result.ShowtimeGroups.Should().HaveCount(1);
        result.ShowtimeGroups[0].Showtimes.Should().HaveCount(2);
        result.ShowtimeGroups[0].Showtimes.Should().Contain(s => s.AvailableSeats == 15);
        result.ShowtimeGroups[0].Showtimes.Should().Contain(s => s.AvailableSeats == 20);
        result.ShowtimeGroups[0].Showtimes.All(s => s.RoomName == "Phòng 01").Should().BeTrue();
    }

    [Test]
    public async Task GetMovieDetailsWithShowtimesAsync_NonExistingMovie_ReturnsNull()
    {
        // ========== ACT ==========
        var result = await _movieService.GetMovieDetailsWithShowtimesAsync(999);

        // ========== ASSERT ==========
        result.Should().BeNull();
    }

    [Test]
    public async Task GetMovieDetailsWithShowtimesAsync_MovieWithNoShowtimes_ReturnsEmptyGroups()
    {
        // ========== ARRANGE ==========
        var movie = new Movie
        {
            Title = "No Showtime Movie",
            Description = "This movie has no upcoming showtimes",
            Director = "Unknown",
            DurationMinutes = 90,
            IsActive = true,
            Status = MovieStatus.ComingSoon,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _movieService.GetMovieDetailsWithShowtimesAsync(movie.Id);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result!.Title.Should().Be("No Showtime Movie");
        result.ShowtimeGroups.Should().BeEmpty();
    }

    [Test]
    public async Task GetMovieDetailsWithShowtimesAsync_ExcludesPastShowtimes()
    {
        // ========== ARRANGE ==========
        var room = new Room { Name = "Phòng 01", Capacity = 20, Type = "2D" };
        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync();

        var movie = new Movie
        {
            Title = "Movie With Past Showtimes",
            IsActive = true,
            Status = MovieStatus.NowShowing,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        var futureSt = new Showtime
        {
            MovieId = movie.Id, RoomId = room.Id,
            StartTime = DateTime.UtcNow.AddHours(3), EndTime = DateTime.UtcNow.AddHours(5),
            BasePrice = 80000, TotalSeats = 20, BookedSeatsCount = 0, IsActive = true,
            Room = room
        };
        var pastSt = new Showtime
        {
            MovieId = movie.Id, RoomId = room.Id,
            StartTime = DateTime.UtcNow.AddHours(-5), EndTime = DateTime.UtcNow.AddHours(-3),
            BasePrice = 70000, TotalSeats = 20, BookedSeatsCount = 0, IsActive = true,
            Room = room
        };
        _dbContext.Showtimes.AddRange(futureSt, pastSt);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _movieService.GetMovieDetailsWithShowtimesAsync(movie.Id);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result!.ShowtimeGroups.Should().HaveCount(1);
        result.ShowtimeGroups[0].Showtimes.Should().HaveCount(1);
        result.ShowtimeGroups[0].Showtimes[0].BasePrice.Should().Be(80000);
    }

    [Test]
    public async Task GetMovieDetailsWithShowtimesAsync_MultipleDaysGroupedCorrectly()
    {
        // ========== ARRANGE ==========
        var room = new Room { Name = "Phòng 01", Capacity = 20, Type = "3D" };
        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync();

        var movie = new Movie
        {
            Title = "Long Running Movie",
            IsActive = true,
            Status = MovieStatus.NowShowing,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        var vietnamTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTz);
        var todayVn = new DateTime(nowVn.Year, nowVn.Month, nowVn.Day, 14, 0, 0, DateTimeKind.Unspecified);

        var day1Utc = TimeZoneInfo.ConvertTimeToUtc(todayVn.AddDays(0), vietnamTz).AddHours(2);
        var day1bUtc = TimeZoneInfo.ConvertTimeToUtc(todayVn.AddDays(0), vietnamTz).AddHours(5);
        var day2Utc = TimeZoneInfo.ConvertTimeToUtc(todayVn.AddDays(1), vietnamTz).AddHours(2);
        var day3Utc = TimeZoneInfo.ConvertTimeToUtc(todayVn.AddDays(2), vietnamTz).AddHours(2);

        var showtimes = new[]
        {
            new Showtime { MovieId = movie.Id, RoomId = room.Id, StartTime = day1Utc, EndTime = day1Utc.AddHours(2), BasePrice = 80000, TotalSeats = 20, BookedSeatsCount = 0, IsActive = true, Room = room },
            new Showtime { MovieId = movie.Id, RoomId = room.Id, StartTime = day1bUtc, EndTime = day1bUtc.AddHours(2), BasePrice = 80000, TotalSeats = 20, BookedSeatsCount = 0, IsActive = true, Room = room },
            new Showtime { MovieId = movie.Id, RoomId = room.Id, StartTime = day2Utc, EndTime = day2Utc.AddHours(2), BasePrice = 80000, TotalSeats = 20, BookedSeatsCount = 0, IsActive = true, Room = room },
            new Showtime { MovieId = movie.Id, RoomId = room.Id, StartTime = day3Utc, EndTime = day3Utc.AddHours(2), BasePrice = 80000, TotalSeats = 20, BookedSeatsCount = 0, IsActive = true, Room = room }
        };
        _dbContext.Showtimes.AddRange(showtimes);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _movieService.GetMovieDetailsWithShowtimesAsync(movie.Id);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result!.ShowtimeGroups.Should().HaveCount(3);
        result.ShowtimeGroups.All(g => g.Showtimes.All(s => s.RoomName == "Phòng 01" && s.RoomType == "3D")).Should().BeTrue();
    }

    [Test]
    public async Task GetMovieDetailsWithShowtimesAsync_ShowtimeContainsRoomInfo()
    {
        // ========== ARRANGE ==========
        var room = new Room { Name = "Phòng IMAX", Capacity = 50, Type = "IMAX" };
        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync();

        var movie = new Movie
        {
            Title = "IMAX Movie",
            IsActive = true,
            Status = MovieStatus.NowShowing,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        var showtime = new Showtime
        {
            MovieId = movie.Id,
            RoomId = room.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(3),
            BasePrice = 150000,
            TotalSeats = 50,
            BookedSeatsCount = 10,
            IsActive = true,
            Room = room
        };
        _dbContext.Showtimes.Add(showtime);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _movieService.GetMovieDetailsWithShowtimesAsync(movie.Id);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result!.ShowtimeGroups.Should().HaveCount(1);
        var st = result.ShowtimeGroups[0].Showtimes[0];
        st.RoomName.Should().Be("Phòng IMAX");
        st.RoomType.Should().Be("IMAX");
        st.BasePrice.Should().Be(150000);
        st.AvailableSeats.Should().Be(40);
    }

    [Test]
    public async Task GetMovieDetailsWithShowtimesAsync_ExcludesInactiveShowtimes()
    {
        // ========== ARRANGE ==========
        var room = new Room { Name = "Phòng 01", Capacity = 20, Type = "2D" };
        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync();

        var movie = new Movie
        {
            Title = "Movie With Mixed Showtimes",
            IsActive = true,
            Status = MovieStatus.NowShowing,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        var activeSt = new Showtime
        {
            MovieId = movie.Id, RoomId = room.Id,
            StartTime = DateTime.UtcNow.AddHours(1), EndTime = DateTime.UtcNow.AddHours(3),
            BasePrice = 80000, TotalSeats = 20, BookedSeatsCount = 0, IsActive = true,
            Room = room
        };
        var inactiveSt = new Showtime
        {
            MovieId = movie.Id, RoomId = room.Id,
            StartTime = DateTime.UtcNow.AddHours(4), EndTime = DateTime.UtcNow.AddHours(6),
            BasePrice = 80000, TotalSeats = 20, BookedSeatsCount = 0, IsActive = false,
            Room = room
        };
        _dbContext.Showtimes.AddRange(activeSt, inactiveSt);
        await _dbContext.SaveChangesAsync();

        // ========== ACT ==========
        var result = await _movieService.GetMovieDetailsWithShowtimesAsync(movie.Id);

        // ========== ASSERT ==========
        result.Should().NotBeNull();
        result!.ShowtimeGroups.Sum(g => g.Showtimes.Count).Should().Be(1);
        result.ShowtimeGroups[0].Showtimes[0].RoomName.Should().Be("Phòng 01");
    }

    #endregion
}
