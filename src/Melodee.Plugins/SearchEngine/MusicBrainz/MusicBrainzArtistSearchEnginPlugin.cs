using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Plugins.SearchEngine.MusicBrainz.Data;

namespace Melodee.Plugins.SearchEngine.MusicBrainz;

public class MusicBrainzArtistSearchEnginPlugin(MusicBrainzRepository repository) : IArtistSearchEnginePlugin
{
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
