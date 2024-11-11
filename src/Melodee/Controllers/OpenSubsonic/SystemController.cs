using Mapster;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class SystemController(OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{
    
    //getOpenSubsonicExtensions

    /// <summary>
    /// Return system ping response
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/ping.view")]
    public async Task<IActionResult> Ping(CancellationToken cancellationToken = default) 
        => new JsonResult(await openSubsonicApiService.AuthenticateSubsonicApiAsync(ApiRequest, cancellationToken).ConfigureAwait(false));
    
    /// <summary>
    /// Get details about the software license.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/getLicense.view")]
    public async Task<IActionResult> GetLicense(CancellationToken cancellationToken = default) 
        => new JsonResult(await openSubsonicApiService.GetLicense(ApiRequest, cancellationToken).ConfigureAwait(false));    
}
