import React, { useState } from 'react';
import {
  Table,
  Button,
  Space,
  Popconfirm,
  Input,
  Select,
  Row,
  Col,
  Pagination,
  Tag,
  Image,
  Tooltip,
} from 'antd';
import {
  EditOutlined,
  DeleteOutlined,
  EyeOutlined,
  SearchOutlined,
  PlusOutlined,
} from '@ant-design/icons';
import './MovieList.css';

/**
 * MovieList Component
 * Presentational component: Displays movies in table format
 * Props-based, NO hooks, NO API calls
 */
export function MovieList({
  movies = [],
  loading = false,
  pagination = {},
  filters = {},
  onAdd,
  onEdit,
  onDelete,
  onToggleStatus,
  onSearch,
  onSort,
}) {
  const [selectedRowKeys, setSelectedRowKeys] = useState([]);

  const columns = [
    {
      title: 'Poster',
      dataIndex: 'posterUrl',
      width: 80,
      render: (posterUrl) => (
        <Image
          src={posterUrl}
          alt="Poster"
          width={50}
          height={75}
          preview
          fallback="https://via.placeholder.com/50x75?text=No+Image"
        />
      ),
    },
    {
      title: 'Tên Phim',
      dataIndex: 'title',
      key: 'title',
      width: 250,
      sorter: (a, b) => a.title.localeCompare(b.title),
      render: (title) => <strong>{title}</strong>,
    },
    {
      title: 'Thể Loại',
      dataIndex: 'genre',
      key: 'genre',
      width: 120,
      render: (genre) => <Tag color="blue">{genre}</Tag>,
    },
    {
      title: 'Thời Lượng',
      dataIndex: 'durationMinutes',
      key: 'durationMinutes',
      width: 100,
      render: (duration) => `${duration} phút`,
    },
    {
      title: 'Ngày Khởi Chiếu',
      dataIndex: 'releaseDate',
      key: 'releaseDate',
      width: 130,
      sorter: (a, b) => new Date(a.releaseDate) - new Date(b.releaseDate),
      render: (date) => new Date(date).toLocaleDateString('vi-VN'),
    },
    {
      title: 'Trạng Thái',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 100,
      render: (isActive) => (
        <Tag color={isActive ? 'green' : 'red'}>
          {isActive ? 'Đang Chiếu' : 'Dừng'}
        </Tag>
      ),
    },
    {
      title: 'Đánh Giá',
      dataIndex: 'rating',
      key: 'rating',
      width: 80,
      render: (rating) => `${rating?.toFixed(1) || 'N/A'} / 10`,
    },
    {
      title: 'Hành Động',
      key: 'action',
      width: 200,
      fixed: 'right',
      render: (_, record) => (
        <Space size="small">
          <Tooltip title="Xem chi tiết">
            <Button
              type="primary"
              icon={<EyeOutlined />}
              size="small"
              onClick={() => onEdit(record.id, 'view')}
            />
          </Tooltip>

          <Tooltip title="Chỉnh sửa">
            <Button
              type="default"
              icon={<EditOutlined />}
              size="small"
              onClick={() => onEdit(record.id)}
            />
          </Tooltip>

          <Tooltip title="Xóa">
            <Popconfirm
              title="Xóa Phim"
              description="Bạn có chắc muốn xóa phim này?"
              onConfirm={() => onDelete(record.id)}
              okText="Có"
              cancelText="Không"
              okButtonProps={{ danger: true }}
            >
              <Button
                type="default"
                danger
                icon={<DeleteOutlined />}
                size="small"
                loading={loading}
              />
            </Popconfirm>
          </Tooltip>
        </Space>
      ),
    },
  ];

  const rowSelection = {
    selectedRowKeys,
    onChange: (newSelectedRowKeys) => setSelectedRowKeys(newSelectedRowKeys),
  };

  return (
    <div className="movie-list">
      {/* Header: Search & Filter */}
      <Row gutter={[16, 16]} className="movie-list-header">
        <Col xs={24} sm={12} md={8}>
          <Input
            placeholder="Tìm kiếm theo tên phim..."
            prefix={<SearchOutlined />}
            value={filters.searchText}
            onChange={(e) => onSearch(e.target.value)}
            allowClear
          />
        </Col>

        <Col xs={24} sm={12} md={6}>
          <Select
            placeholder="Sắp xếp theo..."
            value={filters.sortBy}
            onChange={onSort}
            style={{ width: '100%' }}
            options={[
              { label: 'Ngày Khởi Chiếu', value: 'releaseDate' },
              { label: 'Tên Phim', value: 'title' },
              { label: 'Đánh Giá', value: 'rating' },
            ]}
          />
        </Col>

        <Col xs={24} sm={12} md={10} style={{ textAlign: 'right' }}>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={onAdd}
            size="large"
          >
            Thêm Phim Mới
          </Button>
        </Col>
      </Row>

      {/* Movies Table */}
      <Table
        columns={columns}
        dataSource={movies}
        loading={loading}
        rowKey="id"
        rowSelection={rowSelection}
        pagination={{
          current: pagination.pageNum,
          pageSize: pagination.pageSize,
          total: pagination.total,
          onChange: (pageNum, pageSize) => {
            pagination.setPageNum(pageNum);
            pagination.setPageSize(pageSize);
          },
          showSizeChanger: true,
          showTotal: (total) => `Tổng cộng ${total} phim`,
        }}
        scroll={{ x: 'max-content' }}
      />
    </div>
  );
}
