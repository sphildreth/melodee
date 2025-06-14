using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Common.Configuration;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Collection.Extensions;
using MelodeeModelsCollection = Melodee.Common.Models.Collection;

namespace Melodee.Blazor.Controllers.Melodee.Extensions;

public static class AlbumExtensions
{
    public static Album ToAlbumModel(this MelodeeModelsCollection.AlbumDataInfo album, string baseUrl, User currentUser)
    {
        return new Album(album.ApiKey,
            Artist.BlankArtist() with
            {
                Id = album.ArtistApiKey,
                Name = album.ArtistName
            },
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
