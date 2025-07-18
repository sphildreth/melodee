using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Serializable]
[Index(nameof(UserName), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class User : DataModelBase
{
    public const string CacheRegion = "urn:region:user";
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string UserName { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string UserNameNormalized { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    [DataType(DataType.EmailAddress)]
    public required string Email { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string EmailNormalized { get; set; }

    /// <summary>
    ///     This is the PublicKey (really its a private key) used to encrypt and decrypt the users password for Subsonic
    ///     clients authentication. When this
    ///     changes the users password will need reset.
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string PublicKey { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    [DataType(DataType.Password)]
    public required string PasswordEncrypted { get; set; }

    public Instant? LastLoginAt { get; set; }

    public Instant? LastActivityAt { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsEditor { get; set; }

    /// <summary>
    ///     Can the user modify their settings, not system settings.
    /// </summary>
    public bool HasSettingsRole { get; set; } = true;

    public bool HasDownloadRole { get; set; } = true;

    public bool HasUploadRole { get; set; } = true;

    public bool HasPlaylistRole { get; set; } = true;

    public bool HasCoverArtRole { get; set; } = true;

    public bool HasCommentRole { get; set; } = true;

    public bool HasPodcastRole { get; set; } = true;

    public bool HasStreamRole { get; set; } = true;

    public bool HasJukeboxRole { get; set; } = true;

    public bool HasShareRole { get; set; } = true;

    public bool IsScrobblingEnabled { get; set; }

    [MaxLength(MaxLengthDefinitions.HashOrGuidLength)]
    public string? LastFmSessionKey { get; set; }

    /// <summary>
    ///     Pipe seperated list. Don't randomize songs in these genres. e.g. 'HOLIDAY|CHRISTMAS'
    /// </summary>
    [MaxLength(MaxLengthDefinitions.MaxIndexableLength)]
    public string? HatedGenres { get; set; }

    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public ICollection<Player> Players { get; set; } = new List<Player>();

    public ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();

    public ICollection<PlayQueue> PlayQues { get; set; } = new List<PlayQueue>();

    public ICollection<Share> Shares { get; set; } = new List<Share>();

    public ICollection<UserAlbum> UserAlbums { get; set; } = new List<UserAlbum>();

    public ICollection<UserArtist> UserArtists { get; set; } = new List<UserArtist>();

    public ICollection<UserSong> UserSongs { get; set; } = new List<UserSong>();

    public ICollection<UserPin> Pins { get; set; } = new List<UserPin>();

    public static User BlankUser => new()
    {
        UserName = string.Empty,
        UserNameNormalized = string.Empty,
        Email = string.Empty,
        EmailNormalized = string.Empty,
        PublicKey = string.Empty,
        PasswordEncrypted = string.Empty,
        CreatedAt = default
    };
}
