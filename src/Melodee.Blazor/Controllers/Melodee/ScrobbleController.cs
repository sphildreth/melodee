using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Blazor.Services;
using Melodee.Common.Configuration;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class ScrobbleController(
    Serilog.ILogger logger,
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    SongService songService,
    ScrobbleService scrobbleService,
    IConfiguration configuration,
    IBlacklistService blacklistService,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    
    [HttpPost]
    public async Task<IActionResult> ScrobbleSong([FromBody]ScrobbleRequest scrobbleRequest, CancellationToken cancellationToken = default)
    {
        var userResult = await userService.GetByApiKeyAsync(scrobbleRequest.UserId, cancellationToken).ConfigureAwait(false);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            return Unauthorized(new { error = "Unknown user" });
        }

        if (await blacklistService.IsEmailBlacklistedAsync(userResult.Data.Email).ConfigureAwait(false) || 
            await blacklistService.IsIpBlacklistedAsync(GetRequestIp(HttpContext)).ConfigureAwait(false))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "User is blacklisted" });
        } 
        
        var songRequest = await songService.GetByApiKeyAsync(scrobbleRequest.SongId, cancellationToken).ConfigureAwait(false);
        if (!songRequest.IsSuccess || songRequest.Data == null)
        {
            logger.Warning("[{ControllerName}] [{MethodName}] Scrobble request for unknown song [{Request}]",
                nameof(ScrobbleController),
                nameof(ScrobbleSong),
                scrobbleRequest);
            return BadRequest(new { error = "Unknown song" });
        }
        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        await scrobbleService.InitializeAsync(configuration, cancellationToken).ConfigureAwait(false);
        
        OperationResult<bool>? result = null;
        
        if (scrobbleRequest.ScrobbleTypeValue == ScrobbleRequestType.NowPlaying)
        {
            result = await scrobbleService.NowPlaying(
                    userResult.Data.ToUserInfo(),
                    scrobbleRequest.SongId,
                    scrobbleRequest.PlayedDuration,
                    scrobbleRequest.PlayerName,
                    cancellationToken)
                .ConfigureAwait(false); 
            
        }
        else if (scrobbleRequest.ScrobbleTypeValue == ScrobbleRequestType.Played)
        {
            result = await scrobbleService.Scrobble(
                    userResult.Data.ToUserInfo(),
                    scrobbleRequest.SongId,
                    false,
                    scrobbleRequest.PlayerName,
                    cancellationToken)
                .ConfigureAwait(false);        
        }

        if (result != null)
        {
            if (result.IsSuccess)
            {
                return Ok();
            }
            logger.Warning("[{ControllerName}] [{MethodName}] Scrobble request for unknown song [{Request}] Message [{Message}",
                nameof(ScrobbleController),
                nameof(ScrobbleSong),
                scrobbleRequest,
                result.Messages?.First() ?? "Unknown error");
            return BadRequest(result.Messages?.First() ?? "Unknown error");
        }
        logger.Warning("[{ControllerName}] [{MethodName}] Scrobble request for unknown song [{Request}] Message [{Message}",
            nameof(ScrobbleController),
            nameof(ScrobbleSong),
            scrobbleRequest,
            $"Unknown scrobble type: { scrobbleRequest.ScrobbleType}");        
        return BadRequest($"Unknown scrobble type: { scrobbleRequest.ScrobbleType}");
    }
    
}
