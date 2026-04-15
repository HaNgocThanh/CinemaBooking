namespace CinemaBooking.Domain.Exceptions;

/// <summary>
/// Base exception class cho tất cả domain exceptions.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>Error code dùng để identify loại lỗi.</summary>
    public string Code { get; }

    /// <summary>User-friendly message.</summary>
    public string UserMessage { get; }

    protected DomainException(string code, string userMessage) 
        : base(userMessage)
    {
        Code = code;
        UserMessage = userMessage;
    }

    protected DomainException(string code, string userMessage, Exception innerException) 
        : base(userMessage, innerException)
    {
        Code = code;
        UserMessage = userMessage;
    }
}
