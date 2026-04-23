import { motion } from 'framer-motion';
import { Link } from 'react-router-dom';
import LoginForm from '@/features/auth/components/LoginForm';

/**
 * LoginPage - Premium Minimalist Authentication
 * 
 * Design inspiration: Apple, Stripe
 * Features:
 * - Full-screen gradient background (slate-950 → slate-900)
 * - Centered form card with subtle shadow
 * - Premium typography with font-light
 * - Smooth entrance animations
 * - Security badge
 */
export default function LoginPage() {
  const containerVariants = {
    hidden: { opacity: 0 },
    visible: {
      opacity: 1,
      transition: { duration: 0.6, staggerChildren: 0.1 },
    },
  };

  const itemVariants = {
    hidden: { opacity: 0, y: 20 },
    visible: { opacity: 1, y: 0, transition: { duration: 0.6 } },
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-950 via-slate-900 to-slate-950 flex flex-col items-center justify-center px-4 py-12 relative overflow-hidden">
      {/* Background Decorative Elements */}
      <div className="absolute top-0 left-0 w-96 h-96 bg-rose-600/5 rounded-full blur-3xl -translate-x-1/2 -translate-y-1/2" />
      <div className="absolute bottom-0 right-0 w-96 h-96 bg-rose-600/5 rounded-full blur-3xl translate-x-1/2 translate-y-1/2" />

      <motion.div
        variants={containerVariants}
        initial="hidden"
        animate="visible"
        className="relative z-10 w-full max-w-md"
      >
        {/* Header Section */}
        <motion.div variants={itemVariants} className="text-center mb-12">
          {/* Logo */}
          <motion.div
            whileHover={{ scale: 1.05 }}
            className="flex items-center justify-center gap-3 mb-6"
          >
            <div className="w-12 h-12 bg-gradient-to-br from-rose-600 to-rose-500 rounded-xl flex items-center justify-center">
              <span className="text-white font-bold text-xl">🎬</span>
            </div>
            <h1 className="text-3xl font-light tracking-tight text-slate-100">
              CinemaBooking
            </h1>
          </motion.div>

          {/* Subtitle */}
          <motion.p
            variants={itemVariants}
            className="text-slate-400 text-center text-base leading-relaxed"
          >
            Đăng nhập để khám phá những bộ phim tuyệt vời
            <br />
            và đặt vé xem phim yêu thích của bạn
          </motion.p>
        </motion.div>

        {/* Form Card */}
        <motion.div
          variants={itemVariants}
          className="bg-slate-900/50 backdrop-blur-xl border border-slate-800 rounded-2xl p-8 shadow-2xl"
        >
          <LoginForm />
        </motion.div>

        {/* Footer Links */}
        <motion.div variants={itemVariants} className="mt-8 text-center">
          <div className="flex flex-col gap-4">
            {/* Divider */}
            <div className="flex items-center gap-4 text-sm">
              <div className="h-px bg-gradient-to-r from-transparent to-slate-700 flex-1" />
              <span className="text-slate-500">hoặc</span>
              <div className="h-px bg-gradient-to-l from-transparent to-slate-700 flex-1" />
            </div>

            {/* Register Link */}
            <p className="text-slate-400 text-sm">
              Chưa có tài khoản?{' '}
              <Link
                to="/register"
                className="text-rose-500 hover:text-rose-400 font-semibold transition-colors duration-200"
              >
                Đăng ký ngay
              </Link>
            </p>

            {/* Forgot Password Link */}
            <p className="text-slate-500 text-xs">
              <Link
                to="/forgot-password"
                className="text-slate-400 hover:text-slate-300 transition-colors duration-200"
              >
                Quên mật khẩu?
              </Link>
            </p>
          </div>
        </motion.div>

        {/* Security Badge */}
        <motion.div
          variants={itemVariants}
          className="mt-12 flex items-center justify-center gap-2 text-xs text-slate-500"
        >
          <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
            <path
              fillRule="evenodd"
              d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z"
              clipRule="evenodd"
            />
          </svg>
          <span>Kết nối an toàn với mã hóa SSL</span>
        </motion.div>
      </motion.div>
    </div>
  );
}
