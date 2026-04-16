import { useMutation, useQuery } from '@tanstack/react-query';
import axiosClient from '../../../services/axiosClient';
import queryClient from '../../../config/queryClient';

/**
 * Login user
 * @param {Object} data - { email, password }
 * @returns {Promise}
 */
const loginUser = (data) => {
  return axiosClient.post('/auth/login', data);
};

/**
 * Register user
 * @param {Object} data - { email, password, fullName }
 * @returns {Promise}
 */
const registerUser = (data) => {
  return axiosClient.post('/auth/register', data);
};

/**
 * Logout user
 */
const logoutUser = () => {
  localStorage.removeItem('jwtToken');
  localStorage.removeItem('user');
  return Promise.resolve();
};

/**
 * Get current user
 */
const getCurrentUser = () => {
  return axiosClient.get('/auth/me');
};

// ===== React Query Hooks =====

/**
 * Hook for login mutation
 */
export function useLogin() {
  return useMutation({
    mutationFn: loginUser,
    onSuccess: (data) => {
      localStorage.setItem('jwtToken', data.token);
      localStorage.setItem('user', JSON.stringify(data.user));
      queryClient.setQueryData(['user'], data.user);
    },
  });
}

/**
 * Hook for register mutation
 */
export function useRegister() {
  return useMutation({
    mutationFn: registerUser,
  });
}

/**
 * Hook for logout
 */
export function useLogout() {
  return useMutation({
    mutationFn: logoutUser,
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
    queryFn: getCurrentUser,
    enabled: !!localStorage.getItem('jwtToken'),
    staleTime: 1000 * 60 * 10, // 10 minutes
  });
}
