namespace Melodee.Common.Models.OpenSubsonic;

/// <summary>
/// Artist record used in list and index operations.
/// </summary>
/// <param name="Id">Artist id</param>
/// <param name="Name">Artist name</param>
/// <param name="ArtistImageUrl">Artist image url</param>
/// <param name="CoverArt">A covertArt id.</param>
/// <param name="AlbumCount">Artist album count</param>
/// <param name="UserRating">Artist rating [1-5]</param>
/// <param name="AverageRating">Artist average rating [1.0-5.0]</param>
public record Artist
(
    string Id,
    string Name,
    string? ArtistImageUrl,
    string? CoverArt,
    int AlbumCount,
    int? UserRating,
    decimal? AverageRating
);

