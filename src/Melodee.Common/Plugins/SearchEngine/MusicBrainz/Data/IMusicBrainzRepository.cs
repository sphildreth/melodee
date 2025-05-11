using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Album = Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models.Materialized.Album;

namespace Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;

public interface IMusicBrainzRepository
{
    Task<Album?> GetAlbumByMusicBrainzId(Guid musicBrainzId, CancellationToken cancellationToken = default);

    Task<PagedResult<ArtistSearchResult>> SearchArtist(ArtistQuery query, int maxResults,
        CancellationToken cancellationToken = default);

    Task<OperationResult<bool>> ImportData(CancellationToken cancellationToken = default);
}
