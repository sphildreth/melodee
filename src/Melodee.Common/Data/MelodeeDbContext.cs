using Melodee.Common.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Data;

public class MelodeeDbContext : DbContext
{
    public DbSet<Album> Albums { get; set; }
    
    public DbSet<AlbumDisc> AlbumDiscs { get; set; }
    
    public DbSet<Artist> Artists { get; set; }
    
    public DbSet<Bookmark> Bookmarks { get; set; }
    
    public DbSet<Contributor> Contributors { get; set; }
    
    public DbSet<Library> Libraries { get; set; }
    
    public DbSet<LibraryScanHistory> LibraryScanHistories { get; set; }
    
    public DbSet<Player> Players { get; set; }
    
    public DbSet<Playlist> Playlists { get; set; }
    
    public DbSet<PlaylistSong> PlaylistSong { get; set; }
    
    public DbSet<PlayQueue> PlayQues { get; set; }
    
    public DbSet<RadioStation> RadioStations { get; set; }
    
    public DbSet<Scrobble> Scrobbles { get; set; }
    
    public DbSet<Share> Shares { get; set; }
    
    public DbSet<Song> Songs { get; set; }
    
    public DbSet<User> Users { get; set; }
    
    public DbSet<UserAlbum> UserAlbums { get; set; }
    
    public DbSet<UserArtist> UserArtists { get; set; }
    
    public DbSet<UserSong> UserSongs { get; set; }
    

    
}
