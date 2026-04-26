import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Tabs, Spin, Empty, Modal, Button } from 'antd';
import { PlayCircleOutlined, ClockCircleOutlined, CalendarOutlined, VideoCameraOutlined } from '@ant-design/icons';
import { getMovieDetails } from '@/services/movieDetailsApi';

/**
 * Chuyen doi "dd/MM/yyyy" thanh "Hôm nay dd/MM" / "Ngày mai dd/MM" / "Thứ X dd/MM"
 */
function formatDateLabel(dateStr) {
  const [day, month, year] = dateStr.split('/').map(Number);
  const date = new Date(year, month - 1, day);

  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const target = new Date(year, month - 1, day);
  target.setHours(0, 0, 0, 0);

  const diffDays = Math.round((target - today) / (1000 * 60 * 60 * 24));

  const dayNames = ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'];
  const dayName = dayNames[date.getDay()];

  let prefix = diffDays === 0 ? 'Hôm nay' : diffDays === 1 ? 'Ngày mai' : `${dayName}`;
  return `${prefix} ${day.toString().padStart(2, '0')}/${month.toString().padStart(2, '0')}`;
}

/**
 * Trich ID video YouTube tu URL
 */
function extractYouTubeId(url) {
  if (!url) return null;
  const match = url.match(/(?:youtu\.be\/|youtube\.com\/(?:embed\/|v\/|watch\?v=|watch\?.+&v=))([^&?]+)/);
  return match ? match[1] : null;
}

