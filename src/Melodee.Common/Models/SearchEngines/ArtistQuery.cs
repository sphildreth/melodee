using Melodee.Common.Utility;

namespace Melodee.Common.Models.SearchEngines;

public sealed record ArtistQuery : Query
{
    public KeyValue KeyValue => new(SafeParser.Hash(MusicBrainzIdValue?.ToString() ?? SpotifyId ?? NameNormalized).ToString(), NameNormalized);

    public string[]? AlbumNamesNormalized => AlbumKeyValues?.Where(x => x.Value != null).Select(x => x.Value!).ToArray();

    /// <summary>
    ///     Album information of Year and NormalizedName
    /// </summary>
    public KeyValue[]? AlbumKeyValues { get; init; }

    public Guid[]? AlbumMusicBrainzIds { get; set; }
}
