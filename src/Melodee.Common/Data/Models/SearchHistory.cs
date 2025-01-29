using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using NodaTime;

namespace Melodee.Common.Data.Models;

public class SearchHistory
{
    public int Id { get; set; }

    [Required] public required Instant CreatedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();

    [RequiredGreaterThanZero] public int ByUserId { get; set; }

    [NotMapped] public Guid ByUserApiKey { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string? ByUserAgent { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public string? SearchQuery { get; set; }

    public int FoundArtistsCount { get; set; }

    public int FoundAlbumsCount { get; set; }

    public int FoundSongsCount { get; set; }

    public int FoundOtherItems { get; set; }

    [RequiredGreaterThanZero] public double SearchDurationInMs { get; set; }

    [NotMapped] public Duration SearchDurationInMillisecondsValue => Duration.FromMilliseconds(SearchDurationInMs);

    [NotMapped] public int Count => FoundArtistsCount + FoundAlbumsCount + FoundSongsCount + FoundOtherItems;
}
