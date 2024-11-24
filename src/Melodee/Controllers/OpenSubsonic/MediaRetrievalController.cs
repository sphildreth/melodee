using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Serialization;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class MediaRetrievalController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{
    //download
    //getCaptions
    //getLyrics
    //hls
    //stream

    /// <summary>
    ///     Returns the avatar (personal image) for a user.
    /// </summary>
    /// <param name="username">The user in question.. 	</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/getAvatar.view")]
    public async Task<IActionResult> GetAvatarAsync(string username, CancellationToken cancellationToken = default)
    {
        return new FileContentResult((byte[])(await openSubsonicApiService.GetAvatarAsync(username,
                null,
                ApiRequest,
                cancellationToken)).ResponseData.Data!,
            "image/png");
    }    
    
    /// <summary>
    ///     Returns a cover art image.
    /// </summary>
    /// <param name="id">Composite ID of type:apikey</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/getCoverArt.view")]
    public async Task<IActionResult> GetCoverArtAsync(string id, CancellationToken cancellationToken = default)
    {
        return new FileContentResult((byte[])(await openSubsonicApiService.GetCoverArtAsync(id,
                null,
                ApiRequest,
                cancellationToken)).ResponseData.Data!,
            "image/jpeg");
    }


    /// <summary>
    ///     Streams a given media file.
    /// </summary>
    /// <param name="request">Stream model for parameters for streaming.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/stream.view")]
    public async Task<IActionResult> Stream(StreamRequest request, CancellationToken cancellationToken = default)
    {
        var streamResult = await openSubsonicApiService.StreamAsync(request, ApiRequest, cancellationToken).ConfigureAwait(false);
        if (streamResult.IsSuccess)
        {
            foreach (var responseHeader in streamResult.ResponseHeaders)
            {
                Response.Headers[responseHeader.Key] = responseHeader.Value;
            }

            await Response.Body
                .WriteAsync(streamResult.Bytes.AsMemory(0, streamResult.Bytes.Length), cancellationToken)
                .ConfigureAwait(false);
            return new EmptyResult();
        }

        //Returns binary data on success, or an XML document on error (in which case the HTTP content type will start with “text/xml”).
        // TODO figure out XML details for error result

        throw new NotImplementedException();
    }
}
