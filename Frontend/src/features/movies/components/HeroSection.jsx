import { motion } from 'framer-motion';
import { Carousel } from 'antd';

/**
 * HeroSection Component - Premium Minimalist Banner
 * Features atmospheric lighting and ultra-large typography
 * Uses framer-motion for smooth entrance animations
 * Uses antd Carousel for looping through featured movies
 */
export default function HeroSection({ featuredMovies = [] }) {
  if (!featuredMovies || featuredMovies.length === 0) return null;

  return (
    <div className="relative w-screen left-1/2 right-1/2 -ml-[50vw] -mr-[50vw] overflow-hidden">
      <Carousel autoplay autoplaySpeed={10000} effect="fade" draggable pauseOnHover={false}>
        {featuredMovies.map((featuredMovie, index) => (
          <div key={featuredMovie.id || index}>
            <section className="relative w-full min-h-screen flex items-center overflow-hidden bg-slate-950">
              {/* Background Image - Full Width */}
              <div className="absolute inset-0 z-0">
                <img
                  src={featuredMovie.bannerUrl || featuredMovie.posterUrl}
                  alt={featuredMovie.title}
                  className="w-full h-full object-cover object-center"
                />

                {/* Premium Gradient Overlay */}
                <div className="absolute inset-0 bg-gradient-to-r from-slate-950 via-slate-950/70 to-transparent" />

                {/* Atmospheric Lighting Effects - Blur Glow */}
                <motion.div
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  transition={{ duration: 1.5 }}
                  className="absolute top-1/2 -right-32 w-96 h-96 bg-rose-600 rounded-full blur-3xl opacity-5"
                />
                <motion.div
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  transition={{ duration: 2 }}
                  className="absolute bottom-1/4 -left-40 w-80 h-80 bg-rose-600 rounded-full blur-3xl opacity-3"
                />
              </div>

              {/* Content Container - With 150px Margins */}
              <div className="relative z-10 w-full px-6 sm:px-12 lg:px-[150px] py-20 flex flex-col justify-center min-h-screen">
                <div className="max-w-3xl">
                  {/* Badge */}
                  <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.6 }}
                    className="mb-8"
                  >
                    <span className="inline-block px-4 py-2 bg-gradient-to-r from-rose-600 to-rose-500 text-white text-xs font-semibold rounded-full uppercase tracking-widest shadow-lg shadow-rose-600/20">
                      NOW SHOWING
                    </span>
                  </motion.div>

                  {/* Ultra-Large Title */}
                  <motion.h1
                    initial={{ opacity: 0, y: 30 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.8, delay: 0.1 }}
                    className="text-6xl sm:text-7xl lg:text-8xl font-light tracking-tight text-slate-100 mb-6 leading-[1.1]"
                  >
                    {featuredMovie.title}
                  </motion.h1>

                  {/* Metadata Line */}
                  <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.6, delay: 0.2 }}
                    className="flex items-center gap-4 text-slate-400 text-sm mb-8"
                  >
                    <span>{featuredMovie.durationMinutes} phút</span>
                    <span className="w-1 h-1 bg-rose-600 rounded-full" />
                    <span>{featuredMovie.genre}</span>
                    <span className="w-1 h-1 bg-rose-600 rounded-full" />
                    <div className="flex items-center gap-2">
                      <span className="text-yellow-500">★</span>
                      <span>{featuredMovie.ratingCode || featuredMovie.rating || 'N/A'}/10</span>
                    </div>
                  </motion.div>

                  {/* Description */}
                  <motion.p
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.6, delay: 0.3 }}
                    className="text-lg text-slate-300 mb-10 max-w-2xl leading-relaxed font-light line-clamp-3"
                  >
                    {featuredMovie.description}
                  </motion.p>

                  {/* CTA Buttons */}
                  <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.6, delay: 0.4 }}
                    className="flex gap-4"
                  >
                    {/* Primary Button */}
                    <motion.button
                      whileHover={{ scale: 1.02, boxShadow: '0 20px 40px rgba(225, 29, 72, 0.3)' }}
                      whileTap={{ scale: 0.98 }}
                      className="px-8 py-4 bg-gradient-to-r from-rose-600 to-rose-500 text-white font-semibold rounded-xl hover:shadow-2xl transition-all duration-300"
                    >
                      Mua vé ngay
                    </motion.button>

                    {/* Secondary Button */}
                    <motion.button
                      whileHover={{ scale: 1.02, backgroundColor: 'rgba(255, 255, 255, 0.1)' }}
                      whileTap={{ scale: 0.98 }}
                      className="px-8 py-4 border-1.5 border-white/30 text-slate-100 font-semibold rounded-xl backdrop-blur-sm hover:border-white/50 transition-all duration-300"
                    >
                      Xem trailer
                    </motion.button>
                  </motion.div>
                </div>
              </div>

            </section>
          </div>
        ))}
      </Carousel>

      {/* Scroll Indicator (Outside Carousel so it stays constant) */}
      <motion.div
        animate={{ y: [0, 8, 0] }}
        transition={{ duration: 3, repeat: Infinity }}
        className="absolute bottom-8 left-1/2 transform -translate-x-1/2 z-10 pointer-events-none"
      >
        <svg
          className="w-6 h-6 text-slate-400"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={1.5}
            d="M19 14l-7 7m0 0l-7-7m7 7V3"
          />
        </svg>
      </motion.div>
    </div>
  );
}
