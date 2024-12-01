using Melodee.Common.Data.Contants;
using Melodee.Common.Extensions;
using Melodee.Common.Models.OpenSubsonic;

namespace Melodee.Common.Data.Models.Extensions;

public static class PlaylistExtensions
{
    
    public static string ToCoverArtId(this Playlist playlist) => playlist.ToApiKey();
    
    public static string ToApiKey(this Playlist playlist) => $"playlist{OpenSubsonicServer.ApiIdSeparator }{playlist.ApiKey}";
    
    public static Common.Models.OpenSubsonic.Playlist ToApiPlaylist(this Playlist playlist)
    {
        return new Common.Models.OpenSubsonic.Playlist
        {
            Id = playlist.ToApiKey(),
            Name = playlist.Name,
            Comment = playlist.Comment,
            Owner = playlist.User.UserName,
            Public = playlist.IsPublic,
            SongCount = playlist.SongCount ?? 0,
            Duration = playlist.Duration.ToSeconds(),
            Created = playlist.CreatedAt.ToString(),
            Changed = playlist.LastUpdatedAt?.ToString() ?? playlist.CreatedAt.ToString(),
            CoverArt = playlist.ToCoverArtId(),
            AllowedUsers = [],
            Entry = playlist
                .Songs?
                .OrderBy(x => x.PlaylistOrder)
                .Select(x => x.Song.ToApiChild(x.Song.AlbumDisc.Album, x.Song.UserSongs.FirstOrDefault()))
                .ToArray() 
        };
    }
}
