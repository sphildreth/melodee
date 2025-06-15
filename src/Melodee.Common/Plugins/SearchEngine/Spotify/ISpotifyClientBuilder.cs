using SpotifyAPI.Web;

namespace Melodee.Common.Plugins.SearchEngine.Spotify;

public interface ISpotifyClientBuilder
{
    SpotifyClientConfig Config { get; }
    SpotifyClient? BuildClient(string token);
}
