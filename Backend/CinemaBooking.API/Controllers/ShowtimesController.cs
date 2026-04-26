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
}
