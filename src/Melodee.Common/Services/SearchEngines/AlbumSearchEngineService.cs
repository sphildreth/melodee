using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;
using Melodee.Common.Plugins.SearchEngine;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Plugins.SearchEngine.Spotify;
using Melodee.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Common.Services.SearchEngines;

public class AlbumSearchEngineService(
    ILogger logger,
    ICacheManager cacheManager,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> melodeeDbContextFactory,
    ArtistSearchEngineService artistSearchEngineService)
    : ServiceBase(logger, cacheManager, melodeeDbContextFactory)
{
    private IMelodeeConfiguration _configuration = new MelodeeConfiguration([]);
    private bool _initialized;

    public async Task InitializeAsync(IMelodeeConfiguration? configuration = null, CancellationToken cancellationToken = default)
    {
        _configuration = configuration ?? await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        await artistSearchEngineService.InitializeAsync(_configuration, cancellationToken);
        _initialized = true;
    }

    private void CheckInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException($"{nameof(AlbumSearchEngineService)} is not initialized.");
        }
    }

    public async Task<PagedResult<AlbumSearchResult>> DoSearchAsync(AlbumQuery query, int? maxResults, CancellationToken cancellationToken = default)
    {
        CheckInitialized();
        using (Operation.At(LogEventLevel.Debug).Time("[{ServiceName}] [{Method}] Query [{Query}]", nameof(AlbumSearchEngineService), nameof(DoSearchAsync), query))
        {
            var result = new List<AlbumSearchResult>();
            var maxResultsValue = maxResults ?? _configuration.GetValue<int>(SettingRegistry.SearchEngineDefaultPageSize);
            var totalCount = 0;
            long operationTime = 0;

            // Search for artist then return all albums for artist rank against name match
            var searchResult = await artistSearchEngineService.DoSearchAsync(new ArtistQuery
            {
                Name = query.Artist ?? string.Empty,
                MusicBrainzId = query.ArtistMusicBrainzId
            }, maxResultsValue, cancellationToken).ConfigureAwait(false);

            if (searchResult.Data.Any())
            {
                var artist = searchResult.Data.OrderByDescending(x => x.Rank).FirstOrDefault();
                result.AddRange(artist?.Releases ?? []);
                totalCount = artist?.Releases?.Length ?? 0;
            }

            return new PagedResult<AlbumSearchResult>
            {
                OperationTime = operationTime,
                CurrentPage = 1,
                TotalCount = totalCount,
                TotalPages = 1,
                Data = result.OrderBy(x => x.Rank).ToArray()
            };
        }
    }
}
