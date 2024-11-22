using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Serialization;
using Melodee.Results;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class AlbumSongController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{
    //getAlbumList
    //getRandomSongs
    //getSongsByGenre
    //getNowPlaying
    //getStarred
    //getStarred2

    /// <summary>
    ///     Returns a list of random, newest, highest rated etc. albums.
    /// </summary>
    /// <param name="getAlbumListRequest">Request model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/getAlbumList2.view")]
    public async Task<IActionResult> GetAlbumList2(GetAlbumListRequest getAlbumListRequest, CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetAlbumList2Async(getAlbumListRequest, ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }
}
