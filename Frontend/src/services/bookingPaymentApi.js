import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import axiosClient from '@/services/axiosClient';

/**
 * Booking Payment API Service
 * Handles: QR payment submission, status polling, admin approve/reject
 */

// ========================================
// API Functions
// ========================================

const bookingPaymentApi = {
  /**
   * Submit payment confirmation (customer clicks "I have transferred")
   * @param {number} bookingId
   * @returns {Promise}
   */
  submitPayment: (bookingId) =>
    axiosClient.put(`/bookings/${bookingId}/submit`),

  /**
   * Get current booking status (for polling on payment page)
   * @param {number} bookingId
   * @returns {Promise}
   */
  getStatus: (bookingId) =>
    axiosClient.get(`/bookings/${bookingId}/status`),

  /**
   * Admin: Get all bookings with optional status filter
   * @param {Object} params - { status, page, pageSize }
   * @returns {Promise}
   */
  getAllBookings: (params = {}) => {
    const searchParams = new URLSearchParams();
    if (params.status !== undefined && params.status !== null)
      searchParams.set('status', params.status);
    if (params.page) searchParams.set('page', params.page);
    if (params.pageSize) searchParams.set('pageSize', params.pageSize);
    const query = searchParams.toString();
    return axiosClient.get(`/admin/bookings${query ? `?${query}` : ''}`);
  },

  /**
   * Admin: Approve booking (confirm payment received)
   * @param {number} bookingId
   * @returns {Promise}
   */
  approveBooking: (bookingId) =>
    axiosClient.post(`/admin/bookings/${bookingId}/approve`),

  /**
   * Admin: Reject booking (cancel order, release seats)
   * @param {number} bookingId
   * @param {string} reason - optional rejection reason
   * @returns {Promise}
   */
  rejectBooking: (bookingId, reason = null) =>
    axiosClient.post(`/admin/bookings/${bookingId}/reject`, reason ? { reason } : {}),

  /**
   * Get E-Ticket details for a booking
   * @param {number} bookingId
   * @returns {Promise}
   */
  getETicket: (bookingId) =>
    axiosClient.get(`/bookings/${bookingId}/ticket`),

  /**
   * Get booking history for current user
   * @returns {Promise}
   */
  getMyHistory: () =>
    axiosClient.get('/bookings/my-history'),

  /**
   * Get ALL bookings for current user (including Pending/AwaitingConfirmation)
   * @returns {Promise}
   */
  getAllMyBookings: () =>
    axiosClient.get('/bookings/my-all-bookings'),
};

// ========================================
// React Query Hooks
// ========================================

/**
 * Hook for submitting payment confirmation
 */
export function useSubmitPayment() {
  return useMutation({
    mutationFn: ({ bookingId }) => bookingPaymentApi.submitPayment(bookingId),
  });
}

/**
 * Hook for polling booking status
 * @param {number} bookingId
 * @param {Object} options - React Query options
 */
export function useBookingStatus(bookingId, options = {}) {
  const { enabled = true, interval = 4000, ...rest } = options;
  return useQuery({
    queryKey: ['booking-status', bookingId],
    queryFn: () => bookingPaymentApi.getStatus(bookingId),
    enabled: !!bookingId && enabled !== false,
    refetchInterval: enabled !== false ? interval : false,
    refetchIntervalInBackground: false,
    ...rest,
  });
}

/**
 * Hook for admin: fetch all bookings
 */
export function useAllBookings(params = {}) {
  return useQuery({
    queryKey: ['admin-bookings', params],
    queryFn: () => bookingPaymentApi.getAllBookings(params),
    staleTime: 30 * 1000,
  });
}

/**
 * Hook for admin: approve booking
 */
export function useApproveBooking() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ bookingId }) => bookingPaymentApi.approveBooking(bookingId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-bookings'] });
    },
  });
}

/**
 * Hook for admin: reject booking
 */
export function useRejectBooking() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ bookingId, reason }) => bookingPaymentApi.rejectBooking(bookingId, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-bookings'] });
    },
  });
}

/**
 * Hook for fetching E-Ticket details
 * @param {number} bookingId
 */
export function useETicket(bookingId) {
  return useQuery({
    queryKey: ['e-ticket', bookingId],
    queryFn: () => bookingPaymentApi.getETicket(bookingId),
    enabled: !!bookingId,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Hook for fetching booking history of current user (completed bookings only)
 */
export function useMyHistory() {
  return useQuery({
    queryKey: ['my-bookings'],
    queryFn: () => bookingPaymentApi.getMyHistory(),
    staleTime: 30 * 1000,
  });
}

/**
 * Hook for fetching ALL bookings of current user (including pending/awaiting)
 */
export function useAllMyBookings() {
  return useQuery({
    queryKey: ['all-my-bookings'],
    queryFn: () => bookingPaymentApi.getAllMyBookings(),
    staleTime: 30 * 1000,
  });
}

export default bookingPaymentApi;
