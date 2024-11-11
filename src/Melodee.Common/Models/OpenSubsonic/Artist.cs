namespace Melodee.Common.Models.OpenSubsonic;

/// <summary>
///     An artist from ID3 tags.
/// </summary>
/// <param name="Id">The id of the artist</param>
/// <param name="Name">The artist name.</param>
/// <param name="CoverArt">A covertArt id.</param>
/// <param name="AlbumCount">Artist album count.</param>
/// <param name="UserRating">User rating.</param>
/// <param name="ArtistImageUrl">An url to an external image source.</param>
/// <param name="Starred">Date the artist was starred. [ISO 8601]</param>
/// <param name="MusicBrainzId">The artist MusicBrainzID.</param>
/// <param name="SortName">The artist sort name.</param>
/// <param name="Roles">The list of all roles this artist have in the library.</param>
public record Artist(
    string Id,
    string Name,
    string? CoverArt,
    int? AlbumCount,
    int? UserRating,
    string? ArtistImageUrl,
    string? Starred,
    string? MusicBrainzId,
    string? SortName,
    string[]? Roles
);
