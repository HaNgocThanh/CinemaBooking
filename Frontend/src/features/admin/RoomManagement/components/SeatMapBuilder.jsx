import { useState, useCallback, useRef } from 'react';
import { Button, InputNumber, message, Space, Tooltip, Divider, Alert } from 'antd';

const CELL_SIZE = 40;
const CELL_GAP = 4;
const AISLE = null; // null = lối đi

const TOOL_TYPES = {
  REGULAR: 'REGULAR',
  VIP: 'VIP',
  AISLE: 'AISLE',
};

const TOOL_CONFIG = {
  [TOOL_TYPES.REGULAR]: {
    label: 'Ghe Thuong',
    bg: '#e6f4ff',
    border: '#1890ff',
    color: '#1890ff',
  },
  [TOOL_TYPES.VIP]: {
    label: 'Ghe VIP',
    bg: '#fff7e6',
    border: '#fa8c16',
    color: '#fa8c16',
  },
  [TOOL_TYPES.AISLE]: {
    label: 'Lối đi (Xoa)',
    bg: '#f5f5f5',
    border: '#d9d9d9',
    color: '#999',
  },
};

function generateSeatLabels(grid) {
  const labelMap = {};
  const seatCounters = {};

  for (let row = 0; row < grid.length; row++) {
    for (let col = 0; col < grid[row].length; col++) {
      const cell = grid[row][col];
      if (cell === AISLE) continue;

      const letter = String.fromCharCode(65 + row);
      if (!seatCounters[letter]) seatCounters[letter] = 0;
      seatCounters[letter]++;
      labelMap[`${row}-${col}`] = `${letter}${seatCounters[letter]}`;
    }
  }

  return labelMap;
}

function buildPayload(grid, labelMap) {
  const seats = [];
  for (let row = 0; row < grid.length; row++) {
    for (let col = 0; col < grid[row].length; col++) {
      if (grid[row][col] === AISLE) continue;

      const label = labelMap[`${row}-${col}`];
      const [rowLetter, seatNum] = [label[0], parseInt(label.slice(1), 10)];
      seats.push({
        row: rowLetter,
        number: seatNum,
        type: grid[row][col],
        gridRow: row,
        gridColumn: col,
      });
    }
  }
  return seats;
}

