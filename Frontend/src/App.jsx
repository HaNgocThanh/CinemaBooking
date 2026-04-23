import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import queryClient from './config/queryClient';
import { AuthProvider } from './context/AuthContext';
import { MainLayout, AdminLayout } from './layouts';
import { HomePage, MoviesPage } from './pages';
import LoginPage from './pages/auth/LoginPage';
import AdminMovieManagementPage from './pages/admin/MovieManagementPage';
import './App.css';

/**
 * Main App Component
 * Setup React Router, React Query, and Auth Context
 * 
 * Routes:
 * /                         → HomePage (full-width with custom Navbar/Footer)
 * /login                    → LoginPage (fullscreen, no header)
 * 
 * Customer Routes (MainLayout):
 * /movies                   → MoviesPage
 * /bookings                 → BookingsPage
 * /showtimes                → ShowtimesPage
 * 
 * Admin Routes (AdminLayout):
 * /admin                    → Redirect to /admin/movies
 * /admin/movies             → Movie Management
 * /admin/showtimes          → Showtime Management
 * /admin/bookings           → Booking Management
 * /admin/promotions         → Promotion Management
 * /admin/users              → User Management
 */
function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <Router>
          <Routes>
            {/* Public Routes - Full Width, No Layout */}
            <Route path="/" element={<HomePage />} />
            <Route path="/login" element={<LoginPage />} />

            {/* Customer Routes - With MainLayout */}
            <Route
              path="/*"
              element={
                <MainLayout>
                  <Routes>
                    {/* <Route path="/register" element={<RegisterPage />} /> */}
                    <Route path="/movies" element={<MoviesPage />} />
                    {/* <Route path="/bookings" element={<BookingsPage />} /> */}
                    {/* <Route path="/showtimes" element={<ShowtimesPage />} /> */}
                    {/* <Route path="/cinemas" element={<CinemasPage />} /> */}
                    {/* <Route path="/promotions" element={<PromotionsPage />} /> */}
                  </Routes>
                </MainLayout>
              }
            />

            {/* Admin Routes - With AdminLayout */}
            <Route
              path="/admin/*"
              element={
                <AdminLayout>
                  <Routes>
                    {/* Default: redirect /admin → /admin/movies */}
                    <Route index element={<Navigate to="/admin/movies" replace />} />
                    <Route path="movies" element={<AdminMovieManagementPage />} />
                    {/* <Route path="showtimes" element={<AdminShowtimeManagementPage />} /> */}
                    {/* <Route path="bookings" element={<AdminBookingManagementPage />} /> */}
                    {/* <Route path="promotions" element={<AdminPromotionManagementPage />} /> */}
                    {/* <Route path="users" element={<AdminUserManagementPage />} /> */}
                    {/* <Route path="settings" element={<AdminSettingsPage />} /> */}
                  </Routes>
                </AdminLayout>
              }
            />
          </Routes>
        </Router>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
