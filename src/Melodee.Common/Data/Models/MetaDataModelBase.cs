using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(Name), IsUnique = true)]
[Index(nameof(SortName))]
public abstract class MetaDataModelBase : DataModelBase
{
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Name { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? SortName { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public string? AlternateNames { get; set; }

    public Instant? LastPlayedAt { get; set; }

    public Instant? LastMetaDataUpdatedAt { get; set; }

    public int PlayedCount { get; set; }

    public string? ItunesId { get; set; }

    public string? AmgId { get; set; }

    public string? DiscogsId { get; set; }

    public string? MusicBrainzId { get; set; }

    public string? LastFmId { get; set; }

    public string? SpotifyId { get; set; }

    /// <summary>
    ///     Average of all user ratings
    /// </summary>
    public decimal CalculatedRating { get; set; }
}
