using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CinemaBooking.Application.DTOs.Common;
using CinemaBooking.Application.DTOs.Showtimes;
using CinemaBooking.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShowtimesController : ControllerBase
{
    private readonly IShowtimeService _showtimeService;
    private readonly ILogger<ShowtimesController> _logger;

    public ShowtimesController(IShowtimeService showtimeService, ILogger<ShowtimesController> logger)
    {
        _showtimeService = showtimeService ?? throw new ArgumentNullException(nameof(showtimeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResponse<List<ShowtimeResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiSuccessResponse<List<ShowtimeResponseDto>>>> GetAllShowtimes()
    {
        _logger.LogInformation("Fetching all showtimes");

        var showtimes = await _showtimeService.GetAllShowtimesAsync();

        return Ok(new ApiSuccessResponse<List<ShowtimeResponseDto>>(
            data: showtimes,
            message: $"Lay danh sach {showtimes.Count} suat chieu thanh cong.",
            traceId: HttpContext.TraceIdentifier
        ));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiSuccessResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiSuccessResponse<object>>> CreateShowtime([FromBody] CreateShowtimeDto dto)
    {
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
            var showtimeId = await _showtimeService.CreateShowtimeAsync(dto);

            _logger.LogInformation("Showtime created successfully with ID: {ShowtimeId}", showtimeId);

            return CreatedAtAction(
                actionName: nameof(GetAllShowtimes),
                value: new ApiSuccessResponse<object>(
                    data: new { ShowtimeId = showtimeId },
                    message: "Tao suat chieu thanh cong.",
                    traceId: HttpContext.TraceIdentifier
                )
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating showtime");
            return BadRequest(new ApiErrorResponse(
                code: "CREATE_SHOWTIME_FAILED",
                message: ex.Message,
                httpStatus: 400,
                details: null,
                traceId: HttpContext.TraceIdentifier
            ));
        }
    }

    /// <summary>
    /// Lay chi tiet mot suat chieu theo ID.
    /// </summary>
    /// <param name="id">ID cua suat chieu.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<ShowtimeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiSuccessResponse<ShowtimeResponseDto>>> GetShowtimeById(int id)
    {
        _logger.LogInformation("Fetching showtime with ID: {ShowtimeId}", id);

        var showtime = await _showtimeService.GetByIdAsync(id);

        if (showtime == null)
        {
            return NotFound(new ApiErrorResponse(
                code: "SHOWTIME_NOT_FOUND",
                message: $"Khong tim thay suat chieu voi ID: {id}",
                httpStatus: 404,
                details: null,
                traceId: HttpContext.TraceIdentifier
            ));
        }

        return Ok(new ApiSuccessResponse<ShowtimeResponseDto>(
            data: showtime,
            message: "Lay chi tiet suat chieu thanh cong.",
            traceId: HttpContext.TraceIdentifier
        ));
    }

    /// <summary>
    /// Lay danh sach tat ca ghe cua mot suat chieu.
    /// </summary>
    /// <param name="id">ID cua suat chieu.</param>
    [HttpGet("{id:int}/seats")]
    [ProducesResponseType(typeof(ApiSuccessResponse<List<ShowtimeSeatDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiSuccessResponse<List<ShowtimeSeatDto>>>> GetSeatsByShowtime(int id)
    {
        _logger.LogInformation("Fetching seats for showtime with ID: {ShowtimeId}", id);

        var seats = await _showtimeService.GetSeatsByShowtimeAsync(id);

        return Ok(new ApiSuccessResponse<List<ShowtimeSeatDto>>(
            data: seats,
            message: $"Lay danh sach {seats.Count} ghe thanh cong.",
            traceId: HttpContext.TraceIdentifier
        ));
    }

    /// <summary>
    /// Cap nhat mot suat chieu theo ID.
    ///
    /// Cac truong nullable — neu khong cung cap, gia tri hien tai duoc giu nguyen.
    ///
    /// Khi StartTime duoc cung cap, EndTime se tu dong duoc tinh lai:
    /// EndTime = StartTime + DurationMinutes cua phim + 15 phut nghi.
    ///
    /// Collision check: neu thay doi RoomId hoac StartTime, he thong se kiem tra
    /// xem co suat chieu nao khac trung gio trong cung phong hay khong.
    /// Neu co, tra ve 409 Conflict.
    ///
    /// Rang buoc:
    /// - Neu suat chieu da co ve duoc dat (BookedSeatsCount > 0), chi cho phep sua:
    ///   StartTime (neu khong gay collision), BasePrice, IsActive.
    ///   Khong cho doi MovieId hoac RoomId khi da co ve.
    /// </summary>
    /// <param name="id">ID cua suat chieu can cap nhat.</param>
    /// <param name="dto">Cac truong can cap nhat.</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiSuccessResponse<object>>> UpdateShowtime(
        int id,
        [FromBody] UpdateShowtimeDto dto)
    {
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
            await _showtimeService.UpdateShowtimeAsync(id, dto);

            _logger.LogInformation("Showtime updated successfully with ID: {ShowtimeId}", id);

            return Ok(new ApiSuccessResponse<object>(
                data: new { ShowtimeId = id },
                message: "Cap nhat suat chieu thanh cong.",
                traceId: HttpContext.TraceIdentifier
            ));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Showtime not found: {ShowtimeId}", id);
            return NotFound(new ApiErrorResponse(
                code: "SHOWTIME_NOT_FOUND",
                message: ex.Message,
                httpStatus: 404,
                details: null,
                traceId: HttpContext.TraceIdentifier
            ));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict or invalid operation updating showtime: {ShowtimeId}", id);

            var code = ex.Message.Contains("trung gio")
                ? "SHOWTIME_CONFLICT"
                : "UPDATE_SHOWTIME_FAILED";

            int statusCode = code == "SHOWTIME_CONFLICT" ? 409 : 400;

            return StatusCode(statusCode, new ApiErrorResponse(
                code: code,
                message: ex.Message,
                httpStatus: statusCode,
                details: null,
                traceId: HttpContext.TraceIdentifier
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating showtime: {ShowtimeId}", id);
            return BadRequest(new ApiErrorResponse(
                code: "UPDATE_SHOWTIME_FAILED",
                message: ex.Message,
                httpStatus: 400,
                details: null,
                traceId: HttpContext.TraceIdentifier
            ));
        }
    }
}
