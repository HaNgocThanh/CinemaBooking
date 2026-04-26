import Header from '@/components/Header';
import { Outlet } from 'react-router-dom';

/**
 * Main layout for customer website
 * Includes Header and Footer
 */
export function MainLayout({ children }) {
  return (
    <div className="flex flex-col min-h-screen">
      <Header />

      <main className="flex-1 bg-slate-900">
        {children}
        <Outlet />
      </main>

      <footer className="bg-gray-800 text-white p-4">
        <div className="max-w-7xl mx-auto text-center">
          <p>&copy; 2026 Cinema Booking System. All rights reserved.</p>
        </div>
      </footer>
    </div>
  );
}
