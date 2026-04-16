using System.Text.Json.Serialization;

namespace CinemaBooking.Application.DTOs.Common;

/// <summary>
/// Response wrapper cho API responses thành công.
/// Sử dụng trong tất cả các layers (Application, API, etc).
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của data.</typeparam>
public class ApiSuccessResponse<T>
{
    /// <summary>Luôn là true cho success response.</summary>
    [JsonPropertyName("success")]
    public bool Success => true;

    /// <summary>Dữ liệu được trả về (payload).</summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    /// <summary>Thông điệp mô tả kết quả (user-friendly).</summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>Timestamp của response (UTC).</summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Trace ID để track request trong logs.</summary>
    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }

    public ApiSuccessResponse() { }

    public ApiSuccessResponse(T data, string message = "Thành công", string? traceId = null)
    {
        Data = data;
        Message = message;
        TraceId = traceId;
    }
}
