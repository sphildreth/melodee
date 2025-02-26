using Melodee.Common.Extensions;

namespace Melodee.Common.Models.SearchEngines;

public sealed record ImageSearchResult
{
    public required string FromPlugin { get; init; }

    public short Rank { get; init; }

    public bool DoDeleteExistingCoverImages { get; set; }

    public long UniqueId { get; init; }

    public int Width { get; init; }

    public int Height { get; init; }

    public required string ThumbnailUrl { get; init; }

    public required string MediaUrl { get; init; }

    public string UrlValue => ThumbnailUrl.Nullify() ?? MediaUrl;

    public string? Title { get; init; }

    public string? ItunesId { get; init; }

    public string? AmgId { get; init; }

    public string? DiscogsId { get; init; }

    public string? WikiDataId { get; init; }

    public Guid? MusicBrainzId { get; init; }

    public string? LastFmId { get; init; }

    public string? SpotifyId { get; init; }

    public string? ArtistItunesId { get; init; }

    /// <summary>
    ///     All Music Guide Artist Id
    /// </summary>
    public string? ArtistAmgId { get; init; }

    public string? ArtistDiscogsId { get; init; }

    public string? ArtistWikiDataId { get; init; }

    public Guid? ArtistMusicBrainzId { get; init; }

    public string? ArtistLastFmId { get; init; }

    public string? ArtistSpotifyId { get; init; }

    public DateTime? ReleaseDate { get; set; }
}
