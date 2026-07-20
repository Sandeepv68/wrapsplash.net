namespace WrapSplash.Configuration;

internal static class ApiEndpoints
{
    internal const string ApiLocation = "https://api.unsplash.com/";
    internal const string BearerTokenUrl = "https://unsplash.com/oauth/token";

    // Users
    internal const string CurrentUserProfile = "me";
    internal const string UpdateCurrentUserProfile = "me";
    internal const string UsersPublicProfile = "users/{0}";
    internal const string UsersPortfolio = "users/{0}/portfolio";
    internal const string UsersPhotos = "users/{0}/photos";
    internal const string UsersLikedPhotos = "users/{0}/likes";
    internal const string UsersCollections = "users/{0}/collections";
    internal const string UsersStatistics = "users/{0}/statistics";

    // Photos
    internal const string ListPhotos = "photos";
    internal const string ListCuratedPhotos = "photos/curated";
    internal const string GetAPhoto = "photos/{0}";
    internal const string GetARandomPhoto = "photos/random";
    internal const string GetAPhotoStatistics = "photos/{0}/statistics";
    internal const string GetAPhotoDownloadLink = "photos/{0}/download";
    internal const string UpdateAPhoto = "photos/{0}";
    internal const string LikeAPhoto = "photos/{0}/like";
    internal const string UnlikeAPhoto = "photos/{0}/like";

    // Search
    internal const string SearchPhotos = "search/photos";
    internal const string SearchCollections = "search/collections";
    internal const string SearchUsers = "search/users";

    // Stats
    internal const string StatsTotals = "stats/total";
    internal const string StatsMonth = "stats/month";

    // Collections
    internal const string ListCollections = "collections";
    internal const string ListFeaturedCollections = "collections/featured";
    internal const string ListCuratedCollections = "collections/curated";
    internal const string GetCollection = "collections/{0}";
    internal const string GetCuratedCollection = "collections/curated/{0}";
    internal const string GetCollectionPhotos = "collections/{0}/photos";
    internal const string GetCuratedCollectionPhotos = "collections/curated/{0}/photos";
    internal const string ListRelatedCollection = "collections/{0}/related";
    internal const string CreateNewCollection = "collections";
    internal const string UpdateExistingCollection = "collections/{0}";
    internal const string DeleteCollection = "collections/{0}";
    internal const string AddPhotoToCollection = "collections/{0}/add";
    internal const string RemovePhotoFromCollection = "collections/{0}/remove";
}
