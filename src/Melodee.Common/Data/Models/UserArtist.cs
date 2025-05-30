using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(UserId), nameof(ArtistId), IsUnique = true)]
public class UserArtist : DataModelBase
{
    [RequiredGreaterThanZero] public required int UserId { get; set; }

    public User User { get; set; } = null!;

    [RequiredGreaterThanZero] public required int ArtistId { get; set; }

    public Artist Artist { get; set; } = null!;

    public bool IsStarred { get; set; }

    /// <summary>
    ///     When true don't include in randomization's, the user hates it.
    /// </summary>
    public bool IsHated { get; set; }

    public Instant? StarredAt { get; set; }

    public int Rating { get; set; }
}
