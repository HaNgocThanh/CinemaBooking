import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { movieAdminApi } from '../api/movieAdminApi';

/**
 * Custom Hook: useMovies
 * Manages movie list state, pagination, filtering, and mutations
 */
export function useMovies() {
  const queryClient = useQueryClient();
  const [pageNum, setPageNum] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchText, setSearchText] = useState('');
  const [sortBy, setSortBy] = useState('releaseDate');
  const [sortOrder, setSortOrder] = useState('desc');

  // Fetch movies
  const moviesQuery = useQuery({
    queryKey: ['movies', pageNum, pageSize, searchText, sortBy, sortOrder],
    queryFn: () =>
      movieAdminApi.getAllMovies({
        pageNum,
        pageSize,
        search: searchText,
        sortBy,
        sortOrder,
      }),
    keepPreviousData: true,
  });

  // Create movie mutation
  const createMutation = useMutation({
    mutationFn: (data) => movieAdminApi.createMovie(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['movies'] });
    },
  });

  // Update movie mutation
  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => movieAdminApi.updateMovie(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['movies'] });
    },
  });

  // Delete movie mutation
  const deleteMutation = useMutation({
    mutationFn: (id) => movieAdminApi.deleteMovie(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['movies'] });
    },
  });

  // Toggle movie status mutation
  const toggleStatusMutation = useMutation({
    mutationFn: (id) => movieAdminApi.toggleMovieStatus(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['movies'] });
    },
  });

  return {
    // Data & Status
    // API response format: { success, data: [...movies], message, traceId }
    // axiosClient interceptor already unwraps response.data, so we get the object above
    movies: moviesQuery.data?.data || [],
    total: moviesQuery.data?.data?.length || 0,
    isLoading: moviesQuery.isLoading,
    isError: moviesQuery.isError,
    error: moviesQuery.error,

    // Pagination & Filtering
    pagination: {
      pageNum,
      pageSize,
      setPageNum,
      setPageSize,
      total: moviesQuery.data?.data?.length || 0,
    },
    filters: {
      searchText,
      setSearchText,
      sortBy,
      setSortBy,
      sortOrder,
      setSortOrder,
    },

    // Mutations
    createMovie: (data) => createMutation.mutateAsync(data),
    updateMovie: (id, data) => updateMutation.mutateAsync({ id, data }),
    deleteMovie: (id) => deleteMutation.mutateAsync(id),
    toggleMovieStatus: (id) => toggleStatusMutation.mutateAsync(id),

    // Mutation states
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
    isTogglingStatus: toggleStatusMutation.isPending,
  };
}
