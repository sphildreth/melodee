using ServiceStack.DataAnnotations;

namespace Melodee.Services.MusicBrainz.Data.Models;

/// <summary>
/// A release is a physical or digital product that contains audio, such as a CD or digital download. A release includes information like the country, label, barcode, and packaging. A release can contain multiple media, such as a CD, vinyl, or digital media.
/// </summary>
public sealed record Release
{
    [Ignore]    
    public bool IsValid => MusicBrainzId != Guid.Empty && Name != null;

    public long Id { get; init; }
    
    [Index(Unique = true)]
    public Guid MusicBrainzId { get; init; }
    
    [Index(Unique = false)]
    public string? Name { get; init; }
    
    [Index(Unique = false)]
    public string? NameNormalized { get; init; }
    
    [Index(Unique = false)]
    public string? SortName { get; init; }
    
    public required long ArtistCreditId { get; init; }
}    
