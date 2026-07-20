using System.Text.Json;
using WrapSplash.Configuration;
using WrapSplash.Http;
using WrapSplash.Models;

namespace WrapSplash.Services;

/// <summary>
/// A .NET client for the Unsplash API.
/// </summary>
public class WrapSplashClient : IDisposable
{
    private readonly WrapSplashHttpClient _httpClient;
    private string _accessKey = "";
    private string _secretKey = "";
    private string _redirectUri = "";
    private string _code = "";
    private readonly string _grantType = "authorization_code";

    private static readonly string[] AvailableOrders = ["latest", "oldest", "popular"];
    private static readonly string[] AvailableOrientations = ["landscape", "portrait", "squarish"];

    /// <summary>
    /// Initialize the client with API credentials or a bearer token.
    /// </summary>
    /// <param name="options">Configuration options for authentication.</param>
    /// <exception cref="WrapSplashException">Thrown when required options are missing.</exception>
    public WrapSplashClient(WrapSplashOptions options)
    {
        if (options == null)
            throw new WrapSplashException("Initialisation parameters required!");

        var bearerToken = options.BearerToken ?? "";

        if (!string.IsNullOrEmpty(bearerToken))
        {
            var headers = WrapSplashHttpClient.BuildHeaders(bearerToken, null);
            _httpClient = new WrapSplashHttpClient(headers, options.Timeout, options.Retries, options.RetryDelayMs);
            return;
        }

        if (string.IsNullOrEmpty(options.AccessKey))
            throw new WrapSplashException("Access Key missing!");
        if (string.IsNullOrEmpty(options.SecretKey))
            throw new WrapSplashException("Secret Key missing!");
        if (string.IsNullOrEmpty(options.RedirectUri))
            throw new WrapSplashException("Redirect URI missing!");
        if (string.IsNullOrEmpty(options.Code))
            throw new WrapSplashException("Authorization Code missing!");

        _accessKey = options.AccessKey;
        _secretKey = options.SecretKey;
        _redirectUri = options.RedirectUri;
        _code = options.Code;

        var authHeaders = WrapSplashHttpClient.BuildHeaders(null, _accessKey);
        _httpClient = new WrapSplashHttpClient(authHeaders, options.Timeout, options.Retries, options.RetryDelayMs);
    }

    internal WrapSplashClient(WrapSplashHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private void ValidateRequired(string? value, string fieldName)
    {
        if (string.IsNullOrEmpty(value))
        {
            var message = fieldName switch
            {
                "id" => "Parameter : id is required!",
                "query" => "Parameter : query is missing!",
                _ => $"Parameter : {fieldName} is required and cannot be empty!"
            };
            throw new WrapSplashException(message);
        }
    }

    private void ValidateSupportedValue(string? value, string[] allowedValues, string fieldName)
    {
        if (value != null && !allowedValues.Contains(value))
            throw new WrapSplashException($"Parameter : {fieldName} has an unsupported value!");
    }

    private static string BuildUrl(string endpoint, params string?[] pathParams)
    {
        var result = endpoint;
        for (int i = 0; i < pathParams.Length; i++)
        {
            result = result.Replace($"{{{i}}}", Uri.EscapeDataString(pathParams[i]!));
        }
        return ApiEndpoints.ApiLocation + result;
    }

    private static string BuildOAuthUrl(string endpoint, Dictionary<string, object?> queryParams)
    {
        var qs = string.Join("&", WrapSplashHttpClient.BuildQueryParameters(queryParams)
            .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        return endpoint + "?" + qs;
    }

    private static Dictionary<string, object?> CleanParams(params (string key, object? value)[] entries)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var (key, value) in entries)
        {
            if (value != null)
                dict[key] = value;
        }
        return dict;
    }

    private async Task<JsonElement> FetchAsync(
        string url,
        HttpMethod method,
        Dictionary<string, object?>? queryParams = null,
        object? body = null)
    {
        try
        {
            return await _httpClient.MakeRequestAsync(url, method, queryParams, body);
        }
        catch (WrapSplashException)
        {
            throw;
        }
        catch (Exception ex)
        {
            int? statusCode = null;
            string? statusText = null;

            if (ex is System.Net.Http.HttpRequestException httpEx)
            {
                statusCode = (int?)httpEx.StatusCode;
                statusText = httpEx.StatusCode?.ToString();
            }

            throw new WrapSplashException(
                ex.Message,
                statusCode,
                statusText,
                ex);
        }
    }

    // ── Users ──────────────────────────────────────────────────────

