using NodaTime;

namespace Melodee.Common.Models.SearchEngines;

public sealed record AlbumSearchResult
{
    public long UniqueId { get; init; }

    public Instant? ReleaseDate { get; init; }

    public string[]? Genres { get; init; }

    public string? ThumbnailUrl { get; init; }

    public required string Name { get; init; }

    public required string NameNormalized { get; init; }

    public required string SortName { get; init; }

    public Guid? MusicBrainzId { get; init; }

    public ArtistSearchResult? Artist { get; init; }
}
