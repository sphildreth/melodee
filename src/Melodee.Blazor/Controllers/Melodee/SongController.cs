using Asp.Versioning;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class SongController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    SongService songService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{

    
    [HttpGet]
    [Route("/song/stream/{apiKey:guid}")]
    public async Task<IActionResult> StreamSong(Guid apiKey, CancellationToken cancellationToken = default)
    {
        if (!ApiRequest.IsAuthorized)
        {
            return Unauthorized(new { error = "Authorization token is missing" });
        }

        var userResult = await userService.GetByApiKeyAsync(SafeParser.ToGuid(ApiRequest.ApiKey) ?? Guid.Empty, cancellationToken);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            return Unauthorized(new { error = "Authorization token is invalid" });
        }        
        
        var streamResult = await songService.GetStreamForSongAsync(userResult.Data.ToUserInfo(), apiKey, cancellationToken);
        if (!streamResult.IsSuccess)
        {
            return BadRequest(new { error = "Unable to load song" });
        }
        foreach (var responseHeader in streamResult.Data.ResponseHeaders)
        {
            Response.Headers[responseHeader.Key] = responseHeader.Value;
        }
        
        await Response.Body
            .WriteAsync(streamResult.Data.Bytes.AsMemory(0, streamResult.Data.Bytes.Length), cancellationToken)
            .ConfigureAwait(false);
        return Empty;
    }
}
