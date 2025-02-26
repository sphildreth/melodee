using System.Diagnostics;
using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Dapper.SqliteHandlers;
using Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;
using Melodee.Common.Services.SearchEngines;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Quartz;
using Serilog;

namespace Melodee.Common.Jobs;

/// <summary>
///     Housekeeping for Artist Search Engine Repository
/// </summary>
public class ArtistSearchEngineRepositoryHousekeepingJob(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    ArtistSearchEngineService artistSearchEngineService,
    IDbContextFactory<ArtistSearchEngineServiceDbContext> artistSearchEngineServiceDbContextFactory) : JobBase(logger, configurationFactory)
{
    public override async Task Execute(IJobExecutionContext context)
    {
        var startTimeStamp = Stopwatch.GetTimestamp();
        Logger.Information("[{JobName}] Starting job.", nameof(ArtistSearchEngineRepositoryHousekeepingJob));

        var configuration = await ConfigurationFactory.GetConfigurationAsync(context.CancellationToken).ConfigureAwait(false);
        var refreshInDays = configuration.GetValue<int?>(SettingRegistry.SearchEngineArtistSearchDatabaseRefreshInDays) ?? 0;
        if (refreshInDays == 0)
        {
            Logger.Warning("[{JobName}] Skipped refreshing Artist Search Engine Repository. No refresh interval configured.", nameof(ArtistSearchEngineRepositoryHousekeepingJob));
            return;
        }

        var batchSize = SafeParser.ToNumber<int?>(context.Get(JobMapNameRegistry.BatchSize)) ?? configuration.GetValue<int?>(SettingRegistry.DefaultsBatchSize) ?? 50;

        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);

        var refreshOlderThan = now.Minus(Duration.FromDays(refreshInDays));
        var refreshOtherThanDateTime = refreshOlderThan.ToDateTimeUtc();

        Logger.Information("[{JobName}] Refreshing Artist Search Engine Repository older than [{RefreshOlderThan}]", nameof(ArtistSearchEngineRepositoryHousekeepingJob), refreshOlderThan);

        await using (var scopedContext = await artistSearchEngineServiceDbContextFactory.CreateDbContextAsync(context.CancellationToken).ConfigureAwait(false))
        {
            // Refresh a batch of albums for Artists, this is to prevent rate limiting issues. 
            var artistsToRefresh = await scopedContext
                .Artists
                .Where(x => (x.IsLocked == null || x.IsLocked == false) && (x.LastRefreshed == null || x.LastRefreshed <= refreshOtherThanDateTime))
                .OrderByDescending(x => x.LastRefreshed)
                .Take(batchSize)
                .ToListAsync(context.CancellationToken)
                .ConfigureAwait(false);

            if (artistsToRefresh.Count > 0)
            {
                Logger.Information("[{JobName}] Refreshing [{Count}] artists.", nameof(ArtistSearchEngineRepositoryHousekeepingJob), artistsToRefresh.Count);
                await artistSearchEngineService.InitializeAsync(configuration, context.CancellationToken).ConfigureAwait(false);
                await artistSearchEngineService.RefreshArtistAlbums(artistsToRefresh.ToArray(), context.CancellationToken).ConfigureAwait(false);
            }
            else
            {
                Logger.Information("[{JobName}] No artists need refreshing.", nameof(ArtistSearchEngineRepositoryHousekeepingJob));
            }

            Logger.Information("[{JobName}] Completed job in [{Elapsed}].", nameof(ArtistSearchEngineRepositoryHousekeepingJob), Stopwatch.GetElapsedTime(startTimeStamp));
        }
    }
}
