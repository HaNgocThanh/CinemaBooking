import { useState, useEffect, useCallback, useRef } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Card, Button, Result, Spin, Progress, Modal, Typography } from 'antd';
import {
  CheckCircleFilled,
  ExclamationCircleFilled,
  ClockCircleFilled,
  LoadingOutlined,
} from '@ant-design/icons';
import { useSubmitPayment, useBookingStatus } from '@/services/bookingPaymentApi';

const { Text, Title } = Typography;

const TIMEOUT_SECONDS = 300;

function formatCurrency(amount) {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    maximumFractionDigits: 0,
  }).format(amount);
}

function formatTime(seconds) {
  const m = Math.floor(Math.max(0, seconds) / 60);
  const s = Math.max(0, seconds) % 60;
  return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
}

/**
 * PaymentPage - QR payment flow with countdown timer and status polling.
 *
 * Anti-timezone architecture:
 * - Backend calculates RemainingSeconds at the moment of API call
 * - Frontend uses remainingSeconds as source of truth, no Date math
 * - Countdown decrements via setInterval every 1s
 *
 * Flow:
 * 1. Show QR code with countdown (Pending) → Customer scans & transfers
 * 2. Customer clicks "Tôi đã chuyển khoản" → submitPayment API → AwaitingConfirmation
 * 3. Timer HIDDEN, polling every 4s for status
 * 4. If Success → Show success ticket page
 * 5. If Cancelled (admin rejected) → Show error, release seats
 */
