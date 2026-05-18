import { useState } from 'react';
import { Button, message, Drawer } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { RoomTable } from './components/RoomTable';
import { RoomFormModal } from './components/RoomFormModal';
import { SeatMapBuilder } from './components/SeatMapBuilder';
import { SeatMapViewer } from './components/SeatMapViewer';
import { useRooms, useSeatTemplateMutation } from './hooks/useRooms';

export function RoomManagementPage() {
  const {
    rooms,
    isLoading,
    isError,
    error,
    createRoom,
    isCreating,
    updateRoom,
    isUpdating,
    deleteRoom,
    isDeleting,
  } = useRooms();

  const { saveSeatTemplates, isSaving } = useSeatTemplateMutation();

  const [formModalVisible, setFormModalVisible] = useState(false);
  const [editingRoom, setEditingRoom] = useState(null);
  const [selectedRoom, setSelectedRoom] = useState(null);
  const [drawerVisible, setDrawerVisible] = useState(false);
  const [isEditingLayout, setIsEditingLayout] = useState(false);

  const isMutating = isCreating || isUpdating;

  const handleAddRoom = () => {
    setEditingRoom(null);
    setFormModalVisible(true);
  };

  const handleEditRoom = (room) => {
    setEditingRoom(room);
    setFormModalVisible(true);
  };

  const handleViewSeats = (room) => {
    setSelectedRoom(room);
    setIsEditingLayout(false);
    setDrawerVisible(true);
  };

  const handleEditLayout = (room) => {
    setSelectedRoom(room);
    setIsEditingLayout(true);
    setDrawerVisible(true);
  };

  const handleFormSubmit = async (values, roomId) => {
    try {
      if (roomId) {
        await updateRoom(roomId, values);
        message.success('Cap nhat phong chieu thanh cong!');
      } else {
        await createRoom(values);
        message.success('Tao phong chieu thanh cong!');
      }
      setFormModalVisible(false);
      setEditingRoom(null);
    } catch (err) {
      message.error(err?.message || 'Co loi xay ra khi luu phong chieu!');
    }
  };

  const handleDeleteRoom = async (roomId) => {
    try {
      await deleteRoom(roomId);
      message.success('Xoa phong chieu thanh cong!');
    } catch (err) {
      message.error(err?.message || 'Co loi xay ra khi xoa phong chieu!');
    }
  };

  const handleFormCancel = () => {
    setFormModalVisible(false);
    setEditingRoom(null);
  };

  const handleSaveLayout = async (seats, totalSeats) => {
    try {
      await saveSeatTemplates(selectedRoom.id, seats, totalSeats);
      message.success(`Da luu ${totalSeats} ghe cho phong "${selectedRoom.name}"!`);
      setDrawerVisible(false);
      setSelectedRoom(null);
    } catch (err) {
      message.error(err?.message || 'Co loi khi luu so do ghe!');
      throw err;
    }
  };

  return (
    <div className="room-management-page">
      <div className="page-header">
        <div className="page-header-content">
          <h1>Quan Ly Phong Chieu</h1>
          <p>Quan ly danh sach phong chieu va so do ghe trong he thong</p>
        </div>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={handleAddRoom}
          size="large"
        >
          Them Phong
        </Button>
      </div>

      {isError && (
        <div className="error-message">
          <p>Co loi khi tai danh sach phong chieu: {error?.message}</p>
        </div>
      )}

      <RoomTable
        rooms={rooms}
        loading={isLoading}
        onEdit={handleEditRoom}
        onDelete={handleDeleteRoom}
        onViewSeats={handleViewSeats}
        onEditLayout={handleEditLayout}
      />

      <RoomFormModal
        open={formModalVisible}
        onCancel={handleFormCancel}
        onSubmit={handleFormSubmit}
        loading={isMutating}
        initialValues={editingRoom}
        mode={editingRoom ? 'edit' : 'create'}
      />

      <Drawer
        title={isEditingLayout ? `Chinh sua so do - ${selectedRoom?.name || ''}` : `So do ghe - ${selectedRoom?.name || ''}`}
        placement="right"
        width={560}
        onClose={() => {
          setDrawerVisible(false);
          setSelectedRoom(null);
        }}
        open={drawerVisible}
      >
        {selectedRoom && (
          isEditingLayout ? (
            <SeatMapBuilder
              roomId={selectedRoom.id}
              onSave={handleSaveLayout}
              onCancel={() => setDrawerVisible(false)}
              isSaving={isSaving}
            />
          ) : (
            <SeatMapViewer
              roomId={selectedRoom.id}
              roomName={selectedRoom.name}
            />
          )
        )}
      </Drawer>
    </div>
  );
}
