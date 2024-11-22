using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Serialization;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class MediaRetrievalController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{
    //download
    //getAvatar
    //getCaptions
    //getCoverArt
    //getLyrics
    //hls
    //stream

    /// <summary>
    ///     Returns a cover art image.
    /// </summary>
    /// <param name="id">Composite ID of type:apikey</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/getCoverArt.view")]
    public async Task<IActionResult> GetCoverArt(string id, CancellationToken cancellationToken = default)
    {
        var coverArtResult = await openSubsonicApiService.GetCoverArt(id, null, ApiRequest, cancellationToken);
        if (coverArtResult.IsSuccess)
        {
            return new FileContentResult(coverArtResult.ResponseData.Data as byte[], "image/jpeg");
        }

        return new NotFoundResult();
    }

    /// <summary>
    ///     Streams a given media file.
    /// </summary>
    /// <param name="request">Stream model for parameters for streaming.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/stream.view")]
    public async Task<IActionResult> Stream(StreamRequest request, CancellationToken cancellationToken = default)
    {
        // var rangeHeader = headers["Range"];
        // var rangeBegin = rangeHeader.FirstOrDefault();
        // if (!string.IsNullOrEmpty(rangeBegin))
        // {
        //     //bytes=0-
        //     rangeBegin = rangeBegin.Replace("bytes=", string.Empty);
        //     var parts = rangeBegin.Split('-');
        //     rangeBegin = parts[0];
        //     if (!string.IsNullOrEmpty(rangeBegin))
        //     {
        //         long.TryParse(rangeBegin, out result);
        //     }
        // }

        // var defaultFileLength = fileLength - 1;
        // if (headers?.Any(x => x.Key == "Range") != true)
        // {
        //     return defaultFileLength;
        // }
        //
        // long? result = null;
        // var rangeHeader = headers["Range"];
        // string rangeEnd = null;
        // var rangeBegin = rangeHeader.FirstOrDefault();
        // if (!string.IsNullOrEmpty(rangeBegin))
        // {
        //     //bytes=0-
        //     rangeBegin = rangeBegin.Replace("bytes=", string.Empty);
        //     var parts = rangeBegin.Split('-');
        //     rangeBegin = parts[0];
        //     if (parts.Length > 1)
        //     {
        //         rangeEnd = parts[1];
        //     }
        //
        //     if (!string.IsNullOrEmpty(rangeEnd))
        //     {
        //         result = long.TryParse(rangeEnd, out var outValue) ? (int?)outValue : null;
        //     }
        // }        

        //public string AcceptRanges => "bytes";
        //info.ContentRange = $"bytes {beginBytes}-{endBytes}/{contentLength}";

        var streamResult = await openSubsonicApiService.StreamAsync(request, ApiRequest, cancellationToken).ConfigureAwait(false);
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        //Response.Headers.Add("Content-Length", info.Data.ContentLength);
        //w.Header().Set("X-Content-Duration", strconv.FormatFloat(float64(stream.Duration()), 'G', -1, 32))
        // Response.Headers.Add("Content-Duration", info.Data.ContentDuration);
        // if (!info.Data.IsFullRequest)
        // {
        //     Response.Headers.Add("Accept-Ranges", info.Data.AcceptRanges);
        //     Response.Headers.Add("Content-Range", info.Data.ContentRange);
        // }

        // public string ContentType => MimeTypeHelper.Mp3MimeType;

        // estimateContentLength := req.Params(r).BoolOr("estimateContentLength", false)
        //
        // // if Client requests the estimated content-length, send it
        // if estimateContentLength {
        //     length := strconv.Itoa(stream.EstimatedContentLength())
        //     log.Trace(ctx, "Estimated content-length", "contentLength", length)
        //     w.Header().Set("Content-Length", length)
        // }        

        //  Response.Body.WriteAsync()
        throw new NotImplementedException();
    }
}
