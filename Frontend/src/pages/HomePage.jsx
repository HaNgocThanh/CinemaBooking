/**
 * HomePage Component
 * Main landing page of Cinema Booking System
 * 
 * Route: /
 * Layout: MainLayout
 */

export default function HomePage() {
  return (
    <div className="flex flex-col items-center justify-center min-h-96 py-12">
      <h1 className="text-4xl font-bold text-gray-800 mb-4">Cinema Booking System</h1>
      <p className="text-lg text-gray-600 mb-8">Welcome to your cinema booking platform</p>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div className="p-6 bg-white rounded-lg shadow-md">
          <h3 className="text-xl font-semibold mb-2">🎬 Browse Movies</h3>
          <p className="text-gray-600">Explore the latest movies and showtimes</p>
        </div>
        <div className="p-6 bg-white rounded-lg shadow-md">
          <h3 className="text-xl font-semibold mb-2">🎫 Book Tickets</h3>
          <p className="text-gray-600">Select your seats and book tickets easily</p>
        </div>
        <div className="p-6 bg-white rounded-lg shadow-md">
          <h3 className="text-xl font-semibold mb-2">💳 Secure Payment</h3>
          <p className="text-gray-600">Safe and secure payment processing</p>
        </div>
      </div>
    </div>
  );
}
