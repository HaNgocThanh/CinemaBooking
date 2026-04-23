import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { keepPreviousData } from '@tanstack/react-query';
import { movieAdminApi } from '../api/movieAdminApi';

/**
 * Custom Hook: useMovies
 * Manages movie list state, pagination, filtering, and mutations
 * 
 * Response format from backend (after axiosClient unwrap):
 * { success: true, data: [MovieResponseDto...], message: "...", traceId: "..." }
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
    placeholderData: keepPreviousData,
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

  // Extract movies array from response
  // Backend: { success: true, data: [...], message: "..." }
  // axiosClient interceptor returns response.data → { success, data: [...], message }
  const responseData = moviesQuery.data;
  const moviesList = responseData?.data || [];

  return {
    // Data & Status
    movies: moviesList,
    total: moviesList.length,
    isLoading: moviesQuery.isLoading,
    isError: moviesQuery.isError,
    error: moviesQuery.error,

    // Pagination & Filtering
    pagination: {
      pageNum,
      pageSize,
      setPageNum,
      setPageSize,
      total: moviesList.length,
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

    // Mutation states
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}

