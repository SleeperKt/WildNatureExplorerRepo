namespace WildNatureExplorer.Application.Common;

/// <summary>
/// Exception thrown when business logic validation fails (HTTP 422 Unprocessable Entity)
/// Use this for semantic validation errors, e.g., "Email already in use", "Species already exists", "Invalid coordinates"
/// </summary>
public class ValidationException : Exception
{
    public string? ErrorCode { get; set; }

    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
