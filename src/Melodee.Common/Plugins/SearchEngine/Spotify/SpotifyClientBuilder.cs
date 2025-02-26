using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using SpotifyAPI.Web;

namespace Melodee.Common.Plugins.SearchEngine.Spotify;

public class SpotifyClientBuilder(IHttpContextAccessor httpContextAccessor, SpotifyClientConfig spotifyClientConfig) : ISpotifyClientBuilder
{
    public SpotifyClientConfig Config => spotifyClientConfig;

    public async Task<SpotifyClient?> BuildClient(string token)
    {
        if (httpContextAccessor.HttpContext != null)
        {
            return new SpotifyClient(spotifyClientConfig.WithToken(token));
        }

        return null;
    }
}
