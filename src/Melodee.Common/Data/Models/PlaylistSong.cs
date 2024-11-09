using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(SongId), nameof(PlaylistId), IsUnique = true)]
[PrimaryKey(nameof(SongId), nameof(PlaylistId))]
public class PlaylistSong
{
    [RequiredGreaterThanZero] public int SongId { get; set; }

    /// <summary>
    ///     This is to not expose the SongId to API consumers and allow for playlist management by API consumers.
    /// </summary>
    [Required]
    public Guid SongApiKey { get; set; }

    public Song Song { get; set; } = null!;

    [RequiredGreaterThanZero] public int PlaylistId { get; set; }

    public Playlist Playlist { get; set; } = null!;

    public int PlaylistOrder { get; set; }
}
