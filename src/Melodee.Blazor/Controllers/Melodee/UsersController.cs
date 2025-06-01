using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Blazor.Services;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    PlaylistService playlistService,
    IConfiguration configuration,
    IBlacklistService blacklistService,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    [HttpPost]
    [Route("authenticate")]
    public async Task<IActionResult> AuthenticateUserAsync([FromBody] LoginModel model, CancellationToken cancellationToken = default)
    {
        var authResult = await userService.LoginUserAsync(model.Email, model.Password, cancellationToken).ConfigureAwait(false);
        if (!authResult.IsSuccess || authResult.Data == null)
        {
            return Unauthorized();
        }

        if (authResult.Data.IsLocked)
        {
            return Forbid("User is locked");
        }

        if (await blacklistService.IsEmailBlacklistedAsync(authResult.Data.Email).ConfigureAwait(false) || 
            await blacklistService.IsIpBlacklistedAsync(GetRequestIp(HttpContext)).ConfigureAwait(false))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "User is blacklisted" });
        }
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(Configuration.GetSection("MelodeeAuthSettings:Token").Value!);
        var tokenHoursString = Configuration.GetSection("MelodeeAuthSettings:TokenHours").Value;
        var tokenHours = SafeParser.ToNumber<int>(tokenHoursString);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Email, authResult.Data.Email),
                new Claim(ClaimTypes.Name, authResult.Data.UserName),
                new Claim(ClaimTypes.Sid, authResult.Data.ApiKey.ToString())
            ]),
            Expires = DateTime.Now.AddHours(tokenHours),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var serverVersion = configuration.ApiVersion();
        return Ok(new
        {
            user = authResult.Data.ToUserModel(GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false))),
            serverVersion = Configuration.GetSection("MelodeeSettings:Version").Value,
            token = tokenHandler.WriteToken(token)
        });
    }

    /// <summary>
    /// Return information about the current user making the request.
    /// </summary>
    [HttpGet]
    [Route("me")]
    public async Task<IActionResult> AboutMeAsync(CancellationToken cancellationToken = default)
    {
        if (!ApiRequest.IsAuthorized)
        {
            return Unauthorized(new { error = "Authorization token is invalid" });
        }

        var userResult = await userService.GetByApiKeyAsync(SafeParser.ToGuid(ApiRequest.ApiKey) ?? Guid.Empty, cancellationToken).ConfigureAwait(false);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            return Unauthorized(new { error = "Authorization token is invalid" });
        }

        return Ok(userResult.Data.ToUserModel(GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false))));
    }

    /// <summary>
    /// Return the last three songs played by the user
    /// </summary>
    [HttpGet]
    [Route("lastPlayed")]
    public async Task<IActionResult> Last3PlayedSongsForUserAsync(short page, short pageSize, CancellationToken cancellationToken = default)
    {
        if (!ApiRequest.IsAuthorized)
        {
            return Unauthorized(new { error = "Authorization token is invalid" });
        }

        var userResult = await userService.GetByApiKeyAsync(SafeParser.ToGuid(ApiRequest.ApiKey) ?? Guid.Empty, cancellationToken).ConfigureAwait(false);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            return Unauthorized(new { error = "Authorization token is invalid" });
        }

        if (userResult.Data.IsLocked)
        {
            return Forbid("User is locked");
        }
        
        if (await blacklistService.IsEmailBlacklistedAsync(userResult.Data.Email).ConfigureAwait(false) || 
            await blacklistService.IsIpBlacklistedAsync(GetRequestIp(HttpContext)).ConfigureAwait(false))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "User is blacklisted" });
        }

        // TODO this should be paginated
        var userLastPlayedResult = await userService.UserLastPlayedSongsAsync(userResult.Data.Id, 3, cancellationToken).ConfigureAwait(false);
        return Ok(new
        {
            meta = new PaginationMetadata(
                10,
                10,
                1,
                1
            ),
            data = userLastPlayedResult.Data.Where(x => x?.Song != null).Select(x => x!.Song.ToSongDataInfo()).ToArray()
        });
    }

    [HttpGet]
    [Route("playlists")]
    public async Task<IActionResult> UsersPlaylistsAsync(int? page, short? pageSize, CancellationToken cancellationToken = default)
    {
        if (!ApiRequest.IsAuthorized)
        {
            return Unauthorized(new { error = "Authorization token is invalid" });
        }

        var userResult = await userService.GetByApiKeyAsync(SafeParser.ToGuid(ApiRequest.ApiKey) ?? Guid.Empty, cancellationToken).ConfigureAwait(false);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            return Unauthorized(new { error = "Authorization token is invalid" });
        }

        if (userResult.Data.IsLocked)
        {
            return Forbid("User is locked");
        }

        if (await blacklistService.IsEmailBlacklistedAsync(userResult.Data.Email).ConfigureAwait(false) || 
            await blacklistService.IsIpBlacklistedAsync(GetRequestIp(HttpContext)).ConfigureAwait(false))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "User is blacklisted" });
        }
        
        var pageValue = page ?? 1;
        var pageSizeValue = pageSize ?? 50;
        var playlists = await playlistService.ListAsync(userResult.Data.ToUserInfo(), new PagedRequest { Page = pageValue, PageSize = pageSizeValue }, cancellationToken).ConfigureAwait(false);
        var baseUrl = GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false));
        return Ok(new
        {
            meta = new PaginationMetadata(
                playlists.TotalCount,
                pageSizeValue,
                pageValue,
                playlists.TotalPages
            ),
            data = playlists.Data.Select(x => x.ToPlaylistModel(baseUrl, userResult.Data.ToUserModel(baseUrl))).ToArray()
        });
    }
}
