using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Serialization;
using Melodee.Plugins.SearchEngine;
using Melodee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Melodee.Services.SearchEngines;

/// <summary>
/// Uses enabled Image Search plugins to get images for query.
/// </summary>
public class ImageSearchEngineService(
    ILogger logger,
    ICacheManager cacheManager,
    ISerializer serializer,
    ISettingService settingService,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    public async Task<OperationResult<ImageSearchResult[]>> DoSearchAsync(IHttpClientFactory httpClientFactory, string query, int maxResults, CancellationToken token = default)
    {
        var configuration = await settingService.GetMelodeeConfigurationAsync(token);

        var searchEngines = new List<IImageSearchEnginePlugin>
        {
            new BingImageSearchEngine(configuration, serializer, httpClientFactory)
            {
                IsEnabled = configuration.GetValue<bool>(SettingRegistry.SearchEngineEnabledBingImage)
            }
        };
        var result = new List<ImageSearchResult>();
        foreach (var searchEngine in searchEngines.Where(x => x.IsEnabled))
        {
            var searchResult = await searchEngine.DoSearch(query, maxResults, token);
            if (searchResult.IsSuccess)
            {
                result.AddRange(searchResult.Data ?? []);
            }
        }
        return new OperationResult<ImageSearchResult[]>
        {
            Data = result.ToArray()
        };
    }
}
