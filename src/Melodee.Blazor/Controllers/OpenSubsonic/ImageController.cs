using Melodee.Blazor.Filters;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public class ImageController(OpenSubsonicApiService openSubsonicApiService, EtagRepository etagRepository, ISerializer serializer) : ControllerBase(etagRepository, serializer)
{
    /// <summary>
    ///     Returns an image based on id and size.
    /// </summary>
    /// <param name="id">Key for the image request</param>
    /// <param name="size">Image size for the image request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [Route("/images/{id}/{size}")]
    public Task<IActionResult> GetImageAsync(string id, string size, CancellationToken cancellationToken = default)
    {
        return ImageResult(id,openSubsonicApiService.GetImageForApiKeyId(id,
            size,
            ApiRequest,
            cancellationToken));
    }
}
