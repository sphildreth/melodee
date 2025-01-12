using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Services;
using Melodee.Common.Services.Scanning;
using NodaTime;
using Quartz;
using Serilog;

namespace Melodee.Common.Jobs;

/// <summary>
///     Processes inbound directory for media and puts processed into staging directory.
/// </summary>
[DisallowConcurrentExecution]
public sealed class LibraryInboundProcessJob(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    LibraryService libraryService,
    DirectoryProcessorService directoryProcessorService) : JobBase(logger, configurationFactory)
{
    public override async Task Execute(IJobExecutionContext context)
    {
        var inboundLibrary = (await libraryService.GetInboundLibraryAsync(context.CancellationToken).ConfigureAwait(false)).Data;
        var directoryInbound = inboundLibrary.Path;
        if (directoryInbound.Nullify() == null)
        {
            Logger.Warning("[{JobName}] No inbound library configuration found.", nameof(LibraryInboundProcessJob));
            return;
        }

        if (inboundLibrary.IsLocked)
        {
            Logger.Warning("[{JobName}] Skipped processing locked library [{LibraryName}]", nameof(LibraryInboundProcessJob), inboundLibrary.Name);
            return;
        }

        if (!inboundLibrary.NeedsScanning())
        {
            Logger.Debug(
                "[{JobName}] Inbound library does not need scanning. Directory last scanned [{LastScanAt}], Directory last write [{LastWriteTime}]",
                nameof(LibraryInboundProcessJob),
                inboundLibrary.LastScanAt,
                inboundLibrary.LastWriteTime());
            return;
        }

        var dataMap = context.JobDetail.JobDataMap;
        try
        {
            dataMap.Put(JobMapNameRegistry.ScanStatus, ScanStatus.InProcess.ToString());
            await directoryProcessorService.InitializeAsync(null, context.CancellationToken).ConfigureAwait(false);
            var result = await directoryProcessorService.ProcessDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = directoryInbound,
                Name = directoryInbound
            }, inboundLibrary.LastScanAt, context.CancellationToken).ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                Logger.Warning("[{JobName}] Failed to Scan inbound library.", nameof(LibraryInboundProcessJob));
            }

            dataMap.Put(JobMapNameRegistry.ScanStatus, ScanStatus.Idle.ToString());
            dataMap.Put(JobMapNameRegistry.Count, result.Data.NewAlbumsCount + result.Data.NewArtistsCount + result.Data.NewSongsCount);
            await libraryService.CreateLibraryScanHistory(inboundLibrary, new LibraryScanHistory
            {
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                DurationInMs = result.Data.DurationInMs,
                LibraryId = inboundLibrary.Id,
                FoundAlbumsCount = result.Data.NewAlbumsCount,
                FoundArtistsCount = result.Data.NewArtistsCount,
                FoundSongsCount = result.Data.NewSongsCount
            }, context.CancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Logger.Error(e, "[{JobName}] Failed to Scan inbound library.", nameof(LibraryInboundProcessJob));
        }
    }
}