export default function MovieDetailsPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [trailerVisible, setTrailerVisible] = useState(false);

  const movieId = parseInt(id, 10);

  const { data, isLoading, isError, error } = useQuery({
    queryKey: ['movieDetails', movieId],
    queryFn: () => getMovieDetails(movieId),
    enabled: !!movieId,
    staleTime: 60 * 1000,
  });

  const movie = data?.data;
  const showtimeGroups = movie?.showtimeGroups ?? [];

  const youtubeId = movie?.trailerUrl ? extractYouTubeId(movie.trailerUrl) : null;

  const tabItems = showtimeGroups.map((group) => ({
    key: group.date,
    label: (
      <span className="flex items-center gap-1.5 px-2">
        <CalendarOutlined />
        {formatDateLabel(group.date)}
      </span>
    ),
    children: (
      <div className="flex flex-wrap gap-3 py-4">
        {group.showtimes.map((st) => (
          <Button
            key={st.id}
            type="primary"
            size="large"
            className="!rounded-xl !font-semibold !text-base !px-6 !h-12 !bg-rose-600 !border-rose-600 hover:!bg-rose-700 hover:!border-rose-700 transition-colors"
            onClick={() => navigate(`/book/${st.id}`)}
          >
            <span className="flex flex-col items-center leading-tight">
              <span className="text-sm font-normal text-white/80">Phòng {st.roomName}</span>
              <span>
                {new Date(st.startTime).toLocaleTimeString('vi-VN', {
                  hour: '2-digit',
                  minute: '2-digit',
                })}
              </span>
            </span>
          </Button>
        ))}
      </div>
    ),
  }));

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Spin size="large" />
      </div>
    );
  }

  if (isError || !movie) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Empty
          description={error?.message || 'Không thể tải thông tin phim'}
          className="text-slate-400"
        />
      </div>
    );
  }

  const posterFallback = 'https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?w=400&h=600&fit=crop';

  return (
    <div className="min-h-screen bg-slate-950 text-white">
      {/* ===== HERO SECTION ===== */}
      <div
        className="relative min-h-[520px] flex items-center"
        style={{
          backgroundImage: movie.bannerUrl
            ? `linear-gradient(to right, rgba(2,6,23,0.97) 40%, rgba(2,6,23,0.7) 100%), url(${movie.bannerUrl})`
            : 'linear-gradient(135deg, #0f172a 0%, #1e293b 100%)',
          backgroundSize: 'cover',
          backgroundPosition: 'center',
        }}
      >
        <div className="absolute inset-x-0 bottom-0 h-20 bg-gradient-to-t from-slate-950 to-transparent" />

        <div className="max-w-7xl mx-auto px-6 lg:px-16 w-full py-12 flex flex-col md:flex-row gap-10 items-center md:items-start relative z-10">
          {/* Poster */}
          <div className="flex-shrink-0 hidden md:block">
            <img
              src={movie.posterUrl || posterFallback}
              alt={movie.title}
              className="w-56 rounded-2xl shadow-2xl shadow-black/60 object-cover"
              style={{ height: '340px' }}
            />
          </div>

          {/* Movie Info */}
          <div className="flex-1 text-center md:text-left space-y-5">
            {/* Status badge */}
            <div className="flex flex-wrap gap-2 justify-center md:justify-start">
              {movie.status === 'NowShowing' && (
                <span className="px-3 py-1 bg-emerald-600 text-white text-xs font-semibold rounded-full uppercase tracking-wide">
                  Đang Chiếu
                </span>
              )}
              {movie.status === 'ComingSoon' && (
                <span className="px-3 py-1 bg-blue-600 text-white text-xs font-semibold rounded-full uppercase tracking-wide">
                  Sắp Chiếu
                </span>
              )}
              {movie.status === 'Stopped' && (
                <span className="px-3 py-1 bg-red-600 text-white text-xs font-semibold rounded-full uppercase tracking-wide">
                  Ngưng Chiếu
                </span>
              )}
            </div>

            <h1 className="text-4xl lg:text-5xl font-bold text-white leading-tight">
              {movie.title}
            </h1>

            {/* Meta row */}
            <div className="flex flex-wrap gap-4 justify-center md:justify-start text-slate-300 text-sm">
              {movie.durationMinutes && (
                <span className="flex items-center gap-1.5">
                  <ClockCircleOutlined className="text-rose-400" />
                  {movie.durationMinutes} phút
                </span>
              )}
              <span className="flex items-center gap-1.5">
                <VideoCameraOutlined className="text-rose-400" />
                {movie.genre || 'Phim lẻ'}
              </span>
            </div>

            {/* Description */}
            {movie.description && (
              <p className="text-slate-300 text-base leading-relaxed max-w-2xl">
                {movie.description.length > 300
                  ? movie.description.substring(0, 300) + '...'
                  : movie.description}
              </p>
            )}

            {/* Action buttons */}
            <div className="flex flex-wrap gap-3 justify-center md:justify-start pt-2">
              {youtubeId && (
                <Button
                  type="default"
                  size="large"
                  icon={<PlayCircleOutlined />}
                  className="!rounded-xl !border-slate-600 !text-white !bg-slate-800/80 hover:!bg-slate-700 !font-semibold"
                  onClick={() => setTrailerVisible(true)}
                >
                  Xem Trailer
                </Button>
              )}
              <Button
                type="primary"
                size="large"
                className="!rounded-xl !bg-rose-600 hover:!bg-rose-700 !border-rose-600 !font-semibold"
                onClick={() => {
                  const el = document.getElementById('showtimes');
                  el?.scrollIntoView({ behavior: 'smooth' });
                }}
              >
                Đặt Vé Ngay
              </Button>
            </div>
          </div>
        </div>
      </div>

      {/* ===== SHOWTIMES SECTION ===== */}
      <div id="showtimes" className="max-w-4xl mx-auto px-6 py-10">
        <div className="flex items-center gap-3 mb-6">
          <div className="w-1 h-8 bg-rose-600 rounded-full" />
          <h2 className="text-2xl font-bold text-white">Lịch Chiếu</h2>
        </div>

        {showtimeGroups.length === 0 ? (
          <div className="rounded-2xl border border-slate-800 bg-slate-900/50 p-12 text-center">
            <Empty
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              description={
                <span className="text-slate-400 text-base">
                  Hiện chưa có lịch chiếu cho bộ phim này.
                </span>
              }
            />
          </div>
        ) : (
          <div className="rounded-2xl border border-slate-800 bg-slate-900/60 p-6 backdrop-blur-sm">
            <Tabs
              items={tabItems}
              className="custom-showtime-tabs"
              tabPosition="top"
              size="large"
            />
          </div>
        )}

        {/* Back button */}
        <div className="mt-8 text-center">
          <Button
            type="text"
            className="!text-slate-400 hover:!text-white"
            onClick={() => navigate(-1)}
          >
            ← Quay lại
          </Button>
        </div>
      </div>

      {/* ===== TRAILER MODAL ===== */}
      <Modal
        open={trailerVisible}
        onCancel={() => setTrailerVisible(false)}
        footer={null}
        width={800}
        centered
        className="trailer-modal"
        styles={{ body: { padding: 0, background: '#000' } }}
      >
        <div className="relative pt-[56.25%]">
          {youtubeId && (
            <iframe
              className="absolute inset-0 w-full h-full rounded-lg"
              src={`https://www.youtube.com/embed/${youtubeId}?autoplay=1`}
              title="Trailer"
              allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
              allowFullScreen
            />
          )}
        </div>
      </Modal>
    </div>
  );
}
