using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using NodaTime;

namespace Melodee.Common.Models.SearchEngines;

public sealed record AlbumSearchResult
{
    public KeyValue KeyValue => new KeyValue(UniqueId.ToString(), NameNormalized);
    
    public long UniqueId { get; init; }

    [JsonIgnore]
    public string? ReleaseDateParts { get; init; }
    
    public string? ReleaseDate { get; set; }

    public string[]? Genres { get; init; }

    public string? ThumbnailUrl { get; init; }

    public required string Name { get; init; }

    public required string NameNormalized { get; init; }

    public required string SortName { get; init; }
    
    [JsonIgnore]
    public string? MusicBrainzIdRaw { get; init; }
  
    public Guid? MusicBrainzId { get; set; }

    public ArtistSearchResult? Artist { get; init; }
}
