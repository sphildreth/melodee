using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;

namespace Melodee.Plugins.SearchEngine;

public interface IAlbumImageSearchEnginePlugin : IPlugin
{
    Task<OperationResult<ImageSearchResult[]?>> DoSearch(AlbumQuery query, int maxResults, CancellationToken token = default);
}