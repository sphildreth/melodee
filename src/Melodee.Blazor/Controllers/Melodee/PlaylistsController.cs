using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class PlaylistsController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    PlaylistService playlistService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    [HttpGet]
    [Route("{id:guid}")]
    public async Task<IActionResult> PlaylistById(Guid id, CancellationToken cancellationToken = default)
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
        
        var playlistResult = await playlistService.GetByApiKeyAsync(userResult.Data.ToUserInfo(), id, cancellationToken).ConfigureAwait(false);
        if (!playlistResult.IsSuccess || playlistResult.Data == null)
        {
            return NotFound(new { error = "Playlist not found" });
        }
        return Ok(playlistResult.Data.ToPlaylistModel(
            GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false)),
            userResult.Data.ToUserModel(GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false)))));
    }     
    
    [HttpGet]
    public async Task<IActionResult> ListAsync(short page, short pageSize, CancellationToken cancellationToken = default)
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
        
        var playlists = await playlistService.ListAsync(userResult.Data.ToUserInfo(), new PagedRequest { Page = page, PageSize = pageSize }, cancellationToken).ConfigureAwait(false);
        var baseUrl = GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false));
        return Ok(new
        {
            meta = new PaginationMetadata(
                playlists.TotalCount,
                page,
                pageSize,
                playlists.TotalPages
            ),
            data = playlists.Data.Select(x => x.ToPlaylistModel(baseUrl, userResult.Data.ToUserModel(baseUrl))).ToArray()
        });
    }     
    
    [HttpGet]
    [Route("{apiKey:guid}/songs")]
    public async Task<IActionResult> SongsForPlaylist(Guid apiKey, int? page, short? pageSize, CancellationToken cancellationToken = default)
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

        var userInfo = userResult.Data.ToUserInfo();
        var playlistResult = await playlistService.GetByApiKeyAsync(userInfo, apiKey, cancellationToken).ConfigureAwait(false);
        if (!playlistResult.IsSuccess || playlistResult.Data == null)
        {
            return BadRequest(new { error = "Playlist not found" });
        }

        var pageValue = page ?? 1;
        var pageSizeValue = pageSize ?? 50;
        var songsForPlaylistResult = await playlistService.SongsForPlaylistAsync(apiKey,
            userInfo,
            new PagedRequest
            {
                PageSize = pageSizeValue,
                Page = pageValue
            },
            cancellationToken).ConfigureAwait(false);
        var baseUrl = GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false));
        return Ok(new
        {
            meta = new PaginationMetadata(
                songsForPlaylistResult.TotalCount,
                pageSizeValue,
                pageValue,
                songsForPlaylistResult.TotalPages
            ),
            data = songsForPlaylistResult.Data.Select(x => x.ToSongModel(baseUrl, userResult.Data.ToUserModel(baseUrl), userResult.Data.PublicKey))
        });
    }
}
