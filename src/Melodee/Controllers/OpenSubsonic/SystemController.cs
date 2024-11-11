using Mapster;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Controllers.OpenSubsonic.Models;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class SystemController(UserService userService) : ControllerBase
{
    //getLicense
    //getOpenSubsonicExtensions

    /// <summary>
    /// Return system ping response
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("/rest/ping.view")]
    public async Task<IActionResult> Ping(CancellationToken cancellationToken = default)
    {
        var userAuthResult = await userService.AuthenticateSubsonicApiAsync(ApiRequest, cancellationToken);
        
        return new JsonResult(new ResponseModel<PingResponse>
        {
            ResponseData = new PingResponse
            (
                userAuthResult.Data ? "ok" : "failed",
                "1.16.1",
                "Melodee",
                "0.1.1 (tag)",
                true,
                userAuthResult.Data ? null : Error.AuthError
            )
        });
    }
}
