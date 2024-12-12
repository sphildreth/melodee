using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
public abstract class MetaDataModelBase : DataModelBase
{
    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public string? AlternateNames { get; set; }

    public Instant? LastPlayedAt { get; set; }

    public Instant? LastMetaDataUpdatedAt { get; set; }

    public int PlayedCount { get; set; }

    public string? ItunesId { get; set; }

    public string? AmgId { get; set; }

    public string? DiscogsId { get; set; }
    
    public string? WikiDataId { get; set; }

    public Guid? MusicBrainzId { get; set; }

    public string? LastFmId { get; set; }

    public string? SpotifyId { get; set; }

    /// <summary>
    ///     Average of all user ratings
    /// </summary>
    public decimal CalculatedRating { get; set; }
}
