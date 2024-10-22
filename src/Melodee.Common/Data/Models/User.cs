using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Contants;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data.Models;

[Index(nameof(UserName), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class User : DataModelBase
{
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string UserName { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    [DataType(DataType.EmailAddress)]
    public required string Email { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    public required string Name { get; set; }
    
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
    
    public Instant? LastLoginAt { get; set; }
    
    public Instant? LastActivityAt { get; set; }
    
    public bool IsAdmin { get; set; }
    
    public bool HasSettingsRole { get; set; }
    
    public bool HasDownloadRole { get; set; }
    
    public bool HasUploadRole { get; set; }
    
    public bool HasPlaylistRole { get; set; }
    
    public bool HasCoverArtRole { get; set; }
    
    public bool HasCommentRole { get; set; }
    
    public bool HasPodcastRole { get; set; }
    
    public bool HasStreamRole { get; set; }
    
    public bool HasJukeboxRole { get; set; }
    
    public bool HasShareRole { get; set; }
    
    public bool IsScrobblingEnabled { get; set; }

    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    
    public ICollection<Player> Players { get; set; } = new List<Player>();
    
    public ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
    
    public ICollection<PlayQueue> PlayQues { get; set; } = new List<PlayQueue>();
    
    public ICollection<Scrobble> Scrobbles { get; set; } = new List<Scrobble>();
    
    public ICollection<Share> Shares { get; set; } = new List<Share>();
    
    public ICollection<UserAlbum> UserAlbums { get; set; } = new List<UserAlbum>();
    
    public ICollection<UserArtist> UserArtists { get; set; } = new List<UserArtist>();
    
    public ICollection<UserSong> UserSongs { get; set; } = new List<UserSong>();
}
