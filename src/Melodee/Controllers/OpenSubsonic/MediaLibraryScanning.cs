using Melodee.Common.Serialization;
using Melodee.Results;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class MediaLibraryScanning(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{
    /// <summary>
    ///     Initiates a rescan of the media libraries.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/startScan.view")]
    public async Task<IActionResult> StartScanAsync(CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.StartScanAsync(ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }

    /// <summary>
    ///     Returns the current status for media library scanning.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [HttpPost]
    [Route("/rest/getScanStatus.view")]
    public async Task<IActionResult> GetScanStatusAsync(CancellationToken cancellationToken = default)
    {
        return new JsonStringResult(serializer.Serialize(await openSubsonicApiService.GetScanStatusAsync(ApiRequest, cancellationToken).ConfigureAwait(false))!);
    }
}
