using CinemaBooking.Application.DTOs.Common;
using CinemaBooking.Application.DTOs.Movies;
using CinemaBooking.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers;

/// <summary>
/// Controller quản lý các endpoints liên quan đến phim (Movies).
/// 
/// Endpoints công khai:
/// - GET /api/movies              - Danh sách phim
/// - GET /api/movies/{id}         - Chi tiết phim
/// 
/// Endpoints chỉ dành cho Admin:
/// - POST /api/movies             - Tạo phim
/// - PUT /api/movies/{id}         - Cập nhật phim
/// - DELETE /api/movies/{id}      - Xóa phim
/// </summary>
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

    /// <summary>
    /// Lấy danh sách tất cả phim đang hoạt động.
    /// 
    /// Endpoint công khai - không cần JWT Token.
    /// </summary>
    /// <returns>200 OK - Danh sách MovieResponseDto</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiSuccessResponse<List<MovieResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiSuccessResponse<List<MovieResponseDto>>>> GetAllMovies([FromQuery] string? status = null)
    {
        _logger.LogInformation("Fetching all active movies with status: {Status}", status ?? "all");

        var movies = await _movieService.GetAllMoviesAsync(onlyActive: true, status: status);

        return Ok(new ApiSuccessResponse<List<MovieResponseDto>>(
            data: movies,
            message: $"Lấy danh sách {movies.Count} phim thành công.",
            traceId: HttpContext.TraceIdentifier
        ));
    }

    /// <summary>
    /// Lấy chi tiết phim theo ID.
    /// 
    /// Endpoint công khai - không cần JWT Token.
    /// </summary>
    /// <param name="id">ID phim</param>
    /// <returns>
    /// 200 OK - Chi tiết phim
    /// 404 Not Found - Phim không tồn tại
    /// </returns>
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
                message: $"Phim với ID {id} không tồn tại.",
                httpStatus: 404,
                details: null,
                traceId: HttpContext.TraceIdentifier
            ));
        }

        return Ok(new ApiSuccessResponse<MovieResponseDto>(
            data: movie,
            message: "Lấy chi tiết phim thành công.",
            traceId: HttpContext.TraceIdentifier
        ));
    }

    /// <summary>
    /// Tạo phim mới (dành cho Admin).
    /// 
    /// Endpoint bảo vệ bằng JWT Token - cần quyền Admin.
    /// </summary>
    /// <param name="request">CreateMovieDto</param>
    /// <returns>
    /// 201 Created - Phim được tạo thành công
    /// 400 Bad Request - Request không hợp lệ
    /// 401 Unauthorized - Không có JWT Token
    /// </returns>
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
                message: "Request không hợp lệ.",
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
                message: "Phim được tạo thành công.",
                traceId: HttpContext.TraceIdentifier
            )
        );
    }

    /// <summary>
    /// Cập nhật thông tin phim (dành cho Admin).
    /// 
    /// Endpoint bảo vệ bằng JWT Token - cần quyền Admin.
    /// </summary>
    /// <param name="id">ID phim cần cập nhật</param>
    /// <param name="request">UpdateMovieDto</param>
    /// <returns>
    /// 200 OK - Phim được cập nhật thành công
    /// 404 Not Found - Phim không tồn tại
    /// 400 Bad Request - Request không hợp lệ
    /// 401 Unauthorized - Không có JWT Token
    /// </returns>
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
                message: "Request không hợp lệ.",
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
                message: "Phim được cập nhật thành công.",
                traceId: HttpContext.TraceIdentifier
            ));
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Movie with ID {MovieId} not found for update", id);
            return NotFound(new ApiErrorResponse(
                code: "MOVIE_NOT_FOUND",
                message: $"Phim với ID {id} không tồn tại.",
                httpStatus: 404,
                details: null,
                traceId: HttpContext.TraceIdentifier
            ));
        }
    }

    /// <summary>
    /// Xóa phim (dành cho Admin).
    /// 
    /// Endpoint bảo vệ bằng JWT Token - cần quyền Admin.
    /// </summary>
    /// <param name="id">ID phim cần xóa</param>
    /// <returns>
    /// 204 No Content - Phim được xóa thành công
    /// 404 Not Found - Phim không tồn tại
    /// 401 Unauthorized - Không có JWT Token
    /// </returns>
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
                message: $"Phim với ID {id} không tồn tại.",
                httpStatus: 404,
                details: null,
                traceId: HttpContext.TraceIdentifier
            ));
        }
    }
}
