using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Serialization;
using Melodee.Plugins.SearchEngine.MusicBrainz.Data;
using Serilog;

namespace Melodee.Plugins.SearchEngine.MusicBrainz;

/// <summary>
///     https://musicbrainz.org/doc/Cover_Art_Archive/API
/// </summary>
public sealed class MusicBrainzCoverArtArchiveSearchEngine(IMelodeeConfiguration configuration, MusicBrainzRepository repository, ISerializer serializer, IHttpClientFactory httpClientFactory) : IAlbumImageSearchEnginePlugin
{
    public bool StopProcessing { get; } = false;

    public string Id => "3E6C2DD3-AC1A-452D-B52B-4C292BA1CC49";

    public string DisplayName => nameof(MusicBrainzCoverArtArchiveSearchEngine);

    public bool IsEnabled { get; set; }

    public int SortOrder { get; } = 0;

    public async Task<OperationResult<ImageSearchResult[]?>> DoSearch(AlbumQuery query, int maxResults, CancellationToken token = default)
    {
        var result = new List<ImageSearchResult>();

        if (!configuration.GetValue<bool>(SettingRegistry.SearchEngineMusicBrainzEnabled))
        {
            return new OperationResult<ImageSearchResult[]?>("MusicBrainz Cover Art Archive image search plugin is disabled.")
            {
                Data = null
            };
        }

        try
        {
            if (query.MusicBrainzIdValue != null)
            {
                // Get resource group from Melodee MusicBrainz db for given MusicBrainzId
                var mbAlbum = await repository.GetAlbumByMusicBrainzId(query.MusicBrainzIdValue.Value, token).ConfigureAwait(false);
                if (mbAlbum == null)
                {
                    return new OperationResult<ImageSearchResult[]?>($"[{nameof(MusicBrainzCoverArtArchiveSearchEngine)}] unable to find MusicBrainz database Album by Id [{query.MusicBrainzId}]")
                    {
                        Data = null
                    };
                }
                result.Add(new ImageSearchResult
                {
                    FromPlugin = nameof(MusicBrainzCoverArtArchiveSearchEngine),
                    Rank = 10,
                    ThumbnailUrl = string.Empty,
                    MediaUrl = $"https://coverartarchive.org/release-group/{mbAlbum.ReleaseGroupMusicBrainzId}/front"
                });
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "[{PluginName}] attempting to query cover art archive image search plugin failed Query [{Query}]", nameof(MusicBrainzArtistSearchEnginPlugin), query);
        }

        if (result.Count == 0)
        {
            Log.Debug("[{PluginName}] no MusicBrainz Cover Art response for query[{Query}]", nameof(MusicBrainzArtistSearchEnginPlugin), query);
        }

        return new OperationResult<ImageSearchResult[]?>
        {
            Data = result.ToArray()
        };
    }
}
