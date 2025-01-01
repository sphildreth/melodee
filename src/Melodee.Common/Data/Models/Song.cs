using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

/// <summary>
///     Song record.
///     <remarks>See https://opensubsonic.netlify.app/docs/responses/child/</remarks>
/// </summary>
[Serializable]
[Index(nameof(Title))]
[Index(nameof(AlbumDiscId), nameof(SongNumber), IsUnique = true)]
public class Song : MetaDataModelBase
{
    [RequiredGreaterThanZero] public int AlbumDiscId { get; set; }

    public AlbumDisc AlbumDisc { get; set; } = null!;

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Title { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? TitleSort { get; set; }

    [Required]
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public required string TitleNormalized { get; set; }

    /// <summary>
    ///     These are strictly defined in the Genre enum.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string[]? Genres { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string[]? Moods { get; set; }

    /// <summary>
    ///     This is plain text and served to OpenSubsonic clients.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxInputLength)]
    public string? Comment { get; set; }

    /// <summary>
    ///     The track replay gain value. (In Db)
    /// </summary>
    public double? ReplayGain { get; set; }

    /// <summary>
    ///     The track peak value. (Must be positive)
    /// </summary>
    public double? ReplayPeak { get; set; }

    /// <summary>
    ///     When a song is released as a single, it can have unique images different from the album image.
    /// </summary>
    public int? ImageCount { get; set; }

    [RequiredGreaterThanZero] public required int SongNumber { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    [Required]
    public required string FileName { get; set; }

    /// <summary>
    ///     This value has special formatting for content type and timestamp formats. This value often includes single and
    ///     double quotes.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxTextLength)]
    public string? Lyrics { get; set; }

    [RequiredGreaterThanZero] public required long FileSize { get; set; }

    [MaxLength(MaxLengthDefinitions.HashOrGuidLength)]
    [Required]
    public required string FileHash { get; set; }

    /// <summary>
    ///     TIT3
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralLongLength)]
    public string? PartTitles { get; set; }

    [RequiredGreaterThanZero] public required double Duration { get; set; }

    [RequiredGreaterThanZero] public required int SamplingRate { get; set; }

    [RequiredGreaterThanZero] public required int BitRate { get; set; }

    [RequiredGreaterThanZero] public required int BitDepth { get; set; }

    [RequiredGreaterThanZero] public required int BPM { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string ContentType { get; set; }

    public int? ChannelCount { get; set; }

    public bool IsVbr { get; set; }

    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public ICollection<Contributor> Contributors { get; set; } = new List<Contributor>();

    public ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();

    public ICollection<PlayQueue> PlayQues { get; set; } = new List<PlayQueue>();

    public ICollection<UserSong> UserSongs { get; set; } = new List<UserSong>();
}
