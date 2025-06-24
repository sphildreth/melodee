using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Common.Configuration;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Collection.Extensions;
using NodaTime;
using MelodeeModelsCollection = Melodee.Common.Models.Collection;

namespace Melodee.Blazor.Controllers.Melodee.Extensions;

public static class AlbumExtensions
{
    public static Album ToAlbumModel(this MelodeeModelsCollection.AlbumDataInfo album, string baseUrl, User currentUser)
    {
        var artistInfo = new MelodeeModelsCollection.ArtistDataInfo(
            0,
            album.ArtistApiKey,
            false,
            0,
            string.Empty,
            album.ArtistName,
            album.ArtistName.ToNormalizedString() ?? album.ArtistName,
            string.Empty,
            string.Empty,
            0,
            0,
            Instant.MinValue,
            string.Empty,
            null);
        return new Album(album.ApiKey,
            artistInfo.ToArtistModel(baseUrl, currentUser),
            $"{baseUrl}/images/{album.ToApiKey()}/{MelodeeConfiguration.DefaultThumbNailSize}",
            $"{baseUrl}/images/{album.ToApiKey()}/{MelodeeConfiguration.DefaultImageSize}",
            album.Name,
            album.ReleaseDate.Year,
            album.UserStarred,
            album.UserRating,
            album.SongCount,
            album.Duration,
            currentUser.FormatDuration(album.Duration.ToDuration()),
            currentUser.FormatInstant(album.CreatedAt),
            currentUser.FormatInstant(album.LastUpdatedAt)
        );
    }
}
