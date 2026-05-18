import { useEffect, useState, useRef } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Spin, Result, Button, Card, Alert } from 'antd';
import { LoadingOutlined } from '@ant-design/icons';
import axiosClient from '@/services/axiosClient';

/**
 * CheckoutPage - Intermediate page that creates the booking
 * and redirects to PaymentPage with bookingId and totalPrice.
 *
 * Flow: SeatSelection → CheckoutPage → PaymentPage
 *
 * Anti-double-submission guards:
 * - isSubmitting state blocks re-entry
 * - AbortController cancels in-flight requests on unmount
 * - useEffect cleanup prevents React StrictMode double-invoke
 */
export default function CheckoutPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Anti-double-submission guard
  const isSubmittingRef = useRef(false);

  useEffect(() => {
    const controller = new AbortController();

    const createBooking = async () => {
      // Block re-entry: prevent multiple simultaneous calls
      if (isSubmittingRef.current) return;
      isSubmittingRef.current = true;

      try {
        const showtimeId = searchParams.get('showtimeId');
        const seats = searchParams.get('seats');

        if (!showtimeId || !seats) {
          setError('Thông tin đặt vé không hợp lệ.');
          setLoading(false);
          return;
        }

        const seatIds = seats.split(',').map(Number).filter((n) => n > 0);
        if (seatIds.length === 0) {
          setError('Vui lòng chọn ít nhất 1 ghế.');
          setLoading(false);
          return;
        }

        const token = localStorage.getItem('JWT_TOKEN');
        if (!token) {
          navigate('/login', { replace: true });
          return;
        }

        // POST /api/bookings — chỉ gọi một lần, AbortController cancel nếu unmount
        const response = await axiosClient.post(
          '/bookings',
          {
            showtimeId: parseInt(showtimeId, 10),
            seatIds: seatIds,
          },
          { signal: controller.signal }
        );

        const bookingData = response?.data;
        if (!bookingData?.bookingId) {
          throw new Error('Không nhận được thông tin đơn đặt vé từ server.');
        }

        // Thành công → chuyển trang ngay, KHÔNG reset isSubmittingRef
        navigate(
          `/payment?bookingId=${bookingData.bookingId}&totalPrice=${bookingData.totalAmount}&bookingCode=${encodeURIComponent(
            bookingData.bookingCode
          )}`
        );
      } catch (err) {
        // Bỏ qua AbortError (bị cancel do unmount)
        if (err.name === 'CanceledError' || err.code === 'ERR_CANCELED') {
          return;
        }
        console.error('Create booking error:', err);
        const message =
          err.response?.data?.errorMessage ||
          err.response?.data?.message ||
          err.message ||
          'Không thể tạo đơn đặt vé. Vui lòng thử lại.';
        setError(message);
        setLoading(false);
        isSubmittingRef.current = false;
      }
    };

    createBooking();

    // Cleanup: abort request khi component unmount (VD: user back/refresh)
    // Đồng thời reset ref để không block retry
    return () => {
      controller.abort();
      isSubmittingRef.current = false;
    };
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Card style={{ textAlign: 'center', padding: 40 }}>
          <Spin
            indicator={<LoadingOutlined style={{ fontSize: 48, color: '#f97316' }} spin />}
            size="large"
          />
          <div style={{ marginTop: 24, fontSize: 18, color: '#64748b' }}>
            Đang xử lý đơn đặt vé...
          </div>
        </Card>
      </div>
    );
  }

  return (
    <div className="flex items-center justify-center min-h-[60vh]">
      <Result
        status="error"
        title="Không thể tạo đơn đặt vé"
        subTitle={error}
        extra={[
          <Button type="primary" key="back" onClick={() => navigate('/')}>
            Về Trang Chủ
          </Button>,
          <Button key="retry" onClick={() => navigate(-1)}>
            Quay Lại
          </Button>,
        ]}
      />
    </div>
  );
}
