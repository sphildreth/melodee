using Melodee.Common.Extensions;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Utility;

namespace Melodee.Common.Data.Models.Extensions;

public static class SongExtensions
{
    public static Child ToChild(this Song song, Album album, UserSong? userSong)
    {
        Contributor? albumArtist = null;

        return new Child(song.ApiKey.ToString(),
            album.ApiKey.ToString(),
            false,
            song.Title,
            album.Name,
            albumArtist == null ? album.Artist.Name : albumArtist.Artist!.Name,
            song.SongNumber,
            album.ReleaseDate.Year,
            $"album_{album.ApiKey}",
            song.FileSize,
            "audio/mpeg", // TODO this should be the right type for the song media, what if no conversion and flac
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
            album.ApiKey.ToString(),
            album.Artist.ApiKey.ToString(),
            "music",
            "song",
            false,
            song.BPM,
            null,
            song.TitleSort,
            song.MusicBrainzId,
            [], // TODO
            [], // TODO
            album.Artist.Name,
            [], // TODO
            album.Artist.Name,
            [], // TODO
            null, //TODO
            [], // TODO
            null //TODO
        );

    }
}
