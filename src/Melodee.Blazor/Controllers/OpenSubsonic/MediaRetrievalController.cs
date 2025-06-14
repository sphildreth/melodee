using System.Net;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Models;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Results;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public class MediaRetrievalController(ISerializer serializer, EtagRepository etagRepository, OpenSubsonicApiService openSubsonicApiService, IMelodeeConfigurationFactory configurationFactory) : ControllerBase(etagRepository, serializer, configurationFactory)
{
    /// <summary>
    ///     Searches for and returns lyrics for a given song.
    /// </summary>
    /// <param name="artist">The artist name.</param>
    /// <param name="title">The song title.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getLyrics.view")]
    [Route("/rest/getLyrics")]
    public Task<IActionResult> GetLyricsAsync(string? artist, string? title, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetLyricsForArtistAndTitleAsync(artist, title, ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Add support for synchronized lyrics, multiple languages, and retrieval by song ID.
    ///     <remarks>
    ///         Retrieves all structured lyrics from the server for a given song. The lyrics can come from embedded tags
    ///         (SYLT/USLT), LRC file/text file, or any other external source.
    ///     </remarks>
    /// </summary>
    /// <param name="id">The track ID.</param>
    /// ///
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getLyricsBySongId.view")]
    [Route("/rest/getLyricsBySongId")]
    public Task<IActionResult> GetLyricsAsync(string id, CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetLyricsListForSongIdAsync(id, ApiRequest, cancellationToken));
    }


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
        return ImageResult(id, openSubsonicApiService.GetImageForApiKeyId(id,
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
            return File(streamResult.Bytes, streamResult.ContentType ?? string.Empty, streamResult.FileName);
        }

        Response.StatusCode = (int)HttpStatusCode.NotFound;
        return new JsonStringResult(Serializer.Serialize(new ResponseModel
        {
            UserInfo = UserInfo.BlankUserInfo,
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
            UserInfo = UserInfo.BlankUserInfo,
            IsSuccess = false,
            ResponseData = await openSubsonicApiService.NewApiResponse(
                false,
                string.Empty,
                string.Empty,
                Error.DataNotFoundError)
        })!);
    }
}
