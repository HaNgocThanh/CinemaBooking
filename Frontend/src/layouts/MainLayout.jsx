/**
 * Main layout for customer website
 */
export function MainLayout({ children }) {
  return (
    <div className="flex flex-col min-h-screen">
      <header className="bg-primary text-white p-4">
        <div className="max-w-7xl mx-auto">
          <h1 className="text-2xl font-bold">Cinema Booking</h1>
        </div>
      </header>

      <main className="flex-1 bg-gray-50">
        <div className="max-w-7xl mx-auto p-4">
          {children}
        </div>
      </main>

      <footer className="bg-gray-800 text-white p-4">
        <div className="max-w-7xl mx-auto text-center">
          <p>&copy; 2024 Cinema Booking System. All rights reserved.</p>
        </div>
      </footer>
    </div>
  );
}
