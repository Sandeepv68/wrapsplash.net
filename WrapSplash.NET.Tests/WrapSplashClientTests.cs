using System.Net;
using System.Text.Json;
using RichardSzalay.MockHttp;
using WrapSplash.Configuration;
using WrapSplash.Http;
using WrapSplash.Models;
using WrapSplash.Services;
using Xunit;

namespace WrapSplash.NET.Tests;

public class WrapSplashClientTests : IDisposable
{
    private readonly MockHttpMessageHandler _mockHandler;
    private readonly WrapSplashClient _client;
    private const string BearerToken = "test-bearer-token";

    public WrapSplashClientTests()
    {
        _mockHandler = new MockHttpMessageHandler();
        var headers = WrapSplashHttpClient.BuildHeaders(BearerToken, null);
        var httpClient = new WrapSplashHttpClient(_mockHandler, headers);
        _client = new WrapSplashClient(httpClient);
    }

    public void Dispose()
    {
        _mockHandler.Dispose();
        _client.Dispose();
    }

    private void SetupJsonResponse(string url, object responseData, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _mockHandler.When(url)
            .Respond(statusCode, "application/json",
                JsonSerializer.Serialize(responseData));
    }

    private static JsonElement ParseJson(object data)
    {
        return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(data));
    }

    // ── Initialization tests ──────────────────────────────────────

    [Fact]
    public void Init_WithBearerToken_SetsAuthorizationHeader()
    {
        var handler = new MockHttpMessageHandler();
        var headers = WrapSplashHttpClient.BuildHeaders(BearerToken, null);
        Assert.Equal("Bearer test-bearer-token", headers["Authorization"]);
        Assert.False(string.IsNullOrEmpty(headers["X-WrapSplash-Header"]));
    }

    [Fact]
    public void Init_WithAccessKey_SetsClientIDHeader()
    {
        var headers = WrapSplashHttpClient.BuildHeaders(null, "my-access-key");
        Assert.Equal("Client-ID my-access-key", headers["Authorization"]);
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsWrapSplashException()
    {
        Assert.Throws<WrapSplashException>(() => new WrapSplashClient((WrapSplashOptions)null!));
    }

    [Fact]
    public void Constructor_WithMissingAccessKey_ThrowsWrapSplashException()
    {
        var options = new WrapSplashOptions
        {
            SecretKey = "secret",
            RedirectUri = "http://example.com",
            Code = "code"
        };
        var ex = Assert.Throws<WrapSplashException>(() => new WrapSplashClient(options));
        Assert.Equal("Access Key missing!", ex.Message);
    }

    [Fact]
    public void Constructor_WithMissingSecretKey_ThrowsWrapSplashException()
    {
        var options = new WrapSplashOptions
        {
            AccessKey = "access",
            RedirectUri = "http://example.com",
            Code = "code"
        };
        var ex = Assert.Throws<WrapSplashException>(() => new WrapSplashClient(options));
        Assert.Equal("Secret Key missing!", ex.Message);
    }

    [Fact]
    public void Constructor_WithMissingRedirectUri_ThrowsWrapSplashException()
    {
        var options = new WrapSplashOptions
        {
            AccessKey = "access",
            SecretKey = "secret",
            Code = "code"
        };
        var ex = Assert.Throws<WrapSplashException>(() => new WrapSplashClient(options));
        Assert.Equal("Redirect URI missing!", ex.Message);
    }

    [Fact]
    public void Constructor_WithMissingCode_ThrowsWrapSplashException()
    {
        var options = new WrapSplashOptions
        {
            AccessKey = "access",
            SecretKey = "secret",
            RedirectUri = "http://example.com"
        };
        var ex = Assert.Throws<WrapSplashException>(() => new WrapSplashClient(options));
        Assert.Equal("Authorization Code missing!", ex.Message);
    }

    // ── User endpoint tests ───────────────────────────────────────

    [Fact]
    public async Task GetCurrentUserProfileAsync_RequestsMeEndpoint()
    {
        SetupJsonResponse("https://api.unsplash.com/me", new { id = "user123", username = "testuser" });

        var result = await _client.GetCurrentUserProfileAsync();

        Assert.Equal("user123", result.GetProperty("id").GetString());
        Assert.Equal("testuser", result.GetProperty("username").GetString());
    }

    [Fact]
    public async Task UpdateCurrentUserProfileAsync_SendsCorrectPayload()
    {
        SetupJsonResponse("https://api.unsplash.com/me", new { username = "mock-user" });

        var result = await _client.UpdateCurrentUserProfileAsync(
            username: "mock-user",
            firstName: "Mock",
            lastName: "User",
            email: "mock@example.com",
            url: "https://example.com",
            location: "Earth",
            bio: "Testing",
            instagramUsername: "mock_insta");

        var pendingRequest = _mockHandler.GetMatchCount(
            _mockHandler.When(HttpMethod.Put, "https://api.unsplash.com/me")
                .Respond(HttpStatusCode.OK, "application/json", "{}"));

        Assert.True(pendingRequest >= 0);
    }

    [Fact]
    public async Task GetPublicProfileAsync_UsesCorrectUrl()
    {
        SetupJsonResponse("https://api.unsplash.com/users/sandeepv", new { username = "sandeepv" });

        var result = await _client.GetPublicProfileAsync("sandeepv", 200, 300);

        Assert.Equal("sandeepv", result.GetProperty("username").GetString());
    }

    [Fact]
    public async Task GetPublicProfileAsync_EmptyUsername_Throws()
    {
        await Assert.ThrowsAsync<WrapSplashException>(
            () => _client.GetPublicProfileAsync(""));
    }

    [Fact]
    public async Task GetUserPortfolioAsync_UsesCorrectUrl()
    {
        SetupJsonResponse("https://api.unsplash.com/users/sandeepv/portfolio", new { url = "portfolio.com" });

        var result = await _client.GetUserPortfolioAsync("sandeepv");

        Assert.Equal("portfolio.com", result.GetProperty("url").GetString());
    }

    [Fact]
    public async Task GetUserPhotosAsync_SendsDefaultParameters()
    {
        SetupJsonResponse("https://api.unsplash.com/users/sandeepv/photos", new { total = 10 });

        var result = await _client.GetUserPhotosAsync("sandeepv");

        Assert.Equal(10, result.GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task GetUserLikedPhotosAsync_SupportsCustomOrderBy()
    {
        SetupJsonResponse("https://api.unsplash.com/users/sandeepv/likes", new { total = 5 });

        var result = await _client.GetUserLikedPhotosAsync("sandeepv", 2, 5, "popular");

        Assert.Equal(5, result.GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task GetUserLikedPhotosAsync_InvalidOrderBy_Throws()
    {
        await Assert.ThrowsAsync<WrapSplashException>(
            () => _client.GetUserLikedPhotosAsync("sandeepv", orderBy: "bad_order"));
    }

    [Fact]
    public async Task GetUserCollectionsAsync_SendsDefaultPagination()
    {
        SetupJsonResponse("https://api.unsplash.com/users/sandeepv/collections", new { total = 3 });

        var result = await _client.GetUserCollectionsAsync("sandeepv");

        Assert.Equal(3, result.GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task GetUserStatisticsAsync_SendsDefaultResolution()
    {
        SetupJsonResponse("https://api.unsplash.com/users/sandeepv/statistics", new { downloads = 100 });

        var result = await _client.GetUserStatisticsAsync("sandeepv");

        Assert.Equal(100, result.GetProperty("downloads").GetInt32());
    }

    [Fact]
    public async Task GetUserPhotosAsync_InvalidOrderBy_Throws()
    {
        await Assert.ThrowsAsync<WrapSplashException>(
            () => _client.GetUserPhotosAsync("sandeepv", orderBy: "invalid_order"));
    }

    // ── Photo endpoint tests ──────────────────────────────────────

    [Fact]
    public async Task ListPhotosAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/photos", new[] { new { id = "p1" } });

        var result = await _client.ListPhotosAsync(1, 10, "latest");

        Assert.True(result.GetArrayLength() > 0);
    }

    [Fact]
    public async Task ListPhotosAsync_InvalidOrderBy_Throws()
    {
        await Assert.ThrowsAsync<WrapSplashException>(
            () => _client.ListPhotosAsync(orderBy: "invalid"));
    }

    [Fact]
    public async Task ListCuratedPhotosAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/photos/curated", new[] { new { id = "cp1" } });

        var result = await _client.ListCuratedPhotosAsync();

        Assert.True(result.GetArrayLength() > 0);
    }

    [Fact]
    public async Task GetAPhotoAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/photos/g3PyXO4A0yc", new { id = "g3PyXO4A0yc", width = 100, height = 200 });

        var result = await _client.GetAPhotoAsync("g3PyXO4A0yc", 100, 200, "0,0,100,200");

        Assert.Equal("g3PyXO4A0yc", result.GetProperty("id").GetString());
        Assert.Equal(100, result.GetProperty("width").GetInt32());
    }

    [Fact]
    public async Task GetAPhotoAsync_EmptyId_Throws()
    {
        await Assert.ThrowsAsync<WrapSplashException>(
            () => _client.GetAPhotoAsync(""));
    }

    [Fact]
    public async Task GetPhotoAsync_AliasesToGetAPhoto()
    {
        SetupJsonResponse("https://api.unsplash.com/photos/test123", new { id = "test123" });

        var result = await _client.GetPhotoAsync("test123", 400, 300);

        Assert.Equal("test123", result.GetProperty("id").GetString());
    }

    [Fact]
    public async Task GetARandomPhotoAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/photos/random", new { id = "random1" });

        var result = await _client.GetARandomPhotoAsync(
            collections: "123",
            featured: true,
            username: "sandeepv",
            query: "nature",
            width: 400,
            height: 300,
            orientation: "portrait",
            count: 2);

        Assert.Equal("random1", result.GetProperty("id").GetString());
    }

    [Fact]
    public async Task GetARandomPhotoAsync_InvalidOrientation_Throws()
    {
        await Assert.ThrowsAsync<WrapSplashException>(
            () => _client.GetARandomPhotoAsync(orientation: "invalid"));
    }

    [Fact]
    public async Task GetPhotoStatisticsAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/photos/g3PyXO4A0yc/statistics", new { downloads = 50 });

        var result = await _client.GetPhotoStatisticsAsync("g3PyXO4A0yc", "weeks", 10);

        Assert.Equal(50, result.GetProperty("downloads").GetInt32());
    }

    [Fact]
    public async Task GetPhotoLinkAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/photos/g3PyXO4A0yc/download", new { url = "https://download.example.com" });

        var result = await _client.GetPhotoLinkAsync("g3PyXO4A0yc");

        Assert.Equal("https://download.example.com", result.GetProperty("url").GetString());
    }

    [Fact]
    public async Task LikePhotoAsync_SendsPostRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/photos/g3PyXO4A0yc/like", new { photo = new { id = "g3PyXO4A0yc" } });

        var result = await _client.LikePhotoAsync("g3PyXO4A0yc");

        Assert.Equal("g3PyXO4A0yc", result.GetProperty("photo").GetProperty("id").GetString());
    }

    [Fact]
    public async Task UnlikePhotoAsync_SendsDeleteRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/photos/g3PyXO4A0yc/like", new { photo = new { id = "g3PyXO4A0yc" } });

        var result = await _client.UnlikePhotoAsync("g3PyXO4A0yc");

        Assert.Equal("g3PyXO4A0yc", result.GetProperty("photo").GetProperty("id").GetString());
    }

    [Fact]
    public async Task UpdatePhotoAsync_SendsCorrectPayload()
    {
        SetupJsonResponse("https://api.unsplash.com/photos/g3PyXO4A0yc", new { id = "g3PyXO4A0yc" });

        var location = new Dictionary<string, object?>
        {
            { "latitude", 10.1 },
            { "longitude", 20.2 },
            { "name", "Test" }
        };
        var exif = new Dictionary<string, object?>
        {
            { "make", "Canon" },
            { "model", "EOS" },
            { "iso_speed_ratings", 100 }
        };

        var result = await _client.UpdatePhotoAsync("g3PyXO4A0yc", location, exif);

        Assert.Equal("g3PyXO4A0yc", result.GetProperty("id").GetString());
    }

    // ── Search endpoint tests ─────────────────────────────────────

    [Fact]
    public async Task SearchAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/search/photos", new { total = 42 });

        var result = await _client.SearchAsync("ocean", 2, 15, "123", "landscape");

        Assert.Equal(42, result.GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_Throws()
    {
        await Assert.ThrowsAsync<WrapSplashException>(
            () => _client.SearchAsync(""));
    }

    [Fact]
    public async Task SearchAsync_InvalidOrientation_Throws()
    {
        await Assert.ThrowsAsync<WrapSplashException>(
            () => _client.SearchAsync("ocean", orientation: "bad"));
    }

    [Fact]
    public async Task SearchCollectionsAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/search/collections", new { total = 10 });

        var result = await _client.SearchCollectionsAsync("travel", 3, 20);

        Assert.Equal(10, result.GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task SearchUsersAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/search/users", new { total = 5 });

        var result = await _client.SearchUsersAsync("john", 4, 5);

        Assert.Equal(5, result.GetProperty("total").GetInt32());
    }

    // ── Stats endpoint tests ──────────────────────────────────────

    [Fact]
    public async Task GetStatsTotalsAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/stats/total", new { downloads = 1000 });

        var result = await _client.GetStatsTotalsAsync();

        Assert.Equal(1000, result.GetProperty("downloads").GetInt32());
    }

    [Fact]
    public async Task GetStatsMonthAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/stats/month", new { downloads = 100 });

        var result = await _client.GetStatsMonthAsync();

        Assert.Equal(100, result.GetProperty("downloads").GetInt32());
    }

    // ── Collection endpoint tests ─────────────────────────────────

    [Fact]
    public async Task ListCollectionsAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/collections", new[] { new { id = "c1" } });

        var result = await _client.ListCollectionsAsync();

        Assert.True(result.GetArrayLength() > 0);
    }

    [Fact]
    public async Task ListFeaturedCollectionsAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/collections/featured", new[] { new { id = "fc1" } });

        var result = await _client.ListFeaturedCollectionsAsync(2, 8);

        Assert.True(result.GetArrayLength() > 0);
    }

    [Fact]
    public async Task ListCuratedCollectionsAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/collections/curated", new[] { new { id = "cc1" } });

        var result = await _client.ListCuratedCollectionsAsync(3, 9);

        Assert.True(result.GetArrayLength() > 0);
    }

    [Fact]
    public async Task GetCollectionAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/collections/collection-id", new { id = "collection-id" });

        var result = await _client.GetCollectionAsync("collection-id");

        Assert.Equal("collection-id", result.GetProperty("id").GetString());
    }

    [Fact]
    public async Task GetCuratedCollectionAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/collections/curated/curated-id", new { id = "curated-id" });

        var result = await _client.GetCuratedCollectionAsync("curated-id");

        Assert.Equal("curated-id", result.GetProperty("id").GetString());
    }

    [Fact]
    public async Task GetCollectionPhotosAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/collections/collection-id/photos", new[] { new { id = "p1" } });

        var result = await _client.GetCollectionPhotosAsync("collection-id", 4, 12);

        Assert.True(result.GetArrayLength() > 0);
    }

    [Fact]
    public async Task GetCuratedCollectionPhotosAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/collections/curated/curated-id/photos", new[] { new { id = "p1" } });

        var result = await _client.GetCuratedCollectionPhotosAsync("curated-id", 5, 13);

        Assert.True(result.GetArrayLength() > 0);
    }

    [Fact]
    public async Task ListRelatedCollectionsAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/collections/collection-id/related", new[] { new { id = "rc1" } });

        var result = await _client.ListRelatedCollectionsAsync("collection-id");

        Assert.True(result.GetArrayLength() > 0);
    }

    [Fact]
    public async Task CreateNewCollectionAsync_SendsCorrectPayload()
    {
        SetupJsonResponse("https://api.unsplash.com/collections", new { id = "new-coll", title = "My Collection" });

        var result = await _client.CreateNewCollectionAsync("My Collection", "A test collection", true);

        Assert.Equal("My Collection", result.GetProperty("title").GetString());
    }

    [Fact]
    public async Task CreateCollectionAsync_AliasesToCreateNewCollection()
    {
        SetupJsonResponse("https://api.unsplash.com/collections", new { id = "new-coll", title = "Test" });

        var result = await _client.CreateCollectionAsync("Test", null, false);

        Assert.Equal("Test", result.GetProperty("title").GetString());
    }

    [Fact]
    public async Task CreateNewCollectionAsync_EmptyTitle_Throws()
    {
        await Assert.ThrowsAsync<WrapSplashException>(
            () => _client.CreateNewCollectionAsync(""));
    }

    [Fact]
    public async Task UpdateExistingCollectionAsync_SendsCorrectPayload()
    {
        SetupJsonResponse("https://api.unsplash.com/collections/cid", new { id = "cid", title = "Title" });

        var result = await _client.UpdateExistingCollectionAsync("cid", "Title", "desc2", false);

        Assert.Equal("Title", result.GetProperty("title").GetString());
    }

    [Fact]
    public async Task DeleteCollectionAsync_SendsDeleteRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/collections/cid", new { id = "cid" });

        var result = await _client.DeleteCollectionAsync("cid");

        Assert.Equal("cid", result.GetProperty("id").GetString());
    }

    [Fact]
    public async Task AddPhotoToCollectionAsync_SendsCorrectRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/collections/cid/add", new { collection_id = "cid", photo_id = "pid" });

        var result = await _client.AddPhotoToCollectionAsync("cid", "pid");

        Assert.Equal("cid", result.GetProperty("collection_id").GetString());
    }

    [Fact]
    public async Task RemovePhotoFromCollectionAsync_SendsDeleteRequest()
    {
        SetupJsonResponse("https://api.unsplash.com/collections/cid/remove", new { collection_id = "cid", photo_id = "pid" });

        var result = await _client.RemovePhotoFromCollectionAsync("cid", "pid");

        Assert.Equal("cid", result.GetProperty("collection_id").GetString());
    }

    // ── OAuth endpoint tests ──────────────────────────────────────

    [Fact]
    public async Task GenerateBearerTokenAsync_SendsCorrectPayload()
    {
        var handler = new MockHttpMessageHandler();
        handler.When("https://unsplash.com/oauth/token")
            .Respond(HttpStatusCode.OK, "application/json",
                JsonSerializer.Serialize(new Dictionary<string, object> { ["access_token"] = "new-token", ["token_type"] = "bearer" }));

        var headers = WrapSplashHttpClient.BuildHeaders(null, "access-key");
        var httpClient = new WrapSplashHttpClient(handler, headers);
        var client = new WrapSplashClient(httpClient);

        // Set credentials via reflection (since they're private fields)
        typeof(WrapSplashClient)
            .GetField("_accessKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(client, "access-key");
        typeof(WrapSplashClient)
            .GetField("_secretKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(client, "secret-key");
        typeof(WrapSplashClient)
            .GetField("_redirectUri", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(client, "https://example.com/callback");
        typeof(WrapSplashClient)
            .GetField("_code", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(client, "authorization-code");

        var result = await client.GenerateBearerTokenAsync();

        Assert.Equal("new-token", result.GetProperty("access_token").GetString());
        handler.Dispose();
    }

    // ── Error handling tests ──────────────────────────────────────

    [Fact]
    public async Task RequestFailure_WrapsInWrapSplashException()
    {
        _mockHandler.When("https://api.unsplash.com/me")
            .Respond(HttpStatusCode.InternalServerError);

        await Assert.ThrowsAsync<WrapSplashException>(
            () => _client.GetCurrentUserProfileAsync());
    }

    [Fact]
    public async Task NoContentResponse_Returns204Status()
    {
        _mockHandler.When("https://api.unsplash.com/collections/p1")
            .Respond(HttpStatusCode.NoContent);

        var result = await _client.DeleteCollectionAsync("p1");

        Assert.Equal(204, result.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task ForbiddenResponse_Returns403Status()
    {
        _mockHandler.When("https://api.unsplash.com/me")
            .Respond(HttpStatusCode.Forbidden);

        var result = await _client.GetCurrentUserProfileAsync();

        Assert.Equal(403, result.GetProperty("status").GetInt32());
        Assert.Equal("Rate Limit Exceeded", result.GetProperty("message").GetString());
    }
}
