using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(UserId), nameof(Client), nameof(UserAgent))]
public class Player : DataModelBase
{
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Name { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxInputLength)]
    public string? UserAgent { get; set; }

    [RequiredGreaterThanZero] public int UserId { get; set; }

    public User User { get; set; } = null!;

    [Required]
    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public required string Client { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? IpAddress { get; set; }

    [Required] public required Instant LastSeenAt { get; set; }

    public int? MaxBitRate { get; set; }

    public bool ScrobbleEnabled { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? TranscodingId { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public string? Hostname { get; set; }
}
