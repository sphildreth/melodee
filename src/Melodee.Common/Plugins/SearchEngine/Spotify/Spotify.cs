using Melodee.Common.Configuration;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Serialization;

namespace Melodee.Common.Plugins.SearchEngine.Spotify;

public class Spotify(IMelodeeConfiguration configuration, ISerializer serializer, IHttpClientFactory httpClientFactory) 
    : IArtistSearchEnginePlugin, IArtistTopSongsSearchEnginePlugin, IAlbumImageSearchEnginePlugin
{
    public bool StopProcessing { get; } = false;

    public string Id => "BBAC49B7-0EDF-4D31-8A54-C9126509C2CE";

    public string DisplayName => "Spotify Service";

    public bool IsEnabled { get; set; } = false;

    public int SortOrder { get; } = 1;
    
    public Task<OperationResult<ImageSearchResult[]?>> DoAlbumImageSearch(AlbumQuery query, int maxResults, CancellationToken token = default)
    {
        //https://developer.spotify.com/documentation/web-api/reference/search
        //https://johnnycrazy.github.io/SpotifyAPI-NET/docs/client_credentials
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
