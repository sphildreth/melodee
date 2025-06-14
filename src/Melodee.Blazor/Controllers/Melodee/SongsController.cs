using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Blazor.Services;
using Melodee.Common.Configuration;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Collection;
using Melodee.Common.Security;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class SongsController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    SongService songService,
    IConfiguration configuration,
    IBlacklistService blacklistService,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    [HttpGet]
    [Route("{id:guid}")]
    public Task<IActionResult> SongById(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync(short page, short pageSize, string? orderBy, string? orderDirection, CancellationToken cancellationToken = default)
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

        var orderByValue = orderBy ?? nameof(AlbumDataInfo.CreatedAt);
        var orderDirectionValue = orderDirection ?? PagedRequest.OrderDescDirection;

        var listResult = await songService.ListAsync(new PagedRequest
        {
            Page = page,
            PageSize = pageSize,
            OrderBy = new Dictionary<string, string> { { orderByValue, orderDirectionValue } }
        }, userResult.Data.Id, cancellationToken).ConfigureAwait(false);

        var baseUrl = GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false));

        return Ok(new
        {
            meta = new PaginationMetadata(
                listResult.TotalCount,
                pageSize,
                page,
                listResult.TotalPages
            ),
            data = listResult.Data.Select(x => x.ToSongModel(baseUrl, userResult.Data.ToUserModel(baseUrl), userResult.Data.PublicKey)).ToArray()
        });
    }

    [HttpGet]
    [Route("recent")]
    public async Task<IActionResult> RecentlyAddedAsync(short limit, CancellationToken cancellationToken = default)
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

        var songRecentResult = await songService.ListAsync(new PagedRequest
        {
            Page = 1,
            PageSize = limit,
            OrderBy = new Dictionary<string, string> { { nameof(AlbumDataInfo.CreatedAt), PagedRequest.OrderDescDirection } }
        }, userResult.Data.Id, cancellationToken).ConfigureAwait(false);

        var baseUrl = GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false));

        return Ok(new
        {
            meta = new PaginationMetadata(
                songRecentResult.TotalCount,
                limit,
                1,
                songRecentResult.TotalPages
            ),
            data = songRecentResult.Data.Select(x => x.ToSongModel(baseUrl, userResult.Data.ToUserModel(baseUrl), userResult.Data.PublicKey)).ToArray()
        });
    }

    [HttpPost]
    [Route("starred/{apiKey:guid}/{isStarred:bool}")]
    public async Task<IActionResult>? ToggleSongStarred(Guid apiKey, bool isStarred, CancellationToken cancellationToken = default)
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

        var toggleStarredResult = await userService.ToggleSongStarAsync(userResult.Data.Id, apiKey, isStarred, cancellationToken).ConfigureAwait(false);
        if (toggleStarredResult.IsSuccess)
        {
            return Ok();
        }

        return BadRequest("Unable to toggle star for song for user.");
    }

    [HttpGet]
    [Route("/song/stream/{apiKey:guid}/{userApiKey:guid}/{authToken}")]
    public async Task<IActionResult> StreamSong(Guid apiKey, Guid userApiKey, string authToken, CancellationToken cancellationToken = default)
    {
        var userResult = await userService.GetByApiKeyAsync(userApiKey, cancellationToken).ConfigureAwait(false);
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

        var hmacService = new HmacTokenService(userResult.Data.PublicKey);
        var authTokenValidation = hmacService.ValidateTimedToken(authToken.FromBase64());

        if (!authTokenValidation)
        {
            return Unauthorized(new { error = "Invalid Auth Token" });
        }

        var streamResult = await songService.GetStreamForSongAsync(userResult.Data.ToUserInfo(), apiKey, cancellationToken).ConfigureAwait(false);
        if (!streamResult.IsSuccess)
        {
            return BadRequest(new { error = "Unable to load song" });
        }

        Response.Headers.Clear();

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
