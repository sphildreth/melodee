using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;

namespace Melodee.Common.Plugins.SearchEngine;

public interface IArtistSearchEnginePlugin : IPlugin
{
    bool StopProcessing { get; }

    Task<PagedResult<ArtistSearchResult>> DoArtistSearchAsync(ArtistQuery query, int maxResults, CancellationToken cancellationToken = default);
}
