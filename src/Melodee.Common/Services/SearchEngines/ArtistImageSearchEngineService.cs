using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Plugins.SearchEngine;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Serialization;
using Melodee.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Melodee.Common.Services.SearchEngines;

/// <summary>
///     Uses enabled Image Search plugins to get images for artist query.
/// </summary>
public class ArtistImageSearchEngineService(
    ILogger logger,
    ICacheManager cacheManager,
    ISerializer serializer,
    ISettingService settingService,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMusicBrainzRepository musicBrainzRepository,
    IHttpClientFactory httpClientFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    public async Task<OperationResult<ImageSearchResult[]>> DoSearchAsync(ArtistQuery query, int? maxResults, CancellationToken token = default)
    {
        var configuration = await settingService.GetMelodeeConfigurationAsync(token);

        var maxResultsValue = maxResults ?? configuration.GetValue<int>(SettingRegistry.SearchEngineDefaultPageSize);

        var searchEngines = new List<IArtistImageSearchEnginePlugin>
        {
            new MusicBrainzArtistSearchEnginePlugin(musicBrainzRepository)
            {
                IsEnabled = configuration.GetValue<bool>(SettingRegistry.SearchEngineMusicBrainzEnabled)
            }
        };
        var result = new List<ImageSearchResult>();
        foreach (var searchEngine in searchEngines.Where(x => x.IsEnabled).OrderBy(x => x.SortOrder))
        {
            try
            {
                var searchResult = await searchEngine.DoArtistImageSearch(query, maxResultsValue, token);
                if (searchResult.IsSuccess)
                {
                    result.AddRange(searchResult.Data ?? []);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "[{Plugin}] threw error with query [{Query}]", searchEngine.DisplayName, query);
            }
        }

        return new OperationResult<ImageSearchResult[]>
        {
            Data = result.OrderByDescending(x => x.Rank).ToArray()
        };
    }
}