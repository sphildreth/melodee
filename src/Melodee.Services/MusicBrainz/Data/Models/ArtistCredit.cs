using ServiceStack.DataAnnotations;

namespace Melodee.Services.MusicBrainz.Data.Models;

public sealed record ArtistCredit
{
    [Ignore]
    public bool IsValid => Id > 0 && ArtistCount > 0;

    public long Id { get; init; }
    
    public required string Name { get; init; }
    
    public int ArtistCount { get; init; }
    
    public int RefCount { get; init; }
    
    public required Guid Gid { get; init; }
}    