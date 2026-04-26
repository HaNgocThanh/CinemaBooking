import axiosClient from '@/services/axiosClient';

export const roomAdminApi = {
  getAllRooms: () =>
    axiosClient.get('/rooms'),

  getRoomById: (id) =>
    axiosClient.get(`/rooms/${id}`),

  getSeatTemplates: (roomId) =>
    axiosClient.get(`/rooms/${roomId}/seats`),

  createRoom: (data) =>
    axiosClient.post('/rooms', data),

  updateRoom: (id, data) =>
    axiosClient.put(`/rooms/${id}`, data),

  deleteRoom: (id) =>
    axiosClient.delete(`/rooms/${id}`),
};
