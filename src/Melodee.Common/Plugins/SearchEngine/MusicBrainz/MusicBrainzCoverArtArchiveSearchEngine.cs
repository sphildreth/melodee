using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Serilog;

namespace Melodee.Common.Plugins.SearchEngine.MusicBrainz;

/// <summary>
///     https://musicbrainz.org/doc/Cover_Art_Archive/API
/// </summary>
public sealed class MusicBrainzCoverArtArchiveSearchEngine(
    IMelodeeConfiguration configuration,
    IMusicBrainzRepository repository) : IAlbumImageSearchEnginePlugin
{
    public bool StopProcessing { get; private set; }

    public string Id => "3E6C2DD3-AC1A-452D-B52B-4C292BA1CC49";

    public string DisplayName => nameof(MusicBrainzCoverArtArchiveSearchEngine);

    public bool IsEnabled { get; set; }

    public int SortOrder { get; } = 0;

    public async Task<OperationResult<ImageSearchResult[]?>> DoAlbumImageSearch(AlbumQuery query, int maxResults,
        CancellationToken cancellationToken = default)
    {
        StopProcessing = false;

        var result = new List<ImageSearchResult>();

        if (!configuration.GetValue<bool>(SettingRegistry.SearchEngineMusicBrainzEnabled))
        {
            return new OperationResult<ImageSearchResult[]?>(
                "MusicBrainz Cover Art Archive image search plugin is disabled.")
            {
                Data = null
            };
        }

        try
        {
            var artistSearchResult = await repository.SearchArtist(new ArtistQuery
            {
                MusicBrainzId = query.ArtistMusicBrainzId,
                Name = query?.Artist ?? string.Empty,
                AlbumMusicBrainzIds = query?.MusicBrainzIdValue == null ? null : [query.MusicBrainzIdValue.Value],
                AlbumKeyValues =
                [
                    new KeyValue(query?.Year.ToString() ?? string.Empty, query?.NameNormalized)
                ]
            }, 1, cancellationToken).ConfigureAwait(false);
            if (artistSearchResult.IsSuccess)
            {
                var rg = artistSearchResult.Data.FirstOrDefault()?.Releases?.FirstOrDefault()
                    ?.MusicBrainzResourceGroupId;
                if (rg != null)
                {
                    result.Add(new ImageSearchResult
                    {
                        FromPlugin = DisplayName,
                        Rank = 10,
                        ThumbnailUrl = string.Empty,
                        MediaUrl = $"https://coverartarchive.org/release-group/{rg}/front"
                    });
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e,
                "[{PluginName}] attempting to query cover art archive image search plugin failed Query [{Query}]",
                nameof(MusicBrainzArtistSearchEnginePlugin), query);
        }

        if (result.Count == 0)
        {
            Log.Debug("[{PluginName}] no MusicBrainz Cover Art response for query[{Query}]",
                nameof(MusicBrainzArtistSearchEnginePlugin), query);
        }

        return new OperationResult<ImageSearchResult[]?>
        {
            Data = result.ToArray()
        };
    }
}