export function SeatMapBuilder({ roomId, onSave, onCancel, isSaving }) {
  const [rows, setRows] = useState(5);
  const [cols, setCols] = useState(10);
  const [grid, setGrid] = useState(null);
  const [selectedTool, setSelectedTool] = useState(TOOL_TYPES.REGULAR);
  const [isPainting, setIsPainting] = useState(false);
  const labelMapRef = useRef({});

  const handleCreateGrid = () => {
    if (rows < 2 || rows > 20 || cols < 3 || cols > 30) {
      message.warning('So hang: 2-20, So cot: 3-30');
      return;
    }
    const newGrid = Array.from({ length: rows }, () => Array(cols).fill(TOOL_TYPES.REGULAR));
    setGrid(newGrid);
    labelMapRef.current = generateSeatLabels(newGrid);
  };

  const applyTool = useCallback((row, col) => {
    if (!grid) return;
    setGrid((prev) => {
      const next = prev.map((r) => [...r]);
      next[row][col] = selectedTool === TOOL_TYPES.AISLE ? AISLE : selectedTool;
      labelMapRef.current = generateSeatLabels(next);
      return next;
    });
  }, [grid, selectedTool]);

  const handleMouseDown = (row, col) => {
    setIsPainting(true);
    applyTool(row, col);
  };

  const handleMouseEnter = (row, col) => {
    if (isPainting) applyTool(row, col);
  };

  const handleMouseUp = () => {
    setIsPainting(false);
  };

  const handleClearAll = () => {
    if (!grid) return;
    setGrid((prev) => prev.map((r) => r.map(() => TOOL_TYPES.REGULAR)));
    labelMapRef.current = generateSeatLabels(
      grid.map((r) => r.map(() => TOOL_TYPES.REGULAR))
    );
  };

  const handleSave = async () => {
    if (!grid) {
      message.warning('Vui long tao luoi truoc.');
      return;
    }
    const hasSeats = grid.some((r) => r.some((c) => c !== AISLE));
    if (!hasSeats) {
      message.warning('Phai co it nhat 1 ghe (khong phai loi di).');
      return;
    }
    const seats = buildPayload(grid, labelMapRef.current);
    const totalSeats = seats.length;
    await onSave(seats, totalSeats);
  };

  const seatCount = grid
    ? grid.flat().filter((c) => c !== AISLE).length
    : 0;
  const vipCount = grid
    ? grid.flat().filter((c) => c === TOOL_TYPES.VIP).length
    : 0;

  return (
    <div style={{ userSelect: 'none' }} onMouseUp={handleMouseUp} onMouseLeave={handleMouseUp}>
      {/* Toolbar */}
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          gap: 12,
          marginBottom: 16,
          flexWrap: 'wrap',
        }}
      >
        <Space>
          <span style={{ fontWeight: 600, color: '#333' }}>Cong cu:</span>
          {Object.values(TOOL_TYPES).map((tool) => {
            const cfg = TOOL_CONFIG[tool];
            const active = selectedTool === tool;
            return (
              <Tooltip key={tool} title={cfg.label}>
                <Button
                  type={active ? 'primary' : 'default'}
                  onClick={() => setSelectedTool(tool)}
                  style={{
                    backgroundColor: active ? cfg.bg : undefined,
                    borderColor: cfg.border,
                    color: active ? cfg.color : undefined,
                    fontWeight: 600,
                    minWidth: 80,
                  }}
                >
                  {cfg.label}
                </Button>
              </Tooltip>
            );
          })}
        </Space>

        <Divider type="vertical" style={{ height: 24, margin: '0 4px' }} />

        <Button onClick={handleClearAll} size="small" disabled={!grid}>
          Xoa trang
        </Button>
      </div>

      {/* Grid controls */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 16 }}>
        <Space size="small">
          <span>So hang:</span>
          <InputNumber
            min={2}
            max={20}
            value={rows}
            onChange={(v) => setRows(v || 5)}
            style={{ width: 72 }}
            disabled={!!grid}
          />
        </Space>
        <Space size="small">
          <span>So cot:</span>
          <InputNumber
            min={3}
            max={30}
            value={cols}
            onChange={(v) => setCols(v || 10)}
            style={{ width: 72 }}
            disabled={!!grid}
          />
        </Space>
        {!grid ? (
          <Button type="primary" onClick={handleCreateGrid}>
            Tao luoi
          </Button>
        ) : (
          <Button
            onClick={() => {
              setGrid(null);
              labelMapRef.current = {};
            }}
          >
            Chinh sua luoi
          </Button>
        )}

        {grid && (
          <Alert
            type="info"
            showIcon={false}
            style={{
              marginLeft: 'auto',
              fontSize: 12,
              background: '#f0f9ff',
              border: '1px solid #91d5ff',
            }}
            message={
              <Space size={16}>
                <span>
                  Tong ghe: <strong style={{ color: '#1890ff' }}>{seatCount}</strong>
                </span>
                <span>
                  VIP: <strong style={{ color: '#fa8c16' }}>{vipCount}</strong>
                </span>
                <span>
                  Loi di: <strong style={{ color: '#999' }}>{rows * cols - seatCount}</strong>
                </span>
              </Space>
            }
          />
        )}
      </div>

      {/* Seat grid */}
      {grid ? (
        <div
          style={{
            backgroundColor: '#fafafa',
            borderRadius: 8,
            padding: '20px 16px 12px',
            display: 'inline-block',
            minWidth: 280,
            cursor: 'crosshair',
          }}
        >
          <div
            style={{
              textAlign: 'center',
              color: '#888',
              fontSize: 12,
              marginBottom: 16,
              fontWeight: 600,
              letterSpacing: 2,
            }}
          >
            MAN HINH
          </div>

          {/* Column numbers */}
          <div style={{ display: 'flex', marginLeft: CELL_SIZE + CELL_GAP, marginBottom: 4 }}>
            {Array.from({ length: cols }, (_, c) => (
              <div
                key={c}
                style={{
                  width: CELL_SIZE,
                  marginRight: CELL_GAP,
                  textAlign: 'center',
                  fontSize: 11,
                  color: '#999',
                  fontWeight: 600,
                }}
              >
                {c + 1}
              </div>
            ))}
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: CELL_GAP }}>
            {grid.map((rowData, rowIdx) => {
              const rowLetter = String.fromCharCode(65 + rowIdx);
              const rowHasSeats = rowData.some((c) => c !== AISLE);
              return (
                <div key={rowIdx} style={{ display: 'flex', alignItems: 'center', gap: CELL_GAP }}>
                  {/* Row label */}
                  <div
                    style={{
                      width: CELL_SIZE,
                      textAlign: 'center',
                      fontSize: 12,
                      fontWeight: 700,
                      color: '#555',
                    }}
                  >
                    {rowHasSeats ? rowLetter : ''}
                  </div>

                  {/* Cells */}
                  <div style={{ display: 'flex', gap: CELL_GAP }}>
                    {rowData.map((cell, colIdx) => {
                      const label = labelMapRef.current[`${rowIdx}-${colIdx}`];
                      const cfg = cell === AISLE
                        ? TOOL_CONFIG[TOOL_TYPES.AISLE]
                        : TOOL_CONFIG[cell] || TOOL_CONFIG[TOOL_TYPES.REGULAR];
                      const isSeat = cell !== AISLE;

                      return (
                        <Tooltip
                          key={colIdx}
                          title={
                            isSeat
                              ? `${label} (${cfg.label})`
                              : 'Lối đi'
                          }
                        >
                          <div
                            onMouseDown={() => handleMouseDown(rowIdx, colIdx)}
                            onMouseEnter={() => handleMouseEnter(rowIdx, colIdx)}
                            style={{
                              width: CELL_SIZE,
                              height: CELL_SIZE,
                              backgroundColor: cfg.bg,
                              border: `2px solid ${cfg.border}`,
                              borderRadius: 6,
                              display: 'flex',
                              alignItems: 'center',
                              justifyContent: 'center',
                              fontSize: 10,
                              fontWeight: 700,
                              color: isSeat ? cfg.color : '#ccc',
                              cursor: 'pointer',
                              transition: 'transform 80ms, box-shadow 80ms',
                              boxShadow: isSeat ? `0 1px 3px ${cfg.border}33` : 'none',
                            }}
                          >
                            {isSeat ? label : ''}
                          </div>
                        </Tooltip>
                      );
                    })}
                  </div>

                  {/* Row label right side */}
                  <div
                    style={{
                      width: CELL_SIZE,
                      textAlign: 'center',
                      fontSize: 12,
                      fontWeight: 700,
                      color: '#555',
                    }}
                  >
                    {rowHasSeats ? rowLetter : ''}
                  </div>
                </div>
              );
            })}
          </div>

          {/* Legend */}
          <div
            style={{
              marginTop: 16,
              display: 'flex',
              justifyContent: 'center',
              gap: 16,
              fontSize: 12,
              color: '#666',
            }}
          >
            {[
              { tool: TOOL_TYPES.REGULAR, label: 'Thuong' },
              { tool: TOOL_TYPES.VIP, label: 'VIP' },
              { tool: TOOL_TYPES.AISLE, label: 'Loi di' },
            ].map(({ tool, label }) => (
              <div key={tool} style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
                <div
                  style={{
                    width: 14,
                    height: 14,
                    borderRadius: 3,
                    backgroundColor: TOOL_CONFIG[tool].bg,
                    border: `1.5px solid ${TOOL_CONFIG[tool].border}`,
                  }}
                />
                <span>{label}</span>
              </div>
            ))}
          </div>
        </div>
      ) : (
        <div
          style={{
            textAlign: 'center',
            padding: '48px 24px',
            color: '#aaa',
            border: '2px dashed #e0e0e0',
            borderRadius: 8,
          }}
        >
          <div style={{ fontSize: 32, marginBottom: 8 }}>🎲</div>
          <p style={{ margin: 0 }}>
            Nhap so hang / so cot ben tren roi bam <strong>"Tao luoi"</strong>
          </p>
        </div>
      )}

      {/* Action buttons */}
      <div style={{ marginTop: 20, display: 'flex', justifyContent: 'flex-end', gap: 8 }}>
        <Button onClick={onCancel}>Huy</Button>
        <Button
          type="primary"
          onClick={handleSave}
          loading={isSaving}
          disabled={!grid}
        >
          Luu so do ({seatCount} ghe)
        </Button>
      </div>
    </div>
  );
}
