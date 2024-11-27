using ServiceStack.DataAnnotations;

namespace Melodee.Services.MusicBrainz.Data.Models;

public sealed record ArtistCreditName
{
    [AutoIncrement]
    public long Id { get; init; }
    
    [Ignore]
    public bool IsValid => ArtistCreditId > 0 && ArtistId > 0;

    [Index(Unique = false)]
    public long ArtistCreditId { get; init; }
    
    public int Position { get; init; }
    
    [Index(Unique = false)]
    public long ArtistId { get; init; }
    
    public required string Name { get; init; }


}  
