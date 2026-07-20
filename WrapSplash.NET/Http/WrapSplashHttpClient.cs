using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Http;
using Polly;
using Polly.Extensions.Http;
using WrapSplash.Models;

namespace WrapSplash.Http;

internal class WrapSplashHttpClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly int _retries;
    private readonly int _retryDelayMs;

    public WrapSplashHttpClient(
        Dictionary<string, string> headers,
        int timeoutMs = 10000,
        int retries = 2,
        int retryDelayMs = 100)
    {
        _retries = retries;
        _retryDelayMs = retryDelayMs;

        var policy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retries,
                retryAttempt => TimeSpan.FromMilliseconds(retryDelayMs * retryAttempt));

        var handler = new PolicyHttpMessageHandler(policy)
        {
            InnerHandler = new HttpClientHandler()
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromMilliseconds(timeoutMs)
        };

        foreach (var header in headers)
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    internal WrapSplashHttpClient(
        HttpMessageHandler handler,
        Dictionary<string, string> headers,
        int timeoutMs = 10000)
    {
        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromMilliseconds(timeoutMs)
        };

        foreach (var header in headers)
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    internal static string ComputeHash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    internal static Dictionary<string, string> BuildHeaders(string? bearerToken, string? accessKey)
    {
        var headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" },
            { "X-Requested-With", "WrapSplash" }
        };

        if (!string.IsNullOrEmpty(bearerToken))
        {
            headers["Authorization"] = $"Bearer {bearerToken}";
            headers["X-WrapSplash-Header"] = ComputeHash(bearerToken);
        }
        else if (!string.IsNullOrEmpty(accessKey))
        {
            headers["Authorization"] = $"Client-ID {accessKey}";
            headers["X-WrapSplash-Header"] = ComputeHash(accessKey);
        }

        return headers;
    }

    internal static Dictionary<string, string> BuildQueryParameters(Dictionary<string, object?> parameters)
    {
        var clean = new Dictionary<string, string>();
        foreach (var kvp in parameters)
        {
            if (kvp.Value != null && !string.IsNullOrEmpty(kvp.Value.ToString()))
            {
                clean[kvp.Key] = kvp.Value.ToString()!;
            }
        }
        return clean;
    }

    internal static string BuildUrl(string baseUrl, string endpoint, Dictionary<string, string>? pathParams = null)
    {
        var url = baseUrl + endpoint;
        if (pathParams != null)
        {
            foreach (var param in pathParams)
            {
                url = url.Replace($"{{{param.Key}}}", Uri.EscapeDataString(param.Value));
            }
        }
        return url;
    }

    public async Task<JsonElement> MakeRequestAsync(
        string url,
        HttpMethod method,
        Dictionary<string, object?>? queryParameters = null,
        object? body = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(url))
            throw new WrapSplashException("URL required");

        HttpRequestMessage request;
        var effectiveMethod = method.Method.ToUpperInvariant();

        if (effectiveMethod == "GET" || effectiveMethod == "DELETE")
        {
            var requestUrl = url;
            if (queryParameters != null && queryParameters.Count > 0)
            {
                var cleanParams = BuildQueryParameters(queryParameters);
                var queryString = string.Join("&", cleanParams.Select(kvp =>
                    $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                requestUrl += (url.Contains('?') ? "&" : "?") + queryString;
            }
            request = new HttpRequestMessage(method, requestUrl);
        }
        else
        {
            request = new HttpRequestMessage(method, url);
            if (body != null)
            {
                request.Content = JsonContent.Create(body);
            }
            else if (queryParameters != null && queryParameters.Count > 0)
            {
                var cleanParams = BuildQueryParameters(queryParameters);
                request.Content = JsonContent.Create(cleanParams);
            }
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            var noContentResult = new Dictionary<string, object?>
            {
                { "status", 204 },
                { "statusText", "No Content" },
                { "message", "Content Deleted" }
            };
            return JsonSerializer.SerializeToElement(noContentResult);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            var rateLimitResult = new Dictionary<string, object?>
            {
                { "status", 403 },
                { "statusText", "Forbidden" },
                { "message", "Rate Limit Exceeded" }
            };
            return JsonSerializer.SerializeToElement(rateLimitResult);
        }

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            return default;
        }

        return JsonSerializer.Deserialize<JsonElement>(content);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
