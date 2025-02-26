using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;

[Index(nameof(Name))]
[Index(nameof(NameNormalized))]
[Index(nameof(SortName))]
[Index(nameof(ItunesId), IsUnique = true)]
[Index(nameof(AmgId), IsUnique = true)]
[Index(nameof(DiscogsId), IsUnique = true)]
[Index(nameof(WikiDataId), IsUnique = true)]
[Index(nameof(MusicBrainzId), IsUnique = true)]
[Index(nameof(LastFmId), IsUnique = true)]
[Index(nameof(SpotifyId), IsUnique = true)]
public record Artist
{
    [Key] public int Id { get; init; }

    [Required] [MaxLength(2000)] public required string Name { get; init; }

    [Required] [MaxLength(2000)] public required string NameNormalized { get; init; }

    /// <summary>
    /// Alternate names in tag form
    /// </summary>
    [MaxLength(4000)]
    public string? AlternateNames { get; init; }

    [Required] [MaxLength(2000)] public required string SortName { get; init; }

    [MaxLength(255)] public string? ItunesId { get; init; }

    [MaxLength(255)] public string? AmgId { get; init; }

    [MaxLength(255)] public string? DiscogsId { get; init; }

    [MaxLength(255)] public string? WikiDataId { get; init; }

    public Guid? MusicBrainzId { get; init; }

    [MaxLength(255)] public string? LastFmId { get; init; }

    [MaxLength(255)] public string? SpotifyId { get; init; }

    public ICollection<Album> Albums { get; set; } = [];

    [NotMapped] public int Rank { get; set; }

    [NotMapped] public int AlbumCount { get; set; }

    public bool? IsLocked { get; init; }

    /// <summary>
    /// Last time the Artist albums where refreshed from search engine plugins.
    /// </summary>
    public DateTimeOffset? LastRefreshed { get; set; }

    public override string ToString()
    {
        return $"{Name} ({Id}) MusicBrainzId: [{MusicBrainzId}] SpotifyId: [{SpotifyId}]";
    }
}
