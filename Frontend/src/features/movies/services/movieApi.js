import { useQuery, useMutation } from '@tanstack/react-query';
import axiosClient from '../../../services/axiosClient';
import queryClient from '../../../config/queryClient';

/**
 * Fetch all movies
 * @param {Object} filters - { page, limit, status }
 * @returns {Promise}
 */
const fetchMovies = (filters = {}) => {
  const params = new URLSearchParams();
  if (filters.page) params.append('page', filters.page);
  if (filters.limit) params.append('limit', filters.limit);
  if (filters.status) params.append('status', filters.status);

  return axiosClient.get(`/movies?${params.toString()}`);
};

/**
 * Fetch single movie by ID
 * @param {string} movieId
 * @returns {Promise}
 */
const fetchMovieById = (movieId) => {
  return axiosClient.get(`/movies/${movieId}`);
};

/**
 * Fetch showtimes for a movie
 * @param {string} movieId
 * @param {Object} filters - { date, cinemaId }
 * @returns {Promise}
 */
const fetchShowtimes = (movieId, filters = {}) => {
  const params = new URLSearchParams();
  if (filters.date) params.append('date', filters.date);
  if (filters.cinemaId) params.append('cinemaId', filters.cinemaId);

  return axiosClient.get(`/movies/${movieId}/showtimes?${params.toString()}`);
};

/**
 * Fetch seats for a showtime
 * @param {string} showtimeId
 * @returns {Promise}
 */
const fetchSeats = (showtimeId) => {
  return axiosClient.get(`/showtimes/${showtimeId}/seats`);
};

// ===== React Query Hooks =====

/**
 * Hook for fetching all movies
 */
export function useMovies(filters) {
  return useQuery({
    queryKey: ['movies', filters],
    queryFn: () => fetchMovies(filters),
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
}

/**
 * Hook for fetching single movie
 */
export function useMovieById(movieId) {
  return useQuery({
    queryKey: ['movie', movieId],
    queryFn: () => fetchMovieById(movieId),
    enabled: !!movieId,
    staleTime: 1000 * 60 * 10, // 10 minutes
  });
}

/**
 * Hook for fetching showtimes
 */
export function useShowtimes(movieId, filters) {
  return useQuery({
    queryKey: ['showtimes', movieId, filters],
    queryFn: () => fetchShowtimes(movieId, filters),
    enabled: !!movieId,
    staleTime: 1000 * 60 * 2, // 2 minutes
  });
}

/**
 * Hook for fetching seats (with polling every 5 seconds)
 */
export function useSeats(showtimeId) {
  return useQuery({
    queryKey: ['seats', showtimeId],
    queryFn: () => fetchSeats(showtimeId),
    enabled: !!showtimeId,
    staleTime: 1000 * 1, // 1 second
    refetchInterval: 5000, // Poll every 5 seconds
  });
}

/**
 * Hook for locking a seat
 */
export function useLockSeat() {
  return useMutation({
    mutationFn: ({ showtimeId, seatId }) => {
      return axiosClient.post(`/showtimes/${showtimeId}/seats/${seatId}/lock`);
    },
    onSuccess: (data, { showtimeId }) => {
      queryClient.invalidateQueries({ queryKey: ['seats', showtimeId] });
    },
    onError: (error, { showtimeId }) => {
      if (error.message.includes('already locked') || error.message.includes('already booked')) {
        queryClient.invalidateQueries({ queryKey: ['seats', showtimeId] });
      }
    },
  });
}
