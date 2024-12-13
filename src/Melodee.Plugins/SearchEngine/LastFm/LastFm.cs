using Melodee.Common.Configuration;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Serialization;

namespace Melodee.Plugins.SearchEngine.LastFm;

public class LastFm(IMelodeeConfiguration configuration, ISerializer serializer, IHttpClientFactory httpClientFactory) 
    : IArtistSearchEnginePlugin, IArtistTopSongsSearchEnginePlugin, IAlbumImageSearchEnginePlugin, IArtistImageSearchEnginePlugin
{
    public bool StopProcessing { get; } = false;

    public string Id => "3E43D998-2E9C-45B0-8128-EE6F0A41419E";

    public string DisplayName => "Last FM Service";

    public bool IsEnabled { get; set; } = false;

    public int SortOrder { get; } = 1;
    public Task<OperationResult<ImageSearchResult[]?>> DoArtistImageSearch(ArtistQuery query, int maxResults, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<OperationResult<ImageSearchResult[]?>> DoAlbumImageSearch(AlbumQuery query, int maxResults, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResult<SongSearchResult>> DoArtistTopSongsSearchAsync(int forArtist, int maxResults, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResult<ArtistSearchResult>> DoArtistSearchAsync(ArtistQuery query, int maxResults, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
