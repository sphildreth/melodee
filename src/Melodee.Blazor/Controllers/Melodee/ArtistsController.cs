using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Filtering;
using Melodee.Common.Models;
using Melodee.Common.Models.Collection;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Mvc;
using Album = Melodee.Blazor.Controllers.Melodee.Models.Album;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class ArtistsController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    ArtistService artistService,
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
    public async Task<IActionResult> ArtistById(Guid id, CancellationToken cancellationToken = default)
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

        var artistResult = await artistService.GetByApiKeyAsync(id, cancellationToken).ConfigureAwait(false);
        if (!artistResult.IsSuccess || artistResult.Data == null)
        {
            return NotFound(new { error = "Artist not found" });
        }

        return Ok(artistResult.Data.ToArtistDataInfo().ToArtistModel(
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

        var listResult = await artistService.ListAsync(new PagedRequest
        {
            Page = page,
            PageSize = pageSize,
            OrderBy = new Dictionary<string, string> { { orderByValue, orderDirectionValue } }
        }, cancellationToken).ConfigureAwait(false);

        var baseUrl = GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false));

        return Ok(new
        {
            meta = new PaginationMetadata(
                listResult.TotalCount,
                pageSize,
                page,
                listResult.TotalPages
            ),
            data = listResult.Data.Select(x => x.ToArtistModel(baseUrl, userResult.Data.ToUserModel(baseUrl))).ToArray()
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

        
        var artistRecentResult = await artistService.ListAsync(new PagedRequest
        {
            Page = 1,
            PageSize = limit,
            OrderBy = new Dictionary<string, string> { { nameof(AlbumDataInfo.CreatedAt), PagedRequest.OrderDescDirection } }
        }, cancellationToken).ConfigureAwait(false);

        var baseUrl = GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false));

        return Ok(new
        {
            meta = new PaginationMetadata(
                artistRecentResult.TotalCount,
                limit,
                1,
                artistRecentResult.TotalPages
            ),
            data = artistRecentResult.Data.Select(x => x.ToArtistModel(baseUrl, userResult.Data.ToUserModel(baseUrl))).ToArray()
        });
    }

    [HttpGet]
    [Route("{id:guid}/albums")]
    public async Task<IActionResult> ArtistAlbumsAsync(Guid id, short page, short pageSize, CancellationToken cancellationToken = default)
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
        
        var artistResult = await artistService.GetByApiKeyAsync(id, cancellationToken).ConfigureAwait(false);
        if (!artistResult.IsSuccess || artistResult.Data == null)
        {
            return NotFound(new { error = "Artist not found" });
        }
     
        var pageValue = page > 0 ? page : (short)1;
        
        var artistAlbumsResult = await albumService.ListAsync(new PagedRequest
        {
            Page = pageValue,
            PageSize = pageSize,
            FilterBy =
            [
                new FilterOperatorInfo("Id", FilterOperator.Equals, artistResult.Data.Id)
            ],
            OrderBy = new Dictionary<string, string> { { nameof(AlbumDataInfo.CreatedAt), PagedRequest.OrderDescDirection } }
        }, "ar", cancellationToken).ConfigureAwait(false);        
        
        var baseUrl = GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false));

        return Ok(new
        {
            meta = new PaginationMetadata(
                artistAlbumsResult.TotalCount,
                pageSize,
                page,
                artistAlbumsResult.TotalPages
            ),
            data = artistAlbumsResult.Data.Select(x => x.ToAlbumModel(baseUrl, userResult.Data.ToUserModel(baseUrl))).ToArray()
        });
    }

    [HttpGet]
    [Route("{id:guid}/songs")]
    public Task<IActionResult> ArtistSongsAsync(Guid id, short page, short pageSize, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
