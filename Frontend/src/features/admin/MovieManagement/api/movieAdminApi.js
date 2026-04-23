import axiosClient from "@/services/axiosClient";

/**
 * Movie Admin API Service
 * Handles CRUD operations for movie management
 */

export const movieAdminApi = {
  // Get all movies (public endpoint, returns active movies)
  getAllMovies: (params) => 
    axiosClient.get('/movies', { params }),

  // Get single movie by ID
  getMovieById: (id) => 
    axiosClient.get(`/movies/${id}`),

  // Create new movie (Admin only - requires JWT)
  createMovie: (data) => 
    axiosClient.post('/movies', data),

  // Update existing movie (Admin only - requires JWT)
  updateMovie: (id, data) => 
    axiosClient.put(`/movies/${id}`, data),

  // Delete movie (Admin only - requires JWT)
  deleteMovie: (id) => 
    axiosClient.delete(`/movies/${id}`),
};