    /// <summary>Get the current authenticated user's profile.</summary>
    public Task<JsonElement> GetCurrentUserProfileAsync()
        => FetchAsync(ApiEndpoints.ApiLocation + ApiEndpoints.CurrentUserProfile, HttpMethod.Get);

    /// <summary>Update the current authenticated user's profile.</summary>
    public Task<JsonElement> UpdateCurrentUserProfileAsync(
        string? username = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? url = null,
        string? location = null,
        string? bio = null,
        string? instagramUsername = null)
    {
        var queryParams = CleanParams(
            ("username", username),
            ("first_name", firstName),
            ("last_name", lastName),
            ("email", email),
            ("url", url),
            ("location", location),
            ("bio", bio),
            ("instagram_username", instagramUsername));
        return FetchAsync(ApiEndpoints.ApiLocation + ApiEndpoints.UpdateCurrentUserProfile, HttpMethod.Put, queryParams);
    }

    /// <summary>Get a user's public profile.</summary>
    public Task<JsonElement> GetPublicProfileAsync(string username, int? width = null, int? height = null)
    {
        ValidateRequired(username, "username");
        var url = BuildUrl(ApiEndpoints.UsersPublicProfile, username);
        return FetchAsync(url, HttpMethod.Get, CleanParams(("w", width), ("h", height)));
    }

    /// <summary>Get a user's portfolio.</summary>
    public Task<JsonElement> GetUserPortfolioAsync(string username)
    {
        ValidateRequired(username, "username");
        return FetchAsync(BuildUrl(ApiEndpoints.UsersPortfolio, username), HttpMethod.Get);
    }

    /// <summary>Get a user's photos.</summary>
    public Task<JsonElement> GetUserPhotosAsync(
        string username,
        int? page = null,
        int? perPage = null,
        bool? stats = null,
        string? resolution = null,
        int? quantity = null,
        string? orderBy = null)
    {
        ValidateRequired(username, "username");
        ValidateSupportedValue(orderBy, AvailableOrders, "order_by");
        return FetchAsync(
            BuildUrl(ApiEndpoints.UsersPhotos, username),
            HttpMethod.Get,
            CleanParams(
                ("page", page ?? 1),
                ("per_page", perPage ?? 10),
                ("order_by", orderBy ?? "latest"),
                ("stats", stats ?? false),
                ("resolution", resolution ?? "days"),
                ("quantity", quantity ?? 30)));
    }

    /// <summary>Get a user's liked photos.</summary>
    public Task<JsonElement> GetUserLikedPhotosAsync(
        string username,
        int? page = null,
        int? perPage = null,
        string? orderBy = null)
    {
        ValidateRequired(username, "username");
        ValidateSupportedValue(orderBy, AvailableOrders, "order_by");
        return FetchAsync(
            BuildUrl(ApiEndpoints.UsersLikedPhotos, username),
            HttpMethod.Get,
            CleanParams(
                ("page", page ?? 1),
                ("per_page", perPage ?? 10),
                ("order_by", orderBy ?? "latest")));
    }

    /// <summary>Get a user's collections.</summary>
    public Task<JsonElement> GetUserCollectionsAsync(string username, int? page = null, int? perPage = null)
    {
        ValidateRequired(username, "username");
        return FetchAsync(
            BuildUrl(ApiEndpoints.UsersCollections, username),
            HttpMethod.Get,
            CleanParams(("page", page ?? 1), ("per_page", perPage ?? 10)));
    }

    /// <summary>Get a user's statistics.</summary>
    public Task<JsonElement> GetUserStatisticsAsync(string username, string? resolution = null, int? quantity = null)
    {
        ValidateRequired(username, "username");
        return FetchAsync(
            BuildUrl(ApiEndpoints.UsersStatistics, username),
            HttpMethod.Get,
            CleanParams(("resolution", resolution ?? "days"), ("quantity", quantity ?? 30)));
    }

    // ── Photos ─────────────────────────────────────────────────────

    /// <summary>List photos.</summary>
    public Task<JsonElement> ListPhotosAsync(int? page = null, int? perPage = null, string? orderBy = null)
    {
        ValidateSupportedValue(orderBy, AvailableOrders, "order_by");
        return FetchAsync(
            ApiEndpoints.ApiLocation + ApiEndpoints.ListPhotos,
            HttpMethod.Get,
            CleanParams(
                ("page", page ?? 1),
                ("per_page", perPage ?? 10),
                ("order_by", orderBy ?? "latest")));
    }