export default function PaymentPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const bookingId = parseInt(searchParams.get('bookingId') || '0', 10);
  const totalPrice = parseInt(searchParams.get('totalPrice') || '0', 10);
  const bookingCode = searchParams.get('bookingCode') || '';

  const [timeLeft, setTimeLeft] = useState(TIMEOUT_SECONDS);
  const [hasSubmitted, setHasSubmitted] = useState(false);
  const [expired, setExpired] = useState(false);
  const [confirmed, setConfirmed] = useState(false);
  const [rejected, setRejected] = useState(false);

  const submitPayment = useSubmitPayment();

  // QR code URL using VietQR (simulated payment)
  const qrUrl = `https://img.vietqr.io/image/vcb-123456789-compact2.jpg?amount=${totalPrice}&addInfo=DATVE_${bookingId}&theme=light`;

  // ================================================================
  // PHASE 1: Lấy RemainingSeconds từ server khi trang load
  // Luôn dùng giá trị server trả về để đảm bảo sync
  // ================================================================
  const { data: initialStatus } = useBookingStatus(bookingId, {
    enabled: !!bookingId && !hasSubmitted,
    interval: false,
  });

  // Sync timeLeft từ server — chỉ chạy 1 lần khi có response
  useEffect(() => {
    if (initialStatus?.data?.remainingSeconds != null) {
      const serverSeconds = initialStatus.data.remainingSeconds;
      setTimeLeft(serverSeconds);
      if (serverSeconds <= 0) {
        setExpired(true);
      }
    }
  }, [initialStatus]);

  // ================================================================
  // PHASE 2: Countdown timer — decrement từ giá trị server
  // KHÔNG tính toán Date nào ở client
  // ================================================================
  useEffect(() => {
    if (hasSubmitted || expired || confirmed || rejected) return;
    if (timeLeft <= 0) return;

    const timer = setInterval(() => {
      setTimeLeft((prev) => {
        if (prev <= 1) {
          setExpired(true);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(timer);
  }, [hasSubmitted, expired, confirmed, rejected, timeLeft]);

  // ================================================================
  // PHASE 3: Poll trạng thái khi đã submit (chờ admin duyệt)
  // ================================================================
  const { data: statusData } = useBookingStatus(bookingId, {
    enabled: hasSubmitted && !confirmed && !rejected && !expired,
    interval: 4000,
    refetchIntervalInBackground: false,
  });

  useEffect(() => {
    if (!statusData?.data) return;

    const status = statusData.data.status;

    if (status === 'Success') {
      setConfirmed(true);
    } else if (status === 'Cancelled' || status === 'Expired') {
      setRejected(true);
    }
  }, [statusData]);

  // ================================================================
  // PHASE 4: Xử lý nút "Tôi đã chuyển khoản"
  // ================================================================
  const handleSubmitPayment = useCallback(async () => {
    if (!bookingId) return;

    try {
      await submitPayment.mutateAsync({ bookingId });
      setHasSubmitted(true);
    } catch (err) {
      const message =
        err.response?.data?.errorMessage ||
        err.response?.data?.message ||
        err.message ||
        'Không thể xác nhận thanh toán. Vui lòng thử lại.';
      Modal.error({
        title: 'Lỗi xác nhận',
        content: message,
      });
    }
  }, [bookingId, submitPayment]);

  const handleExpiredBack = () => navigate('/', { replace: true });
  const handleViewTicket = () => navigate(`/booking/${bookingId}/ticket`, { replace: true });
  const handleBackToHome = () => navigate('/', { replace: true });

  // ===== INVALID PARAMS =====
  if (!bookingId || !totalPrice) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Result
          status="error"
          title="Thông tin thanh toán không hợp lệ"
          subTitle="Vui lòng chọn ghế và thử lại."
          extra={
            <Button type="primary" onClick={() => navigate('/')}>
              Về Trang Chủ
            </Button>
          }
        />
      </div>
    );
  }

  // ===== EXPIRED STATE =====
  if (expired) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Result
          icon={<ExclamationCircleFilled style={{ color: '#ff4d4f' }} />}
          status="error"
          title="Đơn hàng đã hết hạn"
          subTitle="Thời gian thanh toán đã kết thúc. Vui lòng đặt vé lại."
          extra={
            <Button type="primary" onClick={handleExpiredBack}>
              Chọn Ghế Khác
            </Button>
          }
        />
      </div>
    );
  }

  // ===== CONFIRMED (ADMIN APPROVED) STATE =====
  if (confirmed) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Result
          icon={<CheckCircleFilled style={{ color: '#52c41a', fontSize: 72 }} />}
          status="success"
          title="Thanh toán thành công!"
          subTitle={`Mã đặt vé của bạn: ${bookingCode}`}
          extra={[
            <Button type="primary" key="ticket" onClick={handleViewTicket}>
              Xem Vé
            </Button>,
            <Button key="home" onClick={handleBackToHome}>
              Về Trang Chủ
            </Button>,
          ]}
        />
      </div>
    );
  }

  // ===== REJECTED (ADMIN DECLINED) STATE =====
  if (rejected) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Result
          icon={<ExclamationCircleFilled style={{ color: '#ff4d4f' }} />}
          status="error"
          title="Thanh toán bị từ chối"
          subTitle="Admin đã từ chối đơn hàng này. Vui lòng thử đặt vé lại."
          extra={[
            <Button type="primary" key="retry" onClick={() => navigate('/')}>
              Đặt Vé Khác
            </Button>,
            <Button key="home" onClick={handleBackToHome}>
              Về Trang Chủ
            </Button>,
          ]}
        />
      </div>
    );
  }

  // ===== AWAITING CONFIRMATION STATE =====
  if (hasSubmitted) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Card
          style={{
            maxWidth: 500,
            width: '100%',
            textAlign: 'center',
            borderRadius: 16,
            boxShadow: '0 8px 32px rgba(0,0,0,0.12)',
          }}
          styles={{ body: { padding: '40px 32px' } }}
        >
          <Spin
            indicator={<LoadingOutlined style={{ fontSize: 48, color: '#f97316' }} spin />}
            size="large"
          />
          <Title level={4} style={{ marginTop: 24, marginBottom: 8 }}>
            Đang chờ Admin duyệt...
          </Title>
          <Text type="secondary" style={{ fontSize: 15 }}>
            Đơn của bạn đang được xử lý. Vui lòng chờ trong giây lát.
          </Text>
          <div
            style={{
              marginTop: 24,
              padding: '16px 24px',
              backgroundColor: '#f6ffed',
              borderRadius: 8,
              border: '1px solid #b7eb8f',
            }}
          >
            <Text strong style={{ color: '#52c41a' }}>
              Mã đặt vé: {bookingCode}
            </Text>
          </div>
          <div style={{ marginTop: 16 }}>
            <Text type="secondary" style={{ fontSize: 13 }}>
              Trang này sẽ tự động cập nhật khi admin xác nhận thanh toán.
            </Text>
          </div>
        </Card>
      </div>
    );
  }

  // ===== PENDING PAYMENT STATE (QR + countdown) =====
  const progressPercent = Math.round((timeLeft / TIMEOUT_SECONDS) * 100);
  const isUrgent = timeLeft <= 60;

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center px-4 py-8">
      <Card
        style={{
          maxWidth: 560,
          width: '100%',
          borderRadius: 16,
          boxShadow: '0 8px 32px rgba(0,0,0,0.2)',
          backgroundColor: '#1e293b',
        }}
        styles={{ body: { padding: '32px' } }}
      >
        {/* Header */}
        <div style={{ textAlign: 'center', marginBottom: 24 }}>
          <Title level={4} style={{ color: '#f8fafc', marginBottom: 4 }}>
            Thanh Toán QR Code
          </Title>
          <Text style={{ color: '#94a3b8' }}>
            Quét mã QR bên dưới để thanh toán
          </Text>
        </div>

        {/* Booking info */}
        <div
          style={{
            backgroundColor: '#0f172a',
            borderRadius: 12,
            padding: '16px 20px',
            marginBottom: 20,
            border: '1px solid #334155',
          }}
        >
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}>
            <Text style={{ color: '#94a3b8' }}>Mã đặt vé:</Text>
            <Text strong style={{ color: '#f8fafc', fontFamily: 'monospace', letterSpacing: 1 }}>
              {bookingCode}
            </Text>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between' }}>
            <Text style={{ color: '#94a3b8' }}>Số tiền thanh toán:</Text>
            <Text strong style={{ color: '#f97316', fontSize: 20 }}>
              {formatCurrency(totalPrice)}
            </Text>
          </div>
          <div
            style={{
              marginTop: 12,
              paddingTop: 12,
              borderTop: '1px solid #334155',
              display: 'flex',
              alignItems: 'center',
              gap: 8,
            }}
          >
            <ClockCircleFilled style={{ color: isUrgent ? '#ff4d4f' : '#94a3b8' }} />
            <Text style={{ color: isUrgent ? '#ff4d4f' : '#94a3b8' }}>
              Thời gian còn lại:
            </Text>
            <Text
              strong
              style={{
                color: isUrgent ? '#ff4d4f' : '#f8fafc',
                fontSize: 18,
                fontFamily: 'monospace',
              }}
            >
              {formatTime(timeLeft)}
            </Text>
          </div>
          <Progress
            percent={progressPercent}
            showInfo={false}
            strokeColor={isUrgent ? '#ff4d4f' : '#f97316'}
            trailColor="#334155"
            size="small"
            style={{ marginTop: 8 }}
          />
        </div>

        {/* QR Code */}
        <div
          style={{
            display: 'flex',
            justifyContent: 'center',
            marginBottom: 20,
          }}
        >
          <Card
            style={{
              backgroundColor: '#fff',
              borderRadius: 12,
              border: 'none',
              boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
            }}
          >
            <img
              src={qrUrl}
              alt="QR Code thanh toán"
              width={200}
              height={200}
              style={{ display: 'block' }}
              onError={(e) => {
                e.target.style.display = 'none';
                e.target.parentNode.innerHTML =
                  '<div style="width:200px;height:200px;display:flex;align-items:center;justify-content:center;background:#f6ffed;color:#52c41a;font-weight:bold;font-size:13px;text-align:center;padding:20px;line-height:1.5;">Demo QR Code<br/>DATVE_' +
                  bookingId +
                  '<br/>' +
                  formatCurrency(totalPrice) +
                  '</div>';
              }}
            />
          </Card>
        </div>

        {/* Payment instructions */}
        <div
          style={{
            backgroundColor: '#0f172a',
            borderRadius: 12,
            padding: '12px 16px',
            marginBottom: 24,
            border: '1px solid #334155',
          }}
        >
          <Text style={{ color: '#94a3b8', fontSize: 13 }}>
            <strong style={{ color: '#f8fafc' }}>Hướng dẫn:</strong> Mở ứng dụng ngân hàng hoặc
            ví điện tử, quét mã QR và chuyển khoản đúng số tiền bên trên. Sau khi chuyển xong,
            bấm nút bên dưới.
          </Text>
        </div>

        {/* Action buttons */}
        <Button
          type="primary"
          size="large"
          block
          onClick={handleSubmitPayment}
          loading={submitPayment.isPending}
          disabled={submitPayment.isPending}
          style={{
            height: 52,
            fontSize: 16,
            fontWeight: 700,
            borderRadius: 10,
            backgroundColor: '#52c41a',
            borderColor: '#52c41a',
          }}
        >
          Tôi đã chuyển khoản
        </Button>

        <div style={{ textAlign: 'center', marginTop: 16 }}>
          <Button
            type="text"
            className="!text-slate-400 hover:!text-white"
            onClick={() => navigate(-1)}
            style={{ fontSize: 13 }}
          >
            Hủy đặt vé
          </Button>
        </div>
      </Card>
    </div>
  );
}
