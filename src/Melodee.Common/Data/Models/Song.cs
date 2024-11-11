using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Data.Contants;
using Melodee.Common.Data.Validators;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data.Models;

/// <summary>
///     Song record.
///     <remarks>See https://opensubsonic.netlify.app/docs/responses/child/</remarks>
/// </summary>
[Serializable]
[Index(nameof(Title))]
[Index(nameof(LibraryId))]
[Index(nameof(AlbumDiscId), nameof(SongNumber), IsUnique = true)]
public class Song : MetaDataModelBase
{
    [RequiredGreaterThanZero] public int AlbumDiscId { get; set; }

    [RequiredGreaterThanZero] public int LibraryId { get; set; }

    [RequiredGreaterThanZero] public long MediaUniqueId { get; set; }

    public AlbumDisc AlbumDisc { get; set; } = null!;

    public Library Library { get; set; } = null!;

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Title { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public string? TitleSort { get; set; }

    [Required]
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    public required string TitleNormalized { get; set; }
    
    /// <summary>
    /// These are strictly defined in the Genre enum.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string[]? Genres { get; set; }    

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

    public int? ChannelCount { get; set; }

    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public ICollection<Contributor> Contributors { get; set; } = new List<Contributor>();

    public ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();

    public ICollection<PlayQueue> PlayQues { get; set; } = new List<PlayQueue>();

    public ICollection<Scrobble> Scrobbles { get; set; } = new List<Scrobble>();

    public ICollection<UserSong> UserSongs { get; set; } = new List<UserSong>();
}
