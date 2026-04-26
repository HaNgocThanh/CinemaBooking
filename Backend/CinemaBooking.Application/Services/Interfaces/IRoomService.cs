using System.Collections.Generic;
using System.Threading.Tasks;
using CinemaBooking.Application.DTOs.Rooms;

namespace CinemaBooking.Application.Services.Interfaces;

public interface IRoomService
{
    Task<List<RoomResponseDto>> GetAllRoomsAsync();
    Task<RoomResponseDto?> GetRoomByIdAsync(int id);
    Task<int> CreateRoomAsync(CreateRoomDto dto);
    Task<bool> UpdateRoomAsync(int id, UpdateRoomDto dto);
    Task<bool> DeleteRoomAsync(int id);
    Task<List<SeatTemplateDto>> GetSeatTemplatesByRoomIdAsync(int roomId);
}
