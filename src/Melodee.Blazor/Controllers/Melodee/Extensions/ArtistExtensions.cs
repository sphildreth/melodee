using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Common.Configuration;
using Melodee.Common.Models.Collection.Extensions;
using MelodeeModelsCollection = Melodee.Common.Models.Collection;

namespace Melodee.Blazor.Controllers.Melodee.Extensions;

public static class ArtistExtensions
{
    public static Artist ToArtistModel(this MelodeeModelsCollection.ArtistDataInfo artist, string baseUrl, User currentUser)
    {
        return new Artist(artist.ApiKey,
            $"{baseUrl}/images/{artist.ToApiKey()}/{MelodeeConfiguration.DefaultThumbNailSize}",
            $"{baseUrl}/images/{artist.ToApiKey()}/{MelodeeConfiguration.DefaultImageSize}",
            artist.Name,
            artist.UserStarred,
            artist.UserRating,
            artist.AlbumCount,
            artist.SongCount,
            currentUser.FormatInstant(artist.CreatedAt),
            currentUser.FormatInstant(artist.LastUpdatedAt),
            artist.Biography
        );
    }
}
