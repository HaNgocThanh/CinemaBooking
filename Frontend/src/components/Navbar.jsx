import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { useAuth } from '@/context/AuthContext';

/**
 * Navbar Component - Premium Minimalist Design
 * Sticky navigation with glass-morphism effect
 * Uses framer-motion for smooth interactions
 */
export default function Navbar() {
  const [isOpen, setIsOpen] = useState(false);
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  return (
    <motion.nav
      initial={{ opacity: 0, y: -20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.6 }}
      className="sticky top-0 z-50 w-full backdrop-blur-xl bg-slate-950/75 border-b border-rose-600/10"
    >
      <div className="px-6 sm:px-12 lg:px-[150px]">
        <div className="flex justify-between items-center h-20">
          {/* Logo */}
          <Link
            to="/"
            className="flex items-center space-x-3 flex-shrink-0 group"
          >
            <motion.div
              whileHover={{ scale: 1.1 }}
              whileTap={{ scale: 0.95 }}
              className="w-9 h-9 bg-gradient-to-br from-rose-600 to-rose-700 rounded-lg flex items-center justify-center shadow-lg shadow-rose-600/20"
            >
              <svg
                className="w-5 h-5 text-white"
                fill="currentColor"
                viewBox="0 0 20 20"
              >
                <path d="M4 2a2 2 0 00-2 2v12a2 2 0 002 2h12a2 2 0 002-2V4a2 2 0 00-2-2H4z" />
              </svg>
            </motion.div>
            <span className="text-lg font-light tracking-wider text-slate-100">
              Cinema<span className="font-semibold text-rose-600">Booking</span>
            </span>
          </Link>

          {/* Desktop Menu */}
          <div className="hidden md:flex items-center space-x-12">
            {['Lịch chiếu', 'Rạp', 'Ưu đãi'].map((item) => (
              <motion.div
                key={item}
                whileHover={{ y: -2 }}
                transition={{ type: 'spring', stiffness: 400 }}
              >
                <a
                  href="#"
                  className="text-sm text-slate-400 hover:text-slate-100 transition-colors duration-300 relative group"
                >
                  {item}
                  <span className="absolute bottom-0 left-0 w-0 h-0.5 bg-rose-600 group-hover:w-full transition-all duration-300" />
                </a>
              </motion.div>
            ))}
          </div>

          {/* Auth & Profile */}
          <div className="flex items-center space-x-4">
            {user ? (
              <>
                <div className="hidden sm:flex items-center space-x-3">
                  <div className="text-right">
                    <p className="text-sm font-medium text-slate-100">
                      {user.fullName || user.username}
                    </p>
                    <p className="text-xs text-slate-500">{user.role}</p>
                  </div>
                </div>
                {/* Admin Dashboard Button - Only visible for Admin role */}
                {user.role === 'Admin' && (
                  <motion.button
                    whileHover={{ scale: 1.05 }}
                    whileTap={{ scale: 0.95 }}
                    onClick={() => navigate('/admin/movies')}
                    className="hidden sm:inline-flex items-center gap-2 px-5 py-2.5 text-sm font-semibold text-white bg-gradient-to-r from-rose-600 to-rose-500 rounded-lg shadow-lg shadow-rose-600/20 hover:shadow-rose-600/40 transition-all duration-300"
                  >
                    <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M2.166 4.999A11.954 11.954 0 0010 1.944 11.954 11.954 0 0017.834 5c.11.65.166 1.32.166 2.001 0 5.225-3.34 9.67-8 11.317C5.34 16.67 2 12.225 2 7c0-.682.057-1.35.166-2.001zm11.541 3.708a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                    </svg>
                    Quản Trị
                  </motion.button>
                )}
                <motion.button
                  whileHover={{ scale: 1.05 }}
                  whileTap={{ scale: 0.95 }}
                  onClick={() => {
                    logout();
                    navigate('/login');
                  }}
                  className="hidden sm:inline-block px-6 py-2.5 text-sm text-slate-100 border border-slate-700 rounded-lg hover:border-rose-600/30 hover:bg-rose-600/5 transition-all duration-300"
                >
                  Đăng xuất
                </motion.button>
              </>
            ) : (
              <Link
                to="/login"
                className="hidden sm:inline-block px-6 py-2.5 text-sm text-slate-100 border border-slate-700 rounded-lg hover:border-rose-600/30 hover:bg-rose-600/5 transition-all duration-300"
              >
                Đăng nhập
              </Link>
            )}
            <motion.button
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.95 }}
              className="p-2.5 rounded-lg hover:bg-slate-900/50 transition-colors"
            >
              <svg
                className="w-5 h-5 text-slate-300"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={1.5}
                  d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
                />
              </svg>
            </motion.button>
          </div>

          {/* Mobile Menu Button */}
          <motion.button
            onClick={() => setIsOpen(!isOpen)}
            whileTap={{ scale: 0.95 }}
            className="md:hidden p-2"
          >
            <motion.svg
              animate={{ rotate: isOpen ? 90 : 0 }}
              transition={{ duration: 0.3 }}
              className="w-6 h-6 text-slate-100"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M4 6h16M4 12h16M4 18h16"
              />
            </motion.svg>
          </motion.button>
        </div>

        {/* Mobile Menu */}
        <motion.div
          initial={{ opacity: 0, height: 0 }}
          animate={{ opacity: isOpen ? 1 : 0, height: isOpen ? 'auto' : 0 }}
          transition={{ duration: 0.3 }}
          className="md:hidden overflow-hidden"
        >
          <div className="py-4 space-y-2 border-t border-slate-800">
            {['Lịch chiếu', 'Rạp', 'Ưu đãi'].map((item) => (
              <motion.a
                key={item}
                href="#"
                whileHover={{ x: 4 }}
                className="block px-4 py-3 text-slate-300 hover:text-white hover:bg-slate-900/50 rounded-lg transition-colors"
              >
                {item}
              </motion.a>
            ))}
          </div>
        </motion.div>
      </div>
    </motion.nav>
  );
}
