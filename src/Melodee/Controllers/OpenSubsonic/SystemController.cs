using Melodee.Controllers.OpenSubsonic.Models;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class SystemController : Controller
{
    //getLicense
    //getOpenSubsonicExtensions

    /// <summary>
    /// Return system ping response
    /// </summary>
    /// <param name="u">The username.</param>
    /// <param name="v">The protocol version implemented by the client, i.e., the version of the subsonic-rest-api.xsd schema used</param>
    /// <param name="c">A unique string identifying the client application.</param>
    /// <param name="f">Request data to be returned in this format.</param>
    /// <param name="apiKey">An API key used for authentication</param>
    /// <param name="p">The password, either in clear text or hex-encoded with a “enc:” prefix.</param>
    /// <param name="t">The authentication token computed as md5(password + salt).</param>
    /// <param name="s">A random string (“salt”) used as input for computing the password hash</param>
    [HttpGet("/rest/ping.view")]
    public IActionResult Ping(string? u = null, string? v= null, string? c= null, string? f= null, string? apiKey = null, string? p= null, string? t = null, string? s = null)
    {
        return new JsonResult(new ResponseModel<PingResponse>
        {
            ResponseData = new PingResponse
            (
                "ok",
                "1.16.1",
                "Melodee",
                "0.1.1 (tag)",
                true
            )
        });
    }
}
