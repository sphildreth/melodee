using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Common.Serialization;
using Melodee.Results;
using Melodee.Services;
using Microsoft.AspNetCore.Mvc;

namespace Melodee.Controllers.OpenSubsonic;

public class PodcastController(ISerializer serializer, OpenSubsonicApiService openSubsonicApiService) : ControllerBase
{

    // getPodcasts
    // getNewestPodcasts
    // refreshPodcasts
    // createPodcastChannel
    // deletePodcastChannel
    // deletePodcastEpisode
    // downloadPodcastEpisode

}
