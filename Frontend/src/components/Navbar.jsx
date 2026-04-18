import { useState } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';

/**
 * Navbar Component - Premium Minimalist Design
 * Sticky navigation with glass-morphism effect
 * Uses framer-motion for smooth interactions
 */
export default function Navbar() {
  const [isOpen, setIsOpen] = useState(false);

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
            <Link
              to="/auth/login"
              className="hidden sm:inline-block px-6 py-2.5 text-sm text-slate-100 border border-slate-700 rounded-lg hover:border-rose-600/30 hover:bg-rose-600/5 transition-all duration-300"
            >
              Đăng nhập
            </Link>
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
