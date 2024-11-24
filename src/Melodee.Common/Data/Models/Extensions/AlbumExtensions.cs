using Melodee.Common.Extensions;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.Scrobbling;

namespace Melodee.Common.Data.Models.Extensions;

public static class AlbumExtensions
{
    public static Child ToChild(this Album album, UserAlbum? userAlbum, NowPlayingInfo? nowPlayingInfo = null)
    {
        Contributor? albumArtist = null;

        return new Child(album.ApiKey.ToString(),
            album.ApiKey.ToString(),
            true,
            album.Name,
            album.Name,
            albumArtist == null ? album.Artist.Name : albumArtist.Artist!.Name,
            null,
            album.ReleaseDate.Year,
            $"album_{album.ApiKey}",
            null,
            null,
        null,
            userAlbum?.IsStarred ?? false ? userAlbum.LastUpdatedAt.ToString() : null,
            album.Duration.ToSeconds(),
            null,
            null,
            null,
            null,
            null,
            album.PlayedCount,
            album.LastPlayedAt?.ToString(),
            null,
            album.CreatedAt.ToString(),
            album.ApiKey.ToString(),
            album.Artist.ApiKey.ToString(),
            "music",
            "album",
            false,
            null,
            null,
            album.SortName,
            album.MusicBrainzId,
            [], // TODO
            [], // TODO
            album.Artist.Name,
            [], // TODO
            album.Artist.Name,
            [], // TODO
            null, //TODO
            [], // TODO
            null, //TODO,
            nowPlayingInfo?.User.UserName,
            nowPlayingInfo?.Scrobble.MinutesAgo,
            0,
            nowPlayingInfo?.Scrobble.PlayerName
        );

    }
}
