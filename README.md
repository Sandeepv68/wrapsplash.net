# WrapSplash.NET

[![NuGet](https://img.shields.io/nuget/v/WrapSplash.svg)](https://www.nuget.org/packages/WrapSplash)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)

A .NET wrapper library for the [Unsplash API](https://unsplash.com/documentation). Port of the popular [wrapsplash](https://github.com/SandeepVattapparambil/wrapsplash) npm package.

## Features

- Full coverage of the Unsplash API endpoints
- Built-in retry policies with [Polly](https://github.com/App-vNext/Polly)
- OAuth 2.0 and Bearer Token authentication
- Configurable timeouts and retry behavior
- .NET 8.0+ with nullable reference types and XML documentation

## Installation

```bash
dotnet add package WrapSplash
```

## Quick Start

```csharp
using WrapSplash.Models;
using WrapSplash.Services;

// Bearer Token authentication (simplest)
var client = new WrapSplashClient(new WrapSplashOptions
{
    BearerToken = "your-bearer-token"
});

// Or use OAuth credentials
var client = new WrapSplashClient(new WrapSplashOptions
{
    AccessKey = "your-access-key",
    SecretKey = "your-secret-key",
    RedirectUri = "https://your-callback-url.com",
    Code = "authorization-code"
});

// Fetch a random photo
var photo = await client.GetARandomPhotoAsync();

// Search for photos
var results = await client.SearchAsync("nature", perPage: 10);

// Get a user's profile
var profile = await client.GetPublicProfileAsync("unsplash");

client.Dispose();
```

## API Reference

### Users

| Method | Description |
|--------|-------------|
| `GetCurrentUserProfileAsync()` | Get the authenticated user's profile |
| `UpdateCurrentUserProfileAsync(...)` | Update the current user's profile |
| `GetPublicProfileAsync(username)` | Get a user's public profile |
| `GetUserPortfolioAsync(username)` | Get a user's portfolio link |
| `GetUserPhotosAsync(username, ...)` | Get photos by a user |
| `GetUserLikedPhotosAsync(username, ...)` | Get photos liked by a user |
| `GetUserCollectionsAsync(username, ...)` | Get collections by a user |
| `GetUserStatisticsAsync(username, ...)` | Get a user's statistics |

### Photos

| Method | Description |
|--------|-------------|
| `ListPhotosAsync(...)` | List all photos |
| `ListCuratedPhotosAsync(...)` | List curated photos |
| `GetAPhotoAsync(id, ...)` | Get a photo by ID |
| `GetARandomPhotoAsync(...)` | Get a random photo |
| `GetPhotoStatisticsAsync(id, ...)` | Get photo statistics |
| `GetPhotoLinkAsync(id)` | Get a photo download link |
| `UpdatePhotoAsync(id, ...)` | Update photo location/EXIF data |
| `LikePhotoAsync(id)` | Like a photo |
| `UnlikePhotoAsync(id)` | Unlike a photo |

### Search

| Method | Description |
|--------|-------------|
| `SearchAsync(query, ...)` | Search photos |
| `SearchCollectionsAsync(query, ...)` | Search collections |
| `SearchUsersAsync(query, ...)` | Search users |

### Collections

| Method | Description |
|--------|-------------|
| `ListCollectionsAsync(...)` | List all collections |
| `ListFeaturedCollectionsAsync(...)` | List featured collections |
| `ListCuratedCollectionsAsync(...)` | List curated collections |
| `GetCollectionAsync(id)` | Get a collection by ID |
| `GetCollectionPhotosAsync(id, ...)` | Get photos in a collection |
| `ListRelatedCollectionsAsync(id)` | Get related collections |
| `CreateNewCollectionAsync(title, ...)` | Create a new collection |
| `UpdateExistingCollectionAsync(id, title, ...)` | Update a collection |
| `DeleteCollectionAsync(id)` | Delete a collection |
| `AddPhotoToCollectionAsync(collectionId, photoId)` | Add a photo to a collection |
| `RemovePhotoFromCollectionAsync(collectionId, photoId)` | Remove a photo from a collection |

### Stats

| Method | Description |
|--------|-------------|
| `GetStatsTotalsAsync()` | Get total platform statistics |
| `GetStatsMonthAsync()` | Get statistics for the past 30 days |

### OAuth

| Method | Description |
|--------|-------------|
| `GenerateBearerTokenAsync()` | Exchange an authorization code for a bearer token |

## Configuration

```csharp
var client = new WrapSplashClient(new WrapSplashOptions
{
    BearerToken = "your-token",
    Timeout = 15000,       // Request timeout in milliseconds (default: 10000)
    Retries = 3,           // Number of retry attempts (default: 2)
    RetryDelayMs = 200     // Delay between retries in milliseconds (default: 100)
});
```

## Error Handling

The client throws `WrapSplashException` on API errors with HTTP status code information:

```csharp
using WrapSplash.Models;

try
{
    var photo = await client.GetAPhotoAsync("invalid-id");
}
catch (WrapSplashException ex)
{
    Console.WriteLine($"Error {ex.StatusCode}: {ex.Message}");
}
```

## License

[MIT](LICENSE)
