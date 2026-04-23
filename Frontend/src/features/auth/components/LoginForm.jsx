import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation } from '@tanstack/react-query';
import { motion } from 'framer-motion';
import { useAuth } from '@/context/AuthContext';
import authApi from '@/services/authApi';

/**
 * Login Form Validation Schema
 * Sử dụng Zod để validate email/password
 */
const loginSchema = z.object({
  usernameOrEmail: z
    .string()
    .min(1, 'Vui lòng nhập email hoặc tên đăng nhập')
    .refine(
      (val) => {
        // Accept email or username
        return (
          /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val) || val.length >= 3
        );
      },
      'Email hoặc tên đăng nhập không hợp lệ'
    ),
  password: z
    .string()
    .min(6, 'Mật khẩu phải có ít nhất 6 ký tự'),
  rememberMe: z.boolean().optional(),
});

/**
 * LoginForm Component
 * Premium form com React Hook Form + Zod
 * Features:
 * - Real-time validation
 * - Error messages dưới field
 * - Loading state
 * - Remember me checkbox
 * - Smooth animations
 */
export default function LoginForm() {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [apiError, setApiError] = useState('');

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(loginSchema),
    mode: 'onBlur', // Validate only on blur
  });

  const loginMutation = useMutation({
    mutationFn: (data) => authApi.login({ usernameOrEmail: data.usernameOrEmail, password: data.password }),
    onSuccess: (response) => {
      // Backend retorna: { success: true, data: { token, userId, username, email, fullName, role } }
      const token = response?.data?.token;
      const userData = response?.data ? {
        userId: response.data.userId,
        username: response.data.username,
        email: response.data.email,
        fullName: response.data.fullName,
        role: response.data.role
      } : null;

      if (token && userData) {
        login(token, userData);
        setApiError('');
        // Admin → redirect to admin dashboard, Customer → homepage
        const redirectTo = userData.role === 'Admin' ? '/admin/movies' : '/';
        navigate(redirectTo, { replace: true });
      } else {
        setApiError('Phản hồi từ server không hợp lệ. Vui lòng thử lại.');
      }
    },
    onError: (error) => {
      const message =
        error.response?.data?.error?.message ||
        error.message ||
        'Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.';
      setApiError(message);
    },
  });

  const onSubmit = async (data) => {
    setApiError('');
    await loginMutation.mutateAsync(data);
  };

  const itemVariants = {
    hidden: { opacity: 0, y: 10 },
    visible: { opacity: 1, y: 0, transition: { duration: 0.4 } },
  };

  return (
    <motion.form
      onSubmit={handleSubmit(onSubmit)}
      initial="hidden"
      animate="visible"
      variants={{ visible: { transition: { staggerChildren: 0.08 } } }}
      className="space-y-5"
    >
      {/* API Error Message */}
      {apiError && (
        <motion.div
          variants={itemVariants}
          className="p-4 bg-red-500/10 border border-red-500/30 rounded-lg backdrop-blur-sm"
        >
          <div className="flex gap-3">
            <svg
              className="w-5 h-5 text-red-400 flex-shrink-0 mt-0.5"
              fill="currentColor"
              viewBox="0 0 20 20"
            >
              <path
                fillRule="evenodd"
                d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                clipRule="evenodd"
              />
            </svg>
            <p className="text-sm text-red-400">{apiError}</p>
          </div>
        </motion.div>
      )}

      {/* Email / Username Input */}
      <motion.div variants={itemVariants} className="space-y-2">
        <label
          htmlFor="usernameOrEmail"
          className="block text-sm font-medium text-slate-300"
        >
          Tên đăng nhập hoặc Email
        </label>
        <input
          id="usernameOrEmail"
          type="text"
          placeholder="Nhập tên đăng nhập hoặc email"
          disabled={isSubmitting || loginMutation.isLoading}
          className={`w-full px-4 py-3 bg-slate-800/50 border rounded-lg transition-all duration-200 text-slate-100 placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-rose-500/50 focus:border-rose-500/50 disabled:opacity-50 disabled:cursor-not-allowed ${
            errors.usernameOrEmail
              ? 'border-red-500/50 bg-red-500/5'
              : 'border-slate-700 hover:border-slate-600'
          }`}
          {...register('usernameOrEmail')}
        />
        {errors.usernameOrEmail && (
          <motion.p
            initial={{ opacity: 0, y: -5 }}
            animate={{ opacity: 1, y: 0 }}
            className="text-sm text-red-400"
          >
            {errors.usernameOrEmail.message}
          </motion.p>
        )}
      </motion.div>

      {/* Password Input */}
      <motion.div variants={itemVariants} className="space-y-2">
        <label
          htmlFor="password"
          className="block text-sm font-medium text-slate-300"
        >
          Mật khẩu
        </label>
        <input
          id="password"
          type="password"
          placeholder="••••••••"
          disabled={isSubmitting || loginMutation.isLoading}
          className={`w-full px-4 py-3 bg-slate-800/50 border rounded-lg transition-all duration-200 text-slate-100 placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-rose-500/50 focus:border-rose-500/50 disabled:opacity-50 disabled:cursor-not-allowed ${
            errors.password
              ? 'border-red-500/50 bg-red-500/5'
              : 'border-slate-700 hover:border-slate-600'
          }`}
          {...register('password')}
        />
        {errors.password && (
          <motion.p
            initial={{ opacity: 0, y: -5 }}
            animate={{ opacity: 1, y: 0 }}
            className="text-sm text-red-400"
          >
            {errors.password.message}
          </motion.p>
        )}
      </motion.div>

      {/* Remember Me Checkbox */}
      <motion.div variants={itemVariants} className="flex items-center gap-3">
        <input
          id="rememberMe"
          type="checkbox"
          disabled={isSubmitting || loginMutation.isLoading}
          className="w-4 h-4 rounded bg-slate-800 border-slate-700 text-rose-500 focus:ring-rose-500/50 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
          {...register('rememberMe')}
        />
        <label
          htmlFor="rememberMe"
          className="text-sm text-slate-400 cursor-pointer select-none"
        >
          Ghi nhớ đăng nhập của tôi
        </label>
      </motion.div>

      {/* Submit Button */}
      <motion.button
        variants={itemVariants}
        whileHover={{ scale: 1.01 }}
        whileTap={{ scale: 0.99 }}
        type="submit"
        disabled={isSubmitting || loginMutation.isLoading}
        className="w-full py-3 bg-gradient-to-r from-rose-600 to-rose-500 text-white font-semibold rounded-lg transition-all duration-300 hover:shadow-lg hover:shadow-rose-600/30 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
      >
        {isSubmitting || loginMutation.isLoading ? (
          <>
            <motion.div
              animate={{ rotate: 360 }}
              transition={{ duration: 1, repeat: Infinity, ease: 'linear' }}
              className="w-4 h-4 border-2 border-white border-t-transparent rounded-full"
            />
            <span>Đang đăng nhập...</span>
          </>
        ) : (
          <>
            <span>Đăng Nhập</span>
            <svg
              className="w-5 h-5"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M13 7l5 5m0 0l-5 5m5-5H6"
              />
            </svg>
          </>
        )}
      </motion.button>

      {/* Additional Info */}
      <motion.p
        variants={itemVariants}
        className="text-xs text-slate-500 text-center"
      >
        Bằng cách đăng nhập, bạn đồng ý với{' '}
        <a href="#" className="text-slate-400 hover:text-slate-300">
          Điều khoản sử dụng
        </a>{' '}
        của chúng tôi
      </motion.p>
    </motion.form>
  );
}
