import { useEffect, useRef, useState } from 'react';
import { Modal, Form, Select, DatePicker, InputNumber, Alert } from 'antd';
import dayjs from 'dayjs';

/**
 * ShowtimeFormModal
 * Handles both Create and Edit modes for showtime management.
 *
 * Create mode: MovieId, RoomId, StartTime (auto-calc EndTime), BasePrice
 * Edit mode:   StartTime + BasePrice + IsActive; EndTime is auto-calculated by backend
 */
export function ShowtimeFormModal({
  open,
  onCancel,
  onSubmit,
  loading,
  movies,
  isLoadingMovies,
  rooms,
  isLoadingRooms,
  isEdit,
  editId,
  editInitialData,
}) {
  const [form] = Form.useForm();
  const prevOpenRef = useRef(open);
  const [selectedMovieId, setSelectedMovieId] = useState(null);

  // Get selected movie for validation
  const selectedMovie = movies.find((m) => m.id === selectedMovieId);

  // Reset form when modal fully closes
  useEffect(() => {
    if (!open && prevOpenRef.current) {
      form.resetFields();
      setSelectedMovieId(null);
    }
    prevOpenRef.current = open;
  }, [open, form]);

  // Populate form when editInitialData arrives
  useEffect(() => {
    if (isEdit && editInitialData) {
      form.setFieldsValue({
        movieId: editInitialData.movieId,
        roomId: editInitialData.roomId,
        startTime: dayjs(editInitialData.startTime),
        basePrice: editInitialData.basePrice,
        isActive: editInitialData.isActive,
      });
      setSelectedMovieId(editInitialData.movieId);
    }
  }, [isEdit, editInitialData, form]);

  const handleMovieChange = (movieId) => {
    setSelectedMovieId(movieId);
    form.setFieldsValue({ startTime: null });
  };

  const handleFinish = async (values) => {
    // IMPORTANT: Use format() instead of toISOString() to preserve local timezone (UTC+7).
    // toISOString() converts to UTC which causes timezone shift on Oracle backend.
    const localTimeStr = values.startTime.format('YYYY-MM-DDTHH:mm:ss');

    const payload = isEdit
      ? {
          startTime: localTimeStr,
          basePrice: values.basePrice,
          isActive: values.isActive,
        }
      : {
          movieId: values.movieId,
          roomId: values.roomId,
          startTime: localTimeStr,
          basePrice: values.basePrice,
        };

    await onSubmit(payload);
    form.resetFields();
    setSelectedMovieId(null);
  };

  const disabledDate = (current) => {
    if (!current) return false;
    if (current < dayjs().startOf('day')) return true;
    if (selectedMovie?.releaseDate && current < dayjs(selectedMovie.releaseDate).startOf('day')) return true;
    if (selectedMovie?.endDate && current > dayjs(selectedMovie.endDate).endOf('day')) return true;
    return false;
  };

  const disabledTime = () => ({
    disabledHours: () => Array.from({ length: 24 }, (_, i) => i).filter((h) => h < 8 || h > 23),
  });

  return (
    <Modal
      title={isEdit ? 'Chinh Sua Suat Chieu' : 'Them Suat Chieu Moi'}
      open={open}
      onCancel={onCancel}
      onOk={() => form.submit()}
      okText={isEdit ? 'Cap nhat' : 'Luu'}
      cancelText="Huy"
      confirmLoading={loading}
      width={520}
    >
      <Form
        form={form}
        layout="vertical"
        onFinish={handleFinish}
        autoComplete="off"
      >
        {/* === Create mode: Movie, Room === */}
        {!isEdit && (
          <>
            <Form.Item
              name="movieId"
              label="Phim"
              rules={[{ required: true, message: 'Vui long chon phim' }]}
            >
              <Select
                placeholder="Chon phim"
                showSearch
                optionFilterProp="children"
                loading={isLoadingMovies}
                onChange={handleMovieChange}
                notFoundContent={isLoadingMovies ? 'Dang tai...' : 'Khong co phim nao'}
                filterOption={(input, option) =>
                  (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
                }
                options={movies.map((m) => ({
                  value: m.id,
                  label: m.title,
                }))}
              />
            </Form.Item>

            <Form.Item
              name="roomId"
              label="Phong"
              rules={[{ required: true, message: 'Vui long chon phong' }]}
            >
              <Select
                placeholder="Chon phong"
                showSearch
                optionFilterProp="children"
                loading={isLoadingRooms}
                notFoundContent={isLoadingRooms ? 'Dang tai...' : 'Khong co phong nao'}
                filterOption={(input, option) =>
                  (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
                }
                options={rooms.map((r) => ({
                  value: r.id,
                  label: `${r.name} (${r.type} - ${r.capacity} ghe)`,
                }))}
              />
            </Form.Item>
          </>
        )}

        {/* === StartTime (both modes) === */}
        <Form.Item
          name="startTime"
          label="Thoi gian bat dau"
          rules={[
            { required: true, message: 'Vui long chon ngay va gio bat dau' },
            {
              validator: (_, value) => {
                if (!value || !selectedMovie) return Promise.resolve();

                const start = dayjs(value);
                const releaseDate = selectedMovie.releaseDate ? dayjs(selectedMovie.releaseDate).startOf('day') : null;
                const endDate = selectedMovie.endDate ? dayjs(selectedMovie.endDate).endOf('day') : null;

                if (releaseDate && start.isBefore(releaseDate)) {
                  return Promise.reject(new Error(`Phim bat dau cong chieu tu ${dayjs(releaseDate).format('DD/MM/YYYY')}`));
                }
                if (endDate && start.isAfter(endDate)) {
                  return Promise.reject(new Error(`Phim ket thuc cong chieu vao ${dayjs(endDate).format('DD/MM/YYYY')}`));
                }
                return Promise.resolve();
              },
            },
          ]}
          dependencies={!isEdit ? ['movieId'] : []}
        >
          <DatePicker
            showTime={{ format: 'HH:mm', minuteStep: 15 }}
            format="DD/MM/YYYY HH:mm"
            disabledDate={disabledDate}
            disabledTime={disabledTime}
            placeholder="Chon ngay va gio"
            style={{ width: '100%' }}
          />
        </Form.Item>

        {/* === Movie release window hint === */}
        {!isEdit && selectedMovie && (
          <div style={{ marginBottom: 16 }}>
            {selectedMovie.releaseDate && (
              <Alert
                type="info"
                showIcon
                message={`Phim "${selectedMovie.title}" chieu tu ${dayjs(selectedMovie.releaseDate).format('DD/MM/YYYY')} den ${selectedMovie.endDate ? dayjs(selectedMovie.endDate).format('DD/MM/YYYY') : 'chua xac dinh'}.`}
                style={{ fontSize: 12 }}
              />
            )}
            {selectedMovie.durationMinutes && (
              <p style={{ fontSize: 12, color: '#888', marginTop: 4 }}>
                Thoi luong phim: {selectedMovie.durationMinutes} phut.
                Thoi gian ket thuc se tu dong tinh = Thoi gian bat dau + {selectedMovie.durationMinutes} phut + 15 phut nghi.
              </p>
            )}
          </div>
        )}

        {/* === BasePrice (both modes) === */}
        <Form.Item
          name="basePrice"
          label="Gia co ban (VND)"
          rules={[
            { required: true, message: 'Vui long nhap gia ve' },
            {
              type: 'number',
              min: 10000,
              message: 'Gia phai lon hon 10,000 VND',
            },
          ]}
          initialValue={120000}
        >
          <InputNumber
            min={10000}
            step={5000}
            style={{ width: '100%' }}
            formatter={(value) => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
            parser={(value) => value.replace(/,/g, '')}
          />
        </Form.Item>

        {/* === Edit mode: IsActive toggle === */}
        {isEdit && (
          <Form.Item
            name="isActive"
            label="Trang thai"
            valuePropName="checked"
            initialValue={true}
          >
            <Select
              options={[
                { value: true, label: 'Dang chieu' },
                { value: false, label: 'Ngung chieu' },
              ]}
            />
          </Form.Item>
        )}
      </Form>
    </Modal>
  );
}
