using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(UserId), nameof(AlbumId), IsUnique = true)]
public class UserAlbum : DataModelBase
{
    [RequiredGreaterThanZero] public required int UserId { get; set; }

    public User User { get; set; } = null!;

    [RequiredGreaterThanZero] public required int AlbumId { get; set; }

    public Album Album { get; set; } = null!;

    public int PlayedCount { get; set; }

    /// <summary>
    ///     Can be null if user is just toggling star.
    /// </summary>
    public Instant? LastPlayedAt { get; set; }

    public bool IsStarred { get; set; }

    /// <summary>
    ///     When true don't include in randomization's, the user hates it.
    /// </summary>
    public bool IsHated { get; set; }

    public Instant? StarredAt { get; set; }

    public int Rating { get; set; }
}
