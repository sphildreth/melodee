using System.Net;
using Melodee.Blazor.Filters;
using Melodee.Common.Configuration;
using Melodee.Common.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Blazor.Controllers.OpenSubsonic;

/// <summary>
///     No plans on implementing Podcasts in Melodee.
/// </summary>
public class PodcastController(ISerializer serializer, EtagRepository etagRepository,IMelodeeConfigurationFactory configurationFactory) : ControllerBase(etagRepository, serializer, configurationFactory)
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
