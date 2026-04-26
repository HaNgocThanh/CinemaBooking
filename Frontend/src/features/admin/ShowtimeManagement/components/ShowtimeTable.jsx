import { Table, Tag, Space } from 'antd';
import { ClockCircleOutlined } from '@ant-design/icons';

const { Column } = Table;

/**
 * ShowtimeTable
 * Presentational component: displays showtime list with Ant Design Table
 */
export function ShowtimeTable({ showtimes, loading }) {
  const formatDateTime = (dateString) => {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleString('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const formatCurrency = (amount) => {
    if (amount == null) return '-';
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
    }).format(amount);
  };

  const getStatusTag = (isActive) => {
    return isActive ? (
      <Tag color="green">Dang chieu</Tag>
    ) : (
      <Tag color="red">Da ngung</Tag>
    );
  };

  return (
    <Table
      dataSource={showtimes}
      rowKey="id"
      loading={loading}
      pagination={{
        pageSize: 10,
        showSizeChanger: true,
        showTotal: (total) => `Tong ${total} suat chieu`,
      }}
      locale={{
        emptyText: 'Chua co suat chieu nao. Nhan "Them suat chieu" de tao moi.',
      }}
    >
      <Column
        title="ID"
        dataIndex="id"
        key="id"
        width={60}
        sorter={(a, b) => a.id - b.id}
      />
      <Column
        title="Phim"
        dataIndex="movieTitle"
        key="movieTitle"
        render={(text) => <span style={{ fontWeight: 500 }}>{text}</span>}
        sorter={(a, b) => (a.movieTitle || '').localeCompare(b.movieTitle || '')}
      />
      <Column
        title="Phong"
        dataIndex="roomName"
        key="roomName"
        render={(text) => <span style={{ fontWeight: 500 }}>{text}</span>}
      />
      <Column
        title="Thoi gian bat dau"
        dataIndex="startTime"
        key="startTime"
        render={(text) => (
          <Space>
            <ClockCircleOutlined style={{ color: '#1890ff' }} />
            {formatDateTime(text)}
          </Space>
        )}
        sorter={(a, b) => new Date(a.startTime) - new Date(b.startTime)}
      />
      <Column
        title="Thoi gian ket thuc"
        dataIndex="endTime"
        key="endTime"
        render={(text) => formatDateTime(text)}
      />
      <Column
        title="Gia co ban"
        dataIndex="basePrice"
        key="basePrice"
        render={(text) => (
          <span style={{ color: '#fa8c16', fontWeight: 500 }}>
            {formatCurrency(text)}
          </span>
        )}
        sorter={(a, b) => (a.basePrice || 0) - (b.basePrice || 0)}
      />
      <Column
        title="Ghe"
        key="seats"
        render={(_, record) => (
          <span>
            {record.bookedSeatsCount || 0} / {record.totalSeats || 0}
          </span>
        )}
      />
      <Column
        title="Trang thai"
        dataIndex="isActive"
        key="isActive"
        render={getStatusTag}
        width={120}
      />
    </Table>
  );
}
