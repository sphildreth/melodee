using Melodee.Common.Data.Contants;
using Melodee.Common.Extensions;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.Scrobbling;
using Melodee.Common.Utility;

namespace Melodee.Common.Data.Models.Extensions;

public static class SongExtensions
{
    public static string ToCoverArtId(this Song song) => song.ToApiKey();
    
    public static string ToApiKey(this Song song) => $"song{OpenSubsonicServer.ApiIdSeparator }{song.ApiKey}";
    
    public static Child ToChild(this Song song, Album album, UserSong? userSong, NowPlayingInfo? nowPlayingInfo = null)
    {
        Contributor? albumArtist = null;

        return new Child(song.ToApiKey(),
            album.ToApiKey(),
            false,
            song.Title,
            album.Name,
            albumArtist == null ? album.Artist.Name : albumArtist.Artist!.Name,
            song.SongNumber,
            album.ReleaseDate.Year,
            song.ToCoverArtId(),
            song.FileSize,
            "audio/mpeg", //TODO this should be the right type for the song media, what if no conversion and flac
            Path.GetExtension(song.FileName),
            userSong?.IsStarred ?? false ? userSong.LastUpdatedAt.ToString() : null,
            song.Duration.ToSeconds(),
            song.BitRate,
            song.BitDepth,
            song.SamplingRate,
            song.ChannelCount,
            song.FileName,
            song.PlayedCount,
            song.LastPlayedAt?.ToString(),
            song.AlbumDisc.DiscNumber,
            song.CreatedAt.ToString(),
            album.ToApiKey(),
            album.Artist.ToApiKey(),
            "music",
            "song",
            false,
            song.BPM,
            null,
            song.TitleSort,
            song.MusicBrainzId,
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
