import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:5270';

const axiosClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * REQUEST INTERCEPTOR
 * Kiểm tra JWT token trong localStorage và đính kèm vào header Authorization
 */
axiosClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('JWT_TOKEN');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

/**
 * RESPONSE INTERCEPTOR
 * Xử lý các lỗi từ server (đặc biệt 401 Unauthorized)
 */
axiosClient.interceptors.response.use(
  (response) => {
    // Return only data from response for convenience
    return response.data;
  },
  (error) => {
    // 401: Unauthorized - Token expired or invalid
    if (error.response?.status === 401) {
      localStorage.removeItem('JWT_TOKEN');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }

    // 403: Forbidden - User doesn't have permission
    if (error.response?.status === 403) {
      console.error('Access Forbidden:', error.response?.data?.error?.message);
    }

    // 500: Server Error
    if (error.response?.status === 500) {
      console.error('Server Error:', error.response?.data?.error?.message);
    }

    // Get error message from backend response
    const errorMessage =
      error.response?.data?.error?.message ||
      error.response?.data?.message ||
      error.message ||
      'An error occurred';

    // Return error with message
    return Promise.reject(new Error(errorMessage));
  }
);

export default axiosClient;
