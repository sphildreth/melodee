using ATL.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers;

public class ImageController(Serilog.ILogger logger)
{
    /// <summary>
    ///     Returns an image based on id and size.
    /// </summary>
    /// <param name="apiKey">Key for the image request</param> 
    /// <param name="size">Image size for the image request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [Route("/images/{apiKey}/{size}")]
    public async Task<IActionResult> GetAvatarAsync(string apiKey, string size, CancellationToken cancellationToken = default)
    {
        //        return $"{baseUrl}/images/{apiKey}/{imageSize.ToString().ToLower()}";
        
        logger.Warning("Image Request for [{ApiKey}] Size [{Size}", apiKey, size);
        
        throw new NotImplementedException();
    }
}
