import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Card, Button, Spin, Result, Typography, Tag, Empty, Tabs } from 'antd';
import { Ticket, Calendar, Clock, Armchair, QrCode } from 'lucide-react';
import { useMyHistory, useAllMyBookings } from '@/services/bookingPaymentApi';

const { Title, Text } = Typography;

function formatCurrency(amount) {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    maximumFractionDigits: 0,
  }).format(amount);
}

function formatDate(dateStr) {
  if (!dateStr) return '';
  const [day, month, year] = dateStr.split('/');
  const date = new Date(`${year}-${month}-${day}`);
  return date.toLocaleDateString('vi-VN', {
    weekday: 'long',
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });
}

const STATUS_CONFIG = {
  Success: { color: 'green', label: 'Thành công', bg: 'bg-green-50', border: 'border-green-200', text: 'text-green-700' },
  Cancelled: { color: 'red', label: 'Đã hủy', bg: 'bg-red-50', border: 'border-red-200', text: 'text-red-700' },
  Expired: { color: 'orange', label: 'Hết hạn', bg: 'bg-orange-50', border: 'border-orange-200', text: 'text-orange-700' },
  Pending: { color: 'blue', label: 'Chờ thanh toán', bg: 'bg-blue-50', border: 'border-blue-200', text: 'text-blue-700' },
  AwaitingConfirmation: { color: 'cyan', label: 'Chờ xác nhận', bg: 'bg-cyan-50', border: 'border-cyan-200', text: 'text-cyan-700' },
};

