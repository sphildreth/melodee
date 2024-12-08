using Melodee.Common.Extensions;
using Melodee.Plugins.SearchEngine.MusicBrainz.Data.Enums;
using ServiceStack.DataAnnotations;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models.Materialized;

/// <summary>
/// This is a materialized record for MusicBrainz Artist from all the MusicBrainz export files.
/// </summary>
public sealed record Artist
{
    [AutoIncrement]
    public long Id { get; set; }
   
    
    public required long UniqueId { get; init; }
    
    public required long MusicBrainzArtistId { get; init; }
    
    [Index(unique: false)]
    public required string Name { get; init; }
    
    public required string SortName { get; init; }
    
    [Index(unique: false)]
    public required string NameNormalized { get; init; }
    
    [Index]
    public required Guid MusicBrainzId { get; init; }
    
    [Index(unique: false)]
    public string? AlternateNames { get; init; }

    private string[]? _alternateNames;

    public string[] AlternateNamesValues => (_alternateNames ??= AlternateNames?.ToTags()?.ToArray() ?? []);
}
