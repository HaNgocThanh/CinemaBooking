using CinemaBooking.API.Responses;
using ApplicationException = CinemaBooking.Application.Exceptions.ApplicationException;
using CinemaBooking.Application.Exceptions;
using CinemaBooking.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace CinemaBooking.API.Middlewares;

/// <summary>
/// Global middleware để bắt và xử lý các exceptions từ toàn bộ application.
/// Chuyển đổi các custom exceptions thành HTTP responses với format chuẩn.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Xử lý exception và trả về HTTP response chuẩn.
    /// </summary>
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var traceId = context.TraceIdentifier;
        var timestamp = DateTime.UtcNow;

        // Log exception
        _logger.LogError(
            exception,
            "Unhandled exception occurred. TraceId: {TraceId}",
            traceId
        );

        // Xác định loại exception và HTTP status code
        var (httpStatusCode, errorCode, errorMessage, details) = exception switch
        {
            // ===== Domain Layer Exceptions =====
            SeatAlreadyLockedException saleEx => (
                HttpStatusCode.Conflict, // 409
                saleEx.Code,
                saleEx.UserMessage,
                new Dictionary<string, object> { { "reason", "seat_locked_by_another_user" } }
            ),

            // ===== Application Layer Exceptions =====
            InvalidPromoCodeException ipcEx => (
                HttpStatusCode.BadRequest, // 400
                ipcEx.Code,
                ipcEx.UserMessage,
                new Dictionary<string, object> { { "reason", "promo_code_not_found" } }
            ),

            PromoExpiredException peEx => (
                HttpStatusCode.BadRequest, // 400
                peEx.Code,
                peEx.UserMessage,
                new Dictionary<string, object> { { "reason", "promo_code_expired" } }
            ),

            InvalidShowtimeException iseEx => (
                HttpStatusCode.BadRequest, // 400
                iseEx.Code,
                iseEx.UserMessage,
                new Dictionary<string, object> { { "reason", "showtime_not_found" } }
            ),

            InvalidSeatsException iseaEx => (
                HttpStatusCode.BadRequest, // 400
                iseaEx.Code,
                iseaEx.UserMessage,
                new Dictionary<string, object> { { "reason", "invalid_seats_selection" } }
            ),

            // ===== Generic Application Exception =====
            ApplicationException appEx => (
                HttpStatusCode.BadRequest, // 400
                appEx.Code,
                appEx.UserMessage,
                new Dictionary<string, object> { { "exceptionType", "application_error" } }
            ),

            // ===== Default/Unhandled Exception =====
            _ => (
                HttpStatusCode.InternalServerError, // 500
                "INTERNAL_SERVER_ERROR",
                "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại sau.",
                new Dictionary<string, object> 
                { 
                    { "exceptionType", exception.GetType().Name },
                    { "developmentMessage", exception.Message }
                }
            )
        };

        context.Response.StatusCode = (int)httpStatusCode;

        var response = new ApiErrorResponse(
            errorCode,
            errorMessage,
            (int)httpStatusCode,
            details,
            traceId
        );

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);

        return context.Response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Extension method để đăng ký ExceptionMiddleware trong Startup.
/// </summary>
public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}
