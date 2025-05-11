using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;

namespace Melodee.Common.Plugins.SearchEngine;

public interface IAlbumImageSearchEnginePlugin : IPlugin
{
    bool StopProcessing { get; }

    Task<OperationResult<ImageSearchResult[]?>> DoAlbumImageSearch(AlbumQuery query, int maxResults,
        CancellationToken cancellationToken = default);
}
