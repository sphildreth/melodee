using Melodee.Common.Serialization;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class MediaLibraryScanning(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase(serializer)
{
    /// <summary>
    ///     Initiates a rescan of the media libraries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/startScan.view")]
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
    public Task<IActionResult> GetScanStatusAsync(CancellationToken cancellationToken = default)
    {
        return MakeResult(openSubsonicApiService.GetScanStatusAsync(ApiRequest, cancellationToken));
    }
}
