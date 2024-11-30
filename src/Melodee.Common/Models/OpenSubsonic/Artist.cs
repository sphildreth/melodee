namespace Melodee.Common.Models.OpenSubsonic;

/// <summary>
/// Artist record used in list and index operations.
/// </summary>
/// <param name="Id">Artist ApiKey</param>
/// <param name="Name">Artist name</param>
/// <param name="UserRating">Artist rating [1-5]</param>
/// <param name="AverageRating">Artist average rating [1.0-5.0]</param>
/// <param name="CoverArt">A covertArt id.</param>
/// <param name="AlbumCount">Artist album count</param>
/// <param name="UserStarred">Timestamp when user starred artist</param>
/// <param name="ArtistImageUrl">Artist image url</param>
public record Artist(
    string Id,
    string Name,
    int AlbumCount,
    int? UserRating,
    decimal AverageRating,
    string CoverArt,
    string? ArtistImageUrl = null,
    string? UserStarred = null,
    AlbumList2[]? Album = null
)
{
    /// <summary>
    /// Sometimes its "Starred" and sometimes its "UserStarred" in the responses.
    /// </summary>
    public string? Starred => UserStarred;
}