    /// <summary>List curated photos.</summary>
    public Task<JsonElement> ListCuratedPhotosAsync(int? page = null, int? perPage = null, string? orderBy = null)
    {
        ValidateSupportedValue(orderBy, AvailableOrders, "order_by");
        return FetchAsync(
            ApiEndpoints.ApiLocation + ApiEndpoints.ListCuratedPhotos,
            HttpMethod.Get,
            CleanParams(
                ("page", page ?? 1),
                ("per_page", perPage ?? 10),
                ("order_by", orderBy ?? "latest")));
    }

    /// <summary>Get a photo by ID.</summary>
    public Task<JsonElement> GetAPhotoAsync(string id, int? width = null, int? height = null, string? rect = null)
    {
        ValidateRequired(id, "id");
        return FetchAsync(
            BuildUrl(ApiEndpoints.GetAPhoto, id),
            HttpMethod.Get,
            CleanParams(("w", width), ("h", height), ("rect", rect)));
    }

    /// <summary>Get a photo by ID (alias).</summary>
    public Task<JsonElement> GetPhotoAsync(string id, int? width = null, int? height = null, string? rect = null)
        => GetAPhotoAsync(id, width, height, rect);

    /// <summary>Get a random photo.</summary>
    public Task<JsonElement> GetARandomPhotoAsync(
        string? collections = null,
        bool? featured = null,
        string? username = null,
        string? query = null,
        int? width = null,
        int? height = null,
        string? orientation = null,
        int? count = null)
    {
        ValidateSupportedValue(orientation, AvailableOrientations, "orientation");
        return FetchAsync(
            ApiEndpoints.ApiLocation + ApiEndpoints.GetARandomPhoto,
            HttpMethod.Get,
            CleanParams(
                ("collections", collections),
                ("featured", featured ?? false),
                ("username", username),
                ("query", query),
                ("width", width),
                ("height", height),
                ("orientation", orientation ?? "landscape"),
                ("count", count ?? 1)));
    }

    /// <summary>Get a random photo (alias).</summary>
    public Task<JsonElement> GetRandomPhotoAsync(
        string? collections = null,
        bool? featured = null,
        string? username = null,
        string? query = null,
        int? width = null,
        int? height = null,
        string? orientation = null,
        int? count = null)
        => GetARandomPhotoAsync(collections, featured, username, query, width, height, orientation, count);

    /// <summary>Get photo statistics.</summary>
    public Task<JsonElement> GetPhotoStatisticsAsync(string id, string? resolution = null, int? quantity = null)
    {
        ValidateRequired(id, "id");
        return FetchAsync(
            BuildUrl(ApiEndpoints.GetAPhotoStatistics, id),
            HttpMethod.Get,
            CleanParams(("resolution", resolution ?? "days"), ("quantity", quantity ?? 30)));
    }

    /// <summary>Get a photo download link.</summary>
    public Task<JsonElement> GetPhotoLinkAsync(string id)
    {
        ValidateRequired(id, "id");
        return FetchAsync(BuildUrl(ApiEndpoints.GetAPhotoDownloadLink, id), HttpMethod.Get);
    }

    /// <summary>Update a photo's location and EXIF data.</summary>
    public Task<JsonElement> UpdatePhotoAsync(
        string id,
        Dictionary<string, object?>? location = null,
        Dictionary<string, object?>? exif = null)
    {
        ValidateRequired(id, "id");

        var queryParams = new Dictionary<string, object?>();
        if (location != null)
        {
            if (location.TryGetValue("latitude", out var lat)) queryParams["location[latitude]"] = lat;
            if (location.TryGetValue("longitude", out var lon)) queryParams["location[longitude]"] = lon;
            if (location.TryGetValue("name", out var name)) queryParams["location[name]"] = name;
            if (location.TryGetValue("city", out var city)) queryParams["location[city]"] = city;
            if (location.TryGetValue("country", out var country)) queryParams["location[country]"] = country;
            if (location.TryGetValue("confidential", out var conf)) queryParams["location[confidential]"] = conf;
        }
        if (exif != null)
        {
            if (exif.TryGetValue("make", out var make)) queryParams["exif[make]"] = make;
            if (exif.TryGetValue("model", out var model)) queryParams["exif[model]"] = model;
            if (exif.TryGetValue("exposure_time", out var et)) queryParams["exif[exposure_time]"] = et;
            if (exif.TryGetValue("aperture_value", out var av)) queryParams["exif[aperture_value]"] = av;
            if (exif.TryGetValue("focal_length", out var fl)) queryParams["exif[focal_length]"] = fl;
            if (exif.TryGetValue("iso_speed_ratings", out var iso)) queryParams["exif[iso_speed_ratings]"] = iso;
        }

        return FetchAsync(BuildUrl(ApiEndpoints.UpdateAPhoto, id), HttpMethod.Put, queryParams);
    }

