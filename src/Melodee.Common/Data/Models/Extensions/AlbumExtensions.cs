using Melodee.Common.Data.Contants;
using Melodee.Common.Extensions;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.Scrobbling;

namespace Melodee.Common.Data.Models.Extensions;

public static class AlbumExtensions
{
    public static string ToCoverArtId(this Album album) => album.ToApiKey();
    
    public static string ToApiKey(this Album album) => $"album{OpenSubsonicServer.ApiIdSeparator }{album.ApiKey}";
    
    public static Child ToApiChild(this Album album, UserAlbum? userAlbum, NowPlayingInfo? nowPlayingInfo = null)
    {
        Contributor? albumArtist = null;

        return new Child(album.ToApiKey(),
            album.Artist.ToApiKey(),
            true,
            album.Name,
            album.Name,
            albumArtist == null ? album.Artist.Name : albumArtist.Artist!.Name,
            null,
            album.ReleaseDate.Year,
            album.ToCoverArtId(),
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
            album.ToApiKey(),
            album.Artist.ToApiKey(),
            "music",
            "album",
            false,
            null,
            null,
            album.SortName,
            album.MusicBrainzId?.ToString(),
            [], //TODO
            [], //TODO
            album.Artist.Name,
            [], //TODO
            album.Artist.Name,
            [], //TODO
            null, //TODO
            [], //TODO
            null, //TODO,
            nowPlayingInfo?.User.UserName,
            nowPlayingInfo?.Scrobble.MinutesAgo,
            0,
            nowPlayingInfo?.Scrobble.PlayerName
        );

    }
}
