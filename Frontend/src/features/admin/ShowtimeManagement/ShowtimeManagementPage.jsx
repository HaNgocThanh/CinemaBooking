import { useState } from 'react';
import { Button, message } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { ShowtimeTable } from './components/ShowtimeTable';
import { ShowtimeFormModal } from './components/ShowtimeFormModal';
import { useShowtimes } from './hooks/useShowtimes';
import { showtimeAdminApi } from './api/showtimeAdminApi';

/**
 * ShowtimeManagementPage
 * Container component: orchestrates showtime list, create form, and edit form
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
    updateShowtime,
    isCreating,
    isUpdating,
  } = useShowtimes();

  // === Create modal state ===
  const [createModalVisible, setCreateModalVisible] = useState(false);

  // === Edit modal state ===
  const [editModalVisible, setEditModalVisible] = useState(false);
  const [editingShowtime, setEditingShowtime] = useState(null); // row data from table
  const [editInitialData, setEditInitialData] = useState(null); // fetched from API
  const [isLoadingDetails, setIsLoadingDetails] = useState(false);

  // === Handlers ===

  const handleAddShowtime = () => {
    setCreateModalVisible(true);
  };

  const handleCreateModalCancel = () => {
    setCreateModalVisible(false);
  };

  const handleCreateSubmit = async (formData) => {
    try {
      await createShowtime(formData);
      message.success('Tao suat chieu thanh cong!');
      setCreateModalVisible(false);
    } catch (err) {
      message.error(err?.message || 'Co loi xay ra khi tao suat chieu!');
      throw err; // re-throw to keep modal open
    }
  };

  /** Triggered when user clicks "Sửa" on a table row */
  const handleEdit = async (record) => {
    setEditingShowtime(record);
    setEditInitialData(null);
    setIsLoadingDetails(true);
    setEditModalVisible(true);

    try {
      const response = await showtimeAdminApi.getShowtimeById(record.id);
      setEditInitialData(response.data);
    } catch (err) {
      message.error('Khong the tai chi tiet suat chieu!');
      setEditModalVisible(false);
      setEditingShowtime(null);
    } finally {
      setIsLoadingDetails(false);
    }
  };

  const handleEditModalCancel = () => {
    setEditModalVisible(false);
    setEditingShowtime(null);
    setEditInitialData(null);
  };

  const handleEditSubmit = async (formData) => {
    try {
      await updateShowtime({ id: editingShowtime.id, data: formData });
      message.success('Cap nhat suat chieu thanh cong!');
      setEditModalVisible(false);
      setEditingShowtime(null);
      setEditInitialData(null);
    } catch (err) {
      const apiError = err?.response?.data?.error?.message;
      message.error(apiError || err?.message || 'Co loi xay ra khi cap nhat suat chieu!');
      throw err; // re-throw to keep modal open
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

      <ShowtimeTable
        showtimes={showtimes}
        loading={isLoading}
        onEdit={handleEdit}
      />

      {/* === Create Modal === */}
      <ShowtimeFormModal
        open={createModalVisible}
        onCancel={handleCreateModalCancel}
        onSubmit={handleCreateSubmit}
        loading={isCreating}
        movies={movies}
        isLoadingMovies={isLoadingMovies}
        rooms={rooms}
        isLoadingRooms={isLoadingRooms}
        isEdit={false}
      />

      {/* === Edit Modal === */}
      <ShowtimeFormModal
        open={editModalVisible}
        onCancel={handleEditModalCancel}
        onSubmit={handleEditSubmit}
        loading={isUpdating}
        movies={movies}
        isLoadingMovies={isLoadingMovies}
        rooms={rooms}
        isLoadingRooms={isLoadingRooms}
        isEdit={true}
        editId={editingShowtime?.id}
        editInitialData={editInitialData}
      />
    </div>
  );
}
