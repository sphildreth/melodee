using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Common.Configuration;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Collection.Extensions;
using MelodeeModelsCollection = Melodee.Common.Models.Collection;

namespace Melodee.Blazor.Controllers.Melodee.Extensions;

public static class PlaylistDataInfoExtensions
{
    public static Playlist ToPlaylistModel(this MelodeeModelsCollection.PlaylistDataInfo playlist, string baseUrl, User currentUser)
    {
        return new Playlist(playlist.ApiKey,
            $"{baseUrl}/images/{playlist.ToApiKey()}/{MelodeeConfiguration.DefaultThumbNailSize}",
            $"{baseUrl}/images/{playlist.ToApiKey()}/{MelodeeConfiguration.DefaultImageSize}",
            playlist.Name,
            playlist.Description ?? string.Empty,
            playlist.Duration,
            currentUser.FormatDuration(playlist.Duration.ToDuration()),
            playlist.SongCount,
            playlist.IsPublic,
            playlist.User.ToUserModel(baseUrl),
            playlist.CreatedAt.ToString(),
            playlist.LastUpdatedAt?.ToString() ?? string.Empty
        );
    }
}
