import React, { useEffect } from 'react';
import { Form, Input, InputNumber, Select, Button, Row, Col, Spin, Tag, DatePicker, Switch } from 'antd';
import dayjs from 'dayjs';
import './MovieForm.css';

/**
 * Genre options for movie selection
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
 * Rating code options (MPAA-style)
 */
const ratingCodeOptions = [
  { label: 'P - Phổ biến', value: 'P' },
  { label: 'C13 - Cấm dưới 13 tuổi', value: 'C13' },
  { label: 'C16 - Cấm dưới 16 tuổi', value: 'C16' },
  { label: 'C18 - Cấm dưới 18 tuổi', value: 'C18' },
];

/**
 * MovieForm Component
 * Uses Ant Design Form (instead of react-hook-form) for full compatibility
 * with Ant Design input components (Select, InputNumber, DatePicker, etc.)
 */
export function MovieForm({
  initialData = null,
  loading = false,
  onSubmit,
  onCancel,
  isViewOnly = false,
}) {
  const [form] = Form.useForm();

  // Populate form when initialData changes (edit mode)
  useEffect(() => {
    if (initialData) {
      form.setFieldsValue({
        ...initialData,
        releaseDate: initialData.releaseDate ? dayjs(initialData.releaseDate) : null,
        endDate: initialData.endDate ? dayjs(initialData.endDate) : null,
      });
    } else {
      form.resetFields();
    }
  }, [initialData, form]);

  /**
   * Handle form submission — convert DatePicker values to ISO strings
   * and send to parent via onSubmit callback
   */
  const handleFinish = async (values) => {
    const payload = {
      ...values,
      releaseDate: values.releaseDate ? values.releaseDate.toISOString() : null,
      endDate: values.endDate ? values.endDate.toISOString() : null,
    };
    try {
      await onSubmit(payload);
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
          title: '',
          description: '',
          genre: 'action',
          durationMinutes: 120,
          ratingCode: 'P',
          director: '',
          cast: '',
          posterUrl: '',
          trailerUrl: '',
          isActive: true,
          isFeatured: false,
        }}
        disabled={isViewOnly}
      >
        <Row gutter={[16, 0]}>
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
              rules={[{ min: 10, message: 'Mô tả ít nhất 10 ký tự' }]}
            >
              <Input.TextArea rows={4} placeholder="Nhập mô tả về phim" />
            </Form.Item>

            {/* Genre & Release Date */}
            <Row gutter={16}>
              <Col xs={24} sm={12}>
                <Form.Item
                  name="genre"
                  label="Thể Loại"
                  rules={[{ required: true, message: 'Thể loại là bắt buộc' }]}
                >
                  <Select placeholder="Chọn thể loại" options={genreOptions} />
                </Form.Item>
              </Col>

              <Col xs={24} sm={12}>
                <Form.Item
                  name="releaseDate"
                  label="Ngày Khởi Chiếu"
                  rules={[{ required: true, message: 'Ngày khởi chiếu là bắt buộc' }]}
                >
                  <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
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

            {/* Duration & Rating Code */}
            <Row gutter={16}>
              <Col xs={24} sm={12}>
                <Form.Item
                  name="durationMinutes"
                  label="Thời Lượng (phút)"
                  rules={[
                    { required: true, message: 'Thời lượng là bắt buộc' },
                    { type: 'number', min: 30, message: 'Ít nhất 30 phút' },
                    { type: 'number', max: 300, message: 'Tối đa 300 phút' },
                  ]}
                >
                  <InputNumber min={30} max={300} placeholder="120" style={{ width: '100%' }} />
                </Form.Item>
              </Col>

              <Col xs={24} sm={12}>
                <Form.Item name="ratingCode" label="Xếp Hạng">
                  <Select placeholder="Chọn xếp hạng" options={ratingCodeOptions} />
                </Form.Item>
              </Col>
            </Row>

            {/* Language */}
            <Form.Item name="language" label="Ngôn Ngữ">
              <Input placeholder="VD: Tiếng Việt, English" />
            </Form.Item>
          </Col>

          {/* Right Column: Poster & Meta */}
          <Col xs={24} md={10}>
            {/* Poster URL */}
            <Form.Item name="posterUrl" label="URL Poster">
              <Input placeholder="https://example.com/poster.jpg" />
            </Form.Item>

            {/* Poster Preview */}
            <Form.Item label="Xem Trước Poster">
              <Form.Item noStyle shouldUpdate={(prev, cur) => prev.posterUrl !== cur.posterUrl}>
                {({ getFieldValue }) => {
                  const url = getFieldValue('posterUrl');
                  return url ? (
                    <div className="poster-preview">
                      <img src={url} alt="Poster Preview" />
                    </div>
                  ) : (
                    <div className="poster-placeholder">Chưa có poster</div>
                  );
                }}
              </Form.Item>
            </Form.Item>

            {/* Trailer URL */}
            <Form.Item name="trailerUrl" label="URL Trailer">
              <Input placeholder="https://youtube.com/watch?v=..." />
            </Form.Item>

            {/* Banner URL */}
            <Form.Item name="bannerUrl" label="URL Banner">
              <Input placeholder="https://example.com/banner.jpg" />
            </Form.Item>

            {/* End Date */}
            <Form.Item name="endDate" label="Ngày Kết Thúc">
              <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
            </Form.Item>

            {/* Status */}
            <Form.Item
              name="status"
              label="Trạng Thái Phim"
              initialValue="ComingSoon"
            >
              <Select
                placeholder="Chọn trạng thái"
                options={[
                  { value: 'ComingSoon', label: 'Sắp Chiếu' },
                  { value: 'NowShowing', label: 'Đang Chiếu' },
                  { value: 'Stopped', label: 'Ngưng Chiếu' },
                ]}
              />
            </Form.Item>

            {/* Active & Featured */}
            <Row gutter={16}>
              <Col span={12}>
                <Form.Item name="isActive" label="Kích Hoạt" valuePropName="checked">
                  <Switch checkedChildren="Bật" unCheckedChildren="Tắt" />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="isFeatured" label="Nổi Bật" valuePropName="checked">
                  <Switch checkedChildren="Có" unCheckedChildren="Không" />
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
