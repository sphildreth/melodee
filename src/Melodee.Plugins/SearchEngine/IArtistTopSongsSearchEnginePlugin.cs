using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;

namespace Melodee.Plugins.SearchEngine;

/// <summary>
/// Get the top songs by an artist, ordered by popularity
/// </summary>
public interface IArtistTopSongsSearchEnginePlugin : IPlugin
{
    Task<PagedResult<SongSearchResult>> DoArtistTopSongsSearchAsync(IHttpClientFactory httpClientFactory, int forArtist, int maxResults, CancellationToken cancellationToken = default);
}