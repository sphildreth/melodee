using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

public class MediaLibraryScanning(ISerializer serializer, EtagRepository etagRepository, OpenSubsonicApiService openSubsonicApiService, IMelodeeConfigurationFactory configurationFactory) : ControllerBase(etagRepository, serializer, configurationFactory)
{
    /// <summary>
    ///     Initiates a rescan of the media libraries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/startScan.view")]
    [Route("/rest/startScan")]
    public Task<IActionResult> StartScanAsync(CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.StartScanAsync(ApiRequest, cancellationToken));
    }

    /// <summary>
    ///     Returns the current status for media library scanning.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getScanStatus.view")]
    [Route("/rest/getScanStatus")]
    public Task<IActionResult> GetScanStatusAsync(CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetScanStatusAsync(ApiRequest, cancellationToken));
    }
}
