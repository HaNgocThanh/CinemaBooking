import { motion } from 'framer-motion';
import { MovieCard } from './MovieCard';
import PropTypes from 'prop-types';

/**
 * MovieGrid Component - Premium Grid Layout
 * Responsive with 150px fixed margins
 * Features smooth entrance animations
 */
export default function MovieGrid({
  movies,
  title = 'Phim đang chiếu',
  onBuyTicket,
  onWatchTrailer,
}) {
  if (!movies || movies.length === 0) {
    return (
      <section className="w-full bg-slate-950 py-24 text-center">
        <p className="text-slate-400 text-lg">Không có phim nào để hiển thị</p>
      </section>
    );
  }

  const containerVariants = {
    hidden: { opacity: 0 },
    visible: {
      opacity: 1,
      transition: {
        staggerChildren: 0.1,
        delayChildren: 0.2,
      },
    },
  };

  const itemVariants = {
    hidden: { opacity: 0, y: 20 },
    visible: {
      opacity: 1,
      y: 0,
      transition: { duration: 0.6 },
    },
  };

  return (
    <section className="relative w-full bg-slate-950 py-20 lg:py-28">
      {/* Background Gradient Separator */}
      <div className="absolute top-0 left-0 right-0 h-32 bg-gradient-to-b from-slate-900/50 to-transparent pointer-events-none" />

      <div className="px-6 sm:px-12 lg:px-[150px]">
        {/* Section Title */}
        {title && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
            viewport={{ once: true, margin: '-100px' }}
            className="mb-16"
          >
            <h2 className="text-4xl sm:text-5xl font-light tracking-tight text-slate-100 mb-3">
              {title}
            </h2>
            <motion.div
              initial={{ width: 0 }}
              whileInView={{ width: '100%' }}
              transition={{ duration: 0.8, delay: 0.2 }}
              viewport={{ once: true }}
              className="h-0.5 bg-gradient-to-r from-rose-600 via-rose-500 to-transparent max-w-xs"
            />
          </motion.div>
        )}

        {/* Movie Grid */}
        <motion.div
          variants={containerVariants}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true, margin: '-100px' }}
          className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4 sm:gap-6"
        >
          {movies.map((movie) => (
            <motion.div key={movie.id} variants={itemVariants}>
              <MovieCard
                movie={movie}
                onBuyTicket={() => onBuyTicket(movie.id)}
                onWatchTrailer={() => onWatchTrailer(movie.id)}
              />
            </motion.div>
          ))}
        </motion.div>

        {/* See More Button */}
        {movies.length >= 8 && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
            viewport={{ once: true }}
            className="mt-16 text-center"
          >
            <motion.button
              whileHover={{ scale: 1.02 }}
              whileTap={{ scale: 0.98 }}
              className="px-8 py-3 bg-gradient-to-r from-rose-600 to-rose-500 text-white font-semibold rounded-xl hover:shadow-lg hover:shadow-rose-600/30 transition-all duration-300"
            >
              Xem thêm
            </motion.button>
          </motion.div>
        )}
      </div>
    </section>
  );
}

MovieGrid.propTypes = {
  movies: PropTypes.arrayOf(
    PropTypes.shape({
      id: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
      title: PropTypes.string.isRequired,
      posterUrl: PropTypes.string.isRequired,
    })
  ).isRequired,
  title: PropTypes.string,
  onBuyTicket: PropTypes.func,
  onWatchTrailer: PropTypes.func,
};

MovieGrid.defaultProps = {
  title: 'Phim đang chiếu',
  onBuyTicket: () => {},
  onWatchTrailer: () => {},
};

MovieGrid.propTypes = {
  movies: PropTypes.arrayOf(
    PropTypes.shape({
      id: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
      title: PropTypes.string.isRequired,
      posterUrl: PropTypes.string.isRequired,
    })
  ).isRequired,
  title: PropTypes.string,
  onBuyTicket: PropTypes.func,
  onWatchTrailer: PropTypes.func,
};

MovieGrid.defaultProps = {
  title: 'Phim đang chiếu',
  onBuyTicket: () => {},
  onWatchTrailer: () => {},
};
