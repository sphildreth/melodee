using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;

namespace Melodee.Common.Plugins.SearchEngine;

public interface IAlbumImageSearchEnginePlugin : IPlugin
{
    Task<OperationResult<ImageSearchResult[]?>> DoAlbumImageSearch(AlbumQuery query, int maxResults, CancellationToken token = default);
}
