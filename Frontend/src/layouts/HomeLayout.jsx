import Navbar from '@/components/Navbar';
import Footer from '@/components/Footer';

/**
 * HomeLayout - Layout cho cac trang customer (HomePage, MovieDetailsPage...)
 * Bao gom Navbar va Footer premium.
 */
export function HomeLayout({ children }) {
  return (
    <div className="flex flex-col min-h-screen">
      <Navbar />
      <main className="flex-1 bg-slate-950">
        {children}
      </main>
      <Footer />
    </div>
  );
}
