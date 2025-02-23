using SpotifyAPI.Web;

namespace Melodee.Common.Plugins.SearchEngine.Spotify;

public interface ISpotifyClientBuilder
{
    SpotifyClientConfig Config { get; }
    Task<SpotifyClient?> BuildClient(string token);
}
