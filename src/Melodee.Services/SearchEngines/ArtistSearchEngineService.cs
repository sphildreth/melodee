using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Melodee.Plugins.SearchEngine;
using Melodee.Plugins.SearchEngine.MusicBrainz;
using Melodee.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Melodee.Services.SearchEngines;

public class ArtistSearchEngineService(
    ILogger logger,
    ICacheManager cacheManager,
    ISerializer serializer,
    ISettingService settingService,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    MusicBrainzRepository musicBrainzRepository,
    IHttpClientFactory httpClientFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private readonly ISerializer _serializer = serializer;
    private IArtistSearchEnginePlugin[] _artistSearchEnginePlugins = [];
    private IArtistTopSongsSearchEnginePlugin[] _artistTopSongsSearchEnginePlugins = [];
    private IMelodeeConfiguration _configuration = new MelodeeConfiguration([]);
    private bool _initialized;
   
    public async Task InitializeAsync(IMelodeeConfiguration? configuration = null, CancellationToken cancellationToken = default)
    {
        _configuration = configuration ?? await settingService.GetMelodeeConfigurationAsync(cancellationToken).ConfigureAwait(false);

        _artistSearchEnginePlugins =
        [
           new MelodeeArtistSearchEnginPlugin(ContextFactory),
           new MusicBrainzArtistSearchEnginPlugin(musicBrainzRepository),
        ];

        _artistTopSongsSearchEnginePlugins =
        [
            new MelodeeArtistSearchEnginPlugin(ContextFactory)
        ];        
        
        _initialized = true;
    }

    private void CheckInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException($"{nameof(ArtistSearchEngineService)} is not initialized.");
        }
    }

    public async Task<PagedResult<SongSearchResult>> DoArtistTopSongsSearchAsync(string artistName, int? artistId, int? maxResults, CancellationToken cancellationToken = default)
    {
        CheckInitialized();
        
        var maxResultsValue = maxResults ?? _configuration.GetValue<int>(SettingRegistry.SearchEngineDefaultPageSize);
        long totalCount = 0;
        long operationTime = 0;
        
        var artistIdValue = artistId;
        if (artistIdValue == null)
        {
            var searchResult = await DoSearchAsync(new ArtistQuery { Name = artistName}, maxResults, cancellationToken).ConfigureAwait(false);
            artistIdValue =searchResult.Data.FirstOrDefault(x => x.Id != null)?.Id;
        }

        if (artistId == null)
        {
            return new PagedResult<SongSearchResult>([$"No artist found for [{artistName}]"])
            {
                Data = []
            };
        }
        
        var result = new List<SongSearchResult>();
        
        foreach (var plugin in _artistTopSongsSearchEnginePlugins.Where(x => x.IsEnabled).OrderBy(x => x.SortOrder))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            var pluginResult = await plugin.DoArtistTopSongsSearchAsync(httpClientFactory, artistId.Value, maxResultsValue, cancellationToken).ConfigureAwait(false);
            if (pluginResult is { IsSuccess: true, Data: not null })
            {
                result.AddRange(pluginResult.Data);
                totalCount += pluginResult.TotalCount;
                operationTime += pluginResult.OperationTime ?? 0;
            }
            if (result.Count > maxResultsValue)
            {
                break;
            }            
        }        
        
        return new PagedResult<SongSearchResult>
        {
            OperationTime = operationTime,
            CurrentPage = 1,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : SafeParser.ToNumber<int>((totalCount +  maxResultsValue - 1) / maxResultsValue),
            Data = result.OrderByDescending(x => x.PlayCount).ThenBy(x => x.SortName).ToArray()
        };
    }
    
    public async Task<PagedResult<ArtistSearchResult>> DoSearchAsync(ArtistQuery query, int? maxResults, CancellationToken cancellationToken = default)
    {
        CheckInitialized();
        
        var result = new List<ArtistSearchResult>();

        var maxResultsValue = maxResults ?? _configuration.GetValue<int>(SettingRegistry.SearchEngineDefaultPageSize);

        long operationTime = 0;
        long totalCount = 0;
        foreach (var plugin in _artistSearchEnginePlugins.Where(x => x.IsEnabled).OrderBy(x => x.SortOrder))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            var pluginResult = await plugin.DoSearchAsync(httpClientFactory, query, maxResultsValue, cancellationToken).ConfigureAwait(false);
            if (pluginResult is { IsSuccess: true, Data: not null })
            {
                result.AddRange(pluginResult.Data);
                totalCount += pluginResult.TotalCount;
                operationTime += pluginResult.OperationTime ?? 0;
            }
            if (result.Count > maxResultsValue || plugin.StopProcessing)
            {
                break;
            }            
        }
        return new PagedResult<ArtistSearchResult>
        {
            OperationTime = operationTime,
            CurrentPage = 1,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : SafeParser.ToNumber<int>((totalCount +  maxResultsValue - 1) / maxResultsValue),
            Data = result.OrderByDescending(x => x.Rank).ThenBy(x => x.SortName).ToArray()
        };
    }
}
