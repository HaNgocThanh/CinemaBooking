import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { useAuth } from '@/context/AuthContext';
import authApi from '@/services/authApi';

export default function LoginForm() {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errorMessage, setErrorMessage] = useState('');

  const loginMutation = useMutation((data) => authApi.login(data), {
    onSuccess: (response) => {
      // Gọi hàm login từ AuthContext để update state
      login(response.data?.token, response.data?.user);

      // Xóa error và chuyển hướng về Home
      setErrorMessage('');
      navigate('/');
    },
    onError: (error) => {
      // Hiển thị lỗi từ backend
      setErrorMessage(error.message || 'Đăng nhập thất bại. Vui lòng kiểm tra lại.');
    },
  });

  const handleSubmit = (e) => {
    e.preventDefault();
    setErrorMessage('');

    // Validate form
    if (!email.trim()) {
      setErrorMessage('Vui lòng nhập email hoặc tên đăng nhập.');
      return;
    }
    if (!password.trim()) {
      setErrorMessage('Vui lòng nhập mật khẩu.');
      return;
    }

    // Gọi API
    loginMutation.mutate({ email, password });
  };

  return (
    <div className="w-full max-w-md">
      {/* Logo / Header */}
      <div className="text-center mb-8">
        <h1 className="text-4xl font-bold text-white mb-2">CinemaX</h1>
        <p className="text-slate-300">Đăng nhập để đặt vé xem phim</p>
      </div>

        {/* Form Card */}
        <form
          onSubmit={handleSubmit}
          className="bg-white rounded-2xl shadow-2xl p-8 space-y-6"
        >
          {/* Email Input */}
          <div>
            <label htmlFor="email" className="block text-sm font-medium text-slate-700 mb-2">
              Email hoặc Tên đăng nhập
            </label>
            <input
              id="email"
              type="text"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="user@example.com"
              className="w-full px-4 py-3 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
              disabled={loginMutation.isLoading}
            />
          </div>

          {/* Password Input */}
          <div>
            <label htmlFor="password" className="block text-sm font-medium text-slate-700 mb-2">
              Mật khẩu
            </label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              className="w-full px-4 py-3 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
              disabled={loginMutation.isLoading}
            />
          </div>

          {/* Error Message */}
          {errorMessage && (
            <div className="p-3 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-sm text-red-600">{errorMessage}</p>
            </div>
          )}

          {/* Submit Button */}
          <button
            type="submit"
            disabled={loginMutation.isLoading}
            className="w-full py-3 bg-gradient-to-r from-blue-500 to-blue-600 text-white font-semibold rounded-lg hover:from-blue-600 hover:to-blue-700 transition duration-300 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {loginMutation.isLoading ? (
              <span className="flex items-center justify-center">
                <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Đang đăng nhập...
              </span>
            ) : (
              'Đăng Nhập'
            )}
          </button>

          {/* Forgot Password & Register Links */}
          <div className="flex justify-between items-center text-sm">
            <a href="/forgot-password" className="text-blue-500 hover:text-blue-600 transition">
              Quên mật khẩu?
            </a>
            <span className="text-slate-500">
              Chưa có tài khoản?{' '}
              <a href="/register" className="text-blue-500 hover:text-blue-600 transition font-medium">
                Đăng ký
              </a>
            </span>
          </div>
        </form>
      </div>
    );
}
