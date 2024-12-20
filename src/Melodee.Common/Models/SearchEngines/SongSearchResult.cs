using System.Text.Json.Serialization;

namespace Melodee.Common.Models.SearchEngines;

public sealed record SongSearchResult
{
    public int? Id { get; init; }
    
    public Guid ApiKey { get; set; }
    
    public required string Name { get; init; }
    
    public int SortOrder { get; init; }
    
    public required string SortName { get; init; }
    
    [JsonIgnore]
    public string? MusicBrainzIdRaw { get; init; }
  
    public Guid? MusicBrainzId { get; set; }
    
    public long? PlayCount { get; init; }
    
    public long? Listeners { get; init; }
    
    public string? InfoUrl { get; init; }
    
    public string? ImageUrl { get; init; }
    
    public string? ThumbnailUrl { get; init; }
}
