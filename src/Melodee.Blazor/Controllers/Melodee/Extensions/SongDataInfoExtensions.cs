using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Collection.Extensions;
using MelodeeModelsCollection = Melodee.Common.Models.Collection;

namespace Melodee.Blazor.Controllers.Melodee.Extensions;

public static class SongDataInfoExtensions
{
    public static Song ToSongModel(this MelodeeModelsCollection.SongDataInfo songDataInfo, string baseUrl, User currentUser)
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
            $"{baseUrl}/images/{songDataInfo.ToApiKey()}/32",
            $"{baseUrl}/images/{songDataInfo.ToApiKey()}/512",
            songDataInfo.Title,
            songDataInfo.Duration,
            currentUser.FormatDuration(songDataInfo.Duration.ToDuration()),
            songDataInfo.UserStarred,
            songDataInfo.UserRating,
            new Artist(songDataInfo.ArtistApiKey,
                $"{baseUrl}/images/{artistInfoData.ToApiKey()}/32",
                $"{baseUrl}/images/{artistInfoData.ToApiKey()}/512",
                songDataInfo.ArtistName,
                false,
                0),
            new Album(songDataInfo.AlbumApiKey,
                $"{baseUrl}/images/{albumInfoData.ToApiKey()}/32",
                $"{baseUrl}/images/{albumInfoData.ToApiKey()}/512",
                songDataInfo.AlbumName,
                songDataInfo.ReleaseDate.Year,
                false,
                0
            ));
    }
}
