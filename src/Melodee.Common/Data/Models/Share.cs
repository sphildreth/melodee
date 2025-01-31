using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
public class Share : DataModelBase
{
    [RequiredGreaterThanZero] public required int UserId { get; set; }

    public User User { get; set; } = null!;

    /// <summary>
    ///     Pipe seperated list. These can be either Album and/or SongIds.
    /// </summary>
    [Required]
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public required string ShareIds { get; set; }

    [Required] public required Instant ExpiresAt { get; set; }

    public bool IsDownloadable { get; set; }

    public Instant? LastVisitedAt { get; set; }

    public int VisitCount { get; set; }
}
