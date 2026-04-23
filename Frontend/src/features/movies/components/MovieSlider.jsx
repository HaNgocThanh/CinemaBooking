import React, { useRef, useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { MovieCard } from './MovieCard';
import PropTypes from 'prop-types';

/**
 * MovieSlider Component - Horizontal Scrollable List
 * Features smooth scrolling with next/prev buttons
 */
export default function MovieSlider({
  movies,
  isComingSoon = false,
  onBuyTicket,
  onWatchTrailer,
}) {
  const scrollContainerRef = useRef(null);
  const [showLeftArrow, setShowLeftArrow] = useState(false);
  const [showRightArrow, setShowRightArrow] = useState(true);

  const displayMovies = movies;

  const handleScroll = () => {
    if (scrollContainerRef.current) {
      const { scrollLeft, scrollWidth, clientWidth } = scrollContainerRef.current;
      setShowLeftArrow(scrollLeft > 0);
      setShowRightArrow(scrollLeft < scrollWidth - clientWidth - 10);
    }
  };

  useEffect(() => {
    handleScroll(); // Initial check
    window.addEventListener('resize', handleScroll);
    return () => window.removeEventListener('resize', handleScroll);
  }, [displayMovies]);

  const scroll = (direction) => {
    if (scrollContainerRef.current) {
      const scrollAmount = direction === 'left' ? -600 : 600;
      scrollContainerRef.current.scrollBy({ left: scrollAmount, behavior: 'smooth' });
    }
  };

  if (!movies || movies.length === 0) {
    return (
      <div className="w-full py-12 text-center">
        <p className="text-slate-400 text-lg">Không có phim nào để hiển thị</p>
      </div>
    );
  }

  return (
    <div className="relative w-full group">
      {/* Nút Previous */}
      {showLeftArrow && (
        <motion.button
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          className="absolute left-0 top-1/2 -translate-y-1/2 z-20 w-12 h-24 bg-black/50 hover:bg-black/80 backdrop-blur-md flex items-center justify-center border border-white/10 rounded-r-2xl transition-all"
          onClick={() => scroll('left')}
        >
          <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
          </svg>
        </motion.button>
      )}

      {/* Container trượt ngang */}
      <div 
        ref={scrollContainerRef}
        onScroll={handleScroll}
        className="flex gap-4 sm:gap-6 overflow-x-auto snap-x snap-mandatory scrollbar-hide py-8 px-4 sm:px-0"
        style={{ scrollbarWidth: 'none', msOverflowStyle: 'none' }}
      >
        {displayMovies.map((movie, index) => (
          <div 
            key={`${movie.id}-${index}`} 
            className="flex-none w-[260px] sm:w-[280px] lg:w-[300px] snap-start"
          >
            <MovieCard
              movie={movie}
              onBuyTicket={() => onBuyTicket(movie.id)}
              onWatchTrailer={() => onWatchTrailer(movie.id)}
              isComingSoon={isComingSoon}
            />
          </div>
        ))}
      </div>

      {/* Nút Next */}
      {showRightArrow && (
        <motion.button
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          className="absolute right-0 top-1/2 -translate-y-1/2 z-20 w-12 h-24 bg-black/50 hover:bg-black/80 backdrop-blur-md flex items-center justify-center border border-white/10 rounded-l-2xl transition-all"
          onClick={() => scroll('right')}
        >
          <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
          </svg>
        </motion.button>
      )}
    </div>
  );
}

MovieSlider.propTypes = {
  movies: PropTypes.array.isRequired,
  isComingSoon: PropTypes.bool,
  onBuyTicket: PropTypes.func,
  onWatchTrailer: PropTypes.func,
};
