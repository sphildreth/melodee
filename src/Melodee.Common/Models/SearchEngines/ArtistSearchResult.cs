using Melodee.Common.Extensions;
using Melodee.Common.Utility;

namespace Melodee.Common.Models.SearchEngines;

/// <summary>
///     A result for an artist search engine search
/// </summary>
public sealed record ArtistSearchResult
{
    public KeyValue KeyValue => new KeyValue(UniqueId.ToString(), Name.ToNormalizedString() ?? Name);    

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
    public int Rank { get; init; }

    /// <summary>Artist ApiKey (if found in database)</summary>
    public Guid? ApiKey { get; init; }

    /// <summary>Public URL to an image.</summary>
    public string? ImageUrl { get; init; }

    /// <summary>Any found MusicBrainzId</summary>
    public Guid? MusicBrainzId { get; init; }

    /// <summary>Any alternate names for Artist</summary>
    public string[]? AlternateNames { get; init; }

    /// <summary>Collection of releases for artist</summary>
    public AlbumSearchResult[]? Releases { get; init; }
}
