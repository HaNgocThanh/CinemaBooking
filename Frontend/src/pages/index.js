// ========================================
// Pages - Application Routes
// ========================================
// Pages are routing wrappers that import from features.
//
// Structure:
// - pages/home/HomePage.jsx           → Home page route
// - pages/auth/LoginPage.jsx          → Login page
// - pages/admin/MovieManagementPage.jsx → Admin movie management
//
export { default as HomePage } from './home/HomePage';
export { default as MoviesPage } from './home/MoviesPage';
export { default as PaymentPage } from './customer/PaymentPage';
export { default as CheckoutPage } from './customer/CheckoutPage';
export { default as BookingApprovalPage } from './admin/BookingApprovalPage';
export { default as TicketDetail } from './customer/TicketDetail';
export { default as MyTickets } from './customer/MyTickets';
