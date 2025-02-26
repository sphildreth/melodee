using Melodee.Common.Extensions;

namespace Melodee.Common.Models.SearchEngines;

/// <summary>
///     A result for an artist search engine search
/// </summary>
public sealed record ArtistSearchResult
{
    /// <summary>
    /// If this is populated it is the PkId of the Artist record from the Melodee database.
    /// </summary>
    public int? Id { get; init; }

    public KeyValue KeyValue => new(UniqueId.ToString(), Name.ToNormalizedString() ?? Name);

    /// <summary>Artist name</summary>
    public required string Name { get; init; }

    /// <summary>Artist sort name</summary>
    public string? SortName { get; init; }

    /// <summary>Artist real name</summary>
    public string? RealName { get; init; }

    /// <summary>Name of Plugin who returned result.</summary>
    public required string FromPlugin { get; init; }

    public int? AlbumCount { get; init; }

    /// <summary>UniqueId for the result</summary>
    public long UniqueId { get; init; }

    /// <summary>Ranked, higher number the better quality of the result to the query.</summary>
    public int Rank { get; set; }

    /// <summary>Public URL to an image.</summary>
    public string? ImageUrl { get; init; }

    public string? ThumbnailUrl { get; init; }

    /// <summary>Any found MusicBrainzId</summary>
    public Guid? MusicBrainzId { get; init; }

    /// <summary>
    ///     All Music Guide Artist Id
    /// </summary>
    public string? AmgId { get; init; }

    public string? DiscogsId { get; init; }

    public string? ItunesId { get; init; }

    public string? WikiDataId { get; init; }

    public string? LastFmId { get; init; }

    public string? SpotifyId { get; init; }

    /// <summary>Any alternate names for Artist</summary>
    public string[]? AlternateNames { get; init; }

    /// <summary>Collection of releases for artist</summary>
    public AlbumSearchResult[]? Releases { get; set; }

    /// <summary>
    ///     Similar Artists
    /// </summary>
    public ArtistSearchInfoResult[]? SimilarArtists { get; init; }

    public override string ToString()
    {
        return $"{Name} ({UniqueId}) MusicBrainzId: [{MusicBrainzId}] SpotifyId: [{SpotifyId}]";
    }
}
