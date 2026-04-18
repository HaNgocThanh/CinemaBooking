import axiosClient from './axiosClient';

/**
 * Auth API Service
 * Chứa các hàm gọi API liên quan tới authentication
 */

const authApi = {
  /**
   * Đăng nhập
   * @param {Object} data - { email, password }
   * @returns {Promise} Trả về { token, user }
   */
  login: async (data) => {
    const response = await axiosClient.post('/api/auth/login', data);
    return response.data;
  },

  /**
   * Đăng ký
   * @param {Object} data - { email, password, fullName, phone }
   * @returns {Promise} Trả về { userId, email }
   */
  register: async (data) => {
    const response = await axiosClient.post('/api/auth/register', data);
    return response.data;
  },

  /**
   * Đổi mật khẩu
   * @param {Object} data - { currentPassword, newPassword, confirmPassword }
   * @returns {Promise} Trả về { success, message }
   */
  changePassword: async (data) => {
    const response = await axiosClient.post('/api/auth/change-password', data);
    return response.data;
  },

  /**
   * Xác minh email
   * @param {string} token - Token từ email verification link
   * @returns {Promise}
   */
  verifyEmail: async (token) => {
    const response = await axiosClient.post('/api/auth/verify-email', { token });
    return response.data;
  },

  /**
   * Đăng xuất (optional - chủ yếu xóa token ở client)
   * @returns {Promise}
   */
  logout: async () => {
    const response = await axiosClient.post('/api/auth/logout');
    return response.data;
  },

  /**
   * Quên mật khẩu - Gửi email reset
   * @param {string} email
   * @returns {Promise}
   */
  forgotPassword: async (email) => {
    const response = await axiosClient.post('/api/auth/forgot-password', { email });
    return response.data;
  },

  /**
   * Reset mật khẩu
   * @param {Object} data - { token, newPassword, confirmPassword }
   * @returns {Promise}
   */
  resetPassword: async (data) => {
    const response = await axiosClient.post('/api/auth/reset-password', data);
    return response.data;
  },

  /**
   * Làm mới JWT token
   * @returns {Promise} Trả về token mới
   */
  refreshToken: async () => {
    const response = await axiosClient.post('/api/auth/refresh-token');
    return response.data;
  },
};

export default authApi;
