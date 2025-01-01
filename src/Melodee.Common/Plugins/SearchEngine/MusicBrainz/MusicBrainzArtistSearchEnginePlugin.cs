using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;

namespace Melodee.Common.Plugins.SearchEngine.MusicBrainz;

public class MusicBrainzArtistSearchEnginePlugin(IMusicBrainzRepository repository) : IArtistSearchEnginePlugin, IArtistImageSearchEnginePlugin
{
    public Task<OperationResult<ImageSearchResult[]?>> DoArtistImageSearch(ArtistQuery query, int maxResults, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public bool StopProcessing { get; } = false;

    public string Id => "018A798D-7B68-4F3E-80CD-1BAF03998C0B";

    public string DisplayName => "Music Brainz Database";

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; } = 1;

    public Task<PagedResult<ArtistSearchResult>> DoArtistSearchAsync(ArtistQuery query, int maxResults, CancellationToken cancellationToken = default)
    {
        return repository.SearchArtist(query, maxResults, cancellationToken);
    }
}
