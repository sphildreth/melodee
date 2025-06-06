using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Collection;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class AlbumsController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    AlbumService albumService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    
    [HttpGet]
    [Route("{id:guid}")]
    public async Task<IActionResult> AlbumById(Guid id, CancellationToken cancellationToken = default)
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
        
        var albumResult = await albumService.GetByApiKeyAsync(id, cancellationToken).ConfigureAwait(false);
        if (!albumResult.IsSuccess || albumResult.Data == null)
        {
            return NotFound(new { error = "Album not found" });
        }
        return Ok(albumResult.Data.ToAlbumDataInfo().ToAlbumModel(
            GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false)),
            userResult.Data.ToUserModel(GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false)))));
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
        
        var listResult = await albumService.ListAsync(new PagedRequest
        {
            Page = page,
            PageSize = pageSize,
            OrderBy = new Dictionary<string, string>{{ orderByValue, orderDirectionValue}}
        }, null, cancellationToken).ConfigureAwait(false);
        
        var baseUrl = GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false));

        return Ok(new
        {
            meta = new PaginationMetadata(
                listResult.TotalCount,
                pageSize,
                page,
                listResult.TotalPages
            ),
            data = listResult.Data.Select(x => x.ToAlbumModel(baseUrl, userResult.Data.ToUserModel(baseUrl))).ToArray()
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
        
        var albumRecentResult = await albumService.ListAsync(new PagedRequest
        {
            Page = 1,
            PageSize = limit,
            OrderBy = new Dictionary<string, string>{{ nameof(AlbumDataInfo.CreatedAt), PagedRequest.OrderDescDirection}}
        }, null, cancellationToken).ConfigureAwait(false);
        
        var baseUrl = GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false));
        
        return Ok(new
        {
            meta = new PaginationMetadata(
                albumRecentResult.TotalCount,
                limit,
                1,
                albumRecentResult.TotalPages
            ),
            data = albumRecentResult.Data.Select(x => x.ToAlbumModel(baseUrl, userResult.Data.ToUserModel(baseUrl))).ToArray()
        });
    }
    
    [HttpGet]
    [Route("{id:guid}/songs")]
    public async Task<IActionResult> AlbumSongsAsync(Guid id, CancellationToken cancellationToken = default)
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
        
        var albumResult = await albumService.GetByApiKeyAsync(id, cancellationToken).ConfigureAwait(false);
        if (!albumResult.IsSuccess || albumResult.Data == null)
        {
            return NotFound(new { error = "Album not found" });
        }

        var baseUrl = GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false));
        
        return Ok(new
        {
            meta = new PaginationMetadata(
                albumResult.Data.Songs.Count,
                albumResult.Data.SongCount ?? 0,
                1,
                1
            ),
            data = albumResult.Data.Songs.Select(x => x.ToSongDataInfo().ToSongModel(baseUrl, userResult.Data.ToUserModel(baseUrl), userResult.Data.PublicKey)).ToArray()
        });        
    }
    
}
