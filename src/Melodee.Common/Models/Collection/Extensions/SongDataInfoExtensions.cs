using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Models;

namespace Melodee.Common.Models.Collection.Extensions;

public static class SongDataInfoExtensions
{
    public static string AlbumDetailUrl(this SongDataInfo songDataInfo)
    {
        return $"/data/album/{songDataInfo.AlbumApiKey}";
    }

    public static string ImageUrl(this SongDataInfo songDataInfo, int? size = null)
    {
        return $"/images/{songDataInfo.ToApiKey()}/{size ?? 80}";
    }

    public static string ToApiKey(this SongDataInfo songDataInfo)
    {
        return $"song{OpenSubsonicServer.ApiIdSeparator}{songDataInfo.ApiKey}";
    }

    public static PlaylistSong ToPlaylistSong(this SongDataInfo songDataInfo, int playlistOrder, Common.Data.Models.Song song)
    {
        return new PlaylistSong
        {
            PlaylistId = 1,
            SongId = song.Id,
            Song = song,
            SongApiKey = song.ApiKey,
            PlaylistOrder = playlistOrder
        };
    }
}
