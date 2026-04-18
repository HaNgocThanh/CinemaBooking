import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/context/AuthContext';
import { LogOut, User } from 'lucide-react';

/**
 * Header Component - Thanh Navigation
 */
export default function Header() {
  const navigate = useNavigate();
  const { isAuthenticated, user, logout } = useAuth();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <header className="bg-gray-800 text-white shadow-md">
      <div className="max-w-7xl mx-auto px-4 py-4 flex items-center justify-between">
        {/* Logo */}
        <div
          className="flex items-center gap-2 cursor-pointer"
          onClick={() => navigate('/')}
        >
          <div className="w-10 h-10 bg-gradient-to-r from-blue-500 to-blue-600 rounded-lg flex items-center justify-center">
            <span className="text-lg font-bold">🎬</span>
          </div>
          <h1 className="text-2xl font-bold">CinemaX</h1>
        </div>

        {/* Right Section - Auth Status */}
        <div className="flex items-center gap-4">
          {isAuthenticated ? (
            <>
              {/* User Profile */}
              <div className="flex items-center gap-2 bg-gray-700 px-4 py-2 rounded-lg">
                <User size={18} />
                <span className="text-sm font-medium">
                  {user?.fullName || user?.email || 'User'}
                </span>
              </div>

              {/* Logout Button */}
              <button
                onClick={handleLogout}
                className="flex items-center gap-2 bg-red-600 hover:bg-red-700 px-4 py-2 rounded-lg transition duration-300 font-medium"
              >
                <LogOut size={18} />
                Đăng Xuất
              </button>
            </>
          ) : (
            <>
              {/* Login Button */}
              <button
                onClick={() => navigate('/login')}
                className="bg-blue-600 hover:bg-blue-700 px-6 py-2 rounded-lg transition duration-300 font-medium"
              >
                Đăng Nhập
              </button>

              {/* Register Button */}
              <button
                onClick={() => navigate('/register')}
                className="border border-blue-400 text-blue-400 hover:bg-blue-400 hover:text-white px-6 py-2 rounded-lg transition duration-300 font-medium"
              >
                Đăng Ký
              </button>
            </>
          )}
        </div>
      </div>
    </header>
  );
}
