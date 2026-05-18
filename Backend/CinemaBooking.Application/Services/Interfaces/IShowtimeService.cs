using System.Threading.Tasks;
using System.Collections.Generic;
using CinemaBooking.Application.DTOs.Showtimes;

namespace CinemaBooking.Application.Services.Interfaces;

public interface IShowtimeService
{
    Task<int> CreateShowtimeAsync(CreateShowtimeDto dto);
    Task<List<ShowtimeResponseDto>> GetAllShowtimesAsync();
    Task<ShowtimeResponseDto?> GetByIdAsync(int id);
    Task<List<ShowtimeSeatDto>> GetSeatsByShowtimeAsync(int showtimeId);
    Task UpdateShowtimeAsync(int id, UpdateShowtimeDto dto);
}
