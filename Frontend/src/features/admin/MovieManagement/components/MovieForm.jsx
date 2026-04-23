import React, { useEffect } from 'react';
import { Form, Input, InputNumber, Select, Button, Upload, Row, Col, Spin, Tag } from 'antd';
import { UploadOutlined } from '@ant-design/icons';
import './MovieForm.css';

const { TextArea } = Input;

/**
 * Genre options for Select component
 */
const genreOptions = [
  { label: 'Hành Động', value: 'action' },
  { label: 'Kinh Dị', value: 'horror' },
  { label: 'Tình Cảm', value: 'romance' },
  { label: 'Hài Kịch', value: 'comedy' },
  { label: 'Khoa Học Viễn Tưởng', value: 'scifi' },
  { label: 'Hoạt Hình', value: 'animation' },
  { label: 'Tài Liệu', value: 'documentary' },
  { label: 'Khác', value: 'other' },
];

/**
 * MovieForm Component
 * Uses Ant Design Form API for proper form handling.
 * 
 * Props:
 * - initialData: object | null - Movie data for editing
 * - loading: boolean - Show loading spinner
 * - onSubmit: (formData) => Promise - Form submission handler
 * - onCancel: () => void - Cancel handler
 * - isViewOnly: boolean - Disable all fields for view mode
 */
