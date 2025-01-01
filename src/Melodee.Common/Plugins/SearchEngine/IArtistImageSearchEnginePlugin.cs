using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;

namespace Melodee.Common.Plugins.SearchEngine;

public interface IArtistImageSearchEnginePlugin : IPlugin
{
    Task<OperationResult<ImageSearchResult[]?>> DoArtistImageSearch(ArtistQuery query, int maxResults, CancellationToken token = default);
}
