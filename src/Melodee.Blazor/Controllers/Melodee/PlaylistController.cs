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
public class PlaylistController(
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
    [Route("{apiKey:guid}/songs")]
    public async Task<IActionResult> SongsForPlaylist(Guid apiKey, int? page, short? pageSize, CancellationToken cancellationToken = default)
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

        var userInfo = userResult.Data.ToUserInfo();
        var playlistResult = await playlistService.GetByApiKeyAsync(userInfo, apiKey, cancellationToken);
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
            cancellationToken);
        var baseUrl = GetBaseUrl(Configuration);
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
