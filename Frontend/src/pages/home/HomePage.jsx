import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import Navbar from '@/components/Navbar';
import Footer from '@/components/Footer';
import HeroSection from '@/features/movies/components/HeroSection';
import MovieSlider from '@/features/movies/components/MovieSlider';
import { useNowShowingMovies, useComingSoonMovies } from '@/services/movieApi';

/**
 * HomePage - Premium Minimalist Cinema Booking Homepage
 * 
 * Features:
 * - Full-screen hero with atmospheric lighting
 * - Ultra-large typography (font-light)
 * - 150px fixed margins on all sections
 * - Smooth framer-motion animations throughout
 * - Gradient transitions between sections
 * - Rose-600 as single accent color
 * 
 * Design inspiration: Apple, Nike minimalism
 */
export default function HomePage() {
  const navigate = useNavigate();

  const [activeTab, setActiveTab] = useState('now-showing'); // 'now-showing' | 'coming-soon'

  // Fetch movies from API using React Query
  const { data: nowShowingMovies = [], isLoading: isLoadingNow, error: errorNow } = useNowShowingMovies();
  const { data: comingSoonMovies = [], isLoading: isLoadingSoon, error: errorSoon } = useComingSoonMovies();

  const isLoading = activeTab === 'now-showing' ? isLoadingNow : isLoadingSoon;
  const error = activeTab === 'now-showing' ? errorNow : errorSoon;
  const currentMovies = activeTab === 'now-showing' ? nowShowingMovies : comingSoonMovies;

  // Featured movie - Lấy phim IsFeatured từ nowShowing, nếu không có thì lấy phim đầu tiên
  const featuredMovie = nowShowingMovies.find(m => m.isFeatured) || nowShowingMovies[0] || {
    id: 0,
    title: 'Đang tải phim...',
    description: 'Vui lòng chờ.',
    durationMinutes: 0,
    bannerUrl: 'https://images.unsplash.com/photo-1574375927938-d5a98e8ffe85?w=1920&h=1080&fit=crop',
    posterUrl: 'https://images.unsplash.com/photo-1574375927938-d5a98e8ffe85?w=1920&h=1080&fit=crop',
  };

  const handleBuyTicket = (movieId) => {
    console.log(`Navigate to showtime for movie ${movieId}`);
    navigate(`/showtimes?movie=${movieId}`);
  };

  const handleWatchTrailer = (movieId) => {
    console.log(`Open trailer for movie ${movieId}`);
    // TODO: Implement trailer modal
  };

  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      transition={{ duration: 0.6 }}
      className="min-h-screen bg-slate-950 text-slate-100"
    >
      {/* Navigation */}
      <Navbar />

      {/* Hero Section - Full Screen */}
      <HeroSection featuredMovie={featuredMovie} />

      {/* Main Content Area */}
      <main className="relative z-10 w-full bg-slate-950">
        {/* Loading State */}
        {isLoading && (
          <div className="w-full bg-slate-950 py-24 text-center">
            <motion.div
              animate={{ opacity: [0.5, 1, 0.5] }}
              transition={{ duration: 2, repeat: Infinity }}
              className="text-slate-400"
            >
              <p className="text-lg">Đang tải danh sách phim...</p>
            </motion.div>
          </div>
        )}

        {/* Error State */}
        {error && (
          <div className="w-full bg-slate-950 py-24 text-center">
            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              className="text-red-400"
            >
              <p className="text-lg mb-4">❌ Lỗi khi tải phim</p>
              <p className="text-sm text-slate-400">{error.message}</p>
            </motion.div>
          </div>
        )}

        {/* Tabs Section */}
        <section className="relative w-full bg-slate-950 pt-20 pb-8">
          <div className="absolute top-0 left-0 right-0 h-32 bg-gradient-to-b from-slate-900/50 to-transparent pointer-events-none" />
          <div className="px-6 sm:px-12 lg:px-[150px] flex justify-center gap-8 relative z-10">
            <button
              onClick={() => setActiveTab('now-showing')}
              className={`text-2xl sm:text-3xl font-bold tracking-tight pb-2 transition-all ${
                activeTab === 'now-showing' 
                  ? 'text-transparent bg-clip-text bg-gradient-to-r from-rose-600 to-rose-400 border-b-2 border-rose-600' 
                  : 'text-slate-500 hover:text-slate-300'
              }`}
            >
              ĐANG CHIẾU
            </button>
            <button
              onClick={() => setActiveTab('coming-soon')}
              className={`text-2xl sm:text-3xl font-bold tracking-tight pb-2 transition-all ${
                activeTab === 'coming-soon' 
                  ? 'text-transparent bg-clip-text bg-gradient-to-r from-rose-600 to-rose-400 border-b-2 border-rose-600' 
                  : 'text-slate-500 hover:text-slate-300'
              }`}
            >
              SẮP CHIẾU
            </button>
          </div>
        </section>

        {/* Movies Slider - Render when data is available */}
        {!isLoading && !error && currentMovies.length > 0 && (
          <section className="w-full bg-slate-950 pb-20">
            <div className="px-6 sm:px-12 lg:px-[150px]">
              <MovieSlider
                movies={currentMovies.slice(0, 10)}
                isComingSoon={activeTab === 'coming-soon'}
                onBuyTicket={handleBuyTicket}
                onWatchTrailer={handleWatchTrailer}
              />
              
              {/* Xem Tất Cả Button */}
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.6 }}
                viewport={{ once: true }}
                className="mt-12 text-center"
              >
                <motion.button
                  onClick={() => navigate(`/movies?tab=${activeTab}`)}
                  whileHover={{ scale: 1.02, backgroundColor: 'rgba(255, 255, 255, 0.1)' }}
                  whileTap={{ scale: 0.98 }}
                  className="px-8 py-3 border-1.5 border-white/30 text-slate-100 font-semibold rounded-xl backdrop-blur-sm hover:border-white/50 transition-all duration-300"
                >
                  Xem tất cả {activeTab === 'now-showing' ? 'phim đang chiếu' : 'phim sắp chiếu'}
                </motion.button>
              </motion.div>
            </div>
          </section>
        )}

        {/* Empty State */}
        {!isLoading && !error && currentMovies.length === 0 && (
          <div className="w-full bg-slate-950 py-24 text-center">
            <p className="text-slate-400 text-lg">Không có phim nào để hiển thị</p>
          </div>
        )}

        {/* Newsletter Section */}
        <section className="relative w-full bg-slate-950 py-20 lg:py-28 overflow-hidden">
          <div className="px-6 sm:px-12 lg:px-[150px]">
            {/* Background Atmosphere */}
            <motion.div
              initial={{ opacity: 0 }}
              whileInView={{ opacity: 1 }}
              transition={{ duration: 1 }}
              className="absolute inset-0 pointer-events-none"
            >
              <div className="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 w-96 h-96 bg-rose-600 rounded-full blur-3xl opacity-5" />
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.6 }}
              viewport={{ once: true, margin: '-100px' }}
              className="relative z-10 max-w-2xl mx-auto text-center"
            >
              <h2 className="text-4xl sm:text-5xl lg:text-6xl font-light tracking-tight text-slate-100 mb-6">
                Nhận thông báo phim mới
              </h2>
              <p className="text-slate-400 text-lg mb-8">
                Đăng ký để nhận các thông báo về phim sắp tới, ưu đãi đặc biệt và sự kiện độc quyền.
              </p>

              <div className="flex gap-3">
                <motion.input
                  whileFocus={{ scale: 1.02 }}
                  type="email"
                  placeholder="Nhập email của bạn"
                  className="flex-1 px-6 py-4 bg-slate-900/50 border border-slate-800 rounded-xl text-slate-100 placeholder-slate-500 focus:outline-none focus:border-rose-600/50 focus:bg-slate-900 transition-all duration-300"
                />
                <motion.button
                  whileHover={{ scale: 1.02, boxShadow: '0 20px 40px rgba(225, 29, 72, 0.3)' }}
                  whileTap={{ scale: 0.98 }}
                  className="px-8 py-4 bg-gradient-to-r from-rose-600 to-rose-500 text-white font-semibold rounded-xl whitespace-nowrap hover:shadow-lg transition-all duration-300"
                >
                  Đăng ký
                </motion.button>
              </div>
            </motion.div>
          </div>
        </section>
      </main>

      {/* Footer */}
      <Footer />
    </motion.div>
  );
}
