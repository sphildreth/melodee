using Melodee.Common.Serialization;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace Melodee.Controllers.OpenSubsonic;

public class SystemController(ILogger logger, ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase(serializer)
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
