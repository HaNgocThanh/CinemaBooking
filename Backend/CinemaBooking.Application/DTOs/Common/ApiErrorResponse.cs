using System.Text.Json.Serialization;

namespace CinemaBooking.Application.DTOs.Common;

/// <summary>
/// Response wrapper cho API responses lỗi.
/// Sử dụng trong tất cả các layers (Application, API, etc).
/// </summary>
public class ApiErrorResponse
{
    /// <summary>Luôn là false cho error response.</summary>
    [JsonPropertyName("success")]
    public bool Success => false;

    /// <summary>Thông tin chi tiết về lỗi.</summary>
    [JsonPropertyName("error")]
    public ApiErrorDetails? Error { get; set; }

    /// <summary>Timestamp của response (UTC).</summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Trace ID để track request trong logs.</summary>
    [JsonPropertyName("traceId")]
    public string? TraceId { get; set; }

    public ApiErrorResponse() { }

    public ApiErrorResponse(
        string code,
        string message,
        int httpStatus,
        Dictionary<string, object>? details = null,
        string? traceId = null)
    {
        Error = new ApiErrorDetails
        {
            Code = code,
            Message = message,
            HttpStatus = httpStatus,
            Details = details ?? new()
        };
        TraceId = traceId;
    }
}

/// <summary>
/// Chi tiết thông tin lỗi.
/// </summary>
public class ApiErrorDetails
{
    /// <summary>Error code duy nhất (vd: "SEAT_ALREADY_LOCKED").</summary>
    [JsonPropertyName("code")]
    public required string Code { get; set; }

    /// <summary>Message mô tả lỗi (user-friendly).</summary>
    [JsonPropertyName("message")]
    public required string Message { get; set; }

    /// <summary>HTTP Status Code (400, 409, 422, 500, etc.).</summary>
    [JsonPropertyName("httpStatus")]
    public int HttpStatus { get; set; }

    /// <summary>Thông tin chi tiết bổ sung về lỗi (tuỳ chọn).</summary>
    [JsonPropertyName("details")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Details { get; set; }
}
