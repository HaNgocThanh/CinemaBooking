import axiosClient from '@/services/axiosClient';

/**
 * Showtime Admin API Service
 * Handles CRUD operations for showtime management
 */
export const showtimeAdminApi = {
  getAllShowtimes: () =>
    axiosClient.get('/showtimes'),

  createShowtime: (data) =>
    axiosClient.post('/showtimes', data),

  deleteShowtime: (id) =>
    axiosClient.delete(`/showtimes/${id}`),
};

export const roomApi = {
  getAllRooms: () =>
    axiosClient.get('/rooms'),
};

export const movieApi = {
  getAllMovies: (params) =>
    axiosClient.get('/movies', { params }),
};
