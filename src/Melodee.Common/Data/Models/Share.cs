using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using Melodee.Common.Enums;
using Melodee.Common.Utility;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
public class Share : DataModelBase
{
    /// <summary>
    ///     User who created share
    /// </summary>
    [RequiredGreaterThanZero]
    public required int UserId { get; set; }

    public User User { get; set; } = null!;

    /// <summary>
    ///     PkId of shared item (can be any of the ShareTypes)
    /// </summary>
    [RequiredGreaterThanZero]
    public required int ShareId { get; set; }

    [RequiredGreaterThanZero] public int ShareType { get; set; }

    /// <summary>
    ///     A very short (shorter than a GUID) distinct string id used to build url.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.HashOrGuidLength)]
    [Required]
    public string ShareUniqueId { get; set; } = string.Empty;

    [NotMapped] public ShareType ShareTypeValue => SafeParser.ToEnum<ShareType>(ShareType);

    public Instant? ExpiresAt { get; set; }

    public bool IsDownloadable { get; set; }

    public Instant? LastVisitedAt { get; set; }

    public int VisitCount { get; set; }
}
