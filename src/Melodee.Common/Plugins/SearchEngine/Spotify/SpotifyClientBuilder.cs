using Microsoft.AspNetCore.Http;
using SpotifyAPI.Web;

namespace Melodee.Common.Plugins.SearchEngine.Spotify;

public class SpotifyClientBuilder(IHttpContextAccessor httpContextAccessor, SpotifyClientConfig spotifyClientConfig)
    : ISpotifyClientBuilder
{
    public SpotifyClientConfig Config => spotifyClientConfig;

    public SpotifyClient? BuildClient(string token)
    {
        return httpContextAccessor.HttpContext != null ? new SpotifyClient(spotifyClientConfig.WithToken(token)) : null;
    }
}
