using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Serialization;
using Melodee.Results;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class AlbumSongController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{
    // These are by file structure, versus ID tags
    //getAlbumList
    //getStarred

    //getRandomSongs
    //getSongsByGenre
    //getStarred2

    /// <summary>
    ///     Returns what is currently being played by all users.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getNowPlaying.view")]
    public async Task<IActionResult> GetNowPlayingAsync(CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetNowPlayingAsync(ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }

    /// <summary>
    ///     Returns a list of random, newest, highest rated etc. albums.
    /// </summary>
    /// <param name="getAlbumListRequest">Request model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getAlbumList2.view")]
    public async Task<IActionResult> GetAlbumList2Async(GetAlbumListRequest getAlbumListRequest, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetAlbumList2Async(getAlbumListRequest, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }
}