export function MovieForm({
  initialData = null,
  loading = false,
  onSubmit,
  onCancel,
  isViewOnly = false,
}) {
  const [form] = Form.useForm();

  // Reset form when initialData changes
  useEffect(() => {
    if (initialData) {
      form.setFieldsValue({
        ...initialData,
        releaseDate: initialData.releaseDate
          ? initialData.releaseDate.split('T')[0]
          : undefined,
      });
    } else {
      form.resetFields();
    }
  }, [initialData, form]);

  const handleFinish = async (values) => {
    try {
      await onSubmit(values);
    } catch (error) {
      console.error('Form submission error:', error);
    }
  };

  return (
    <Spin spinning={loading}>
      <Form
        form={form}
        layout="vertical"
        onFinish={handleFinish}
        className="movie-form"
        initialValues={{
          genre: 'action',
          durationMinutes: 120,
          rating: 0,
          isActive: true,
          isFeatured: false,
          releaseDate: new Date().toISOString().split('T')[0],
        }}
        disabled={isViewOnly}
      >
        <Row gutter={[16, 16]}>
          {/* Left Column */}
          <Col xs={24} md={14}>
            {/* Title */}
            <Form.Item
              name="title"
              label="Tên Phim"
              rules={[
                { required: true, message: 'Tên phim là bắt buộc' },
                { min: 3, message: 'Tên phim ít nhất 3 ký tự' },
              ]}
            >
              <Input placeholder="Nhập tên phim" />
            </Form.Item>

            {/* Description */}
            <Form.Item
              name="description"
              label="Mô Tả"
              rules={[
                { required: true, message: 'Mô tả là bắt buộc' },
                { min: 10, message: 'Mô tả ít nhất 10 ký tự' },
              ]}
            >
              <TextArea rows={4} placeholder="Nhập mô tả về phim" />
            </Form.Item>

            {/* Genre & Release Date */}
            <Row gutter={16}>
              <Col xs={24} sm={12}>
                <Form.Item
                  name="genre"
                  label="Thể Loại"
                  rules={[{ required: true, message: 'Thể loại là bắt buộc' }]}
                >
                  <Select
                    placeholder="Chọn thể loại"
                    options={genreOptions}
                  />
                </Form.Item>
              </Col>

              <Col xs={24} sm={12}>
                <Form.Item
                  name="releaseDate"
                  label="Ngày Khởi Chiếu"
                  rules={[{ required: true, message: 'Ngày khởi chiếu là bắt buộc' }]}
                >
                  <Input type="date" />
                </Form.Item>
              </Col>
            </Row>

            {/* Director & Cast */}
            <Row gutter={16}>
              <Col xs={24} sm={12}>
                <Form.Item
                  name="director"
                  label="Đạo Diễn"
                  rules={[{ required: true, message: 'Đạo diễn là bắt buộc' }]}
                >
                  <Input placeholder="Tên đạo diễn" />
                </Form.Item>
              </Col>

              <Col xs={24} sm={12}>
                <Form.Item name="cast" label="Diễn Viên">
                  <Input placeholder="Tên diễn viên (cách nhau bằng dấu phẩy)" />
                </Form.Item>
              </Col>
            </Row>

            {/* Duration & Rating */}
            <Row gutter={16}>
              <Col xs={24} sm={12}>
                <Form.Item
                  name="durationMinutes"
                  label="Thời Lượng (phút)"
                  rules={[
                    { required: true, message: 'Thời lượng là bắt buộc' },
                    { type: 'number', min: 30, message: 'Thời lượng ít nhất 30 phút' },
                    { type: 'number', max: 300, message: 'Thời lượng tối đa 300 phút' },
                  ]}
                >
                  <InputNumber
                    min={30}
                    max={300}
                    placeholder="120"
                    style={{ width: '100%' }}
                  />
                </Form.Item>
              </Col>

              <Col xs={24} sm={12}>
                <Form.Item
                  name="rating"
                  label="Đánh Giá (0-10)"
                  rules={[
                    { type: 'number', min: 0, message: 'Đánh giá không được âm' },
                    { type: 'number', max: 10, message: 'Đánh giá tối đa 10' },
                  ]}
                >
                  <InputNumber
                    min={0}
                    max={10}
                    step={0.1}
                    placeholder="0"
                    style={{ width: '100%' }}
                  />
                </Form.Item>
              </Col>
            </Row>
          </Col>

          {/* Right Column: Poster & Status */}
          <Col xs={24} md={10}>
            {/* Poster URL */}
            <Form.Item name="posterUrl" label="URL Poster (Ảnh Dọc)">
              <Input placeholder="Nhập URL hình ảnh poster dọc (tỉ lệ 2:3)" />
            </Form.Item>

            {/* Poster Preview */}
            <Form.Item noStyle shouldUpdate={(prev, cur) => prev.posterUrl !== cur.posterUrl}>
              {({ getFieldValue }) => {
                const url = getFieldValue('posterUrl');
                return url ? (
                  <div className="poster-preview" style={{ marginBottom: 16 }}>
                    <img src={url} alt="Poster Preview" />
                  </div>
                ) : null;
              }}
            </Form.Item>

            {/* Poster File Upload */}
            <Form.Item label="Hoặc Tải Lên Poster">
              <div className="poster-upload">
                <Upload
                  name="posterFile"
                  listType="picture"
                  maxCount={1}
                  accept="image/*"
                  beforeUpload={() => false}
                >
                  <Button icon={<UploadOutlined />}>
                    Chọn Hình Ảnh
                  </Button>
                </Upload>
              </div>
            </Form.Item>

            {/* Banner URL */}
            <Form.Item name="bannerUrl" label="URL Banner (Ảnh Ngang)">
              <Input placeholder="Nhập URL hình ảnh banner ngang (tỉ lệ 16:9)" />
            </Form.Item>

            {/* Banner Preview */}
            <Form.Item noStyle shouldUpdate={(prev, cur) => prev.bannerUrl !== cur.bannerUrl}>
              {({ getFieldValue }) => {
                const url = getFieldValue('bannerUrl');
                return url ? (
                  <div className="poster-preview" style={{ marginBottom: 16 }}>
                    <img src={url} alt="Banner Preview" style={{ width: '100%', aspectRatio: '16/9', objectFit: 'cover', borderRadius: '8px' }} />
                  </div>
                ) : null;
              }}
            </Form.Item>

            {/* Status */}
            <Row gutter={16}>
              <Col xs={12}>
                <Form.Item name="isActive" label="Trạng Thái">
                  <Select
                    options={[
                      { label: <Tag color="green">Đang Chiếu</Tag>, value: true },
                      { label: <Tag color="red">Dừng Chiếu</Tag>, value: false },
                    ]}
                  />
                </Form.Item>
              </Col>
              <Col xs={12}>
                <Form.Item name="isFeatured" label="Phim Nổi Bật">
                  <Select
                    options={[
                      { label: <Tag color="gold">Có</Tag>, value: true },
                      { label: <Tag color="default">Không</Tag>, value: false },
                    ]}
                  />
                </Form.Item>
              </Col>
            </Row>
          </Col>
        </Row>

        {/* Form Actions */}
        {!isViewOnly && (
          <Row gutter={16} className="movie-form-actions">
            <Col>
              <Button
                type="primary"
                htmlType="submit"
                loading={loading}
                size="large"
              >
                {initialData ? 'Cập Nhật Phim' : 'Thêm Phim Mới'}
              </Button>
            </Col>
            <Col>
              <Button onClick={onCancel} size="large">
                Hủy
              </Button>
            </Col>
          </Row>
        )}
      </Form>
    </Spin>
  );
}
