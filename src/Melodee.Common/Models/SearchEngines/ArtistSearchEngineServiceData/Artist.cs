using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;

[Index(nameof(Name), IsUnique = true)]
[Index(nameof(NameNormalized), IsUnique = true)]
[Index(nameof(SortName), IsUnique = true)]
[Index(nameof(ItunesId), IsUnique = true)]
[Index(nameof(AmgId), IsUnique = true)]
[Index(nameof(DiscogsId), IsUnique = true)]
[Index(nameof(WikiDataId), IsUnique = true)]
[Index(nameof(MusicBrainzId), IsUnique = true)]
[Index(nameof(LastFmId), IsUnique = true)]
[Index(nameof(SpotifyId), IsUnique = true)]
public record Artist
{
    [Key]
    public int Id { get; init; }
    
    [Required]
    [MaxLength(2000)]
    public required string Name { get; init; }
    
    [Required]
    [MaxLength(2000)]
    public required string NameNormalized { get; init; }
    
    [Required]
    [MaxLength(2000)]
    public required string SortName { get; init; }
    
    [MaxLength(255)]
    public string? ItunesId { get; init; }

    [MaxLength(255)]
    public string? AmgId { get; init; }

    [MaxLength(255)]
    public string? DiscogsId { get; init; }

    [MaxLength(255)]
    public string? WikiDataId { get; init; }

    public Guid? MusicBrainzId { get; init; }

    [MaxLength(255)]
    public string? LastFmId { get; init; }

    [MaxLength(255)]
    public string? SpotifyId { get; init; }

    public override string ToString() => $"{Name} ({Id}) MusicBrainzId: [{MusicBrainzId}] SpotifyId: [{SpotifyId}]";
}
