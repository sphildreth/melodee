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
    public required string NormalizedName { get; init; }
    
    public int ReleaseType { get; init; }
    
    public ReleaseType ReleaseTypeValue => SafeParser.ToEnum<ReleaseType>(ReleaseType);
    
    [Index]
    public required Guid MusicBrainzId { get; init; }
    
    public required DateOnly ReleaseDate { get; init; }
}
