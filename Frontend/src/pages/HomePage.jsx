import { useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import Navbar from '../components/Navbar';
import Footer from '../components/Footer';
import HeroSection from '../features/movies/components/HeroSection';
import MovieGrid from '../features/movies/components/MovieGrid';

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

  // Mock featured movie - Real image from Unsplash
  const featuredMovie = {
    id: 1,
    title: 'Một bộ phim của Lee Cronin',
    description: 'Khải chiến trong tập phim này. Hành trình tuyệt vở sắp bắt đầu với những cảnh quay ngoạn mục.',
    duration: 127,
    releaseYear: 2026,
    rating: 8.5,
    backdropUrl:
      'https://images.unsplash.com/photo-1574375927938-d5a98e8ffe85?w=1920&h=1080&fit=crop',
  };

  // Mock movies - Now Showing (Real posters from various sources)
  const nowShowingMovies = [
    {
      id: 1,
      title: 'Dune: Part Two',
      posterUrl: 'https://images.unsplash.com/photo-1485846234645-a62644f84728?w=260&h=390&fit=crop',
    },
    {
      id: 2,
      title: 'The Matrix',
      posterUrl: 'https://images.unsplash.com/photo-1478720568477-152d9e3fb27f?w=260&h=390&fit=crop',
    },
    {
      id: 3,
      title: 'Oppenheimer',
      posterUrl: 'https://images.unsplash.com/photo-1505686994434-e3cc5abf1330?w=260&h=390&fit=crop',
    },
    {
      id: 4,
      title: 'Inception',
      posterUrl: 'https://images.unsplash.com/photo-1596506365368-8ec00ac94d98?w=260&h=390&fit=crop',
    },
    {
      id: 5,
      title: 'Interstellar',
      posterUrl: 'https://images.unsplash.com/photo-1533109752211-118fcccf4dbb?w=260&h=390&fit=crop',
    },
    {
      id: 6,
      title: 'The Dark Knight',
      posterUrl: 'https://images.unsplash.com/photo-1489599849228-ed4dc9ee6131?w=260&h=390&fit=crop',
    },
    {
      id: 7,
      title: 'Avatar',
      posterUrl: 'https://images.unsplash.com/photo-1518676590629-3dcbd9c5a5c9?w=260&h=390&fit=crop',
    },
    {
      id: 8,
      title: 'Tenet',
      posterUrl: 'https://images.unsplash.com/photo-1492684223066-81342ee5ff30?w=260&h=390&fit=crop',
    },
  ];

  // Mock movies - Coming Soon
  const comingSoonMovies = [
    {
      id: 9,
      title: 'Dune: Messiah',
      posterUrl: 'https://images.unsplash.com/photo-1597045866519-12d7ff67b184?w=260&h=390&fit=crop',
    },
    {
      id: 10,
      title: 'Blade Runner 3',
      posterUrl: 'https://images.unsplash.com/photo-1554224311-beee415c15cb?w=260&h=390&fit=crop',
    },
    {
      id: 11,
      title: 'Avatar 4',
      posterUrl: 'https://images.unsplash.com/photo-1612905289329-25a8f5d9ec1f?w=260&h=390&fit=crop',
    },
    {
      id: 12,
      title: 'Aquaman 3',
      posterUrl: 'https://images.unsplash.com/photo-1574190045514-f28d50ff2483?w=260&h=390&fit=crop',
    },
  ];

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
        {/* Now Showing Grid */}
        <MovieGrid
          movies={nowShowingMovies}
          title="Phim đang chiếu"
          onBuyTicket={handleBuyTicket}
          onWatchTrailer={handleWatchTrailer}
        />

        {/* Coming Soon Grid */}
        <MovieGrid
          movies={comingSoonMovies}
          title="Phim sắp tới"
          onBuyTicket={handleBuyTicket}
          onWatchTrailer={handleWatchTrailer}
        />

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
