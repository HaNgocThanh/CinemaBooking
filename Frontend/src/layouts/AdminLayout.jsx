import React, { useState } from 'react';
import { Layout } from 'antd';
import { AdminSidebar } from '@/components/admin/AdminSidebar';
import './AdminLayout.css';

const { Content } = Layout;

/**
 * AdminLayout
 * Layout for admin dashboard.
 * - Fixed sidebar on the left (AdminSidebar component)
 * - No top navbar — sidebar handles all navigation
 * - Content area with margin-left to avoid sidebar overlap
 */
export function AdminLayout({ children }) {
  const [collapsed, setCollapsed] = useState(false);

  const siderWidth = collapsed ? 80 : 250;

  return (
    <Layout className="admin-layout" style={{ minHeight: '100vh' }}>
      {/* Fixed Sidebar */}
      <AdminSidebar
        collapsed={collapsed}
        onCollapse={() => setCollapsed(!collapsed)}
      />

      {/* Main Content — offset by sidebar width */}
      <Layout
        style={{
          marginLeft: siderWidth,
          transition: 'margin-left 0.2s ease',
        }}
      >
        {/* Content Area */}
        <Content className="admin-content">
          {children}
        </Content>

        {/* Footer */}
        <div className="admin-footer">
          <p>&copy; 2026 Cinema Booking System - Admin Panel</p>
        </div>
      </Layout>
    </Layout>
  );
}
