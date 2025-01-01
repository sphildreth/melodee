using System.Text.Json.Serialization;
using Melodee.Common.Enums;

namespace Melodee.Common.Models.SearchEngines;

public sealed record AlbumSearchResult
{
    public int? Id { get; init; }

    public Guid? ApiKey { get; set; }

    public KeyValue KeyValue => new(UniqueId.ToString(), NameNormalized);

    public long UniqueId { get; init; }

    [JsonIgnore] public string? ReleaseDateParts { get; init; }

    public required AlbumType AlbumType { get; init; }

    public string AlbumTypeValue => AlbumType.ToString();

    public string? ReleaseDate { get; set; }

    public string[]? Genres { get; init; }

    public string? CoverUrl { get; init; }

    public required string Name { get; init; }

    public required string NameNormalized { get; init; }

    public required string SortName { get; init; }

    [JsonIgnore] public string? MusicBrainzIdRaw { get; init; }

    [JsonIgnore] public Guid? MusicBrainzResourceGroupId { get; init; }

    public Guid? MusicBrainzId { get; set; }

    public ArtistSearchResult? Artist { get; init; }
}
