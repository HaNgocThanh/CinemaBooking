import { useMemo, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Spin, Empty, Button, Alert } from 'antd';
import { LockOutlined } from '@ant-design/icons';
import axiosClient from '@/services/axiosClient';

/**
 * Validates Single Seat Gap Rule: VIP rows cannot have a single empty seat
 * flanked by TAKEN seats (Booked/Locked/Selected) on both sides.
 * Boundary seats (index 0 and last) are always exempt.
 *
 * @param {Array} allSeats - full seat list from API (backend state)
 * @param {Array} selectedSeats - seats the user has selected in this session
 * @returns {boolean} true = valid, false = violates rule
 */
function isVipSingleGapValid(allSeats, selectedSeats) {
  if (!allSeats || allSeats.length === 0) return true;

  const selectedIds = new Set(selectedSeats.map((s) => s.id));

  const byRow = {};
  for (const seat of allSeats) {
    if (!byRow[seat.rowLetter]) byRow[seat.rowLetter] = [];
    byRow[seat.rowLetter].push(seat);
  }

  for (const rowLetter in byRow) {
    const row = byRow[rowLetter].slice().sort((a, b) => a.gridColumn - b.gridColumn);

    // Skip index 0 (left boundary) and last index (right boundary)
    for (let i = 1; i < row.length - 1; i++) {
      const seat = row[i];

      // Only apply to VIP seats that are currently empty in backend
      if (seat.type !== 'VIP' || seat.status !== 'Available') continue;

      const prev = row[i - 1];
      const next = row[i + 1];

      const prevTaken = prev.status === 'Booked' || prev.status === 'Locked' || selectedIds.has(prev.id);
      const nextTaken = next.status === 'Booked' || next.status === 'Locked' || selectedIds.has(next.id);

      // Violation: flanked by TAKEN seats on both sides
      if (prevTaken && nextTaken) {
        return false;
      }
    }
  }

  return true;
}

const CELL_SIZE = 40;
const CELL_GAP = 4;
const MAX_SEATS = 8;

const SEAT_STYLES = {
  Regular: {
    available: {
      bg: '#d9d9d9',
      border: '#8c8c8c',
      color: '#595959',
    },
    selected: {
      bg: '#52c41a',
      border: '#389e0d',
      color: '#fff',
    },
  },
  VIP: {
    available: {
      bg: '#fff7e6',
      border: '#fa8c16',
      color: '#d46b08',
    },
    selected: {
      bg: '#52c41a',
      border: '#389e0d',
      color: '#fff',
    },
  },
};

const UNAVAILABLE_STYLE = {
  bg: '#f5f5f5',
  border: '#d9d9d9',
  color: '#bfbfbf',
};

function SeatCell({ seat, isSelected, onClick }) {
  const isUnavailable = seat.status !== 'Available';
  const typeStyle = SEAT_STYLES[seat.type] || SEAT_STYLES.Regular;
  const style = isSelected
    ? typeStyle.selected
    : isUnavailable
    ? UNAVAILABLE_STYLE
    : typeStyle.available;

  return (
    <div
      onClick={isUnavailable || isSelected ? undefined : onClick}
      style={{
        width: CELL_SIZE,
        height: CELL_SIZE,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: style.bg,
        border: `2px solid ${style.border}`,
        borderRadius: 6,
        fontSize: 11,
        fontWeight: 700,
        color: style.color,
        cursor: isUnavailable || isSelected ? 'default' : 'pointer',
        userSelect: 'none',
        boxShadow: isSelected ? `0 0 0 2px #52c41a88` : 'none',
        transition: 'all 0.15s ease',
        position: 'relative',
      }}
      title={
        isUnavailable
          ? `${seat.seatNumber} - ${seat.type === 'VIP' ? 'VIP' : 'Thường'} - ${seat.status === 'Locked' ? 'Đang giữ' : 'Đã bán'}`
          : `${seat.seatNumber} - ${seat.type === 'VIP' ? 'VIP' : 'Thường'} - ${isSelected ? 'Đã chọn' : 'Còn trống'}`
      }
    >
      {seat.seatNumber}
      {isUnavailable && (
        <LockOutlined
          style={{
            position: 'absolute',
            fontSize: 10,
            color: '#bfbfbf',
            bottom: 2,
            right: 3,
          }}
        />
      )}
    </div>
  );
}

