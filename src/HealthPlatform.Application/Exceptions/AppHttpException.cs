namespace HealthPlatform.Application.Exceptions;

/// <summary>
/// Maps to a specific HTTP status and machine-readable error code (API exception handler).
/// </summary>
public sealed class AppHttpException : Exception
{
    public AppHttpException(int statusCode, string errorCode, string message)
        : base(message)
    {
        if (statusCode is < 400 or > 599)
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode));
        }

        StatusCode = statusCode;
        ErrorCode = errorCode;
    }

    public int StatusCode { get; }
    public string ErrorCode { get; }
}
