import { Table, Tag, Space, Button, Tooltip, Popconfirm } from 'antd';
import { EditOutlined, DeleteOutlined, EyeOutlined } from '@ant-design/icons';

const { Column } = Table;

export function RoomTable({ rooms, loading, onEdit, onDelete, onViewSeats }) {
  const getTypeTag = (type) => {
    const colorMap = {
      '2D': 'blue',
      '3D': 'purple',
      'IMAX': 'gold',
    };
    return (
      <Tag color={colorMap[type] || 'default'} style={{ fontWeight: 500 }}>
        {type}
      </Tag>
    );
  };

  return (
    <Table
      dataSource={rooms}
      rowKey="id"
      loading={loading}
      pagination={{
        pageSize: 10,
        showSizeChanger: true,
        showTotal: (total) => `Tong ${total} phong`,
      }}
      locale={{
        emptyText: 'Chua co phong chieu nao. Nhan "Them phong" de tao moi.',
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
        title="Ten phong"
        dataIndex="name"
        key="name"
        render={(text) => <span style={{ fontWeight: 500 }}>{text}</span>}
        sorter={(a, b) => (a.name || '').localeCompare(b.name || '')}
      />
      <Column
        title="Loai phong"
        dataIndex="type"
        key="type"
        render={getTypeTag}
        width={120}
      />
      <Column
        title="Suc chua"
        dataIndex="capacity"
        key="capacity"
        align="center"
        width={100}
        render={(cap) => <span style={{ fontWeight: 500 }}>{cap} ghe</span>}
        sorter={(a, b) => (a.capacity || 0) - (b.capacity || 0)}
      />
      <Column
        title="Thao tac"
        key="actions"
        width={160}
        render={(_, record) => (
          <Space size="small">
            <Tooltip title="Xem so do ghe">
              <Button
                type="default"
                icon={<EyeOutlined />}
                size="small"
                onClick={() => onViewSeats(record)}
              />
            </Tooltip>
            <Tooltip title="Chinh sua">
              <Button
                type="primary"
                icon={<EditOutlined />}
                size="small"
                onClick={() => onEdit(record)}
              />
            </Tooltip>
            <Popconfirm
              title="Xac nhan xoa"
              description={`Ban co chac chan muon xoa phong "${record.name}"?`}
              onConfirm={() => onDelete(record.id)}
              okText="Xoa"
              cancelText="Huy"
              okButtonProps={{ danger: true }}
            >
              <Tooltip title="Xoa">
                <Button
                  type="primary"
                  danger
                  icon={<DeleteOutlined />}
                  size="small"
                />
              </Tooltip>
            </Popconfirm>
          </Space>
        )}
      />
    </Table>
  );
}
