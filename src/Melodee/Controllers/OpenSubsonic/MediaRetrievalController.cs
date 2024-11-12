using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class MediaRetrievalController(OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{
    //download
    //getAvatar
    //getCaptions
    //getCoverArt
    //getLyrics
    //hls
    //stream

    /// <summary>
    /// Returns a cover art image.
    /// </summary>
    /// <param name="apiId">Composite ID of type:apikey</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/getCoverArt.view")]
    public async Task<IActionResult> GetCoverArt(string apiId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
