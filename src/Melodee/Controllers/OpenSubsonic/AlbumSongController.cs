using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class AlbumSongController(OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{

    //getAlbumList
    //getAlbumList2
    //getRandomSongs
    //getSongsByGenre
    //getNowPlaying
    //getStarred
    //getStarred2

    /// <summary>
    /// Returns a list of random, newest, highest rated etc. albums.
    /// </summary>
    /// <param name="getAlbumListRequest">Request model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/getAlbumList2.view")]
    public async Task<IActionResult> GetAlbumList2(GetAlbumListRequest getAlbumListRequest, CancellationToken cancellationToken = default)
        => new JsonResult(await openSubsonicApiService.GetAlbumList2Async(getAlbumListRequest, ApiRequest, cancellationToken).ConfigureAwait(false));
}
