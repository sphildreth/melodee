using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Common.Configuration;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Extensions;
using MelodeeDataModels = Melodee.Common.Data.Models;

namespace Melodee.Blazor.Controllers.Melodee.Extensions;

public static class PlaylistExtensions
{
    public static Playlist ToPlaylistModel(this MelodeeDataModels.Playlist playlist, string baseUrl, User currentUser)
    {
        return new Playlist(playlist.ApiKey,
            $"{baseUrl}/images/{playlist.ToApiKey()}/{MelodeeConfiguration.DefaultThumbNailSize}",
            $"{baseUrl}/images/{playlist.ToApiKey()}/{MelodeeConfiguration.DefaultImageSize}",
            playlist.Name,
            playlist.Description,
            playlist.Duration.ToDuration(),
            playlist.Duration,
            currentUser.FormatDuration(playlist.Duration.ToDuration()),
            playlist.SongCount ?? 0);
    }
}
