import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import queryClient from './config/queryClient';
import { MainLayout } from './layouts';
import { HomePage } from './pages';
import './App.css';

/**
 * Main App Component
 * Setup React Router and React Query
 * 
 * Routes:
 * /                  → HomePage
 * /auth/login        → LoginPage (auth feature)
 * /movies            → MoviesPage (movies feature)
 * /bookings          → BookingPage (booking feature)
 */
function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <Router>
        <MainLayout>
          <Routes>
            <Route path="/" element={<HomePage />} />
            {/* Add more routes here as features are developed */}
            {/* <Route path="/auth/login" element={<LoginPage />} /> */}
            {/* <Route path="/movies" element={<MoviesPage />} /> */}
            {/* <Route path="/bookings" element={<BookingsPage />} /> */}
          </Routes>
        </MainLayout>
      </Router>
    </QueryClientProvider>
  );
}

export default App;
