import { useState } from 'react';
import { Button, message } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { ShowtimeTable } from './components/ShowtimeTable';
import { ShowtimeFormModal } from './components/ShowtimeFormModal';
import { useShowtimes } from './hooks/useShowtimes';

/**
 * ShowtimeManagementPage
 * Container component: orchestrates showtime list and create form
 */
export function ShowtimeManagementPage() {
  const {
    showtimes,
    isLoading,
    isError,
    error,
    movies,
    isLoadingMovies,
    rooms,
    isLoadingRooms,
    createShowtime,
    isCreating,
  } = useShowtimes();

  const [modalVisible, setModalVisible] = useState(false);

  const handleAddShowtime = () => {
    setModalVisible(true);
  };

  const handleModalCancel = () => {
    setModalVisible(false);
  };

  const handleFormSubmit = async (formData) => {
    try {
      await createShowtime(formData);
      message.success('Tao suat chieu thanh cong!');
      setModalVisible(false);
    } catch (err) {
      message.error(err?.message || 'Co loi xay ra khi tao suat chieu!');
    }
  };

  return (
    <div className="showtime-management-page">
      <div className="page-header">
        <div className="page-header-content">
          <h1>Quan Ly Suat Chieu</h1>
          <p>Quan ly danh sach suat chieu trong he thong dat ve xem phim</p>
        </div>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={handleAddShowtime}
          size="large"
        >
          Them Suat Chieu
        </Button>
      </div>

      {isError && (
        <div className="error-message">
          <p>Co loi khi tai danh sach suat chieu: {error?.message}</p>
        </div>
      )}

      <ShowtimeTable showtimes={showtimes} loading={isLoading} />

      <ShowtimeFormModal
        open={modalVisible}
        onCancel={handleModalCancel}
        onSubmit={handleFormSubmit}
        loading={isCreating}
        movies={movies}
        isLoadingMovies={isLoadingMovies}
        rooms={rooms}
        isLoadingRooms={isLoadingRooms}
      />
    </div>
  );
}
