using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CinemaBooking.Application.DTOs.Common;
using CinemaBooking.Application.DTOs.Rooms;
using CinemaBooking.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(IRoomService roomService, ILogger<RoomsController> logger)
    {
        _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResponse<List<RoomResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiSuccessResponse<List<RoomResponseDto>>>> GetAllRooms()
    {
        _logger.LogInformation("Fetching all rooms");

        var rooms = await _roomService.GetAllRoomsAsync();

        return Ok(new ApiSuccessResponse<List<RoomResponseDto>>(
            data: rooms,
            message: $"Lay danh sach {rooms.Count} phong thanh cong.",
            traceId: HttpContext.TraceIdentifier
        ));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<RoomResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiSuccessResponse<RoomResponseDto>>> GetRoomById(int id)
    {
        _logger.LogInformation("Fetching room with ID: {RoomId}", id);

        var room = await _roomService.GetRoomByIdAsync(id);
        if (room == null)
        {
            return NotFound(new ApiErrorResponse(
                code: "ROOM_NOT_FOUND",
                message: $"Khong tim thay phong chieu voi ID: {id}",
                httpStatus: StatusCodes.Status404NotFound,
                traceId: HttpContext.TraceIdentifier
            ));
        }

        return Ok(new ApiSuccessResponse<RoomResponseDto>(
            data: room,
            message: $"Lay phong chieu ID {id} thanh cong.",
            traceId: HttpContext.TraceIdentifier
        ));
    }

    [HttpGet("{id:int}/seats")]
    [ProducesResponseType(typeof(ApiSuccessResponse<List<SeatTemplateDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiSuccessResponse<List<SeatTemplateDto>>>> GetSeatTemplatesByRoom(int id)
    {
        _logger.LogInformation("Fetching seat templates for room ID: {RoomId}", id);

        var seats = await _roomService.GetSeatTemplatesByRoomIdAsync(id);

        return Ok(new ApiSuccessResponse<List<SeatTemplateDto>>(
            data: seats,
            message: $"Lay {seats.Count} mau ghe cho phong ID {id} thanh cong.",
            traceId: HttpContext.TraceIdentifier
        ));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiSuccessResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiSuccessResponse<int>>> CreateRoom([FromBody] CreateRoomDto dto)
    {
        _logger.LogInformation("Creating new room: {RoomName}", dto.Name);

        try
        {
            var roomId = await _roomService.CreateRoomAsync(dto);

            return CreatedAtAction(
                nameof(GetRoomById),
                new { id = roomId },
                new ApiSuccessResponse<int>(
                    data: roomId,
                    message: $"Tao phong chieu '{dto.Name}' thanh cong.",
                    traceId: HttpContext.TraceIdentifier
                ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiErrorResponse(
                code: "ROOM_DUPLICATE",
                message: ex.Message,
                httpStatus: StatusCodes.Status400BadRequest,
                traceId: HttpContext.TraceIdentifier
            ));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiSuccessResponse<bool>>> UpdateRoom(int id, [FromBody] UpdateRoomDto dto)
    {
        _logger.LogInformation("Updating room ID: {RoomId}", id);

        var updated = await _roomService.UpdateRoomAsync(id, dto);
        if (!updated)
        {
            return NotFound(new ApiErrorResponse(
                code: "ROOM_NOT_FOUND",
                message: $"Khong tim thay phong chieu voi ID: {id}",
                httpStatus: StatusCodes.Status404NotFound,
                traceId: HttpContext.TraceIdentifier
            ));
        }

        return Ok(new ApiSuccessResponse<bool>(
            data: true,
            message: $"Cap nhat phong chieu ID {id} thanh cong.",
            traceId: HttpContext.TraceIdentifier
        ));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiSuccessResponse<bool>>> DeleteRoom(int id)
    {
        _logger.LogInformation("Deleting room ID: {RoomId}", id);

        try
        {
            var deleted = await _roomService.DeleteRoomAsync(id);
            if (!deleted)
            {
                return NotFound(new ApiErrorResponse(
                    code: "ROOM_NOT_FOUND",
                    message: $"Khong tim thay phong chieu voi ID: {id}",
                    httpStatus: StatusCodes.Status404NotFound,
                    traceId: HttpContext.TraceIdentifier
                ));
            }

            return Ok(new ApiSuccessResponse<bool>(
                data: true,
                message: $"Xoa phong chieu ID {id} thanh cong.",
                traceId: HttpContext.TraceIdentifier
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiErrorResponse(
                code: "ROOM_HAS_ACTIVESHOWTIMES",
                message: ex.Message,
                httpStatus: StatusCodes.Status400BadRequest,
                traceId: HttpContext.TraceIdentifier
            ));
        }
    }
}
