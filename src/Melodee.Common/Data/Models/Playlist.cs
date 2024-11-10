using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(UserId), nameof(Name), IsUnique = true)]
public class Playlist : DataModelBase
{
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Name { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxInputLength)]
    public string? Comment { get; set; }

    [RequiredGreaterThanZero] public int UserId { get; set; }

    public User User { get; set; } = null!;

    public bool IsPublic { get; set; }

    public short? SongCount { get; set; }

    public double Duration { get; set; }

    /// <summary>
    ///     Pipe seperated list. Example 'terrible|sexy|Songs about Love'
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxInputLength)]
    public string? AllowedUserIds { get; set; }

    public ICollection<PlaylistSong> Songs { get; set; } = new List<PlaylistSong>();
}
