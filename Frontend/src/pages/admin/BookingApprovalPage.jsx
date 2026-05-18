import { useState } from 'react';
import {
  Table,
  Tag,
  Button,
  Popconfirm,
  Card,
  Typography,
  Space,
  Input,
  Select,
  message,
  Tooltip,
  Badge,
  Statistic,
  Row,
  Col,
} from 'antd';
import {
  CheckCircleFilled,
  CloseCircleFilled,
  ReloadOutlined,
  SearchOutlined,
  ClockCircleFilled,
  ExclamationCircleFilled,
  CheckCircleOutlined,
} from '@ant-design/icons';
import {
  useAllBookings,
  useApproveBooking,
  useRejectBooking,
} from '@/services/bookingPaymentApi';

const { Title, Text } = Typography;
const { Search } = Input;

/**
 * BookingApprovalPage - Admin page to manage booking approvals.
 *
 * Shows all bookings in a table with status badges.
 * AwaitingConfirmation bookings are highlighted with action buttons:
 * - Approve (green): confirms payment received
 * - Reject (red with Popconfirm): cancels order, releases seats
 */
export default function BookingApprovalPage() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [statusFilter, setStatusFilter] = useState(null);
  const [searchText, setSearchText] = useState('');

  const approveMutation = useApproveBooking();
  const rejectMutation = useRejectBooking();

  // Fetch all bookings with filters
  const { data, isLoading, isFetching, refetch } = useAllBookings({
    status: statusFilter,
    page,
    pageSize,
  });

  const bookings = data?.data?.items || [];
  const totalCount = data?.data?.totalCount || 0;
  const totalPages = data?.data?.totalPages || 0;

  // Count by status for statistics
  const stats = {
    total: totalCount,
    pending: 0,
    awaiting: 0,
    success: 0,
    cancelled: 0,
  };

  // Status badge rendering
  const getStatusBadge = (status, statusValue) => {
    const config = {
      Pending: { color: 'gold', text: 'Chờ thanh toán', icon: <ClockCircleFilled /> },
      AwaitingConfirmation: {
        color: 'blue',
        text: 'Chờ duyệt',
        icon: <ExclamationCircleFilled />,
      },
      Success: { color: 'green', text: 'Thành công', icon: <CheckCircleFilled /> },
      Cancelled: { color: 'red', text: 'Đã hủy', icon: <CloseCircleFilled /> },
      Expired: { color: 'default', text: 'Hết hạn', icon: <ClockCircleFilled /> },
      Refunded: { color: 'purple', text: 'Hoàn tiền', icon: <ReloadOutlined /> },
    };

    const cfg = config[status] || { color: 'default', text: status, icon: null };

    return (
      <Tag
        color={cfg.color}
        icon={cfg.icon}
        style={{ borderRadius: 12, fontWeight: 600, padding: '2px 10px' }}
      >
        {cfg.text}
      </Tag>
    );
  };

  // Format currency
  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
      maximumFractionDigits: 0,
    }).format(amount);
  };

  // Format date/time
  const formatDateTime = (dateStr) => {
    if (!dateStr) return '-';
    const d = new Date(dateStr);
    return d.toLocaleString('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  // Handle approve
  const handleApprove = async (bookingId) => {
    try {
      await approveMutation.mutateAsync({ bookingId });
      message.success('Đã xác nhận thanh toán thành công!');
    } catch (err) {
      message.error(err.message || 'Không thể duyệt đơn. Vui lòng thử lại.');
    }
  };

  // Handle reject
  const handleReject = async (bookingId) => {
    try {
      await rejectMutation.mutateAsync({ bookingId });
      message.success('Đã từ chối đơn đặt vé. Các ghế đã được giải phóng.');
    } catch (err) {
      message.error(err.message || 'Không thể từ chối đơn. Vui lòng thử lại.');
    }
  };

  // Table columns
  const columns = [
    {
      title: 'Mã đặt vé',
      dataIndex: 'bookingCode',
      key: 'bookingCode',
      width: 180,
      render: (code) => (
        <Text strong style={{ fontFamily: 'monospace', color: '#f97316', letterSpacing: 0.5 }}>
          {code}
        </Text>
      ),
    },
    {
      title: 'Phim',
      dataIndex: 'movieTitle',
      key: 'movieTitle',
      width: 160,
      ellipsis: true,
      render: (title) => <Text>{title}</Text>,
    },
    {
      title: 'Phòng',
      dataIndex: 'roomName',
      key: 'roomName',
      width: 100,
    },
    {
      title: 'Suất chiếu',
      dataIndex: 'showtimeStartTime',
      key: 'showtimeStartTime',
      width: 140,
      render: (time) => formatDateTime(time),
    },
    {
      title: 'Ghế',
      dataIndex: 'seats',
      key: 'seats',
      width: 140,
      ellipsis: true,
      render: (seats) => (
        <Tooltip title={seats}>
          <Text style={{ fontFamily: 'monospace', fontSize: 12 }}>{seats}</Text>
        </Tooltip>
      ),
    },
    {
      title: 'Khách hàng',
      key: 'customer',
      width: 160,
      render: (_, record) => (
        <div>
          <Text>{record.customerName || 'Khách lẻ'}</Text>
          {record.customerEmail && (
            <div>
              <Text type="secondary" style={{ fontSize: 12 }}>
                {record.customerEmail}
              </Text>
            </div>
          )}
        </div>
      ),
    },
    {
      title: 'Tổng tiền',
      dataIndex: 'totalAmount',
      key: 'totalAmount',
      width: 130,
      align: 'right',
      render: (amount) => (
        <Text strong style={{ color: '#f97316' }}>
          {formatCurrency(amount)}
        </Text>
      ),
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      width: 150,
      render: (status, record) => getStatusBadge(status, record.statusValue),
    },
    {
      title: 'Thời gian',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 150,
      render: (time) => (
        <Text type="secondary" style={{ fontSize: 12 }}>
          {formatDateTime(time)}
        </Text>
      ),
    },
    {
      title: 'Hành động',
      key: 'actions',
      width: 200,
      fixed: 'right',
      render: (_, record) => {
        const isAwaiting = record.statusValue === 1; // AwaitingConfirmation
        const isPending = record.statusValue === 0; // Pending

        if (!isAwaiting && !isPending) {
          return (
            <Text type="secondary" style={{ fontSize: 12 }}>
              -
            </Text>
          );
        }

        return (
          <Space size={4}>
            {isAwaiting && (
              <>
                <Button
                  type="primary"
                  size="small"
                  icon={<CheckCircleOutlined />}
                  onClick={() => handleApprove(record.id)}
                  loading={approveMutation.isPending}
                  style={{
                    backgroundColor: '#52c41a',
                    borderColor: '#52c41a',
                    fontWeight: 600,
                  }}
                >
                  Duyệt
                </Button>
                <Popconfirm
                  title="Từ chối đơn hàng?"
                  description="Các ghế sẽ được giải phóng và trả lại cho rạp."
                  onConfirm={() => handleReject(record.id)}
                  okText="Từ chối"
                  cancelText="Hủy"
                  okButtonProps={{
                    danger: true,
                    loading: rejectMutation.isPending,
                  }}
                >
                  <Button
                    danger
                    size="small"
                    icon={<CloseCircleFilled />}
                    loading={rejectMutation.isPending}
                    style={{ fontWeight: 600 }}
                  >
                    Từ chối
                  </Button>
                </Popconfirm>
              </>
            )}
            {isPending && (
              <Tag color="gold" style={{ fontSize: 11 }}>
                Đang chờ thanh toán
              </Tag>
            )}
          </Space>
        );
      },
    },
  ];

  // Row highlight for AwaitingConfirmation
  const rowClassName = (record) => {
    if (record.statusValue === 1) return 'booking-awaiting-row';
    return '';
  };

  return (
    <div className="p-6">
      {/* Page header */}
      <div style={{ marginBottom: 24 }}>
        <Title level={3} style={{ marginBottom: 4 }}>
          Quản Lý Đơn Đặt Vé
        </Title>
        <Text type="secondary">
          Xem danh sách tất cả đơn đặt vé. Những đơn "Chờ duyệt" cần được xác nhận hoặc từ chối.
        </Text>
      </div>

      {/* Statistics */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={6}>
          <Card size="small">
            <Statistic
              title="Tổng đơn"
              value={totalCount}
              valueStyle={{ color: '#3f8600' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small">
            <Statistic
              title="Chờ duyệt"
              value={bookings.filter((b) => b.statusValue === 1).length}
              valueStyle={{ color: '#1890ff' }}
              prefix={<ExclamationCircleFilled />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small">
            <Statistic
              title="Thành công"
              value={bookings.filter((b) => b.statusValue === 2).length}
              valueStyle={{ color: '#52c41a' }}
              prefix={<CheckCircleFilled />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card size="small">
            <Statistic
              title="Đã hủy"
              value={bookings.filter((b) => b.statusValue === 3).length}
              valueStyle={{ color: '#cf1322' }}
              prefix={<CloseCircleFilled />}
            />
          </Card>
        </Col>
      </Row>

      {/* Filters */}
      <Card
        size="small"
        style={{ marginBottom: 16 }}
        styles={{ body: { padding: '12px 16px' } }}
      >
        <Space wrap>
          <Text strong style={{ fontSize: 13 }}>
            Lọc:
          </Text>
          <Select
            placeholder="Trạng thái"
            allowClear
            style={{ width: 160 }}
            onChange={(val) => {
              setStatusFilter(val);
              setPage(1);
            }}
            value={statusFilter}
            options={[
              { value: 0, label: 'Chờ thanh toán' },
              { value: 1, label: 'Chờ duyệt' },
              { value: 2, label: 'Thành công' },
              { value: 3, label: 'Đã hủy' },
              { value: 4, label: 'Hết hạn' },
              { value: 5, label: 'Hoàn tiền' },
            ]}
          />
          <Button
            icon={<ReloadOutlined />}
            onClick={() => refetch()}
            loading={isFetching}
          >
            Làm mới
          </Button>
        </Space>
      </Card>

      {/* Table */}
      <Card styles={{ body: { padding: 0 } }}>
        <Table
          columns={columns}
          dataSource={bookings}
          rowKey="id"
          loading={isLoading}
          rowClassName={rowClassName}
          scroll={{ x: 1200 }}
          pagination={{
            current: page,
            pageSize: pageSize,
            total: totalCount,
            showSizeChanger: true,
            showTotal: (total, range) =>
              `${range[0]}-${range[1]} trong ${total} đơn đặt vé`,
            onChange: (p, ps) => {
              setPage(p);
              setPageSize(ps);
            },
            pageSizeOptions: ['10', '20', '50'],
          }}
          style={{ borderRadius: 0 }}
        />
      </Card>

      {/* Highlight style for AwaitingConfirmation rows */}
      <style>{`
        .booking-awaiting-row {
          background-color: #e6f7ff !important;
        }
        .booking-awaiting-row td {
          background-color: transparent !important;
        }
      `}</style>
    </div>
  );
}
