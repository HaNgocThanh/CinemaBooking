import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { roomAdminApi } from '../api/roomAdminApi';

export function useRooms() {
  const queryClient = useQueryClient();

  const roomsQuery = useQuery({
    queryKey: ['rooms'],
    queryFn: () => roomAdminApi.getAllRooms(),
  });

  const createMutation = useMutation({
    mutationFn: (data) => roomAdminApi.createRoom(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['rooms'] });
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => roomAdminApi.updateRoom(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['rooms'] });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id) => roomAdminApi.deleteRoom(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['rooms'] });
    },
  });

  return {
    rooms: roomsQuery.data?.data || [],
    isLoading: roomsQuery.isLoading,
    isError: roomsQuery.isError,
    error: roomsQuery.error,

    createRoom: (data) => createMutation.mutateAsync(data),
    isCreating: createMutation.isPending,

    updateRoom: (id, data) => updateMutation.mutateAsync({ id, data }),
    isUpdating: updateMutation.isPending,

    deleteRoom: (id) => deleteMutation.mutateAsync(id),
    isDeleting: deleteMutation.isPending,
  };
}

export function useSeatTemplates(roomId) {
  return useQuery({
    queryKey: ['seatTemplates', roomId],
    queryFn: () => roomAdminApi.getSeatTemplates(roomId),
    enabled: !!roomId,
  });
}
