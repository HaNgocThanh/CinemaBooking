using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CinemaBooking.Application.DTOs.Common;
using CinemaBooking.Application.DTOs.Movies;
using CinemaBooking.Application.Services.Interfaces;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;
    private readonly ILogger<MoviesController> _logger;

    public MoviesController(IMovieService movieService, ILogger<MoviesController> logger)
    {
        _movieService = movieService ?? throw new ArgumentNullException(nameof(movieService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiSuccessResponse<List<MovieResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiSuccessResponse<List<MovieResponseDto>>>> GetAllMovies([FromQuery] string? status = null)
    {
        _logger.LogInformation("Fetching all active movies with status: {Status}", status ?? "all");

        var movies = await _movieService.GetAllMoviesAsync(onlyActive: true, status: status);

        return Ok(new ApiSuccessResponse<List<MovieResponseDto>>(
            data: movies,
            message: $"Lay danh sach {movies.Count} phim thanh cong.",
            traceId: HttpContext.TraceIdentifier
        ));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiSuccessResponse<MovieResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiSuccessResponse<MovieResponseDto>>> GetMovieById([FromRoute] int id)
    {
        _logger.LogInformation("Fetching movie with ID: {MovieId}", id);

        var movie = await _movieService.GetMovieByIdAsync(id);

        if (movie == null)
        {
            _logger.LogWarning("Movie with ID {MovieId} not found", id);
            return NotFound(new ApiErrorResponse(
                code: "MOVIE_NOT_FOUND",
                message: $"Phim voi ID {id} khong ton tai.",
                httpStatus: 404,
                details: null,
                traceId: HttpContext.TraceIdentifier
            ));
        }

        return Ok(new ApiSuccessResponse<MovieResponseDto>(
            data: movie,
            message: "Lay chi tiet phim thanh cong.",
            traceId: HttpContext.TraceIdentifier
        ));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiSuccessResponse<MovieResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiSuccessResponse<MovieResponseDto>>> CreateMovie(
        [FromBody] CreateMovieDto request)
    {
        _logger.LogInformation("Creating new movie: {Title}", request.Title);

        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiErrorResponse(
                code: "VALIDATION_ERROR",
                message: "Request khong hop le.",
                httpStatus: 400,
                details: null,
                traceId: HttpContext.TraceIdentifier
            ));
        }

        var movie = await _movieService.CreateMovieAsync(request);

        _logger.LogInformation("Movie created successfully with ID: {MovieId}", movie.Id);

        return CreatedAtAction(
            actionName: nameof(GetMovieById),
            routeValues: new { id = movie.Id },
            value: new ApiSuccessResponse<MovieResponseDto>(
                data: movie,
                message: "Phim duoc tao thanh cong.",
                traceId: HttpContext.TraceIdentifier
            )
        );
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiSuccessResponse<MovieResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiSuccessResponse<MovieResponseDto>>> UpdateMovie(
        [FromRoute] int id,
        [FromBody] UpdateMovieDto request)
    {
        _logger.LogInformation("Updating movie with ID: {MovieId}", id);

        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiErrorResponse(
                code: "VALIDATION_ERROR",
                message: "Request khong hop le.",
                httpStatus: 400,
                details: null,
                traceId: HttpContext.TraceIdentifier
            ));
        }

        try
        {
            var movie = await _movieService.UpdateMovieAsync(id, request);

            _logger.LogInformation("Movie with ID {MovieId} updated successfully", id);

            return Ok(new ApiSuccessResponse<MovieResponseDto>(
                data: movie,
                message: "Phim duoc cap nhat thanh cong.",
                traceId: HttpContext.TraceIdentifier
            ));
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Movie with ID {MovieId} not found for update", id);
            return NotFound(new ApiErrorResponse(
                code: "MOVIE_NOT_FOUND",
                message: $"Phim voi ID {id} khong ton tai.",
                httpStatus: 404,
                details: null,
                traceId: HttpContext.TraceIdentifier
            ));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteMovie([FromRoute] int id)
    {
        _logger.LogInformation("Deleting movie with ID: {MovieId}", id);

        try
        {
            await _movieService.DeleteMovieAsync(id);

            _logger.LogInformation("Movie with ID {MovieId} deleted successfully", id);

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Movie with ID {MovieId} not found for delete", id);
            return NotFound(new ApiErrorResponse(
                code: "MOVIE_NOT_FOUND",
                message: $"Phim voi ID {id} khong ton tai.",
                httpStatus: 404,
                details: null,
                traceId: HttpContext.TraceIdentifier
            ));
        }
    }
}
