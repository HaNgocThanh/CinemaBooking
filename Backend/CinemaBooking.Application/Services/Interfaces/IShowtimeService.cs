using System.Threading.Tasks;
using System.Collections.Generic;
using CinemaBooking.Application.DTOs.Showtimes;

namespace CinemaBooking.Application.Services.Interfaces;

public interface IShowtimeService
{
    Task<int> CreateShowtimeAsync(CreateShowtimeDto dto);
    Task<List<ShowtimeResponseDto>> GetAllShowtimesAsync();
}
