import axios from 'axios';

const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request Interceptor: Add JWT token to headers
axiosClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('jwtToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response Interceptor: Handle errors and token refresh
axiosClient.interceptors.response.use(
  (response) => {
    // Return only data from response
    return response.data;
  },
  (error) => {
    if (error.response?.status === 401) {
      // Unauthorized - token expired or invalid
      localStorage.removeItem('jwtToken');
      localStorage.removeItem('user');
      window.location.href = '/auth/login';
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
