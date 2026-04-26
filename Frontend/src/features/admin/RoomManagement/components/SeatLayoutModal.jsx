import { Modal } from 'antd';
import { SeatLayoutGrid } from './SeatLayoutGrid';

export function SeatLayoutModal({ open, onCancel, room }) {
  return (
    <Modal
      title={`So Do Ghe - ${room?.name || ''}`}
      open={open}
      onCancel={onCancel}
      footer={null}
      width={500}
      destroyOnClose
    >
      <SeatLayoutGrid roomId={room?.id} roomName={room?.name} />
    </Modal>
  );
}
