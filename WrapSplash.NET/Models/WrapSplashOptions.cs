namespace WrapSplash.Models;

/// <summary>
/// Options for configuring the WrapSplash client.
/// </summary>
public class WrapSplashOptions
{
    /// <summary>Unsplash API access key.</summary>
    public string? AccessKey { get; set; }

    /// <summary>Unsplash API secret key.</summary>
    public string? SecretKey { get; set; }

    /// <summary>OAuth redirect URI.</summary>
    public string? RedirectUri { get; set; }

    /// <summary>OAuth authorization code.</summary>
    public string? Code { get; set; }

    /// <summary>Bearer token for authenticated requests. When set, AccessKey/SecretKey/RedirectUri/Code are not required.</summary>
    public string? BearerToken { get; set; }

    /// <summary>HTTP request timeout in milliseconds. Default is 10000 (10 seconds).</summary>
    public int Timeout { get; set; } = 10000;

    /// <summary>Number of retry attempts for failed requests. Default is 2.</summary>
    public int Retries { get; set; } = 2;

    /// <summary>Delay between retry attempts in milliseconds. Default is 100.</summary>
    public int RetryDelayMs { get; set; } = 100;
}
