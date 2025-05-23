using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Extensions;
using MelodeeDataModels = Melodee.Common.Data.Models;

namespace Melodee.Blazor.Controllers.Melodee.Extensions;

public static class PlaylistExtensions
{
    public static Playlist ToPlaylistModel(this MelodeeDataModels.Playlist playlist, string baseUrl, User currentUser)
    {
        return new Playlist(playlist.ApiKey,
            $"{baseUrl}/images/{playlist.ToApiKey()}/32",
            $"{baseUrl}/images/{playlist.ToApiKey()}/512",
            playlist.Name,
            playlist.Duration.ToDuration(),
            playlist.Duration,
            currentUser.FormatDuration(playlist.Duration.ToDuration()),
            playlist.SongCount ?? 0);
    }
}
