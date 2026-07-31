namespace TreviaApp.Domain.Exceptions;

public class DomainException : Exception
{
    public string ErrorCode { get; }
    public string? Details { get; }
    public Dictionary<string, object?>? ValidationErrors { get; }

    public DomainException(string message, string errorCode, string? details = null)
        : base(message)
    {
        ErrorCode = errorCode;
        Details = details;
    }

    public DomainException(string message, string errorCode, Dictionary<string, object?> validationErrors)
        : base(message)
    {
        ErrorCode = errorCode;
        ValidationErrors = validationErrors;
    }

    public DomainException(string message, string errorCode, Exception innerException, string? details = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Details = details;
    }
}
