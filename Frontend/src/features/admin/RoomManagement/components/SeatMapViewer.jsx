import { useMemo } from 'react';
import { Spin, Empty } from 'antd';
import { useSeatTemplates } from '../hooks/useRooms';

const CELL_SIZE = 40;
const CELL_GAP = 4;

const SEAT_COLORS = {
  Regular: { bg: '#e6f4ff', border: '#1890ff', label: 'Thuong' },
  VIP: { bg: '#fff7e6', border: '#fa8c16', label: 'VIP' },
  Couple: { bg: '#f9f0ff', border: '#722ed1', label: 'Couple' },
};

function SeatCell({ seat }) {
  const colors = SEAT_COLORS[seat.type] || SEAT_COLORS.Regular;
  return (
    <div
      style={{
        width: CELL_SIZE,
        height: CELL_SIZE,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: colors.bg,
        border: `2px solid ${colors.border}`,
        borderRadius: 6,
        fontSize: 11,
        fontWeight: 700,
        color: colors.border,
        userSelect: 'none',
        boxShadow: `0 1px 3px ${colors.border}22`,
        cursor: 'default',
      }}
      title={`${seat.row}${seat.number} - ${colors.label}`}
    >
      {seat.row}{seat.number}
    </div>
  );
}

function AisleCell() {
  return (
    <div
      style={{
        width: CELL_SIZE,
        height: CELL_SIZE,
        backgroundColor: 'transparent',
        border: '1px dashed transparent',
        borderRadius: 6,
      }}
    />
  );
}

export function SeatMapViewer({ roomId, roomName, className }) {
  const { data: seats, isLoading, isError } = useSeatTemplates(roomId);

  const { matrix, maxCol, maxRow } = useMemo(() => {
    if (!seats?.data?.length) return { matrix: [], maxCol: 0, maxRow: 0 };

    const list = seats.data;

    const maxRowVal = Math.max(...list.map((s) => s.gridRow));
    const maxColVal = Math.max(...list.map((s) => s.gridColumn));

    const matrix = Array.from({ length: maxRowVal + 1 }, () =>
      Array.from({ length: maxColVal + 1 }, () => null)
    );

    for (const seat of list) {
      matrix[seat.gridRow][seat.gridColumn] = seat;
    }

    return { matrix, maxCol: maxColVal, maxRow: maxRowVal };
  }, [seats]);

  if (!roomId) return null;

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: 24 }}>
        <Spin size="large" />
        <p style={{ marginTop: 12, color: '#888' }}>Dang tai so do ghe...</p>
      </div>
    );
  }

  if (isError) {
    return (
      <Empty
        description="Khong the tai so do ghe. Vui long thu lai."
        style={{ margin: '16px 0' }}
      />
    );
  }

  if (!matrix.length || !seats?.data?.length) {
    return (
      <Empty
        description="Phong nay chua co mau ghe nao."
        style={{ margin: '16px 0' }}
      />
    );
  }

  const seatCount = seats.data.length;
  const vipCount = seats.data.filter((s) => s.type === 'VIP').length;
  const regularCount = seats.data.filter((s) => s.type === 'Regular').length;

  return (
    <div style={{ display: 'inline-block' }} className={className}>
      {/* Header */}
      <div style={{ marginBottom: 12 }}>
        <div style={{ fontWeight: 700, fontSize: 15, color: '#1a1a1a', marginBottom: 8 }}>
          So do ghe: {roomName}
        </div>
        <div style={{ display: 'flex', gap: 12, fontSize: 12, color: '#666' }}>
          <span>
            Tong: <strong style={{ color: '#333' }}>{seatCount}</strong> ghe
          </span>
          <span style={{ color: SEAT_COLORS.Regular.border }}>
            Thuong: <strong>{regularCount}</strong>
          </span>
          <span style={{ color: SEAT_COLORS.VIP.border }}>
            VIP: <strong>{vipCount}</strong>
          </span>
        </div>
      </div>

      {/* Legend */}
      <div style={{ display: 'flex', gap: 12, marginBottom: 12 }}>
        {Object.entries(SEAT_COLORS).map(([type, cfg]) => (
          <div key={type} style={{ display: 'flex', alignItems: 'center', gap: 4, fontSize: 12 }}>
            <div
              style={{
                width: 14,
                height: 14,
                borderRadius: 3,
                backgroundColor: cfg.bg,
                border: `1.5px solid ${cfg.border}`,
              }}
            />
            <span style={{ color: '#555' }}>{cfg.label}</span>
          </div>
        ))}
      </div>

      {/* Screen label */}
      <div
        style={{
          textAlign: 'center',
          color: '#aaa',
          fontSize: 11,
          fontWeight: 700,
          letterSpacing: 3,
          marginBottom: 16,
          padding: '4px 0',
          borderBottom: '2px solid #e8e8e8',
        }}
      >
        MAN HINH
      </div>

      {/* Column numbers */}
      <div style={{ display: 'flex', marginLeft: CELL_SIZE + CELL_GAP, marginBottom: 4 }}>
        {Array.from({ length: maxCol + 1 }, (_, c) => (
          <div
            key={c}
            style={{
              width: CELL_SIZE,
              marginRight: CELL_GAP,
              textAlign: 'center',
              fontSize: 10,
              color: '#bbb',
              fontWeight: 600,
            }}
          >
            {c + 1}
          </div>
        ))}
      </div>

      {/* Matrix grid */}
      <div style={{ display: 'flex', flexDirection: 'column', gap: CELL_GAP }}>
        {matrix.map((rowData, rowIdx) => {
          const rowLetter = String.fromCharCode(65 + rowIdx);
          const hasSeats = rowData.some((cell) => cell !== null);

          return (
            <div
              key={rowIdx}
              style={{ display: 'flex', alignItems: 'center', gap: CELL_GAP }}
            >
              {/* Row label left */}
              <div
                style={{
                  width: CELL_SIZE,
                  textAlign: 'center',
                  fontSize: 12,
                  fontWeight: 700,
                  color: '#888',
                }}
              >
                {hasSeats ? rowLetter : ''}
              </div>

              {/* Cells */}
              <div style={{ display: 'flex', gap: CELL_GAP }}>
                {rowData.map((cell, colIdx) => {
                  if (cell === null) {
                    return <AisleCell key={colIdx} />;
                  }
                  return (
                    <SeatCell
                      key={colIdx}
                      seat={{
                        row: cell.row,
                        number: cell.number,
                        type: cell.type,
                      }}
                    />
                  );
                })}
              </div>

              {/* Row label right */}
              <div
                style={{
                  width: CELL_SIZE,
                  textAlign: 'center',
                  fontSize: 12,
                  fontWeight: 700,
                  color: '#888',
                }}
              >
                {hasSeats ? rowLetter : ''}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
