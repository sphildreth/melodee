using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using NodaTime;

namespace Melodee.Common.Data.Models;

public class ShareActivity
{
    public int Id { get; set; }

    [RequiredGreaterThanZero] public required int ShareId { get; set; }

    /// <summary>
    ///     Populated if user is authenticated when viewing share.
    /// </summary>
    public int? UserId { get; set; }

    [Required] public required Instant CreatedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();

    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string? ByUserAgent { get; set; }

    [Required]
    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public required string Client { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? IpAddress { get; set; }
}