    /// <summary>Like a photo.</summary>
    public Task<JsonElement> LikePhotoAsync(string id)
    {
        ValidateRequired(id, "id");
        return FetchAsync(BuildUrl(ApiEndpoints.LikeAPhoto, id), HttpMethod.Post);
    }

    /// <summary>Unlike a photo.</summary>
    public Task<JsonElement> UnlikePhotoAsync(string id)
    {
        ValidateRequired(id, "id");
        return FetchAsync(BuildUrl(ApiEndpoints.UnlikeAPhoto, id), HttpMethod.Delete);
    }

    // ── OAuth ──────────────────────────────────────────────────────

    /// <summary>Exchange the authorization code for a bearer token.</summary>
    public Task<JsonElement> GenerateBearerTokenAsync()
    {
        ValidateRequired(_accessKey, "access_key");
        ValidateRequired(_secretKey, "secret_key");
        ValidateRequired(_redirectUri, "redirect_uri");
        ValidateRequired(_code, "code");

        var url = BuildOAuthUrl(ApiEndpoints.BearerTokenUrl, CleanParams(
            ("client_id", _accessKey),
            ("client_secret", _secretKey),
            ("redirect_uri", _redirectUri),
            ("code", _code),
            ("grant_type", _grantType)));

        return FetchAsync(url, HttpMethod.Post);
    }

    // ── Search ─────────────────────────────────────────────────────

    /// <summary>Search photos.</summary>
    public Task<JsonElement> SearchAsync(
        string query,
        int? page = null,
        int? perPage = null,
        string? collections = null,
        string? orientation = null)
    {
        ValidateRequired(query, "query");
        ValidateSupportedValue(orientation, AvailableOrientations, "orientation");
        return FetchAsync(
            ApiEndpoints.ApiLocation + ApiEndpoints.SearchPhotos,
            HttpMethod.Get,
            CleanParams(
                ("query", query),
                ("page", page ?? 1),
                ("per_page", perPage ?? 10),
                ("collections", collections),
                ("orientation", orientation)));
    }

    /// <summary>Search collections.</summary>
    public Task<JsonElement> SearchCollectionsAsync(string query, int? page = null, int? perPage = null)
    {
        ValidateRequired(query, "query");
        return FetchAsync(
            ApiEndpoints.ApiLocation + ApiEndpoints.SearchCollections,
            HttpMethod.Get,
            CleanParams(
                ("query", query),
                ("page", page ?? 1),
                ("per_page", perPage ?? 10)));
    }

    /// <summary>Search users.</summary>
    public Task<JsonElement> SearchUsersAsync(string query, int? page = null, int? perPage = null)
    {
        ValidateRequired(query, "query");
        return FetchAsync(
            ApiEndpoints.ApiLocation + ApiEndpoints.SearchUsers,
            HttpMethod.Get,
            CleanParams(
                ("query", query),
                ("page", page ?? 1),
                ("per_page", perPage ?? 10)));
    }

    // ── Stats ──────────────────────────────────────────────────────

    /// <summary>Get total stats.</summary>
    public Task<JsonElement> GetStatsTotalsAsync()
        => FetchAsync(ApiEndpoints.ApiLocation + ApiEndpoints.StatsTotals, HttpMethod.Get);

    /// <summary>Get monthly stats.</summary>
    public Task<JsonElement> GetStatsMonthAsync()
        => FetchAsync(ApiEndpoints.ApiLocation + ApiEndpoints.StatsMonth, HttpMethod.Get);

    // ── Collections ────────────────────────────────────────────────

    /// <summary>List collections.</summary>
    public Task<JsonElement> ListCollectionsAsync(int? page = null, int? perPage = null)
        => FetchAsync(
            ApiEndpoints.ApiLocation + ApiEndpoints.ListCollections,
            HttpMethod.Get,
            CleanParams(("page", page ?? 1), ("per_page", perPage ?? 10)));

    /// <summary>List featured collections.</summary>
    public Task<JsonElement> ListFeaturedCollectionsAsync(int? page = null, int? perPage = null)
        => FetchAsync(
            ApiEndpoints.ApiLocation + ApiEndpoints.ListFeaturedCollections,
            HttpMethod.Get,
            CleanParams(("page", page ?? 1), ("per_page", perPage ?? 10)));