function AisleCell() {
  return (
    <div
      style={{
        width: CELL_SIZE,
        height: CELL_SIZE,
      }}
    />
  );
}

function formatCurrency(amount) {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    maximumFractionDigits: 0,
  }).format(amount);
}

function formatTime(dateStr) {
  if (!dateStr) return '';
  const d = new Date(dateStr);
  return d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
}

export default function SeatSelectionPage() {
  const { showtimeId } = useParams();
  const navigate = useNavigate();
  const [selectedSeats, setSelectedSeats] = useState([]);
  const [maxReached, setMaxReached] = useState(false);
  const [gapViolation, setGapViolation] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const id = parseInt(showtimeId, 10);

  const { data, isLoading, isError, error } = useQuery({
    queryKey: ['showtimeSeats', id],
    queryFn: () => axiosClient.get(`/showtimes/${id}`),
    enabled: !!id,
    staleTime: 30 * 1000,
  });

  const { data: seatsData, isLoading: seatsLoading, isError: seatsError } = useQuery({
    queryKey: ['seats', id],
    queryFn: () => axiosClient.get(`/showtimes/${id}/seats`),
    enabled: !!id,
    staleTime: 15 * 1000,
  });

  const showtime = data?.data;
  const seats = seatsData?.data || [];

  const { matrix, maxCol } = useMemo(() => {
    if (!seats.length) return { matrix: [], maxCol: 0 };

    const maxRowVal = Math.max(...seats.map((s) => s.gridRow));
    const maxColVal = Math.max(...seats.map((s) => s.gridColumn));

    const m = Array.from({ length: maxRowVal + 1 }, () =>
      Array.from({ length: maxColVal + 1 }, () => null)
    );

    for (const seat of seats) {
      m[seat.gridRow][seat.gridColumn] = seat;
    }

    return { matrix: m, maxCol: maxColVal };
  }, [seats]);

  const handleSeatClick = (seat) => {
    if (seat.status !== 'Available') return;

    setMaxReached(false);
    setGapViolation(false);
    setSelectedSeats((prev) => {
      const already = prev.find((s) => s.id === seat.id);
      if (already) {
        return prev.filter((s) => s.id !== seat.id);
      }
      if (prev.length >= MAX_SEATS) {
        setMaxReached(true);
        return prev;
      }
      return [...prev, seat];
    });
  };

  const totalPrice = useMemo(() => {
    return selectedSeats.reduce((sum, seat) => sum + (seat.price || showtime?.basePrice || 0), 0);
  }, [selectedSeats, showtime]);

  const handlePayment = () => {
    if (isSubmitting) return;
    setGapViolation(false);

    if (!isVipSingleGapValid(seats, selectedSeats)) {
      setGapViolation(true);
      return;
    }

    setIsSubmitting(true);
    navigate(
      `/checkout?showtimeId=${id}&seats=${selectedSeats.map((s) => s.id).join(',')}`
    );
  };

  if (isLoading || seatsLoading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Spin size="large" />
      </div>
    );
  }

  if (isError || seatsError || !showtime) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Empty
          description={error?.message || 'Không thể tải thông tin suất chiếu'}
        />
      </div>
    );
  }

  if (!matrix.length || !seats.length) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Empty description="Không tìm thấy sơ đồ ghế cho suất chiếu này." />
      </div>
    );
  }

  const availableCount = seats.filter((s) => s.status === 'Available').length;
  const bookedCount = seats.filter((s) => s.status === 'Booked').length;
  const lockedCount = seats.filter((s) => s.status === 'Locked').length;
  const regularCount = seats.filter((s) => s.type === 'Regular' && s.status === 'Available').length;
  const vipCount = seats.filter((s) => s.type === 'VIP' && s.status === 'Available').length;

  return (
    <div className="min-h-screen bg-slate-950 text-white">
      {/* Header */}
      <div className="max-w-5xl mx-auto px-6 py-8">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-white mb-1">Chọn Ghế</h1>
          <p className="text-slate-400 text-sm">
            Phim: <span className="text-white font-medium">{showtime.movieTitle}</span>
            {' · '}
            Phòng: <span className="text-white font-medium">{showtime.roomName}</span>
            {' · '}
            {formatTime(showtime.startTime)} — {formatTime(showtime.endTime)}
          </p>
        </div>

        {maxReached && (
          <Alert
            type="warning"
            message={`Bạn chỉ có thể chọn tối đa ${MAX_SEATS} ghế mỗi lần đặt.`}
            showIcon
            closable
            className="mb-4"
          />
        )}

        {gapViolation && (
          <Alert
            type="error"
            message="Khu vực VIP không được để lại một ghế trống đơn lẻ ở giữa hai ghế đã chọn. Vui lòng chọn lại!"
            showIcon
            closable
            onClose={() => setGapViolation(false)}
            className="mb-4"
          />
        )}

        {/* Legend */}
        <div
          style={{
            display: 'flex',
            gap: 16,
            marginBottom: 24,
            flexWrap: 'wrap',
            fontSize: 13,
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
            <div
              style={{
                width: 18,
                height: 18,
                borderRadius: 4,
                background: SEAT_STYLES.Regular.available.bg,
                border: `2px solid ${SEAT_STYLES.Regular.available.border}`,
              }}
            />
            <span className="text-slate-300">Ghế thường ({regularCount})</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
            <div
              style={{
                width: 18,
                height: 18,
                borderRadius: 4,
                background: SEAT_STYLES.VIP.available.bg,
                border: `2px solid ${SEAT_STYLES.VIP.available.border}`,
              }}
            />
            <span className="text-slate-300">VIP ({vipCount})</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
            <div
              style={{
                width: 18,
                height: 18,
                borderRadius: 4,
                background: SEAT_STYLES.Regular.selected.bg,
                border: `2px solid ${SEAT_STYLES.Regular.selected.border}`,
              }}
            />
            <span className="text-slate-300">Đang chọn</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
            <div
              style={{
                width: 18,
                height: 18,
                borderRadius: 4,
                background: UNAVAILABLE_STYLE.bg,
                border: `2px solid ${UNAVAILABLE_STYLE.border}`,
              }}
            />
            <span className="text-slate-300">Đã bán / Đang giữ ({bookedCount + lockedCount})</span>
          </div>
        </div>

        {/* Screen */}
        <div
          style={{
            backgroundColor: '#1e293b',
            borderRadius: 12,
            padding: '16px 20px',
            display: 'inline-block',
            minWidth: 300,
          }}
        >
          <div
            style={{
              textAlign: 'center',
              color: '#64748b',
              fontSize: 11,
              fontWeight: 700,
              letterSpacing: 4,
              marginBottom: 20,
              padding: '6px 0',
              borderBottom: '2px solid #334155',
            }}
          >
            MÀN HÌNH
          </div>

          {/* Column numbers */}
          <div
            style={{
              display: 'flex',
              marginLeft: CELL_SIZE + CELL_GAP,
              marginBottom: 4,
            }}
          >
            {Array.from({ length: maxCol + 1 }, (_, c) => (
              <div
                key={c}
                style={{
                  width: CELL_SIZE,
                  marginRight: CELL_GAP,
                  textAlign: 'center',
                  fontSize: 10,
                  color: '#475569',
                  fontWeight: 600,
                }}
              >
                {c + 1}
              </div>
            ))}
          </div>

          {/* Seat grid */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: CELL_GAP }}>
            {matrix.map((rowData, rowIdx) => {
              const rowLetter = String.fromCharCode(65 + rowIdx);
              const hasSeats = rowData.some((cell) => cell !== null);

              return (
                <div
                  key={rowIdx}
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: CELL_GAP,
                  }}
                >
                  <div
                    style={{
                      width: CELL_SIZE,
                      textAlign: 'center',
                      fontSize: 12,
                      fontWeight: 700,
                      color: '#64748b',
                    }}
                  >
                    {hasSeats ? rowLetter : ''}
                  </div>

                  <div style={{ display: 'flex', gap: CELL_GAP }}>
                    {rowData.map((cell, colIdx) => {
                      if (cell === null) {
                        return <AisleCell key={colIdx} />;
                      }
                      const isSelected = selectedSeats.some((s) => s.id === cell.id);
                      return (
                        <SeatCell
                          key={colIdx}
                          seat={cell}
                          isSelected={isSelected}
                          onClick={() => handleSeatClick(cell)}
                        />
                      );
                    })}
                  </div>

                  <div
                    style={{
                      width: CELL_SIZE,
                      textAlign: 'center',
                      fontSize: 12,
                      fontWeight: 700,
                      color: '#64748b',
                    }}
                  >
                    {hasSeats ? rowLetter : ''}
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        {/* Summary panel */}
        {selectedSeats.length > 0 && (
          <div
            style={{
              marginTop: 24,
              padding: '20px 24px',
              backgroundColor: '#1e293b',
              borderRadius: 12,
              border: '1px solid #334155',
            }}
          >
            <div style={{ marginBottom: 16 }}>
              <div style={{ fontWeight: 700, fontSize: 16, marginBottom: 8 }}>
                Ghế bạn chọn:
              </div>
              <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                {selectedSeats
                  .sort((a, b) => a.seatNumber.localeCompare(b.seatNumber))
                  .map((seat) => (
                    <div
                      key={seat.id}
                      style={{
                        padding: '4px 12px',
                        borderRadius: 6,
                        background:
                          seat.type === 'VIP'
                            ? SEAT_STYLES.VIP.available.bg
                            : SEAT_STYLES.Regular.available.bg,
                        border: `1px solid ${
                          seat.type === 'VIP'
                            ? SEAT_STYLES.VIP.available.border
                            : SEAT_STYLES.Regular.available.border
                        }`,
                        color:
                          seat.type === 'VIP'
                            ? SEAT_STYLES.VIP.available.color
                            : SEAT_STYLES.Regular.available.color,
                        fontSize: 13,
                        fontWeight: 700,
                        cursor: 'pointer',
                      }}
                      onClick={() => handleSeatClick(seat)}
                      title="Bỏ chọn"
                    >
                      {seat.seatNumber}{' '}
                      <span style={{ fontWeight: 400, fontSize: 11 }}>
                        ({seat.type === 'VIP' ? 'VIP' : 'Thường'})
                      </span>
                    </div>
                  ))}
              </div>
            </div>

            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                paddingTop: 16,
                borderTop: '1px solid #334155',
              }}
            >
              <div>
                <div style={{ color: '#94a3b8', fontSize: 13 }}>
                  {selectedSeats.length} ghế ×{' '}
                  {formatCurrency(showtime.basePrice)}/ghế
                </div>
                <div style={{ fontSize: 24, fontWeight: 800, color: '#f97316' }}>
                  {formatCurrency(totalPrice)}
                </div>
              </div>
              <Button
                type="primary"
                size="large"
                disabled={isSubmitting}
                loading={isSubmitting}
                onClick={handlePayment}
                style={{
                  backgroundColor: '#f97316',
                  borderColor: '#f97316',
                  fontWeight: 700,
                  fontSize: 16,
                  height: 48,
                  paddingInline: 32,
                }}
              >
                Tiến hành thanh toán
              </Button>
            </div>
          </div>
        )}

        {/* Empty selection hint */}
        {selectedSeats.length === 0 && (
          <div
            style={{
              marginTop: 24,
              textAlign: 'center',
              color: '#64748b',
              fontSize: 14,
            }}
          >
            Nhấn vào ghế để chọn (tối đa {MAX_SEATS} ghế)
          </div>
        )}

        {/* Stats row */}
        <div
          style={{
            marginTop: 24,
            display: 'flex',
            gap: 24,
            color: '#64748b',
            fontSize: 13,
          }}
        >
          <span>
            Còn trống: <strong style={{ color: '#52c41a' }}>{availableCount}</strong> ghế
          </span>
          <span>
            Đã bán: <strong style={{ color: '#8c8c8c' }}>{bookedCount}</strong> ghế
          </span>
          <span>
            Đang giữ: <strong style={{ color: '#8c8c8c' }}>{lockedCount}</strong> ghế
          </span>
        </div>

        {/* Back */}
        <div style={{ marginTop: 32, textAlign: 'center' }}>
          <Button
            type="text"
            className="!text-slate-400 hover:!text-white"
            onClick={() => navigate(-1)}
          >
            ← Quay lại
          </Button>
        </div>
      </div>
    </div>
  );
}
