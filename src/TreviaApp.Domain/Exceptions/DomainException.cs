namespace TreviaApp.Domain.Exceptions;

/// <summary>
/// Represents the DomainException domain entity.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Gets Error Code.
    /// </summary>
    public string ErrorCode { get; }
    /// <summary>
    /// Gets Details.
    /// </summary>
    public string? Details { get; }
    /// <summary>
    /// Gets Validation Errors.
    /// </summary>
    public Dictionary<string, object?>? ValidationErrors { get; }

    /// <summary>
    /// Initializes a new instance of the DomainException class.
    /// </summary>
    public DomainException(string message, string errorCode, string? details = null)
        : base(message)
    {
        ErrorCode = errorCode;
        Details = details;
    }

    /// <summary>
    /// Initializes a new instance of the DomainException class.
    /// </summary>
    public DomainException(string message, string errorCode, Dictionary<string, object?> validationErrors)
        : base(message)
    {
        ErrorCode = errorCode;
        ValidationErrors = validationErrors;
    }

    /// <summary>
    /// Initializes a new instance of the DomainException class.
    /// </summary>
    public DomainException(string message, string errorCode, Exception innerException, string? details = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Details = details;
    }
}
