using System.Net;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Common.Serialization;
using Melodee.Results;
using Melodee.Services;
using Melodee.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class MediaRetrievalController(ISerializer serializer, EtagRepository etagRepository, OpenSubsonicApiService openSubsonicApiService) : ControllerBase(etagRepository, serializer)
{
    //TODO
    //getLyrics
    //getLyricsBySongId //https://opensubsonic.netlify.app/docs/endpoints/getlyricsbysongid/

    
    [HttpGet]
    [HttpPost]
    [Route("/rest/hls.view")]
    [Route("/rest/hls")]
    [Route("/rest/getCaptions.view")]
    [Route("/rest/getCaptions")]
    public IActionResult DeprecatedWontImplement()
    {
        HttpContext.Response.Headers.Append("Cache-Control", "no-cache");
        return StatusCode((int)HttpStatusCode.Gone);
    }     

    /// <summary>
    ///     Returns the avatar (personal image) for a user.
    /// </summary>
    /// <param name="username">The user in question.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getAvatar.view")]
    [Route("/rest/getAvatar")]
    public async Task<IActionResult> GetAvatarAsync(string username, CancellationToken cancellationToken = default)
    {
        return new FileContentResult((byte[])(await openSubsonicApiService.GetAvatarAsync(username,
                ApiRequest,
                cancellationToken)).ResponseData.Data!,
            "image/png");
    }

    /// <summary>
    ///     Returns a cover art image.
    /// </summary>
    /// <param name="id">Composite ID of type:apikey</param>
    /// <param name="size">If specified, scale image to this size.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getCoverArt.view")]
    [Route("/rest/getCoverArt")]
    public Task<IActionResult> GetCoverArtAsync(string id, string? size, CancellationToken cancellationToken = default)
    {
        return ImageResult(openSubsonicApiService.GetImageForApiKeyId(id,
            size,
            ApiRequest,
            cancellationToken));
    }

    /// <summary>
    ///     Downloads a given media file.
    /// </summary>
    /// <param name="request">Stream model for parameters for downloading.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/download.view")]
    [Route("/rest/download")]
    public async Task<IActionResult> DownloadAsync(StreamRequest request, CancellationToken cancellationToken = default)
    {
        request.IsDownloadingRequest = true;
        var streamResult = await openSubsonicApiService.StreamAsync(request, ApiRequest, cancellationToken).ConfigureAwait(false);
        if (streamResult.IsSuccess)
        {
            return File(streamResult.Bytes, streamResult.ContentType, streamResult.FileName);
        }

        Response.StatusCode = (int)HttpStatusCode.NotFound;
        return new JsonStringResult(Serializer.Serialize(new ResponseModel
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
        return new JsonStringResult(Serializer.Serialize(new ResponseModel
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
