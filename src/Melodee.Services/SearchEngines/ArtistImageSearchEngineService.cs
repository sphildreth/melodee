using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Serialization;
using Melodee.Plugins.SearchEngine;
using Melodee.Plugins.SearchEngine.LastFm;
using Melodee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Melodee.Services.SearchEngines;

/// <summary>
///     Uses enabled Image Search plugins to get images for artist query.
/// </summary>
public class ArtistImageSearchEngineService(
    ILogger logger,
    ICacheManager cacheManager,
    ISerializer serializer,
    ISettingService settingService,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IHttpClientFactory httpClientFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    public async Task<OperationResult<ImageSearchResult[]>> DoSearchAsync(ArtistQuery query, int? maxResults, CancellationToken token = default)
    {
        var configuration = await settingService.GetMelodeeConfigurationAsync(token);

        var maxResultsValue = maxResults ?? configuration.GetValue<int>(SettingRegistry.SearchEngineDefaultPageSize);

        var searchEngines = new List<IArtistImageSearchEnginePlugin>
        {
            new LastFm(configuration, serializer, httpClientFactory)
            {
                IsEnabled = configuration.GetValue<bool>(SettingRegistry.SearchEngineLastFmEnabled)
            },            
            new BingAlbumImageSearchEngine(configuration, serializer, httpClientFactory)
            {
                IsEnabled = configuration.GetValue<bool>(SettingRegistry.SearchEngineBingImageEnabled)
            }
        };
        var result = new List<ImageSearchResult>();
        foreach (var searchEngine in searchEngines.Where(x => x.IsEnabled))
        {
            var searchResult = await searchEngine.DoArtistImageSearch(query, maxResultsValue, token);
            if (searchResult.IsSuccess)
            {
                result.AddRange(searchResult.Data ?? []);
            }
        }

        return new OperationResult<ImageSearchResult[]>
        {
            Data = result.OrderByDescending(x => x.Rank).ToArray()
        };
    }
}
