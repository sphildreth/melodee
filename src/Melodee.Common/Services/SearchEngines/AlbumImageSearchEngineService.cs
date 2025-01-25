using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Plugins.SearchEngine;
using Melodee.Common.Plugins.SearchEngine.ITunes;
using Melodee.Common.Plugins.SearchEngine.LastFm;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Plugins.SearchEngine.Spotify;
using Melodee.Common.Serialization;
using Melodee.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Melodee.Common.Services.SearchEngines;

/// <summary>
///     Uses enabled Image Search plugins to get images for album query.
/// </summary>
public class AlbumImageSearchEngineService(
    ILogger logger,
    ICacheManager cacheManager,
    ISerializer serializer,
    SettingService settingService,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMusicBrainzRepository musicBrainzRepository,
    IHttpClientFactory httpClientFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    public async Task<OperationResult<ImageSearchResult[]>> DoSearchAsync(AlbumQuery query, int? maxResults, CancellationToken token = default)
    {
        var configuration = await configurationFactory.GetConfigurationAsync(token);

        var maxResultsValue = maxResults ?? configuration.GetValue<int>(SettingRegistry.SearchEngineDefaultPageSize);

        var searchEngines = new List<IAlbumImageSearchEnginePlugin>
        {
            new MusicBrainzCoverArtArchiveSearchEngine(configuration, musicBrainzRepository)
            {
                IsEnabled = configuration.GetValue<bool>(SettingRegistry.SearchEngineMusicBrainzEnabled)
            },
            new ITunesSearchEngine(Logger, serializer, httpClientFactory)
            {
                IsEnabled = configuration.GetValue<bool>(SettingRegistry.SearchEngineITunesEnabled)
            },
            new Spotify(Logger, configuration, serializer, settingService, httpClientFactory)
            {
                IsEnabled = configuration.GetValue<bool>(SettingRegistry.SearchEngineSpotifyEnabled)
            },            
            new LastFm(Logger, configuration, serializer, httpClientFactory)
            {
                IsEnabled = configuration.GetValue<bool>(SettingRegistry.SearchEngineLastFmEnabled)
            }        
        };
        var result = new List<ImageSearchResult>();
        foreach (var searchEngine in searchEngines.Where(x => x.IsEnabled).OrderBy(x => x.SortOrder))
        {
            try
            {
                var searchResult = await searchEngine.DoAlbumImageSearch(query, maxResultsValue, token);
                if (searchResult.IsSuccess)
                {
                    result.AddRange(searchResult.Data ?? []);
                }                
                if (searchEngine.StopProcessing || result.Count >= maxResultsValue)
                {
                    break;
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
