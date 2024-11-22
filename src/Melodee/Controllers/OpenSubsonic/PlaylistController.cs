using Melodee.Common.Serialization;
using Melodee.Results;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class PlaylistController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{
    //getPlaylist
    //createPlaylist
    //updatePlaylist
    //deletePlaylist

    /// <summary>
    ///     Returns all playlists a user is allowed to play.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/getPlaylists.view")]
    public async Task<IActionResult> GetGenres(CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetPlaylists(ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }
}