export default function MyTickets() {
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('completed');

  // Two queries: one for completed bookings, one for all bookings
  const completedQuery = useMyHistory();
  const allQuery = useAllMyBookings();

  // Use active query based on selected tab
  const currentQuery = activeTab === 'completed' ? completedQuery : allQuery;
  const { data, isLoading, isError, error } = currentQuery;

  // DEBUG: trace the raw API response
  console.log(`[MyTickets] Tab: ${activeTab} | Raw response data:`, data);
  console.log('[MyTickets] Query status - isLoading:', isLoading, '| isError:', isError, '| error:', error);

  if (isLoading) {
    return (
      <div className="min-h-[60vh] flex items-center justify-center">
        <Spin size="large" tip="Đang tải dữ liệu..." />
      </div>
    );
  }

  if (isError) {
    console.log('[MyTickets] Error details:', error);
    return (
      <div className="min-h-[60vh] flex items-center justify-center px-4">
        <Result
          status="error"
          title="Không thể tải lịch sử đặt vé"
          subTitle={error?.message || 'Vui lòng đăng nhập để xem lịch sử.'}
          extra={
            <Button type="primary" onClick={() => navigate('/login')}>
              Đăng Nhập
            </Button>
          }
        />
      </div>
    );
  }

  // ✅ Parse items từ response wrapper (axiosClient trả về response.data)
  // API returns: { success, data: { items, totalCount }, message, traceId }
  const parsedData = data?.data || data || {};
  const rawItems = Array.isArray(parsedData.items) ? parsedData.items : [];
  const totalCount = parsedData.totalCount ?? rawItems.length ?? 0;

  console.log(
    '[MyTickets] Parsed result - rawItems count:',
    rawItems.length,
    '| totalCount:',
    totalCount,
    '| items:',
    rawItems
  );

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      {/* Page Header */}
      <div className="flex items-center gap-3 mb-8">
        <div className="w-12 h-12 bg-blue-600 rounded-xl flex items-center justify-center">
          <Ticket size={24} className="text-white" />
        </div>
        <div>
          <Title level={2} style={{ margin: 0 }}>
            Vé của tôi
          </Title>
          <Text type="secondary">
            {totalCount > 0
              ? `${totalCount} đơn đặt vé`
              : 'Chưa có đơn đặt vé nào'}
          </Text>
        </div>
      </div>

      {/* Tab Selector */}
      <Tabs
        activeKey={activeTab}
        onChange={setActiveTab}
        items={[
          {
            key: 'completed',
            label: '✅ Vé Hoàn Tất',
            children: null, // Content rendered below
          },
          {
            key: 'all',
            label: '📋 Tất Cả Đơn Hàng',
            children: null,
          },
        ]}
        style={{ marginBottom: 24 }}
      />

      {/* Empty State */}
      {rawItems.length === 0 && (
        <Card style={{ borderRadius: 16, textAlign: 'center', padding: 48 }}>
          <Empty
            image={
              <div style={{ fontSize: 64, opacity: 0.3 }}>
                🎬
              </div>
            }
            description={
              <div>
                <Text strong style={{ fontSize: 16 }}>
                  {activeTab === 'completed'
                    ? 'Chưa có vé nào hoàn tất'
                    : 'Chưa có đơn đặt vé nào'}
                </Text>
                <br />
                <Text type="secondary">
                  {activeTab === 'completed'
                    ? 'Các vé đã thanh toán thành công sẽ hiển thị ở đây.'
                    : 'Hãy đặt vé xem phim yêu thích của bạn ngay!'}
                </Text>
              </div>
            }
          >
            <Button type="primary" onClick={() => navigate('/')}>
              Khám phá phim
            </Button>
          </Empty>
        </Card>
      )}

      {/* Booking Cards */}
      <div className="flex flex-col gap-4">
        {rawItems.map((booking) => {
          // Case-insensitive status matching (backend might return lowercase)
          const statusKey = Object.keys(STATUS_CONFIG).find(
            (k) => k.toLowerCase() === (booking.status || '').toLowerCase()
          );
          const cfg = statusKey ? STATUS_CONFIG[statusKey] : { color: 'default', label: booking.status || 'Unknown', bg: 'bg-gray-50', border: 'border-gray-200', text: 'text-gray-700' };
          const isSuccess = statusKey === 'Success';

          return (
            <Card
              key={booking.bookingId}
              style={{
                borderRadius: 16,
                boxShadow: '0 2px 12px rgba(0,0,0,0.08)',
                border: '1px solid #f0f0f0',
              }}
              styles={{ body: { padding: 0 } }}
            >
              <div className="flex flex-col sm:flex-row">
                {/* Poster */}
                <div
                  className="sm:w-32 w-full"
                  style={{
                    minHeight: 160,
                    background: '#1e293b',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    flexShrink: 0,
                    borderRadius: '16px 0 0 16px',
                  }}
                >
                  {booking.posterUrl ? (
                    <img
                      src={booking.posterUrl}
                      alt={booking.movieTitle}
                      className="w-full h-full object-cover"
                      style={{ borderRadius: '16px 0 0 16px', display: 'block' }}
                      onError={(e) => {
                        e.target.style.display = 'none';
                      }}
                    />
                  ) : (
                    <span style={{ fontSize: 48, opacity: 0.4 }}>🎬</span>
                  )}
                </div>

                {/* Info */}
                <div className="flex-1 p-4 sm:p-5">
                  {/* Top row: Movie title + Status */}
                  <div className="flex items-start justify-between gap-3 mb-3">
                    <Title
                      level={4}
                      className="m-0"
                      ellipsis={{ rows: 1 }}
                      style={{ flex: 1 }}
                    >
                      {booking.movieTitle}
                    </Title>
                    <Tag color={cfg.color} className="shrink-0">
                      {cfg.label}
                    </Tag>
                  </div>

                  {/* Booking code */}
                  <Text
                    type="secondary"
                    style={{ fontSize: 12, fontFamily: 'monospace', letterSpacing: 1 }}
                  >
                    MÃ: {booking.bookingCode}
                  </Text>

                  {/* Detail rows */}
                  <div className="flex flex-wrap gap-x-6 gap-y-1 mt-2">
                    <div className="flex items-center gap-1.5">
                      <Calendar size={14} className="text-gray-400" />
                      <Text style={{ fontSize: 13, color: '#64748b' }}>
                        {formatDate(booking.showDate)}
                      </Text>
                    </div>
                    <div className="flex items-center gap-1.5">
                      <Clock size={14} className="text-gray-400" />
                      <Text style={{ fontSize: 13, color: '#64748b' }}>
                        {booking.startTime}
                      </Text>
                    </div>
                    <div className="flex items-center gap-1.5">
                      <Armchair size={14} className="text-gray-400" />
                      <Text style={{ fontSize: 13, color: '#f97316', fontWeight: 600 }}>
                        {booking.seatNames}
                      </Text>
                    </div>
                  </div>

                  {/* Footer: Price + Action */}
                  <div className="flex items-center justify-between mt-4 pt-3" style={{ borderTop: '1px solid #f0f0f0' }}>
                    <div>
                      <Text type="secondary" style={{ fontSize: 12 }}>
                        {booking.totalTickets} vé
                      </Text>
                      <Text
                        strong
                        style={{
                          fontSize: 18,
                          color: '#f97316',
                          display: 'block',
                        }}
                      >
                        {formatCurrency(booking.totalAmount)}
                      </Text>
                    </div>

                    {isSuccess && (
                      <Button
                        type="primary"
                        icon={<QrCode size={16} />}
                        onClick={() => navigate(`/booking/${booking.bookingId}/ticket`)}
                        style={{
                          borderRadius: 8,
                          fontWeight: 600,
                          backgroundColor: '#334155',
                          borderColor: '#334155',
                        }}
                      >
                        Xem Vé
                      </Button>
                    )}
                  </div>
                </div>
              </div>
            </Card>
          );
        })}
      </div>
    </div>
  );
}
