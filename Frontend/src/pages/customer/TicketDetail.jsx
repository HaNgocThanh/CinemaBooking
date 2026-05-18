import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Card, Button, Result, Spin, Typography, Divider, Row, Col } from 'antd';
import { CheckCircleFilled, PrinterOutlined, HomeOutlined, WarningFilled } from '@ant-design/icons';
import { useETicket } from '@/services/bookingPaymentApi';

const { Text, Title } = Typography;

/**
 * Formats a VND amount to currency string.
 */
function formatCurrency(amount) {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    maximumFractionDigits: 0,
  }).format(amount);
}

/**
 * TicketDetail - E-Ticket display page for customers.
 *
 * Layout: A cinema ticket card with:
 * - Top section: Movie poster, title, cinema info, showtime, seats
 * - Bottom section: QR code for ticket verification
 * - Actions: Print ticket, go home
 */
export default function TicketDetail() {
  const { bookingId } = useParams();
  const navigate = useNavigate();
  const id = parseInt(bookingId || '0', 10);

  const { data, isLoading, isError, error } = useETicket(id);

  const ticket = data?.data;

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 flex items-center justify-center">
        <Spin size="large" tip="Đang tải thông tin vé..." />
      </div>
    );
  }

  if (isError || !ticket) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 flex items-center justify-center px-4">
        <Result
          status="error"
          icon={<WarningFilled style={{ color: '#ff4d4f' }} />}
          title="Không thể tải thông tin vé"
          subTitle={
            error?.message ||
            'Vé không tồn tại hoặc chưa được thanh toán. Vui lòng kiểm tra lại.'
          }
          extra={
            <Button type="primary" onClick={() => navigate('/')}>
              Về Trang Chủ
            </Button>
          }
        />
      </div>
    );
  }

  const qrCodeUrl = `https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${encodeURIComponent(ticket.qrCodeData)}`;

  const handlePrint = () => window.print();
  const handleHome = () => navigate('/', { replace: true });

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 py-8 px-4">
      {/* Success banner */}
      <div className="max-w-2xl mx-auto mb-6 flex items-center gap-3 justify-center">
        <CheckCircleFilled style={{ fontSize: 28, color: '#52c41a' }} />
        <Title level={3} style={{ color: '#52c41a', margin: 0 }}>
          Thanh toán thành công
        </Title>
      </div>

      {/* Ticket Card */}
      <Card
        className="max-w-2xl mx-auto overflow-hidden"
        style={{
          borderRadius: 16,
          boxShadow: '0 16px 48px rgba(0,0,0,0.4)',
          background: 'linear-gradient(135deg, #1e293b 0%, #0f172a 100%)',
        }}
        styles={{ body: { padding: 0 } }}
      >
        {/* ===== TOP SECTION: Movie & Show Info ===== */}
        <div>
          {/* Header with poster and movie info */}
          <Row gutter={[0, 0]}>
            {/* Poster */}
            <Col
              xs={24}
              sm={8}
              style={{
                background: '#000',
                minHeight: 220,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                overflow: 'hidden',
              }}
            >
              {ticket.posterUrl ? (
                <img
                  src={ticket.posterUrl}
                  alt={ticket.movieTitle}
                  style={{
                    width: '100%',
                    height: 220,
                    objectFit: 'cover',
                    display: 'block',
                  }}
                  onError={(e) => {
                    e.target.style.display = 'none';
                  }}
                />
              ) : (
                <div
                  style={{
                    width: '100%',
                    height: 220,
                    background: 'linear-gradient(135deg, #334155, #1e293b)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    color: '#94a3b8',
                    fontSize: 48,
                  }}
                >
                  🎬
                </div>
              )}
            </Col>

            {/* Movie & Show Info */}
            <Col xs={24} sm={16} style={{ padding: '20px 24px' }}>
              {/* Movie Title */}
              <Title
                level={3}
                style={{
                  color: '#f8fafc',
                  marginBottom: 4,
                  lineHeight: 1.3,
                }}
                ellipsis={{ rows: 2 }}
              >
                {ticket.movieTitle}
              </Title>

              {/* Booking code */}
              <div
                style={{
                  display: 'inline-block',
                  backgroundColor: 'rgba(82, 196, 26, 0.15)',
                  border: '1px solid #52c41a',
                  borderRadius: 6,
                  padding: '2px 10px',
                  marginBottom: 16,
                }}
              >
                <Text style={{ color: '#52c41a', fontFamily: 'monospace', fontSize: 12, letterSpacing: 1 }}>
                  MÃ VÉ: {ticket.bookingCode}
                </Text>
              </div>

              {/* Info rows */}
              <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
                <InfoRow label="Rạp chiếu" value={ticket.cinemaName} />
                <InfoRow label="Phòng" value={ticket.roomName} />
                <InfoRow label="Suất chiếu" value={`${ticket.showDate}`} />
                <InfoRow label="Giờ chiếu" value={`${ticket.startTime} — ${ticket.endTime}`} />
                <InfoRow label="Số ghế" value={ticket.seatNames} highlight />
              </div>
            </Col>
          </Row>

          {/* Divider with "notch" style */}
          <div
            style={{
              position: 'relative',
              borderTop: '2px dashed #334155',
              margin: '0 0',
            }}
          >
            {/* Left notch */}
            <div
              style={{
                position: 'absolute',
                left: -12,
                top: -12,
                width: 24,
                height: 24,
                borderRadius: '50%',
                background: 'linear-gradient(135deg, #0f172a, #1e293b)',
                border: '2px dashed #334155',
              }}
            />
            {/* Right notch */}
            <div
              style={{
                position: 'absolute',
                right: -12,
                top: -12,
                width: 24,
                height: 24,
                borderRadius: '50%',
                background: 'linear-gradient(135deg, #0f172a, #1e293b)',
                border: '2px dashed #334155',
              }}
            />
          </div>

          {/* ===== BOTTOM SECTION: QR Code & Actions ===== */}
          <div style={{ padding: '24px', textAlign: 'center' }}>
            <Text
              style={{
                color: '#94a3b8',
                fontSize: 12,
                display: 'block',
                marginBottom: 16,
                textTransform: 'uppercase',
                letterSpacing: 2,
              }}
            >
              Quét mã QR tại quầy để nhận vé
            </Text>

            {/* QR Code */}
            <div
              style={{
                display: 'inline-block',
                backgroundColor: '#fff',
                borderRadius: 12,
                padding: 12,
                marginBottom: 20,
                boxShadow: '0 4px 16px rgba(0,0,0,0.2)',
              }}
            >
              <img
                src={qrCodeUrl}
                alt="Mã QR xác nhận vé"
                width={180}
                height={180}
                style={{ display: 'block' }}
                onError={(e) => {
                  e.target.style.display = 'none';
                  e.target.parentNode.innerHTML = `
                    <div style="width:180px;height:180px;display:flex;align-items:center;
                      justify-content:center;background:#f6ffed;color:#52c41a;font-weight:bold;
                      font-size:11px;text-align:center;padding:12px;line-height:1.6;">
                      ${ticket.qrCodeData}
                    </div>
                  `;
                }}
              />
            </div>

            {/* Booking summary */}
            <div
              style={{
                backgroundColor: '#0f172a',
                borderRadius: 10,
                padding: '12px 20px',
                marginBottom: 20,
                border: '1px solid #334155',
              }}
            >
              <Row gutter={16} justify="center">
                <Col>
                  <Text style={{ color: '#94a3b8', fontSize: 12 }}>Số vé</Text>
                  <Text
                    style={{
                      color: '#f8fafc',
                      display: 'block',
                      fontWeight: 600,
                      fontSize: 16,
                    }}
                  >
                    {ticket.totalTickets}
                  </Text>
                </Col>
                <Col style={{ borderLeft: '1px solid #334155', borderRight: '1px solid #334155', padding: '0 16px' }}>
                  <Text style={{ color: '#94a3b8', fontSize: 12 }}>Tổng tiền</Text>
                  <Text
                    style={{
                      color: '#f97316',
                      display: 'block',
                      fontWeight: 700,
                      fontSize: 16,
                    }}
                  >
                    {formatCurrency(ticket.totalAmount)}
                  </Text>
                </Col>
                <Col>
                  <Text style={{ color: '#94a3b8', fontSize: 12 }}>Thanh toán</Text>
                  <Text
                    style={{
                      color: '#52c41a',
                      display: 'block',
                      fontWeight: 600,
                      fontSize: 16,
                    }}
                  >
                    {ticket.paymentMethod}
                  </Text>
                </Col>
              </Row>
            </div>

            {/* Action Buttons */}
            <Row gutter={12} justify="center">
              <Col>
                <Button
                  size="large"
                  icon={<PrinterOutlined />}
                  onClick={handlePrint}
                  style={{
                    height: 48,
                    paddingInline: 28,
                    borderRadius: 10,
                    fontWeight: 600,
                  }}
                >
                  In vé
                </Button>
              </Col>
              <Col>
                <Button
                  type="primary"
                  size="large"
                  icon={<HomeOutlined />}
                  onClick={handleHome}
                  style={{
                    height: 48,
                    paddingInline: 28,
                    borderRadius: 10,
                    fontWeight: 600,
                    backgroundColor: '#334155',
                    borderColor: '#334155',
                  }}
                >
                  Về Trang Chủ
                </Button>
              </Col>
            </Row>

            {/* Disclaimer */}
            <Text
              style={{
                color: '#64748b',
                fontSize: 11,
                display: 'block',
                marginTop: 16,
              }}
            >
              Vui lòng xuất trình mã QR này tại quầy vé trước khi vào phòng chiếu.
              <br />
              Vé đã thanh toán không được hoàn tiền.
            </Text>
          </div>
        </div>
      </Card>
    </div>
  );
}

/**
 * Helper component for info rows.
 */
function InfoRow({ label, value, highlight = false }) {
  return (
    <div style={{ display: 'flex', gap: 8 }}>
      <Text style={{ color: '#64748b', fontSize: 13, minWidth: 80 }}>{label}</Text>
      <Text
        style={{
          color: highlight ? '#f97316' : '#f8fafc',
          fontSize: 13,
          fontWeight: highlight ? 700 : 500,
        }}
      >
        {value}
      </Text>
    </div>
  );
}
