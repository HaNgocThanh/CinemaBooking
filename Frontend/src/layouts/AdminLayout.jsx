import React, { useState } from 'react';
import { Layout } from 'antd';
import { AdminSidebar } from '@/components/admin/AdminSidebar';
import './AdminLayout.css';

const { Content } = Layout;

/**
 * AdminLayout
 * Dedicated layout for admin dashboard with fixed sidebar navigation.
 * No top navbar — all navigation lives in AdminSidebar.
 */
export function AdminLayout({ children }) {
  const [collapsed, setCollapsed] = useState(false);

  return (
    <Layout className="admin-layout" style={{ minHeight: '100vh' }}>
      {/* Fixed Sidebar (AdminSidebar component) */}
      <AdminSidebar
        collapsed={collapsed}
        onCollapse={() => setCollapsed(!collapsed)}
      />

      {/* Main Content — offset by sidebar width */}
      <Layout
        style={{
          marginLeft: collapsed ? 80 : 250,
          transition: 'margin-left 0.2s',
        }}
      >
        {/* Content Area */}
        <Content className="admin-content">
          {children}
        </Content>

      </Layout>
    </Layout>
  );
}
