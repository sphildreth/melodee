using Melodee.Common.Configuration;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Serialization;

namespace Melodee.Plugins.SearchEngine;

/// <summary>
/// https://musicbrainz.org/doc/Cover_Art_Archive/API
/// </summary>
public sealed class MusicBrainzCoverArtArchiveSearchEngine(IMelodeeConfiguration configuration, ISerializer serializer, IHttpClientFactory httpClientFactory) : IImageSearchEnginePlugin
{
    public bool StopProcessing { get; } = false;
    
    public string Id => "3E6C2DD3-AC1A-452D-B52B-4C292BA1CC49";

    public string DisplayName => nameof(MusicBrainzCoverArtArchiveSearchEngine);

    public bool IsEnabled { get; set; } = false;

    public int SortOrder { get; } = 0;
    public Task<OperationResult<ImageSearchResult[]?>> DoSearch(string query, int maxResults, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}
