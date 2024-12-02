using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

/// <summary>
/// No plans on implementing Podcasts in Melodee.
/// </summary>
public class PodcastController : ControllerBase
{
    [HttpGet]
    [HttpPost]
    [Route("/rest/getPodcasts.view")]
    [Route("/rest/getNewestPodcasts.view")]
    [Route("/rest/refreshPodcasts.view")]
    [Route("/rest/createPodcastChannel.view")]
    [Route("/rest/deletePodcastChannel.view")]
    [Route("/rest/deletePodcastEpisode.view")]
    [Route("/rest/downloadPodcastEpisode.view")]
    public IActionResult NotImplemented()
    {
        HttpContext.Response.Headers.Append("Cache-Control", "no-cache");
        return StatusCode((int)HttpStatusCode.NotImplemented);
    }
}
