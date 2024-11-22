using Melodee.Common.Serialization;
using Melodee.Services;

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
