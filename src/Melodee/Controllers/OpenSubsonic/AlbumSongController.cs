using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class AlbumSongController(ApiService apiService) : ControllerBase
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
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/getAlbumList2.view")]
    public async Task<IActionResult> GetAlbumList2(GetAlbumListRequest apiRequest, CancellationToken cancellationToken = default)
    {
        return new JsonResult(new GetAlbumList2Response
        {
            ResponseData = new ApiResponse(
                "ok",
                "1.16.1",
                "Melodee",
                "0.1.1 (tag)",
                true,
                null
            ),
            AlbumList2 = []
        });
    }
}
