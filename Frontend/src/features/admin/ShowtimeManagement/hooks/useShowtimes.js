import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { showtimeAdminApi, roomApi, movieApi } from '../api/showtimeAdminApi';

/**
 * Custom Hook: useShowtimes
 * Manages showtime list state, mutations, and dependent data (movies, rooms)
 */
export function useShowtimes() {
  const queryClient = useQueryClient();

  // Fetch all showtimes
  const showtimesQuery = useQuery({
    queryKey: ['showtimes'],
    queryFn: () => showtimeAdminApi.getAllShowtimes(),
  });

  // Fetch all movies (for dropdown in form)
  const moviesQuery = useQuery({
    queryKey: ['movies'],
    queryFn: () => movieApi.getAllMovies(),
  });

  // Fetch all rooms (for dropdown in form)
  const roomsQuery = useQuery({
    queryKey: ['rooms'],
    queryFn: () => roomApi.getAllRooms(),
  });

  // Create showtime mutation
  const createMutation = useMutation({
    mutationFn: (data) => showtimeAdminApi.createShowtime(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['showtimes'] });
    },
  });

  return {
    // Data
    showtimes: showtimesQuery.data?.data || [],
    isLoading: showtimesQuery.isLoading,
    isError: showtimesQuery.isError,
    error: showtimesQuery.error,

    // Dependent data for form dropdowns
    movies: moviesQuery.data?.data || [],
    isLoadingMovies: moviesQuery.isLoading,
    rooms: roomsQuery.data?.data || [],
    isLoadingRooms: roomsQuery.isLoading,

    // Mutations
    createShowtime: (data) => createMutation.mutateAsync(data),

    // Mutation states
    isCreating: createMutation.isPending,
  };
}
