using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Common.Configuration;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Collection.Extensions;
using MelodeeModelsCollection = Melodee.Common.Models.Collection;

namespace Melodee.Blazor.Controllers.Melodee.Extensions;

public static class SongDataInfoExtensions
{
    public static Song ToSongModel(this MelodeeModelsCollection.SongDataInfo songDataInfo, string baseUrl, User currentUser, string userSecret)
    {
        var artistInfoData = MelodeeModelsCollection.ArtistDataInfo.BlankArtistDataInfo with
        {
            ApiKey = songDataInfo.ArtistApiKey
        };
        var albumInfoData = MelodeeModelsCollection.AlbumDataInfo.BlankAlbumDataInfo with
        {
            ApiKey = songDataInfo.AlbumApiKey
        };
        return new Song(
            songDataInfo.ApiKey,
            new Artist(songDataInfo.ArtistApiKey,
                $"{baseUrl}/images/{artistInfoData.ToApiKey()}/{MelodeeConfiguration.DefaultThumbNailSize}",
                $"{baseUrl}/images/{artistInfoData.ToApiKey()}/{MelodeeConfiguration.DefaultImageSize}",
                songDataInfo.ArtistName,
                false,
                0,
                0,
                0,
                string.Empty,
                string.Empty
            ),
            new Album(songDataInfo.AlbumApiKey,
                $"{baseUrl}/images/{albumInfoData.ToApiKey()}/{MelodeeConfiguration.DefaultThumbNailSize}",
                $"{baseUrl}/images/{albumInfoData.ToApiKey()}/{MelodeeConfiguration.DefaultImageSize}",
                songDataInfo.AlbumName,
                songDataInfo.ReleaseDate.Year,
                false,
                0,
                0,
                0,
                string.Empty,
                string.Empty,
                string.Empty
            ),            
            $"{baseUrl}/song/stream/{songDataInfo.ApiKey}/{currentUser.Id}/{currentUser.CreateAuthUrlFragment(userSecret, songDataInfo.ApiKey.ToString())}",
            $"{baseUrl}/images/{songDataInfo.ToApiKey()}/{MelodeeConfiguration.DefaultThumbNailSize}",
            $"{baseUrl}/images/{songDataInfo.ToApiKey()}/{MelodeeConfiguration.DefaultImageSize}",
            songDataInfo.Title,
            songDataInfo.Duration,
            currentUser.FormatDuration(songDataInfo.Duration.ToDuration()),
            songDataInfo.UserStarred,
            songDataInfo.UserRating,
            songDataInfo.SongNumber,
            320,
            songDataInfo.PlayedCount,
            currentUser.FormatInstant(songDataInfo.CreatedAt),
            currentUser.FormatInstant(songDataInfo.LastUpdatedAt),
            songDataInfo.Genre);
    }
}
