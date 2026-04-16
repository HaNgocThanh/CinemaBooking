// ⚠️ MOVED: This file has been moved to CinemaBooking.Infrastructure.Services.AuthService
// Reason: AuthService uses ApplicationDbContext (Infrastructure Layer)
// 
// Application Layer should NOT directly depend on Infrastructure concrete classes.
// 
// Proper structure:
// - Interface (IAuthService) → Application.Services.Interfaces
// - Implementation (AuthService) → Infrastructure.Services
// 
// The service is registered in DI container in Program.cs:
//   builder.Services.AddScoped<IAuthService, AuthService>();

