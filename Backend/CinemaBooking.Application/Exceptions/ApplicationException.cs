namespace CinemaBooking.Application.Exceptions;

/// <summary>
/// Base exception class cho tất cả application-layer exceptions.
/// </summary>
public abstract class ApplicationException : Exception
{
    /// <summary>Error code dùng để identify loại lỗi.</summary>
    public string Code { get; }

    /// <summary>User-friendly message.</summary>
    public string UserMessage { get; }

    protected ApplicationException(string code, string userMessage)
        : base(userMessage)
    {
        Code = code;
        UserMessage = userMessage;
    }

    protected ApplicationException(string code, string userMessage, Exception innerException)
        : base(userMessage, innerException)
    {
        Code = code;
        UserMessage = userMessage;
    }
}
