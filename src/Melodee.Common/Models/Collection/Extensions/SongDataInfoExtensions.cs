using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;

namespace Melodee.Common.Models.Collection.Extensions;

public static class SongDataInfoExtensions
{
    public static double DurationInSeconds(this SongDataInfo song)
    {
        return song.Duration / 1000;
    }

    public static string AlbumDetailUrl(this SongDataInfo songDataInfo)
    {
        return $"/data/album/{songDataInfo.AlbumApiKey}";
    }

    public static string ImageUrl(this SongDataInfo songDataInfo, ImageSize? size = null)
    {
        return $"/images/{songDataInfo.ToApiKey()}/{size ?? ImageSize.Thumbnail}";
    }

    public static string ToApiKey(this SongDataInfo songDataInfo)
    {
        return $"song{OpenSubsonicServer.ApiIdSeparator}{songDataInfo.ApiKey}";
    }

    public static string ToApiStreamUrl(this SongDataInfo song, IMelodeeConfiguration configuration)
    {
        var baseUrl = configuration.GetValue<string>(SettingRegistry.SystemBaseUrl);
        if (baseUrl.Nullify() == null || baseUrl == MelodeeConfiguration.RequiredNotSetValue)
        {
            throw new Exception($"Configuration setting [{SettingRegistry.SystemBaseUrl}] is invalid.");
        }

        return $"{baseUrl}/rest/stream?id={song.ToApiKey()}";
    }

    public static PlaylistSong ToPlaylistSong(this SongDataInfo songDataInfo, int playlistOrder, Data.Models.Song song)
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
