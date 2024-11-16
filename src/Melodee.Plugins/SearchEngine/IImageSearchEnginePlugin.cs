using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;

namespace Melodee.Plugins.SearchEngine;

public interface IImageSearchEnginePlugin : IPlugin
{
    Task<OperationResult<ImageSearchResult[]?>> DoSearch(string query, int maxResults, CancellationToken token = default);
}
