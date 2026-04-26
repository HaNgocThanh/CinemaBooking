import React from 'react';
import { Layout, Menu, Button, Avatar, Dropdown } from 'antd';
import {
  VideoCameraOutlined,
  ClockCircleOutlined,
  ShoppingOutlined,
  DollarOutlined,
  UserOutlined,
  SettingOutlined,
  HomeOutlined,
  LogoutOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  AppstoreOutlined,
} from '@ant-design/icons';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '@/context/AuthContext';

const { Sider } = Layout;

/**
 * Admin Sidebar Navigation Menu Items
 */
const menuItems = [
  {
    key: '/admin/movies',
    icon: <VideoCameraOutlined />,
    label: <Link to="/admin/movies">Quản Lý Phim</Link>,
  },
  {
    key: '/admin/rooms',
    icon: <AppstoreOutlined />,
    label: <Link to="/admin/rooms">Quản Lý Phòng Chiếu</Link>,
  },
  {
    key: '/admin/showtimes',
    icon: <ClockCircleOutlined />,
    label: <Link to="/admin/showtimes">Quản Lý Lịch Chiếu</Link>,
  },
  {
    key: '/admin/bookings',
    icon: <ShoppingOutlined />,
    label: <Link to="/admin/bookings">Quản Lý Đơn Đặt</Link>,
  },
  {
    key: '/admin/promotions',
    icon: <DollarOutlined />,
    label: <Link to="/admin/promotions">Quản Lý Khuyến Mãi</Link>,
  },
  {
    key: '/admin/users',
    icon: <UserOutlined />,
    label: <Link to="/admin/users">Quản Lý Người Dùng</Link>,
  },
  {
    key: '/admin/settings',
    icon: <SettingOutlined />,
    label: <Link to="/admin/settings">Cài Đặt</Link>,
  },
];

/**
 * AdminSidebar Component
 *
 * Fixed sidebar for Admin Dashboard containing:
 * - Logo + collapse toggle
 * - Navigation menu
 * - User info + logout
 * - Back to homepage link
 *
 * Props:
 * - collapsed: boolean
 * - onCollapse: () => void
 */
export function AdminSidebar({ collapsed, onCollapse }) {
  const location = useLocation();
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <Sider
      trigger={null}
      collapsible
      collapsed={collapsed}
      width={250}
      className="admin-sider"
      style={{
        position: 'fixed',
        left: 0,
        top: 0,
        bottom: 0,
        height: '100vh',
        zIndex: 100,
        overflowY: 'auto',
        overflowX: 'hidden',
      }}
    >
      {/* Logo + Toggle */}
      <div className="admin-logo">
        <VideoCameraOutlined style={{ fontSize: '24px', color: 'white' }} />
        {!collapsed && <span>CineAdmin</span>}
        <Button
          type="text"
          icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
          onClick={onCollapse}
          style={{
            color: 'white',
            marginLeft: collapsed ? 0 : 'auto',
            fontSize: '16px',
          }}
          size="small"
        />
      </div>

      {/* Navigation Menu */}
      <Menu
        theme="dark"
        mode="inline"
        selectedKeys={[location.pathname]}
        items={menuItems}
        className="admin-menu"
      />

      {/* Bottom Section: User Info + Home Link */}
      <div className="admin-sidebar-footer">
        {/* Back to Homepage */}
        <Menu
          theme="dark"
          mode="inline"
          selectable={false}
          items={[
            {
              key: 'home',
              icon: <HomeOutlined />,
              label: <Link to="/">Về Trang Chủ</Link>,
            },
          ]}
          className="admin-menu"
          style={{ borderTop: '1px solid rgba(255,255,255,0.08)' }}
        />

        {/* User Info + Logout */}
        <div className="admin-sidebar-user">
          {!collapsed ? (
            <div className="admin-sidebar-user-info">
              <Avatar
                size={32}
                icon={<UserOutlined />}
                style={{ backgroundColor: '#667eea', flexShrink: 0 }}
              />
              <div className="admin-sidebar-user-text">
                <span className="admin-sidebar-user-name">
                  {user?.fullName || user?.username || 'Admin'}
                </span>
                <span className="admin-sidebar-user-role">
                  {user?.role || 'Admin'}
                </span>
              </div>
              <Button
                type="text"
                icon={<LogoutOutlined />}
                onClick={handleLogout}
                style={{ color: 'rgba(255,255,255,0.65)', marginLeft: 'auto' }}
                size="small"
                title="Đăng xuất"
              />
            </div>
          ) : (
            <div style={{ textAlign: 'center', padding: '12px 0' }}>
              <Dropdown
                menu={{
                  items: [
                    {
                      key: 'logout',
                      icon: <LogoutOutlined />,
                      label: 'Đăng Xuất',
                      danger: true,
                      onClick: handleLogout,
                    },
                  ],
                }}
                trigger={['click']}
              >
                <Avatar
                  size={32}
                  icon={<UserOutlined />}
                  style={{ backgroundColor: '#667eea', cursor: 'pointer' }}
                />
              </Dropdown>
            </div>
          )}
        </div>
      </div>
    </Sider>
  );
}
