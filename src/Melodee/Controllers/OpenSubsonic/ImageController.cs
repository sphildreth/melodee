using Melodee.Common.Serialization;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace Melodee.Controllers.OpenSubsonic;

public class ImageController(ILogger logger, OpenSubsonicApiService openSubsonicApiService, ISerializer serializer) : ControllerBase(serializer)
{
    /// <summary>
    ///     Returns an image based on id and size.
    /// </summary>
    /// <param name="id">Key for the image request</param>
    /// <param name="size">Image size for the image request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [Route("/images/{apiKey}/{size}")]
    public Task<IActionResult> GetImageAsync(string id, string size, CancellationToken cancellationToken = default)
    {
        return ImageResult(openSubsonicApiService.GetImageForApiKeyId(id,
            size,
            ApiRequest,
            cancellationToken));
    }
}
