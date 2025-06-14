using Asp.Versioning;
using Melodee.Blazor.Controllers.Melodee.Extensions;
using Melodee.Blazor.Controllers.Melodee.Models;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.Melodee;

/// <summary>
///     This controller is used to get meta-information about the API.
/// </summary>
[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class SystemController(
    ISerializer serializer,
    EtagRepository etagRepository,
    UserService userService,
    StatisticsService statisticsService,
    IConfiguration configuration,
    IMelodeeConfigurationFactory configurationFactory) : ControllerBase(
    etagRepository,
    serializer,
    configuration,
    configurationFactory)
{
    [HttpGet]
    [Route("info")]
    public async Task<IActionResult> GetServerInfo(CancellationToken cancellationToken = default)
    {
        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var versionInfo = configuration.ApiVersion();

        var versionParts = versionInfo.Split('.');
        if (versionParts.Length < 3)
        {
            return BadRequest(new { error = "Invalid version format" });
        }

        var majorVersion = SafeParser.ToNumber<int?>(versionParts[0]) ?? 0;
        var minorVersion = SafeParser.ToNumber<int?>(versionParts[1]) ?? 0;
        var patchVersion = SafeParser.ToNumber<int?>(versionParts[2]) ?? 0;

        return Ok(new ServerInfo(configuration.GetValue<string>(SettingRegistry.OpenSubsonicServerType) ?? "Melodee",
            "Melodee API",
            majorVersion,
            minorVersion,
            patchVersion));
    }

    /// <summary>
    ///     Return some statistics about the system.
    /// </summary>
    [HttpGet]
    [Route("stats")]
    public async Task<IActionResult> GetSystemStatsAsync(CancellationToken cancellationToken = default)
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

        var statsResult = await statisticsService.GetStatisticsAsync(cancellationToken).ConfigureAwait(false);

        return Ok(statsResult.Data.Where(x => x.IncludeInApiResult ?? false).Select(x => x.ToStatisticModel()).ToArray());
    }
}
