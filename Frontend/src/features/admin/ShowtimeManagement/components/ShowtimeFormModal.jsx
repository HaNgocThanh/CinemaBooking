import { useEffect } from 'react';
import { Modal, Form, Select, DatePicker, InputNumber, Spin } from 'antd';
import dayjs from 'dayjs';

/**
 * ShowtimeFormModal
 * Presentational component: form for creating a new showtime
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
}) {
  const [form] = Form.useForm();

  useEffect(() => {
    if (open) {
      form.resetFields();
    }
  }, [open, form]);

  const handleFinish = (values) => {
    const payload = {
      movieId: values.movieId,
      roomId: values.roomId,
      startTime: values.dateTime[0].toISOString(),
      endTime: values.dateTime[1].toISOString(),
      basePrice: values.basePrice,
    };
    onSubmit(payload);
  };

  const disabledDate = (current) => {
    return current && current < dayjs().startOf('day');
  };

  const disabledTime = () => ({
    disabledHours: () => Array.from({ length: 24 }, (_, i) => i).filter(
      (h) => h < 8 || h > 23
    ),
  });

  return (
    <Modal
      title="Them Suat Chieu Moi"
      open={open}
      onCancel={onCancel}
      onOk={() => form.submit()}
      okText="Luu"
      cancelText="Huy"
      confirmLoading={loading}
      destroyOnClose
      width={520}
    >
      <Spin spinning={loading}>
        <Form
          form={form}
          layout="vertical"
          onFinish={handleFinish}
          autoComplete="off"
        >
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
              notFoundContent={
                isLoadingMovies ? 'Dang tai...' : 'Khong co phim nao'
              }
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
              notFoundContent={
                isLoadingRooms ? 'Dang tai...' : 'Khong co phong nao'
              }
              filterOption={(input, option) =>
                (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
              }
              options={rooms.map((r) => ({
                value: r.id,
                label: `${r.name} (${r.type} - ${r.capacity} ghe)`,
              }))}
            />
          </Form.Item>

          <Form.Item
            name="dateTime"
            label="Ngay va Gio chieu"
            rules={[{ required: true, message: 'Vui long chon ngay va gio' }]}
          >
            <DatePicker.RangePicker
              showTime={{
                format: 'HH:mm',
                minuteStep: 15,
              }}
              format="DD/MM/YYYY HH:mm"
              disabledDate={disabledDate}
              disabledTime={disabledTime}
              placeholder={['Bat dau', 'Ket thuc']}
              style={{ width: '100%' }}
            />
          </Form.Item>

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
              formatter={(value) =>
                `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')
              }
              parser={(value) => value.replace(/,/g, '')}
            />
          </Form.Item>
        </Form>
      </Spin>
    </Modal>
  );
}
