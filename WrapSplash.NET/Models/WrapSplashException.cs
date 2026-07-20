namespace WrapSplash.Models;

/// <summary>
/// Exception thrown by the WrapSplash client when an API request fails.
/// </summary>
public class WrapSplashException : Exception
{
    /// <summary>The HTTP status code of the failed request, if available.</summary>
    public int? StatusCode { get; }

    /// <summary>The HTTP status text of the failed request, if available.</summary>
    public string? StatusText { get; }

    /// <summary>The original exception that caused this error, if available.</summary>
    public object? Cause { get; }

    public WrapSplashException(string message)
        : base(message)
    {
    }

    public WrapSplashException(string message, Exception innerException)
        : base(message, innerException)
    {
        if (innerException is System.Net.Http.HttpRequestException httpEx)
        {
            StatusCode = (int?)httpEx.StatusCode;
            StatusText = httpEx.StatusCode?.ToString();
        }
    }

    public WrapSplashException(string message, int? statusCode, string? statusText, object? cause = null)
        : base(message)
    {
        StatusCode = statusCode;
        StatusText = statusText;
        Cause = cause;
    }

    public WrapSplashException(string message, int? statusCode, string? statusText, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        StatusText = statusText;
    }
}
