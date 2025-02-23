using Melodee.Common.Data.Constants;
using Melodee.Common.Extensions;

namespace Melodee.Common.Data.Models.Extensions;

public static class PlaylistExtensions
{
    public static string ToImageFileName(this Playlist playlist, string libraryPath)
    {
        return Path.Combine(libraryPath, Playlist.ImagesDirectoryName, $"{playlist.Id.ToStringPadLeft(8)}.gif");
    }    
    
    public static string ToCoverArtId(this Playlist playlist, bool? isDynamicPlaylist = false)
    {
        if (isDynamicPlaylist ?? false)
        {
            return $"dpl{OpenSubsonicServer.ApiIdSeparator}{playlist.ApiKey}";    
        }
        return playlist.ToApiKey();
    }

    public static string ToApiKey(this Playlist playlist, bool? isDynamicPlaylist = false)
    {
        if (isDynamicPlaylist ?? false)
        {
            return $"dpl{OpenSubsonicServer.ApiIdSeparator}{playlist.ApiKey}";    
        }
        return $"playlist{OpenSubsonicServer.ApiIdSeparator}{playlist.ApiKey}";
    }

    public static Common.Models.OpenSubsonic.Playlist ToApiPlaylist(this Playlist playlist, bool includeSongs = true, bool? isDynamicPlaylist = false)
    {
        return new Common.Models.OpenSubsonic.Playlist
        {
            Id = playlist.ToApiKey(isDynamicPlaylist),
            Name = playlist.Name,
            Comment = playlist.Comment,
            Owner = playlist.User.UserName,
            Public = playlist.IsPublic,
            SongCount = playlist.SongCount ?? 0,
            Duration = playlist.Duration.ToSeconds(),
            Created = playlist.CreatedAt.ToString(),
            Changed = playlist.LastUpdatedAt?.ToString() ?? playlist.CreatedAt.ToString(),
            CoverArt = playlist.ToCoverArtId(isDynamicPlaylist),
            AllowedUsers = playlist.AllowedUserIds?.ToTags()?.ToArray() ?? [],
            Entry = includeSongs ? playlist
                .Songs?
                .OrderBy(x => x.PlaylistOrder)
                .Select(x => x.Song.ToApiChild(x.Song.Album, x.Song.UserSongs.FirstOrDefault()))
                .ToArray() : null
        };
    }
}
