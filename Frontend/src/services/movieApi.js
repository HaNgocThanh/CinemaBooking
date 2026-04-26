import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import axiosClient from './axiosClient';

/**
 * Movie API Service using React Query
 * Cung cấp hooks để fetch, tạo, cập nhật, xóa phim
 */

// ========================================
// API Functions (non-hook)
// ========================================

const movieApi = {
  /**
   * Lấy danh sách tất cả phim đang hoạt động
   * @param {string} status - Trạng thái phim (now-showing / coming-soon)
   * @returns {Promise} Trả về { success, data, message }
   */
  getAllMovies: async (status) => {
    const params = status ? `?status=${status}` : '';
    const response = await axiosClient.get(`/movies${params}`);
    return response.data;
  },

  /**
   * Lấy chi tiết phim theo ID
   * @param {number} movieId - ID phim
   * @returns {Promise} Trả về { success, data, message }
   */
  getMovieById: async (movieId) => {
    const response = await axiosClient.get(`/movies/${movieId}`);
    return response.data;
  },

  /**
   * Tạo phim mới (Admin only)
   * @param {Object} data - CreateMovieDto
   * @returns {Promise} Trả về { success, data, message }
   */
  createMovie: async (data) => {
    const response = await axiosClient.post('/movies', data);
    return response.data;
  },

  /**
   * Cập nhật phim (Admin only)
   * @param {number} movieId - ID phim
   * @param {Object} data - UpdateMovieDto
   * @returns {Promise} Trả về { success, data, message }
   */
  updateMovie: async (movieId, data) => {
    const response = await axiosClient.put(`/movies/${movieId}`, data);
    return response.data;
  },

  /**
   * Xóa phim (Admin only)
   * @param {number} movieId - ID phim
   * @returns {Promise} Trả về 204 No Content
   */
  deleteMovie: async (movieId) => {
    const response = await axiosClient.delete(`/movies/${movieId}`);
    return response.data;
  },
};

// ========================================
// React Query Hooks
// ========================================

/**
 * Hook lấy danh sách phim
 * @param {Object} options - React Query options
 * @returns {Object} { data, isLoading, error, refetch }
 */
export const useMovies = (options = {}) => {
  return useQuery({
    queryKey: ['movies'],
    queryFn: async () => {
      const response = await movieApi.getAllMovies();
      // movieApi.getAllMovies() returns { success, data: [...], message, traceId }
      // Don't call .data again - that's handled by axiosClient & movieApi
      return response || [];
    },
    staleTime: 5 * 60 * 1000, // 5 phút
    gcTime: 10 * 60 * 1000, // 10 phút (formerly cacheTime in v5)
    ...options,
  });
};

/**
 * Hook lấy danh sách phim đang chiếu
 * @param {Object} options - React Query options
 */
export const useNowShowingMovies = (options = {}) => {
  return useQuery({
    queryKey: ['movies', 'NowShowing'],
    queryFn: async () => {
      const response = await movieApi.getAllMovies('NowShowing');
      return response || [];
    },
    staleTime: 5 * 60 * 1000,
    gcTime: 10 * 60 * 1000,
    ...options,
  });
};

/**
 * Hook lấy danh sách phim sắp chiếu
 * @param {Object} options - React Query options
 */
export const useComingSoonMovies = (options = {}) => {
  return useQuery({
    queryKey: ['movies', 'ComingSoon'],
    queryFn: async () => {
      const response = await movieApi.getAllMovies('ComingSoon');
      return response || [];
    },
    staleTime: 5 * 60 * 1000,
    gcTime: 10 * 60 * 1000,
    ...options,
  });
};

/**
 * Hook lấy chi tiết phim theo ID
 * @param {number} movieId - ID phim
 * @param {Object} options - React Query options
 * @returns {Object} { data, isLoading, error, refetch }
 */
export const useMovieById = (movieId, options = {}) => {
  return useQuery({
    queryKey: ['movie', movieId],
    queryFn: async () => {
      const response = await movieApi.getMovieById(movieId);
      return response.data;
    },
    enabled: !!movieId, // Chỉ fetch khi movieId không null
    staleTime: 5 * 60 * 1000,
    gcTime: 10 * 60 * 1000,
    ...options,
  });
};

/**
 * Hook tạo phim mới (Admin)
 * @param {Object} options - React Query mutation options
 * @returns {Object} { mutate, mutateAsync, isLoading, error }
 */
export const useCreateMovie = (options = {}) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data) => movieApi.createMovie(data),
    onSuccess: () => {
      // Invalidate movies list cache
      queryClient.invalidateQueries({ queryKey: ['movies'] });
    },
    ...options,
  });
};

/**
 * Hook cập nhật phim (Admin)
 * @param {Object} options - React Query mutation options
 * @returns {Object} { mutate, mutateAsync, isLoading, error }
 */
export const useUpdateMovie = (options = {}) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ movieId, data }) => movieApi.updateMovie(movieId, data),
    onSuccess: (response) => {
      // Invalidate movies list và specific movie cache
      queryClient.invalidateQueries({ queryKey: ['movies'] });
      if (response.data?.id) {
        queryClient.invalidateQueries({ queryKey: ['movie', response.data.id] });
      }
    },
    ...options,
  });
};

/**
 * Hook xóa phim (Admin)
 * @param {Object} options - React Query mutation options
 * @returns {Object} { mutate, mutateAsync, isLoading, error }
 */
export const useDeleteMovie = (options = {}) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (movieId) => movieApi.deleteMovie(movieId),
    onSuccess: () => {
      // Invalidate movies list cache
      queryClient.invalidateQueries({ queryKey: ['movies'] });
    },
    ...options,
  });
};

// ========================================
// Showtime & Seat Hooks (Customer)
// ========================================

/**
 * Hook fetch showtimes cho một phim
 * @param {number} movieId
 * @param {Object} filters - { date, cinemaId }
 */
export function useShowtimes(movieId, filters) {
  return useQuery({
    queryKey: ['showtimes', movieId, filters],
    queryFn: () => {
      const params = new URLSearchParams();
      if (filters?.date) params.append('date', filters.date);
      if (filters?.cinemaId) params.append('cinemaId', filters.cinemaId);
      return axiosClient.get(`/movies/${movieId}/showtimes?${params.toString()}`);
    },
    enabled: !!movieId,
    staleTime: 1000 * 60 * 2,
  });
}

/**
 * Hook fetch seats cho một suất chiếu (polling mỗi 5s)
 * @param {number} showtimeId
 */
export function useSeats(showtimeId) {
  return useQuery({
    queryKey: ['seats', showtimeId],
    queryFn: () => axiosClient.get(`/showtimes/${showtimeId}/seats`),
    enabled: !!showtimeId,
    staleTime: 1000,
    refetchInterval: 5000,
  });
}

/**
 * Hook lock seat (mutation)
 */
export function useLockSeat() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ showtimeId, seatId }) =>
      axiosClient.post(`/showtimes/${showtimeId}/seats/${seatId}/lock`),
    onSuccess: (data, { showtimeId }) => {
      queryClient.invalidateQueries({ queryKey: ['seats', showtimeId] });
    },
    onError: (error, { showtimeId }) => {
      if (error.message?.includes('already locked') || error.message?.includes('already booked')) {
        queryClient.invalidateQueries({ queryKey: ['seats', showtimeId] });
      }
    },
  });
}

export default movieApi;