    /// <summary>List curated collections.</summary>
    public Task<JsonElement> ListCuratedCollectionsAsync(int? page = null, int? perPage = null)
        => FetchAsync(
            ApiEndpoints.ApiLocation + ApiEndpoints.ListCuratedCollections,
            HttpMethod.Get,
            CleanParams(("page", page ?? 1), ("per_page", perPage ?? 10)));

    /// <summary>Get a collection by ID.</summary>
    public Task<JsonElement> GetCollectionAsync(string id)
    {
        ValidateRequired(id, "id");
        return FetchAsync(BuildUrl(ApiEndpoints.GetCollection, id), HttpMethod.Get);
    }

    /// <summary>Get a curated collection by ID.</summary>
    public Task<JsonElement> GetCuratedCollectionAsync(string id)
    {
        ValidateRequired(id, "id");
        return FetchAsync(BuildUrl(ApiEndpoints.GetCuratedCollection, id), HttpMethod.Get);
    }

    /// <summary>Get photos in a collection.</summary>
    public Task<JsonElement> GetCollectionPhotosAsync(string id, int? page = null, int? perPage = null)
    {
        ValidateRequired(id, "id");
        return FetchAsync(
            BuildUrl(ApiEndpoints.GetCollectionPhotos, id),
            HttpMethod.Get,
            CleanParams(("page", page ?? 1), ("per_page", perPage ?? 10)));
    }

    /// <summary>Get photos in a curated collection.</summary>
    public Task<JsonElement> GetCuratedCollectionPhotosAsync(string id, int? page = null, int? perPage = null)
    {
        ValidateRequired(id, "id");
        return FetchAsync(
            BuildUrl(ApiEndpoints.GetCuratedCollectionPhotos, id),
            HttpMethod.Get,
            CleanParams(("page", page ?? 1), ("per_page", perPage ?? 10)));
    }

    /// <summary>List related collections.</summary>
    public Task<JsonElement> ListRelatedCollectionsAsync(string id)
    {
        ValidateRequired(id, "id");
        return FetchAsync(BuildUrl(ApiEndpoints.ListRelatedCollection, id), HttpMethod.Get);
    }

    /// <summary>Create a new collection.</summary>
    public Task<JsonElement> CreateNewCollectionAsync(string title, string? description = null, bool isPrivate = false)
    {
        ValidateRequired(title, "title");
        return FetchAsync(
            ApiEndpoints.ApiLocation + ApiEndpoints.CreateNewCollection,
            HttpMethod.Post,
            CleanParams(("title", title), ("description", description), ("@private", isPrivate)));
    }

    /// <summary>Create a collection (alias).</summary>
    public Task<JsonElement> CreateCollectionAsync(string title, string? description = null, bool isPrivate = false)
        => CreateNewCollectionAsync(title, description, isPrivate);

    /// <summary>Update an existing collection.</summary>
    public Task<JsonElement> UpdateExistingCollectionAsync(string id, string title, string? description = null, bool isPrivate = false)
    {
        ValidateRequired(id, "id");
        ValidateRequired(title, "title");
        return FetchAsync(
            BuildUrl(ApiEndpoints.UpdateExistingCollection, id),
            HttpMethod.Put,
            CleanParams(("title", title), ("description", description), ("@private", isPrivate)));
    }

    /// <summary>Update a collection (alias).</summary>
    public Task<JsonElement> UpdateCollectionAsync(string id, string title, string? description = null, bool isPrivate = false)
        => UpdateExistingCollectionAsync(id, title, description, isPrivate);

    /// <summary>Delete a collection.</summary>
    public Task<JsonElement> DeleteCollectionAsync(string id)
    {
        ValidateRequired(id, "id");
        return FetchAsync(BuildUrl(ApiEndpoints.DeleteCollection, id), HttpMethod.Delete);
    }

    /// <summary>Add a photo to a collection.</summary>
    public Task<JsonElement> AddPhotoToCollectionAsync(string collectionId, string photoId)
    {
        ValidateRequired(collectionId, "collection_id");
        ValidateRequired(photoId, "photo_id");
        return FetchAsync(
            BuildUrl(ApiEndpoints.AddPhotoToCollection, collectionId),
            HttpMethod.Post,
            CleanParams(("photo_id", photoId)));
    }

    /// <summary>Remove a photo from a collection.</summary>
    public Task<JsonElement> RemovePhotoFromCollectionAsync(string collectionId, string photoId)
    {
        ValidateRequired(collectionId, "collection_id");
        ValidateRequired(photoId, "photo_id");
        return FetchAsync(
            BuildUrl(ApiEndpoints.RemovePhotoFromCollection, collectionId),
            HttpMethod.Delete,
            CleanParams(("photo_id", photoId)));
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
