import React, { createContext, useContext, useState, useEffect } from 'react';

/**
 * AuthContext - Quản lý trạng thái xác thực toàn ứng dụng
 */
const AuthContext = createContext(null);

/**
 * AuthProvider - Bọc ứng dụng để cung cấp auth state
 */
export function AuthProvider({ children }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [user, setUser] = useState(null);
  const [isLoading, setIsLoading] = useState(true);

  // Khởi tạo từ localStorage khi component mount
  useEffect(() => {
    const token = localStorage.getItem('JWT_TOKEN');
    const storedUser = localStorage.getItem('user');

    if (token && storedUser) {
      try {
        const userData = JSON.parse(storedUser);
        setUser(userData);
        setIsAuthenticated(true);
      } catch (error) {
        console.error('Failed to parse stored user:', error);
        localStorage.removeItem('JWT_TOKEN');
        localStorage.removeItem('user');
      }
    }
    setIsLoading(false);
  }, []);

  /**
   * Hàm login - Lưu token và user vào state và localStorage
   */
  const login = (token, userData) => {
    localStorage.setItem('JWT_TOKEN', token);
    localStorage.setItem('user', JSON.stringify(userData));
    setUser(userData);
    setIsAuthenticated(true);
  };

  /**
   * Hàm logout - Xóa token và user từ state và localStorage
   */
  const logout = () => {
    localStorage.removeItem('JWT_TOKEN');
    localStorage.removeItem('user');
    setUser(null);
    setIsAuthenticated(false);
  };

  const value = {
    isAuthenticated,
    user,
    isLoading,
    login,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

/**
 * Custom Hook - Dùng AuthContext
 */
export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth phải được dùng bên trong <AuthProvider>');
  }
  return context;
}
