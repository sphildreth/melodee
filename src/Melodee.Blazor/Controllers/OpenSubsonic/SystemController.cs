using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public class SystemController(EtagRepository etagRepository, ISerializer serializer, OpenSubsonicApiService openSubsonicApiService, IMelodeeConfigurationFactory configurationFactory) : ControllerBase(etagRepository, serializer, configurationFactory)
{
    /// <summary>
    ///     List the OpenSubsonic extensions supported by this server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getOpenSubsonicExtensions.view")]
    [Route("/rest/getOpenSubsonicExtensions")]
    public Task<IActionResult> GetOpenSubsonicExtensionsAsync(CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetOpenSubsonicExtensionsAsync(ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Return system ping response
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/ping")]
    [Route("/rest/ping")]
    [Route("/rest/ping.view")]
    public Task<IActionResult> Ping(CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.PingAsync(ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Get details about the software license.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getLicense.view")]
    [Route("/rest/getLicense")]
    public Task<IActionResult> GetLicenseAsync(CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetLicenseAsync(ApiRequest, cancellationToken));
    }
}
