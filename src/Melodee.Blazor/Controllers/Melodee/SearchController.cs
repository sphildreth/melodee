using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Blazor.Services;
using Melodee.Common.Configuration;
using Melodee.Common.Models.Search;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.Melodee;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class SearchController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    SearchService searchService,
    IConfiguration configuration,
    IBlacklistService blacklistService,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    [HttpPost]
    public async Task<IActionResult> SearchAsync([FromBody] SearchRequest searchRequest, CancellationToken cancellationToken = default)
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

        var pageSizeValue = searchRequest.PageSize ?? 50;

        var includedTyped = SearchInclude.Data;
        var typeValue = searchRequest.Type?.Split(',');
        if (typeValue is { Length: > 0 })
        {
            includedTyped = SearchInclude.Data;
            foreach (var t in typeValue)
            {
                if (Enum.TryParse<SearchInclude>(t, out var include))
                {
                    includedTyped |= include;
                }
            }
        }

        var searchResult = await searchService.DoSearchAsync(userResult.Data.ApiKey,
                ApiRequest.ApiRequestPlayer.UserAgent,
                searchRequest.Query,
                searchRequest.AlbumPageValue,
                searchRequest.ArtistPageValue,
                searchRequest.SongPageValue,
                pageSizeValue,
                includedTyped,
                cancellationToken)
            .ConfigureAwait(false);
        var baseUrl = GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false));
        return Ok(new
        {
            meta = new PaginationMetadata(
                searchResult.Data.TotalCount,
                pageSizeValue,
                1,
                searchResult.Data.TotalCount < 1 ? 0 : (searchResult.Data.TotalCount + pageSizeValue - 1) / pageSizeValue
            ),
            data = searchResult.Data.ToSearchResultModel(baseUrl, userResult.Data.ToUserModel(baseUrl), userResult.Data.PublicKey)
        });
    }

    [HttpGet]
    [Route("songs")]
    public async Task<IActionResult> SearchSongsAsync(string q, short? page, short? pageSize, CancellationToken cancellationToken = default)
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


        var searchResult = await searchService.DoSearchAsync(userResult.Data.ApiKey,
                ApiRequest.ApiRequestPlayer.UserAgent,
                q,
                0,
                0,
                pageValue,
                pageSizeValue,
                SearchInclude.Songs,
                cancellationToken)
            .ConfigureAwait(false);
        var baseUrl = GetBaseUrl(await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false));
        return Ok(new
        {
            meta = new PaginationMetadata(
                searchResult.Data.TotalCount,
                pageSizeValue,
                pageValue,
                searchResult.Data.TotalCount < 1 ? 0 : (searchResult.Data.TotalCount + pageSizeValue - 1) / pageSizeValue
            ),
            data = searchResult.Data.Songs.Select(x => x.ToSongModel(baseUrl, userResult.Data.ToUserModel(baseUrl), userResult.Data.PublicKey))
        });
    }
}
