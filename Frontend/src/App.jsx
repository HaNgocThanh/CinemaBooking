import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import queryClient from './config/queryClient';
import { AuthProvider } from './context/AuthContext';
import { MainLayout } from './layouts';
import { HomePage } from './pages';
import LoginPage from './pages/auth/LoginPage';
import './App.css';

/**
 * Main App Component
 * Setup React Router, React Query, and Auth Context
 * 
 * Routes:
 * /                  → HomePage (full-width with custom Navbar/Footer)
 * /login             → LoginPage (fullscreen, no header)
 * /movies            → MoviesPage (with MainLayout)
 * /bookings          → BookingsPage (with MainLayout)
 */
function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <Router>
          <Routes>
            {/* Public Routes - Full Width, No MainLayout */}
            <Route path="/" element={<HomePage />} />
            <Route path="/login" element={<LoginPage />} />

            {/* Protected Routes - With MainLayout */}
            <Route
              path="/*"
              element={
                <MainLayout>
                  <Routes>
                    {/* Add more routes here as features are developed */}
                    {/* <Route path="/register" element={<RegisterPage />} /> */}
                    {/* <Route path="/movies" element={<MoviesPage />} /> */}
                    {/* <Route path="/bookings" element={<BookingsPage />} /> */}
                    {/* <Route path="/showtimes" element={<ShowtimesPage />} /> */}
                    {/* <Route path="/cinemas" element={<CinemasPage />} /> */}
                    {/* <Route path="/promotions" element={<PromotionsPage />} /> */}
                  </Routes>
                </MainLayout>
              }
            />
          </Routes>
        </Router>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
