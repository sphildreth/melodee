using Melodee.Common.Serialization;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace Melodee.Controllers.OpenSubsonic;

public class ImageController(ILogger logger, ISerializer serializer) : ControllerBase(serializer)
{
    /// <summary>
    ///     Returns an image based on id and size.
    /// </summary>
    /// <param name="apiKey">Key for the image request</param>
    /// <param name="size">Image size for the image request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [Route("/images/{apiKey}/{size}")]
    public async Task<IActionResult> GetImageAsync(string apiKey, string size, CancellationToken cancellationToken = default)
    {
        // TODO From logs using Dsub
        // images/artist_3f9b4979-570d-4d9c-a172-36faafcce50a/large
        
        //        return $"{baseUrl}/images/{apiKey}/{imageSize.ToString().ToLower()}";

        logger.Warning("Image Request for [{ApiKey}] Size [{Size}]", apiKey, size);

        throw new NotImplementedException();
    }
}
