import { useSeatTemplates } from '../hooks/useRooms';
import { Spin, Empty, Tag } from 'antd';

const SEAT_COLORS = {
  Regular: { bg: '#e6f4ff', border: '#1890ff', label: 'Thuong' },
  VIP: { bg: '#fff7e6', border: '#fa8c16', label: 'VIP' },
};

function SeatCell({ seat }) {
  const colors = SEAT_COLORS[seat.type] || SEAT_COLORS.Regular;
  return (
    <div
      style={{
        width: '36px',
        height: '36px',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: colors.bg,
        border: `2px solid ${colors.border}`,
        borderRadius: '6px',
        fontSize: '11px',
        fontWeight: 600,
        color: '#333',
        cursor: 'default',
        userSelect: 'none',
      }}
    >
      {seat.row}{seat.number}
    </div>
  );
}

function RowLabel({ label }) {
  return (
    <div
      style={{
        width: '24px',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        fontWeight: 700,
        fontSize: '13px',
        color: '#555',
      }}
    >
      {label}
    </div>
  );
}

export function SeatLayoutGrid({ roomId, roomName }) {
  const { data, isLoading, isError } = useSeatTemplates(roomId);

  if (!roomId) return null;

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: '24px' }}>
        <Spin size="large" />
        <p style={{ marginTop: 12, color: '#888' }}>Dang tai so do ghe...</p>
      </div>
    );
  }

  if (isError) {
    return <Empty description="Khong the tai so do ghe." style={{ margin: '16px 0' }} />;
  }

  const seats = data?.data || [];

  if (seats.length === 0) {
    return <Empty description="Phong nay chua co mau ghe nao." style={{ margin: '16px 0' }} />;
  }

  const rowGroups = seats.reduce((acc, seat) => {
    if (!acc[seat.row]) acc[seat.row] = [];
    acc[seat.row].push(seat);
    return acc;
  }, {});

  const sortedRows = Object.keys(rowGroups).sort();

  return (
    <div style={{ marginTop: 16 }}>
      <div style={{ marginBottom: 8, display: 'flex', alignItems: 'center', gap: 16 }}>
        <span style={{ fontWeight: 600, color: '#333' }}>So do ghe: {roomName}</span>
        <div style={{ display: 'flex', gap: 12 }}>
          <Tag color="blue" style={{ fontSize: 12 }}>
            <span style={{ opacity: 0.7 }}>Thuong</span>
          </Tag>
          <Tag color="orange" style={{ fontSize: 12 }}>
            <span style={{ opacity: 0.7 }}>VIP</span>
          </Tag>
        </div>
      </div>

      <div
        style={{
          backgroundColor: '#f0f0f0',
          borderRadius: 8,
          padding: '20px 16px 12px',
          display: 'inline-block',
          minWidth: 280,
        }}
      >
        <div
          style={{
            textAlign: 'center',
            color: '#888',
            fontSize: 12,
            marginBottom: 12,
            fontWeight: 500,
          }}
        >
          MAN HINH
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
          {sortedRows.map((row) => {
            const rowSeats = rowGroups[row].sort((a, b) => a.number - b.number);
            return (
              <div
                key={row}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 4,
                  justifyContent: 'center',
                }}
              >
                <RowLabel label={row} />
                <div style={{ display: 'flex', gap: 4 }}>
                  {rowSeats.map((seat) => (
                    <SeatCell
                      key={seat.id}
                      seat={{ row: seat.row, number: seat.number, type: seat.type }}
                    />
                  ))}
                </div>
                <RowLabel label={row} />
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}
