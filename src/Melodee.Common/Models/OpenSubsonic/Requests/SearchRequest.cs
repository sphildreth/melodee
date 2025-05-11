using Melodee.Common.Extensions;

namespace Melodee.Common.Models.OpenSubsonic.Requests;

/// <summary>
/// </summary>
/// <param name="Query">Search query.</param>
/// <param name="ArtistCount">Maximum number of artists to return.</param>
/// <param name="ArtistOffset">Search result offset for artists. Used for paging.</param>
/// <param name="AlbumCount">Maximum number of albums to return.</param>
/// <param name="AlbumOffset">Search result offset for albums. Used for paging.</param>
/// <param name="SongCount">Maximum number of songs to return.</param>
/// <param name="SongOffset">Search result offset for songs. Used for paging.</param>
/// <param name="MusicFolderId">Only return results from music folder with the given ID. </param>
public record SearchRequest(
    string Query,
    int? ArtistCount,
    int? ArtistOffset,
    int? AlbumCount,
    int? AlbumOffset,
    int? SongCount,
    int? SongOffset,
    string? MusicFolderId
)
{
    public string QueryValue => Query.Nullify() == null ? string.Empty : $"%{Query}%";

    public string QueryNormalizedValue =>
        Query.Nullify() == null ? string.Empty : $"%{QueryValue.ToNormalizedString()}%";

    public bool IsValid => QueryValue.Nullify() != null;
}
