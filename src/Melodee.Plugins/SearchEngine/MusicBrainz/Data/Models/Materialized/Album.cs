using Melodee.Common.Enums;
using Melodee.Common.Utility;
using Melodee.Plugins.SearchEngine.MusicBrainz.Data.Enums;
using ServiceStack.DataAnnotations;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models.Materialized;

/// <summary>
/// This is a materialized release for MusicBrainz release from all the MusicBrainz export files.
/// </summary>
public sealed record Album
{
    [AutoIncrement]
    public long Id { get; set; }
    
    public required long UniqueId { get; init; }
    
    public required long ArtistId { get; init; }
    
    [Index(unique: false)]
    public required string Name { get; init; }
    
    public required string SortName { get; init; }
    
    [Index(unique: false)]
    public required string NameNormalized { get; init; }
    
    public int ReleaseType { get; init; }
    
    public ReleaseType ReleaseTypeValue => SafeParser.ToEnum<ReleaseType>(ReleaseType);

    public bool DoIncludeInArtistSearch => ReleaseDate > DateTime.MinValue && 
                                           ReleaseTypeValue != Enums.ReleaseType.Single && 
                                           ReleaseTypeValue != Enums.ReleaseType.Broadcast;
    
    [Index]
    public required Guid MusicBrainzId { get; init; }
    
    public required Guid ReleaseGroupMusicBrainzId { get; init; }
    
    public required DateTime ReleaseDate { get; init; }
    
    public string? ContributorIds { get; init; }
}
