import { motion } from 'framer-motion';
import PropTypes from 'prop-types';

/**
 * MovieCard Component - Premium Hover Animation
 * Uses framer-motion for smooth scale & overlay effects
 * 
 * Features:
 * - Image scale: 100% → 108% on hover (300ms)
 * - Smooth overlay fade (0% → 100%)
 * - CTA buttons with staggered animation
 */
export function MovieCard({ movie, onBuyTicket, onWatchTrailer }) {
  const handleBuyTicket = (e) => {
    e.stopPropagation();
    onBuyTicket(movie.id);
  };

  const handleWatchTrailer = (e) => {
    e.stopPropagation();
    onWatchTrailer(movie.id);
  };

  return (
    <motion.article
      whileHover="hover"
      initial="initial"
      variants={{
        initial: { opacity: 1, y: 0 },
        hover: { opacity: 1, y: 0 },
      }}
      className="relative w-full aspect-[2/3] rounded-2xl overflow-hidden group cursor-pointer"
    >
      {/* Poster Image with Scale Animation */}
      <motion.img
        src={movie.posterUrl}
        alt={movie.title}
        className="w-full h-full object-cover"
        variants={{
          initial: { scale: 1 },
          hover: { scale: 1.08 },
        }}
        transition={{ duration: 0.4, ease: 'easeOut' }}
      />

      {/* Gradient Overlay - Appears on hover */}
      <motion.div
        className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/40 to-transparent backdrop-blur-sm"
        variants={{
          initial: { opacity: 0 },
          hover: { opacity: 1 },
        }}
        transition={{ duration: 0.3 }}
      />

      {/* CTA Buttons Container */}
      <motion.div
        className="absolute inset-0 flex flex-col items-center justify-end pb-8 px-4"
        variants={{
          initial: { opacity: 0 },
          hover: { opacity: 1 },
        }}
        transition={{ duration: 0.3 }}
      >
        <motion.div
          className="flex gap-3 w-full"
          variants={{
            initial: { y: 10 },
            hover: { y: 0 },
          }}
          transition={{ duration: 0.4, delay: 0.1 }}
        >
          {/* Buy Ticket Button */}
          <motion.button
            onClick={handleBuyTicket}
            whileHover={{ scale: 1.05, boxShadow: '0 20px 40px rgba(225, 29, 72, 0.3)' }}
            whileTap={{ scale: 0.95 }}
            className="flex-1 px-4 py-3 bg-gradient-to-r from-rose-600 to-rose-500 text-white font-semibold rounded-xl transition-all duration-200 hover:shadow-lg"
          >
            Mua vé
          </motion.button>

          {/* Trailer Button */}
          <motion.button
            onClick={handleWatchTrailer}
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            className="flex-1 px-4 py-3 border-1.5 border-white/40 text-white font-semibold rounded-xl backdrop-blur-md hover:border-white/60 hover:bg-white/5 transition-all duration-200"
          >
            Trailer
          </motion.button>
        </motion.div>
      </motion.div>
    </motion.article>
  );
}

MovieCard.propTypes = {
  movie: PropTypes.shape({
    id: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
    title: PropTypes.string.isRequired,
    posterUrl: PropTypes.string.isRequired,
  }).isRequired,
  onBuyTicket: PropTypes.func,
  onWatchTrailer: PropTypes.func,
};

MovieCard.defaultProps = {
  onBuyTicket: () => {},
  onWatchTrailer: () => {},
};
