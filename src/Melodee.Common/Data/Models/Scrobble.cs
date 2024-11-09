using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Contants;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(UserId), nameof(ServiceUrl), nameof(SongId), nameof(PlayTimeInMs), IsUnique = true)]
public class Scrobble : DataModelBase
{
    [RequiredGreaterThanZero] public int UserId { get; set; }

    public User User { get; set; } = null!;

    [Required]
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public required string ServiceUrl { get; set; }

    [RequiredGreaterThanZero] public int SongId { get; set; }

    public Song Song { get; set; } = null!;

    [RequiredGreaterThanZero] public long PlayTimeInMs { get; set; }

    [NotMapped] public Duration PlayTimeValue => Duration.FromMilliseconds(PlayTimeInMs);

    [Required] public Instant EnqueueTime { get; set; }
}
