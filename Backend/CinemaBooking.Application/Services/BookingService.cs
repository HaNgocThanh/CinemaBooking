// ⚠️ MOVED: This file has been moved to CinemaBooking.Infrastructure.Services.BookingService
// Reason: BookingService uses ApplicationDbContext (Infrastructure Layer)
// 
// Application Layer should NOT directly depend on Infrastructure concrete classes.
// 
// Proper structure:
// - Interface (IBookingService) → Application.Services.Interfaces
// - Implementation (BookingService) → Infrastructure.Services
// 
// The service is registered in DI container in Program.cs:
//   builder.Services.AddScoped<IBookingService, BookingService>();

