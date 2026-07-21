<p align="center">
  <img src="logo/logo.png" alt="WrapSplash.NET" />
</p>

# WrapSplash.NET v1.0.0

[![NuGet](https://img.shields.io/nuget/v/WrapSplash.svg)](https://www.nuget.org/packages/WrapSplash)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
![Maintenance](https://img.shields.io/badge/Maintained%3F-yes-green.svg)
[![CI](https://github.com/SandeepVattapparambil/wrapsplash.net/actions/workflows/ci.yml/badge.svg)](https://github.com/SandeepVattapparambil/wrapsplash.net/actions/workflows/ci.yml)

WrapSplash.NET is a simple, async-first API wrapper for the popular [Unsplash](https://unsplash.com/) platform, built for .NET 8.0+. It provides full coverage of the Unsplash API with built-in retry policies, OAuth 2.0 and Bearer Token authentication, and a clean, ergonomic C# API surface.

Unsplash provides beautiful high quality free images and photos that you can download and use for any project without any attribution.

Before using the Unsplash API, you need to **register as a developer** and **read the API Guidelines.**

> **Note:** Every application must abide by the [API Guidelines](https://unsplash.com/documentation). Specifically, remember to hotlink images and trigger a download when appropriate.

This library is a .NET port of the popular [wrapsplash](https://github.com/SandeepVattapparambil/wrapsplash) npm package.

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [API Documentation](#api-documentation)
  - [Authorization](#authorization)
    - [Public Actions](#public-actions)
    - [User Authentication](#user-authentication)
    - [Generate Bearer Token](#generate-bearer-token)
  - [Users APIs](#users-apis)
    - [Get Current User's Profile](#get-current-users-profile)
    - [Update Current User's Profile](#update-current-users-profile)
    - [Get User's Public Profile](#get-users-public-profile)
    - [Get User's Portfolio Link](#get-users-portfolio-link)
    - [Get User's Photos](#get-users-photos)
    - [Get User Liked Photos](#get-user-liked-photos)
    - [Get User's Collections](#get-users-collections)
    - [Get User's Statistics](#get-users-statistics)
  - [Photos APIs](#photos-apis)
    - [List Photos](#list-photos)
    - [List Curated Photos](#list-curated-photos)
    - [Get a Photo by Id](#get-a-photo-by-id)
    - [Get a Random Photo](#get-a-random-photo)
    - [Get a Photo's Statistics](#get-a-photos-statistics)
    - [Get a Photo's Download Link](#get-a-photos-download-link)
    - [Update a Photo](#update-a-photo)
    - [Like a Photo](#like-a-photo)
    - [Unlike a Photo](#unlike-a-photo)
  - [Search APIs](#search-apis)
    - [Search Photos](#search-photos)
    - [Search Collections](#search-collections)
    - [Search Users](#search-users)
  - [Collections APIs](#collections-apis)
    - [List Collections](#list-collections)
    - [List Featured Collections](#list-featured-collections)
    - [List Curated Collections](#list-curated-collections)
    - [Get a Collection](#get-a-collection)
    - [Get a Curated Collection](#get-a-curated-collection)
    - [Get a Collection's Photos](#get-a-collections-photos)
    - [Get a Curated Collection's Photos](#get-a-curated-collections-photos)
    - [List a Collection's Related Collections](#list-a-collections-related-collections)
    - [Create a New Collection](#create-a-new-collection)
    - [Update an Existing Collection](#update-an-existing-collection)
    - [Delete a Collection](#delete-a-collection)
    - [Add a Photo to a Collection](#add-a-photo-to-a-collection)
    - [Remove a Photo from a Collection](#remove-a-photo-from-a-collection)
  - [Stats APIs](#stats-apis)
    - [Totals](#stats-totals)
    - [Month](#stats-month)
- [Error Handling](#error-handling)
- [Development](#development)
- [Tests](#tests)
- [License](#license)
- [Acknowledgements](#acknowledgements)

## Features

- Full coverage of the Unsplash API endpoints
- Built-in retry policies with [Polly](https://github.com/App-vNext/Polly)
- OAuth 2.0 and Bearer Token authentication
- Configurable timeouts and retry behavior
- .NET 8.0+ with nullable reference types and XML documentation
- Async/await throughout

## Installation

Install the package from NuGet:

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

// Fetch a random photo
var photo = await client.GetARandomPhotoAsync();

// Search for photos
var results = await client.SearchAsync("nature", perPage: 10);

// Get a user's profile
var profile = await client.GetPublicProfileAsync("unsplash");

client.Dispose();
```

### Using OAuth Credentials

```csharp
var client = new WrapSplashClient(new WrapSplashOptions
{
    AccessKey = "your-access-key",
    SecretKey = "your-secret-key",
    RedirectUri = "https://your-callback-url.com",
    Code = "authorization-code"
});

// Generate a bearer token for write access
var token = await client.GenerateBearerTokenAsync();

client.Dispose();
```

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

| Option | Type | Default | Description |
| --- | --- | --- | --- |
| `AccessKey` | `string` | `null` | Unsplash API access key |
| `SecretKey` | `string` | `null` | Unsplash API secret key |
| `RedirectUri` | `string` | `null` | OAuth redirect URI |
| `Code` | `string` | `null` | OAuth authorization code |
| `BearerToken` | `string` | `null` | Bearer token for authenticated requests. When set, `AccessKey`/`SecretKey`/`RedirectUri`/`Code` are not required |
| `Timeout` | `int` | `10000` | HTTP request timeout in milliseconds |
| `Retries` | `int` | `2` | Number of retry attempts for failed requests |
| `RetryDelayMs` | `int` | `100` | Delay between retry attempts in milliseconds |

## API Documentation

### Schema

The API base URL is `https://api.unsplash.com/`. Responses are sent as JSON.

When retrieving a list of objects, an abbreviated or summary version of that object is returned - i.e., a subset of its attributes. To get a full detailed version of that object, fetch it individually.

#### Error Messages

If an error occurs, whether on the server or client side, the error message(s) will be returned in an `errors` array:

```
422 Unprocessable Entity
```
```json
{
  "errors": ["Username is missing", "Password cannot be blank"]
}
```

### Authorization

#### Public Actions

Many actions can be performed without requiring authentication from a specific user. To authenticate requests in this way, pass your application's access key via the HTTP `Authorization` header:

```
Authorization: Client-ID YOUR_ACCESS_KEY
```

You can also pass this value using a `client_id` query parameter:

```
https://api.unsplash.com/photos/?client_id=YOUR_ACCESS_KEY
```

If only your access key is sent, attempting to perform non-public actions that require user authorization will result in a `401 Unauthorized` response.

#### User Authentication

The Unsplash API uses OAuth2 to authenticate and authorize Unsplash users. Unsplash's OAuth2 paths live at `https://unsplash.com/oauth/`.

Before using WrapSplash.NET:
- Developers are required to create a developer account from [Unsplash](https://unsplash.com/developers).
- Create a new App from Your Apps page.
- Get the `AccessKey`, `SecretKey`, `RedirectUri`, and `Code`.
- If you have a Bearer Token, you can pass it directly. Otherwise, you can generate one using `GenerateBearerTokenAsync()`.

> **Note:** The authorization code can be obtained by clicking the `Authorize` link next to `Callback URLs`. The authorization code is a one-time use code - you must generate it again if the action fails.

#### Generate Bearer Token

A method to generate a Bearer Token for `write_access` to private data. The constructor in this case requires `AccessKey`, `SecretKey`, `RedirectUri`, and `Code` to generate the bearer token.

> **Note:** No additional parameters are required for this method.

```csharp
var client = new WrapSplashClient(new WrapSplashOptions
{
    AccessKey = "your-access-key",
    SecretKey = "your-secret-key",
    RedirectUri = "https://your-callback-url.com",
    Code = "authorization-code"
});

var token = await client.GenerateBearerTokenAsync();
```

If successful, the response will contain:

```json
{
    "access_token": "YOUR_BEARER_TOKEN_HERE",
    "token_type": "bearer",
    "scope": "public read_photos write_photos",
    "created_at": 1436544465
}
```

Then use the bearer token:

```csharp
var client = new WrapSplashClient(new WrapSplashOptions
{
    BearerToken = "YOUR_BEARER_TOKEN_HERE"
});
```

### Users APIs

#### Get Current User's Profile

Get the current user's profile. Requires the `read_user` scope and a Bearer Token.

```
GET /me
```

```csharp
var profile = await client.GetCurrentUserProfileAsync();
```

#### Update Current User's Profile

Update the current user's profile. Requires the `write_user` scope and a Bearer Token.

```
PUT /me
```

| Parameter | Type | Description | Optional |
| --- | --- | --- | --- |
| `username` | `string` | The username of the current user | yes |
| `firstName` | `string` | The first name of the current user | yes |
| `lastName` | `string` | The last name of the current user | yes |
| `email` | `string` | The email of the current user | yes |
| `url` | `string` | The portfolio/personal URL of the current user | yes |
| `location` | `string` | The location of the current user | yes |
| `bio` | `string` | The bio of the current user | yes |
| `instagramUsername` | `string` | The Instagram username of the current user | yes |

```csharp
var result = await client.UpdateCurrentUserProfileAsync(
    username: "new-username",
    firstName: "John",
    lastName: "Doe",
    bio: "Photographer"
);
```

#### Get User's Public Profile

Retrieve public details on a given user.

```
GET /users/:username
```

| Parameter | Type | Description | Optional |
| --- | --- | --- | --- |
| `username` | `string` | The username of the particular user | no |
| `width` | `int` | Width of the profile picture in pixels | yes |
| `height` | `int` | Height of the profile picture in pixels | yes |

> **Note:** When optional `width` & `height` are specified, the profile image will be included in the `profile_image` object as `custom`.

```csharp
var profile = await client.GetPublicProfileAsync("unsplash", 600, 600);
```

#### Get User's Portfolio Link

Retrieve a single user's portfolio link.

```
GET /users/:username/portfolio
```

| Parameter | Type | Description | Optional |
| --- | --- | --- | --- |
| `username` | `string` | The username of the particular user | no |

```csharp
var portfolio = await client.GetUserPortfolioAsync("unsplash");
```

#### Get User's Photos

Get a list of photos uploaded by a particular user.

```
GET /users/:username/photos
```

| Parameter | Type | Description | Optional | Default |
| --- | --- | --- | --- | --- |
| `username` | `string` | The username of the particular user | no | |
| `page` | `int` | Page number to retrieve | yes | 1 |
| `perPage` | `int` | Number of items per page | yes | 10 |
| `stats` | `bool` | Show the stats for each user's photo | yes | false |
| `resolution` | `string` | The frequency of the stats | yes | `days` |
| `quantity` | `int` | The amount for each stat | yes | 30 |
| `orderBy` | `string` | How to sort the photos. Valid values: `latest`, `oldest`, `popular` | yes | `latest` |

```csharp
var photos = await client.GetUserPhotosAsync("unsplash", page: 1, perPage: 10, orderBy: "latest");
```

#### Get User Liked Photos

Get a list of photos liked by a user.

```
GET /users/:username/likes
```

| Parameter | Type | Description | Optional | Default |
| --- | --- | --- | --- | --- |
| `username` | `string` | The username of the particular user | no | |
| `page` | `int` | Page number to retrieve | yes | 1 |
| `perPage` | `int` | Number of items per page | yes | 10 |
| `orderBy` | `string` | How to sort the photos. Valid values: `latest`, `oldest`, `popular` | yes | `latest` |

```csharp
var liked = await client.GetUserLikedPhotosAsync("unsplash", page: 1, perPage: 10);
```

#### Get User's Collections

Get a list of collections created by the user.

```
GET /users/:username/collections
```

| Parameter | Type | Description | Optional | Default |
| --- | --- | --- | --- | --- |
| `username` | `string` | The username of the particular user | no | |
| `page` | `int` | Page number to retrieve | yes | 1 |
| `perPage` | `int` | Number of items per page | yes | 10 |

```csharp
var collections = await client.GetUserCollectionsAsync("unsplash", page: 1, perPage: 10);
```

#### Get User's Statistics

Get a user's account statistics.

```
GET /users/:username/statistics
```

| Parameter | Type | Description | Optional | Default |
| --- | --- | --- | --- | --- |
| `username` | `string` | The username of the particular user | no | |
| `resolution` | `string` | The frequency of the stats | yes | `days` |
| `quantity` | `int` | The amount for each stat | yes | 30 |

```csharp
var stats = await client.GetUserStatisticsAsync("unsplash", "days", 30);
```

### Photos APIs

#### List Photos

Get a single page from the list of all photos.

```
GET /photos
```

| Parameter | Type | Description | Optional | Default |
| --- | --- | --- | --- | --- |
| `page` | `int` | Page number to retrieve | yes | 1 |
| `perPage` | `int` | Number of items per page | yes | 10 |
| `orderBy` | `string` | How to sort the photos. Valid values: `latest`, `oldest`, `popular` | yes | `latest` |

```csharp
var photos = await client.ListPhotosAsync(page: 1, perPage: 10, orderBy: "latest");
```

#### List Curated Photos

Get a single page from the list of the curated photos.

```
GET /photos/curated
```

| Parameter | Type | Description | Optional | Default |
| --- | --- | --- | --- | --- |
| `page` | `int` | Page number to retrieve | yes | 1 |
| `perPage` | `int` | Number of items per page | yes | 10 |
| `orderBy` | `string` | How to sort the photos. Valid values: `latest`, `oldest`, `popular` | yes | `latest` |

```csharp
var photos = await client.ListCuratedPhotosAsync(page: 1, perPage: 10);
```

#### Get a Photo by Id

Retrieve a single photo.

```
GET /photos/:id
```

| Parameter | Type | Description | Optional |
| --- | --- | --- | --- |
| `id` | `string` | The photo's ID | no |
| `width` | `int` | Image width in pixels | yes |
| `height` | `int` | Image height in pixels | yes |
| `rect` | `string` | 4 comma-separated integers representing x, y, width, height of the cropped rectangle | yes |

> **Note:** Supplying the optional `width` or `height` parameters will result in the custom photo URL being added to the `urls` object.

```csharp
var photo = await client.GetAPhotoAsync("photo-id", 500, 500);

// Or use the alias
var photo = await client.GetPhotoAsync("photo-id", 500, 500);
```

#### Get a Random Photo

Retrieve a single random photo, given optional filters.

```
GET /photos/random
```

| Parameter | Type | Description | Optional | Default |
| --- | --- | --- | --- | --- |
| `collections` | `string` | The public collection ID(s) to filter selection. If multiple, comma-separated | yes | |
| `featured` | `bool` | Limit selection to featured photos | yes | false |
| `username` | `string` | Limit selection to a single user | yes | |
| `query` | `string` | Limit selection to photos matching a search term | yes | |
| `width` | `int` | The image width in pixels | yes | |
| `height` | `int` | The image height in pixels | yes | |
| `orientation` | `string` | Filter by photo orientation. Valid values: `landscape`, `portrait`, `squarish` | yes | `landscape` |
| `count` | `int` | The number of photos to return (max: 30) | yes | 1 |

> **Note:** You can't use the `collections` and `query` parameters in the same request.
> When supplying a `count` parameter - and only then - the response will be an array of photos, even if the value of `count` is 1.

```csharp
var photo = await client.GetARandomPhotoAsync();

// Or with filters
var photos = await client.GetARandomPhotoAsync(
    query: "nature",
    orientation: "landscape",
    count: 5
);
```

#### Get a Photo's Statistics

Retrieve total number of downloads, views and likes of a single photo, as well as the historical breakdown of these stats in a specific timeframe (default is 30 days).

```
GET /photos/:id/statistics
```

| Parameter | Type | Description | Optional | Default |
| --- | --- | --- | --- | --- |
| `id` | `string` | The photo's ID | no | |
| `resolution` | `string` | The frequency of the stats | yes | `days` |
| `quantity` | `int` | The amount for each stat (1-30) | yes | 30 |

> **Note:** Currently, the only resolution param supported is `days`. The quantity param can be any number between 1 and 30.

```csharp
var stats = await client.GetPhotoStatisticsAsync("photo-id", "days", 10);
```

#### Get a Photo's Download Link

Retrieve a single photo's download link. Preferably hit this endpoint if a photo is downloaded in your application for use (e.g., to be displayed on a blog article, to be shared on social media).

```
GET /photos/:id/download
```

| Parameter | Type | Description | Optional |
| --- | --- | --- | --- |
| `id` | `string` | The photo's ID | no |

> **Note:** This is different than the concept of a view, which is tracked automatically when you hotlink an image.

```csharp
var link = await client.GetPhotoLinkAsync("photo-id");
```

#### Update a Photo

Update a photo on behalf of the logged-in user. This requires the `write_photos` scope and a Bearer Token.

```
PUT /photos/:id
```

| Parameter | Type | Description | Optional |
| --- | --- | --- | --- |
| `id` | `string` | The photo's ID | no |
| `location` | `Dictionary<string, object?>` | The location object holding location data | yes |
| `exif` | `Dictionary<string, object?>` | The EXIF object holding EXIF data | yes |

##### Location Object

| Key | Description |
| --- | --- |
| `latitude` | The photo location's latitude |
| `longitude` | The photo location's longitude |
| `name` | The photo location's name |
| `city` | The photo location's city |
| `country` | The photo location's country |
| `confidential` | The photo location's confidentiality |

##### EXIF Object

| Key | Description |
| --- | --- |
| `make` | Camera's brand |
| `model` | Camera's model |
| `exposure_time` | Camera's exposure time |
| `aperture_value` | Camera's aperture value |
| `focal_length` | Camera's focal length |
| `iso_speed_ratings` | Camera's ISO |

```csharp
var location = new Dictionary<string, object?> { { "country", "India" } };
var exif = new Dictionary<string, object?> { { "make", "Canon" }, { "model", "EOS R5" } };

var result = await client.UpdatePhotoAsync("photo-id", location, exif);
```

#### Like a Photo

Like a photo on behalf of the logged-in user. This requires the `write_likes` scope.

```
POST /photos/:id/like
```

| Parameter | Type | Description | Optional |
| --- | --- | --- | --- |
| `id` | `string` | The photo's ID | no |

> **Note:** This action is idempotent; sending the POST request to a single photo multiple times has no additional effect.

```csharp
var result = await client.LikePhotoAsync("photo-id");
```

#### Unlike a Photo

Remove a user's like of a photo.

```
DELETE /photos/:id/like
```

| Parameter | Type | Description | Optional |
| --- | --- | --- | --- |
| `id` | `string` | The photo's ID | no |

> **Note:** This action is idempotent; sending the DELETE request to a single photo multiple times has no additional effect.

```csharp
var result = await client.UnlikePhotoAsync("photo-id");
```

### Search APIs

#### Search Photos

Get a single page of photo results for a particular query.

```
GET /search/photos
```

| Parameter | Type | Description | Optional | Default |
| --- | --- | --- | --- | --- |
| `query` | `string` | The search query | no | |
| `page` | `int` | Page number to retrieve | yes | 1 |
| `perPage` | `int` | Number of items per page | yes | 10 |
| `collections` | `string` | Collection ID(s) to narrow search. If multiple, comma-separated | yes | |
| `orientation` | `string` | Filter search results by photo orientation. Valid values: `landscape`, `portrait`, `squarish` | yes | |

```csharp
var results = await client.SearchAsync("cars", page: 1, perPage: 10, orientation: "landscape");
```

#### Search Collections

Get a single page of collection results for a query.

```
GET /search/collections
```

| Parameter | Type | Description | Optional | Default |
| --- | --- | --- | --- | --- |
| `query` | `string` | The search query | no | |
| `page` | `int` | Page number to retrieve | yes | 1 |
| `perPage` | `int` | Number of items per page | yes | 10 |

```csharp
var results = await client.SearchCollectionsAsync("cars", page: 1, perPage: 10);
```

#### Search Users

Get a single page of user results for a query.

```
GET /search/users
```

| Parameter | Type | Description | Optional | Default |
| --- | --- | --- | --- | --- |
| `query` | `string` | The search query | no | |
| `page` | `int` | Page number to retrieve | yes | 1 |
| `perPage` | `int` | Number of items per page | yes | 10 |

```csharp
var results = await client.SearchUsersAsync("sandeep", page: 1, perPage: 10);
```

### Collections APIs

#### Link Relations

Collections have the following link relations:

| Rel | Description |
| --- | --- |
| `self` | API location of this collection |
| `html` | HTML location of this collection |
| `photos` | API location of this collection's photos |
| `related` | API location of this collection's related collections (non-curated collections only) |
| `download` | Download location of this collection's zip file (curated collections only) |

#### List Collections

Get a single page from the list of all collections.

```
GET /collections
```

| Parameter | Type | Description | Optional | Default |
| --- | --- | --- | --- | --- |
| `page` | `int` | Page number to retrieve | yes | 1 |
| `perPage` | `int` | Number of items per page | yes | 10 |

```csharp
var collections = await client.ListCollectionsAsync(page: 1, perPage: 10);
```

#### List Featured Collections

Get a single page from the list of featured collections.

```
GET /collections/featured
```

| Parameter | Type | Description | Optional | Default |
| --- | --- | --- | --- | --- |
| `page` | `int` | Page number to retrieve | yes | 1 |
| `perPage` | `int` | Number of items per page | yes | 10 |

```csharp
var collections = await client.ListFeaturedCollectionsAsync(page: 1, perPage: 10);
```

#### List Curated Collections

Get a single page from the list of curated collections.

```
GET /collections/curated
```

| Parameter | Type | Description | Optional | Default |
| --- | --- | --- | --- | --- |
| `page` | `int` | Page number to retrieve | yes | 1 |
| `perPage` | `int` | Number of items per page | yes | 10 |

```csharp
var collections = await client.ListCuratedCollectionsAsync(page: 1, perPage: 10);
```

#### Get a Collection

Retrieve a single collection. To view a user's private collections, the `read_collections` scope is required.

```
GET /collections/:id
```

| Parameter | Type | Description | Optional |
| --- | --- | --- | --- |
| `id` | `string` | The collection ID | no |

```csharp
var collection = await client.GetCollectionAsync("collection-id");
```

#### Get a Curated Collection

Retrieve a single curated collection. To view a user's private collections, the `read_collections` scope is required.

```
GET /collections/curated/:id
```

| Parameter | Type | Description | Optional |
| --- | --- | --- | --- |
| `id` | `string` | The collection ID | no |

```csharp
var collection = await client.GetCuratedCollectionAsync("collection-id");
```

#### Get a Collection's Photos

Retrieve a collection's photos.

```
GET /collections/:id/photos
```

| Parameter | Type | Description | Optional | Default |
| --- | --- | --- | --- | --- |
| `id` | `string` | The collection ID | no | |
| `page` | `int` | Page number to retrieve | yes | 1 |
| `perPage` | `int` | Number of items per page | yes | 10 |

```csharp
var photos = await client.GetCollectionPhotosAsync("collection-id", page: 1, perPage: 10);
```

#### Get a Curated Collection's Photos

Retrieve a curated collection's photos.

```
GET /collections/curated/:id/photos
```

| Parameter | Type | Description | Optional | Default |
| --- | --- | --- | --- | --- |
| `id` | `string` | The collection ID | no | |
| `page` | `int` | Page number to retrieve | yes | 1 |
| `perPage` | `int` | Number of items per page | yes | 10 |

```csharp
var photos = await client.GetCuratedCollectionPhotosAsync("collection-id", page: 1, perPage: 10);
```

#### List a Collection's Related Collections

Retrieve a list of collections related to this one.

```
GET /collections/:id/related
```

| Parameter | Type | Description | Optional |
| --- | --- | --- | --- |
| `id` | `string` | The collection ID | no |

```csharp
var related = await client.ListRelatedCollectionsAsync("collection-id");
```

#### Create a New Collection

Create a new collection. This requires the `write_collections` scope.

```
POST /collections
```

| Parameter | Type | Description | Optional | Default |
| --- | --- | --- | --- | --- |
| `title` | `string` | The title of the collection | no | |
| `description` | `string` | The collection's description | yes | |
| `isPrivate` | `bool` | Whether to make this collection private | yes | false |

```csharp
var collection = await client.CreateNewCollectionAsync("My Collection", "A collection of nature photos");

// Or use the alias
var collection = await client.CreateCollectionAsync("My Collection");
```

#### Update an Existing Collection

Update an existing collection belonging to the logged-in user. This requires the `write_collections` scope.

```
PUT /collections/:id
```

| Parameter | Type | Description | Optional | Default |
| --- | --- | --- | --- | --- |
| `id` | `string` | The collection ID | no | |
| `title` | `string` | The title of the collection | no | |
| `description` | `string` | The collection's description | yes | |
| `isPrivate` | `bool` | Whether to make this collection private | yes | false |

```csharp
var result = await client.UpdateExistingCollectionAsync("collection-id", "Updated Title", "New description");

// Or use the alias
var result = await client.UpdateCollectionAsync("collection-id", "Updated Title");
```

#### Delete a Collection

Delete a collection belonging to the logged-in user. This requires the `write_collections` scope.

```
DELETE /collections/:id
```

| Parameter | Type | Description | Optional |
| --- | --- | --- | --- |
| `id` | `string` | The collection ID | no |

```csharp
await client.DeleteCollectionAsync("collection-id");
```

#### Add a Photo to a Collection

Add a photo to one of the logged-in user's collections. Requires the `write_collections` scope.

```
POST /collections/:collection_id/add
```

| Parameter | Type | Description | Optional |
| --- | --- | --- | --- |
| `collectionId` | `string` | The collection ID | no |
| `photoId` | `string` | The photo ID | no |

> **Note:** If the photo is already in the collection, this action has no effect.

```csharp
await client.AddPhotoToCollectionAsync("collection-id", "photo-id");
```

#### Remove a Photo from a Collection

Remove a photo from one of the logged-in user's collections. Requires the `write_collections` scope.

```
DELETE /collections/:collection_id/remove
```

| Parameter | Type | Description | Optional |
| --- | --- | --- | --- |
| `collectionId` | `string` | The collection ID | no |
| `photoId` | `string` | The photo ID | no |

```csharp
await client.RemovePhotoFromCollectionAsync("collection-id", "photo-id");
```

### Stats APIs

#### Stats Totals

Get a list of counts for all of Unsplash.

```
GET /stats/total
```

```csharp
var totals = await client.GetStatsTotalsAsync();
```

#### Stats Month

Get the overall Unsplash stats for the past 30 days.

```
GET /stats/month
```

```csharp
var monthStats = await client.GetStatsMonthAsync();
```

## Error Handling

The client throws `WrapSplashException` on API errors with HTTP status code information.

### WrapSplashException Properties

| Property | Type | Description |
| --- | --- | --- |
| `StatusCode` | `int?` | The HTTP status code of the failed request |
| `StatusText` | `string?` | The HTTP status text of the failed request |
| `Cause` | `object?` | The original exception that caused this error |
| `Message` | `string` | The error message |

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

### Special Responses

- **204 No Content:** Returns `{ status: 204, statusText: "No Content", message: "Content Deleted" }`
- **403 Forbidden:** Returns `{ status: 403, statusText: "Forbidden", message: "Rate Limit Exceeded" }`
- **Other errors:** Throws `WrapSplashException` with the HTTP status code and error details

## Development

```bash
# Clone the repository
git clone https://github.com/SandeepVattapparambil/wrapsplash.net.git
cd wrapsplash.net

# Restore dependencies
dotnet restore WrapSplash.slnx

# Build
dotnet build WrapSplash.slnx --configuration Release

# Run tests
dotnet test WrapSplash.slnx --configuration Release --verbosity normal
```

## Tests

WrapSplash.NET uses [xUnit](https://xunit.net/) as the testing framework with [Moq](https://github.com/moq/moq4) for mocking. Test files are available in the `WrapSplash.NET.Tests/` folder.

```bash
dotnet test WrapSplash.NET.Tests --verbosity normal
```

### Continuous Integration (CI)

This project uses GitHub Actions for continuous integration. On every push or pull request to the `master` branch, the CI pipeline:

1. Restores dependencies
2. Builds the solution
3. Runs all tests

On release creation, the CI pipeline additionally:
1. Packs the NuGet package
2. Publishes to NuGet.org via OIDC trusted publishers

## License

[MIT](LICENSE)

Copyright (c) 2026 - Sandeep Vattapparambil

## Acknowledgements

Thanks, and Kudos to team [Unsplash](https://unsplash.com/) for creating a wonderful platform for sharing beautiful high quality free images and photos.
