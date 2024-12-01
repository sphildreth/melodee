using System.Xml;
using System.Xml.Serialization;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Common.Serialization;
using Melodee.Results;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class SystemController(Serilog.ILogger logger, ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{
    /// <summary>
    ///     List the OpenSubsonic extensions supported by this server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getOpenSubsonicExtensions.view")]
    public async Task<IActionResult> GetOpenSubsonicExtensionsAsync(CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetOpenSubsonicExtensionsAsync(ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }


    /// <summary>
    ///     Return system ping response
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/ping.view")]
    [Route("/rest/ping")]
    public async Task<IActionResult> Ping(CancellationToken cancellationToken = default)
    {

        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.PingAsync(ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }

    /// <summary>
    ///     Get details about the software license.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getLicense.view")]
    public async Task<IActionResult> GetLicenseAsync(CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetLicenseAsync(ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }
}
