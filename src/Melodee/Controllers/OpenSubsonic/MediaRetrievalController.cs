using System.Net;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Common.Serialization;
using Melodee.Results;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class MediaRetrievalController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase(serializer)
{
    //download
    //getCaptions
    //getLyrics
    //getLyricsBySongId //https://opensubsonic.netlify.app/docs/endpoints/getlyricsbysongid/
    //hls
    //stream

    /// <summary>
    ///     Returns the avatar (personal image) for a user.
    /// </summary>
    /// <param name="username">The user in question.. 	</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getAvatar.view")]
    [Route("/rest/getAvatar")]
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
    [HttpGet]
    [HttpPost]
    [Route("/rest/getCoverArt.view")]
    [Route("/rest/getCoverArt")]
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
    [HttpGet]
    [HttpPost]
    [Route("/rest/stream.view")]
    [Route("/rest/stream")]
    public async Task<IActionResult> StreamAsync(StreamRequest request, CancellationToken cancellationToken = default)
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

        Response.StatusCode = (int)HttpStatusCode.NotFound;
        return new JsonStringResult(serializer.Serialize(new ResponseModel
        {
            UserInfo = OpenSubsonicApiService.BlankUserInfo,
            IsSuccess = false,
            ResponseData = await openSubsonicApiService.NewApiResponse(
                false,
                string.Empty,
                string.Empty,
                Error.DataNotFoundError)
        })!);
    }
}
