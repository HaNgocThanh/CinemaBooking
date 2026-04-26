import { useEffect } from 'react';
import { Modal, Form, Input, InputNumber, Select, Spin } from 'antd';

const ROOM_TYPES = [
  { value: '2D', label: '2D' },
  { value: '3D', label: '3D' },
  { value: 'IMAX', label: 'IMAX' },
];

export function RoomFormModal({
  open,
  onCancel,
  onSubmit,
  loading,
  initialValues,
  mode,
}) {
  const [form] = Form.useForm();

  useEffect(() => {
    if (open) {
      form.resetFields();
      if (initialValues) {
        form.setFieldsValue({
          name: initialValues.name,
          type: initialValues.type,
          capacity: initialValues.capacity,
        });
      }
    }
  }, [open, form, initialValues]);

  const handleFinish = (values) => {
    onSubmit(values, initialValues?.id);
  };

  const title = mode === 'edit' ? 'Chinh Sua Phong Chieu' : 'Them Phong Chieu Moi';

  return (
    <Modal
      title={title}
      open={open}
      onCancel={onCancel}
      onOk={() => form.submit()}
      okText="Luu"
      cancelText="Huy"
      confirmLoading={loading}
      destroyOnClose
      width={480}
    >
      <Spin spinning={loading}>
        <Form
          form={form}
          layout="vertical"
          onFinish={handleFinish}
          autoComplete="off"
        >
          <Form.Item
            name="name"
            label="Ten phong"
            rules={[
              { required: true, message: 'Vui long nhap ten phong' },
              { max: 50, message: 'Ten phong toi da 50 ky tu' },
            ]}
          >
            <Input placeholder="VD: Phong 01" maxLength={50} />
          </Form.Item>

          <Form.Item
            name="type"
            label="Loai phong"
            rules={[{ required: true, message: 'Vui long chon loai phong' }]}
          >
            <Select
              placeholder="Chon loai phong"
              options={ROOM_TYPES}
              style={{ width: '100%' }}
            />
          </Form.Item>

          <Form.Item
            name="capacity"
            label="Suc chua (so ghe)"
            rules={[
              { required: true, message: 'Vui long nhap suc chua' },
              { type: 'number', min: 1, message: 'Suc chua phai lon hon 0' },
              { type: 'number', max: 1000, message: 'Suc chua toi da 1000 ghe' },
            ]}
          >
            <InputNumber
              min={1}
              max={1000}
              style={{ width: '100%' }}
              placeholder="VD: 20"
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
