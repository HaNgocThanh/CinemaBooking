import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import axiosClient from './axiosClient';

/**
 * Auth API Service
 * Shared authentication API functions + React Query hooks
 */

// ========================================
// API Functions
// ========================================

const authApi = {
  /**
   * Đăng nhập
   * @param {Object} data - { usernameOrEmail, password }
   * @returns {Promise}
   */
  login: (data) => axiosClient.post('/auth/login', data),

  /**
   * Đăng ký
   * @param {Object} data - { email, password, fullName }
   * @returns {Promise}
   */
  register: (data) => axiosClient.post('/auth/register', data),

  /**
   * Đăng xuất (client-side)
   */
  logout: () => {
    localStorage.removeItem('JWT_TOKEN');
    localStorage.removeItem('user');
    return Promise.resolve();
  },

  /**
   * Lấy thông tin user hiện tại
   */
  getCurrentUser: () => axiosClient.get('/auth/me'),

  /**
   * Đổi mật khẩu
   * @param {Object} data - { currentPassword, newPassword, confirmPassword }
   */
  changePassword: (data) => axiosClient.post('/auth/change-password', data),

  /**
   * Quên mật khẩu - Gửi email reset
   * @param {string} email
   */
  forgotPassword: (email) => axiosClient.post('/auth/forgot-password', { email }),

  /**
   * Reset mật khẩu
   * @param {Object} data - { token, newPassword, confirmPassword }
   */
  resetPassword: (data) => axiosClient.post('/auth/reset-password', data),

  /**
   * Làm mới JWT token
   */
  refreshToken: () => axiosClient.post('/auth/refresh-token'),
};

// ========================================
// React Query Hooks
// ========================================

/**
 * Hook for login mutation
 */
export function useLogin() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: authApi.login,
    onSuccess: (data) => {
      if (data?.token) {
        localStorage.setItem('JWT_TOKEN', data.token);
        localStorage.setItem('user', JSON.stringify(data.user));
        queryClient.setQueryData(['user'], data.user);
      }
    },
  });
}

/**
 * Hook for register mutation
 */
export function useRegister() {
  return useMutation({
    mutationFn: authApi.register,
  });
}

/**
 * Hook for logout
 */
export function useLogout() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: authApi.logout,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['user'] });
    },
  });
}

/**
 * Hook for getting current user
 */
export function useCurrentUser() {
  return useQuery({
    queryKey: ['user'],
    queryFn: authApi.getCurrentUser,
    enabled: !!localStorage.getItem('JWT_TOKEN'),
    staleTime: 1000 * 60 * 10,
  });
}

export default authApi;
