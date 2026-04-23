import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import MovieGrid from '@/features/movies/components/MovieGrid';
import { useNowShowingMovies, useComingSoonMovies } from '@/services/movieApi';

/**
 * MoviesPage - Page to display all movies (Now Showing / Coming Soon)
 */
export default function MoviesPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const navigate = useNavigate();
  
  // Lấy tab từ URL, mặc định là now-showing
  const tabFromUrl = searchParams.get('tab') || 'now-showing';
  const [activeTab, setActiveTab] = useState(tabFromUrl);

  // Sync state with URL when URL changes
  useEffect(() => {
    setActiveTab(tabFromUrl);
  }, [tabFromUrl]);

  const handleTabChange = (tab) => {
    setActiveTab(tab);
    setSearchParams({ tab });
  };

  // Fetch data
  const { data: nowShowingMovies = [], isLoading: isLoadingNow, error: errorNow } = useNowShowingMovies();
  const { data: comingSoonMovies = [], isLoading: isLoadingSoon, error: errorSoon } = useComingSoonMovies();

  const isLoading = activeTab === 'now-showing' ? isLoadingNow : isLoadingSoon;
  const error = activeTab === 'now-showing' ? errorNow : errorSoon;
  const currentMovies = activeTab === 'now-showing' ? nowShowingMovies : comingSoonMovies;

  const handleBuyTicket = (movieId) => {
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
      className="min-h-screen bg-slate-950 text-slate-100 pt-24"
    >
      <div className="max-w-7xl mx-auto px-6 sm:px-12">
        {/* Header & Tabs */}
        <div className="mb-12 border-b border-white/10 pb-4">
          <h1 className="text-4xl font-light mb-8">Danh sách Phim</h1>
          <div className="flex gap-8">
            <button
              onClick={() => handleTabChange('now-showing')}
              className={`text-lg font-semibold tracking-wide pb-4 relative transition-all ${
                activeTab === 'now-showing' 
                  ? 'text-rose-500' 
                  : 'text-slate-400 hover:text-slate-200'
              }`}
            >
              ĐANG CHIẾU
              {activeTab === 'now-showing' && (
                <motion.div 
                  layoutId="activeTabIndicator"
                  className="absolute bottom-[-1px] left-0 right-0 h-0.5 bg-rose-500" 
                />
              )}
            </button>
            <button
              onClick={() => handleTabChange('coming-soon')}
              className={`text-lg font-semibold tracking-wide pb-4 relative transition-all ${
                activeTab === 'coming-soon' 
                  ? 'text-rose-500' 
                  : 'text-slate-400 hover:text-slate-200'
              }`}
            >
              SẮP CHIẾU
              {activeTab === 'coming-soon' && (
                <motion.div 
                  layoutId="activeTabIndicator"
                  className="absolute bottom-[-1px] left-0 right-0 h-0.5 bg-rose-500" 
                />
              )}
            </button>
          </div>
        </div>

        {/* Loading State */}
        {isLoading && (
          <div className="py-24 text-center">
            <Spin size="large" />
          </div>
        )}

        {/* Error State */}
        {error && (
          <div className="py-24 text-center text-red-400">
            <p>Đã xảy ra lỗi: {error.message}</p>
          </div>
        )}

        {/* Content */}
        {!isLoading && !error && (
          <div className="-mx-6 sm:-mx-12 lg:-mx-[150px]">
             {/* Re-use MovieGrid but without title */}
             <MovieGrid
                movies={currentMovies}
                title=""
                onBuyTicket={handleBuyTicket}
                onWatchTrailer={handleWatchTrailer}
             />
          </div>
        )}
      </div>
    </motion.div>
  );
}

// Temporary Spin component since we didn't import Ant Design Spin here to keep it lightweight
function Spin() {
  return (
    <motion.div
      animate={{ opacity: [0.5, 1, 0.5] }}
      transition={{ duration: 2, repeat: Infinity }}
      className="text-slate-400 inline-block"
    >
      <p className="text-lg">Đang tải...</p>
    </motion.div>
  );
}
