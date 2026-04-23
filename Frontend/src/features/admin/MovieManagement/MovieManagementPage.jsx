import React, { useState } from 'react';
import { Modal, message, Spin } from 'antd';
import { MovieList } from './components/MovieList';
import { MovieForm } from './components/MovieForm';
import { useMovies } from './hooks/useMovies';
import './MovieManagementPage.css';

/**
 * MovieManagementPage
 * Container component: Orchestrates movie list and form
 */
export function MovieManagementPage() {
  const {
    movies,
    isLoading,
    isError,
    error,
    pagination,
    filters,
    createMovie,
    updateMovie,
    deleteMovie,
    isCreating,
    isUpdating,
    isDeleting,
  } = useMovies();

  const [modalVisible, setModalVisible] = useState(false);
  const [editingMovieId, setEditingMovieId] = useState(null);
  const [editingMovie, setEditingMovie] = useState(null);
  const [viewMode, setViewMode] = useState(null); // 'view', 'edit', or null

  // Handle Add Movie
  const handleAddMovie = () => {
    setEditingMovieId(null);
    setEditingMovie(null);
    setViewMode(null);
    setModalVisible(true);
  };

  // Handle Edit Movie
  const handleEditMovie = (id, mode = 'edit') => {
    setEditingMovieId(id);
    setViewMode(mode);
    const movie = movies.find((m) => m.id === id);
    setEditingMovie(movie || null);
    setModalVisible(true);
  };

  // Handle Form Submit
  const handleFormSubmit = async (formData) => {
    try {
      if (editingMovieId) {
        await updateMovie(editingMovieId, formData);
        message.success('Cập nhật phim thành công!');
      } else {
        await createMovie(formData);
        message.success('Thêm phim mới thành công!');
      }
      setModalVisible(false);
      setEditingMovieId(null);
      setEditingMovie(null);
    } catch (error) {
      message.error(error?.message || 'Có lỗi xảy ra!');
    }
  };

  // Handle Delete Movie
  const handleDeleteMovie = async (id) => {
    try {
      await deleteMovie(id);
      message.success('Xóa phim thành công!');
    } catch (error) {
      message.error(error?.message || 'Không thể xóa phim!');
    }
  };

  // Handle Search
  const handleSearch = (value) => {
    filters.setSearchText(value);
    pagination.setPageNum(1);
  };

  // Handle Sort
  const handleSort = (value) => {
    filters.setSortBy(value);
    pagination.setPageNum(1);
  };

  return (
    <div className="movie-management-page">
      <div className="page-header">
        <h1>Quản Lý Phim</h1>
        <p>Quản lý danh sách phim trong hệ thống đặt vé xem phim</p>
      </div>

      {isError && (
        <div className="error-message">
          <p>Có lỗi khi tải danh sách phim: {error?.message}</p>
        </div>
      )}

      <MovieList
        movies={movies}
        loading={isLoading || isDeleting}
        pagination={pagination}
        filters={filters}
        onAdd={handleAddMovie}
        onEdit={handleEditMovie}
        onDelete={handleDeleteMovie}
        onSearch={handleSearch}
        onSort={handleSort}
      />

      {/* Modal: Add/Edit Movie */}
      <Modal
        title={
          editingMovieId
            ? 'Chỉnh Sửa Phim'
            : viewMode === 'view'
              ? 'Xem Chi Tiết Phim'
              : 'Thêm Phim Mới'
        }
        open={modalVisible}
        onCancel={() => setModalVisible(false)}
        footer={null}
        width={1000}
        className="movie-form-modal"
      >
        <Spin spinning={isUpdating || isCreating}>
          <MovieForm
            initialData={editingMovie}
            loading={isUpdating || isCreating}
            onSubmit={handleFormSubmit}
            onCancel={() => setModalVisible(false)}
            isViewOnly={viewMode === 'view'}
          />
        </Spin>
      </Modal>
    </div>
  );
}
