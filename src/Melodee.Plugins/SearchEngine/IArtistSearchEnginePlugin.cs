using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;

namespace Melodee.Plugins.SearchEngine;

public interface IArtistSearchEnginePlugin : IPlugin
{
    bool StopProcessing { get; }
    
    Task<OperationResult<ArtistSearchResult[]?>> DoSearchAsync(IHttpClientFactory httpClientFactory, ArtistQuery query, int maxResults, CancellationToken cancellationToken = default);
}
