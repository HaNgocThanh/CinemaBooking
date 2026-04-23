using CinemaBooking.API.Middlewares;
using CinemaBooking.Application.Services.Interfaces;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Infrastructure.BackgroundJobs;
using CinemaBooking.Infrastructure.Data;
using CinemaBooking.Infrastructure.Repositories;
using CinemaBooking.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============ JWT Configuration ============
var jwtSecret = builder.Configuration["Jwt:Secret"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Length < 32)
{
    throw new InvalidOperationException("JWT Secret trong appsettings phải có ít nhất 32 ký tự.");
}

var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,  // Không cho phép clock skew (ngay lập tức hết hạn)
        NameClaimType = "UserId",
        RoleClaimType = "Role"
    };

    // Lỗi xác thực chi tiết (cho development)
    if (builder.Environment.IsDevelopment())
    {
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync("{\"error\": \"Token expired\"}");
                }
                return Task.CompletedTask;
            }
        };
    }
});

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseOracle(connectionString);
});

// Register repositories
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<IShowtimeRepository, ShowtimeRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();

// Register application services
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMovieService, MovieService>();

// 🚀 Register background jobs
builder.Services.AddHostedService<SeatCleanupWorker>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 🔓 CORS Configuration - Allow ALL origins (development only!)
builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline
// 🔓 Use CORS middleware FIRST (before anything else)
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
    .WithHeaders("Content-Type", "Authorization")
);

// Register exception middleware AFTER CORS
app.UseExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 🚀 Skip HTTPS redirection in development to avoid CORS issues
// app.UseHttpsRedirection();

// ✅ Thêm Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

