using Melodee.Common.Data.Contants;
using Melodee.Common.Extensions;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.Scrobbling;
using Melodee.Common.Utility;

namespace Melodee.Common.Data.Models.Extensions;

public static class AlbumExtensions
{
    public static string ToCoverArtId(this Album album) => album.ToApiKey();
    
    public static string ToApiKey(this Album album) => $"album{OpenSubsonicServer.ApiIdSeparator }{album.ApiKey}";

    public static AlbumID3 ToArtistID3(this Album album, UserAlbum? userAlbum, NowPlayingInfo? nowPlayingInfo)
    {
        return new AlbumID3
        {
            Id = album.ToApiKey(),
            Name = album.Name,
            Artist = album.Artist.Name,
            ArtistId = album.Artist.ToApiKey(),
            CoverArt = album.ToCoverArtId(),
            SongCount = album.SongCount ?? 0,
            Duration = album.Duration.ToSeconds(),
            PlayCount = album.PlayedCount,
            CreatedRaw = album.CreatedAt,
            Starred = userAlbum?.StarredAt?.ToString(),
            Year = album.ReleaseDate.Year,
            Genres = album.Genres
        };
    }
    
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
            null, //TODO
            SafeParser.ToNumber<int>(album.CalculatedRating),
            userAlbum?.Rating,
            Username: nowPlayingInfo?.User.UserName,
            MinutesAgo: nowPlayingInfo?.Scrobble.MinutesAgo,
            PlayerId: 0,
            PlayerName: nowPlayingInfo?.Scrobble.PlayerName
        );

    }
}
