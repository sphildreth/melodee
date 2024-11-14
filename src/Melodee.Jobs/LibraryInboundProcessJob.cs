using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Services;
using Melodee.Services.Scanning;
using NodaTime;
using Quartz;
using Serilog;

namespace Melodee.Jobs;

/// <summary>
/// Processes inbound directory for media and puts processed into staging directory.
/// </summary>
[DisallowConcurrentExecution]
public sealed class LibraryInboundProcessJob(
    ILogger logger,
    SettingService settingService,
    LibraryService libraryService,
    DirectoryProcessorService directoryProcessorService) : JobBase(logger, settingService)
{
    
    public override async Task Execute(IJobExecutionContext context)
    {
        var inboundLibrary = (await libraryService.GetInboundLibraryAsync(context.CancellationToken).ConfigureAwait(false)).Data;
        var directoryInbound = inboundLibrary.Path;
        if (directoryInbound.Nullify() == null)
        {
            Logger.Warning("No inbound library configuration found.");
            return;
        }
        if (!inboundLibrary.NeedsScanning())
        {
            Logger.Debug($"Inbound library does not need scanning. Directory last scanned [{ inboundLibrary.LastScanAt }], Directory last write [{ inboundLibrary.LastWriteTime()}]");
            return;
        }
        
        var dataMap = context.JobDetail.JobDataMap;
        var jobDataMap = (LibraryScanHistory)dataMap.Get(nameof(LibraryScanHistory));        
        
        try
        {
            jobDataMap.ScanStatus = ScanStatus.InProcess;
            await directoryProcessorService.InitializeAsync(null, context.CancellationToken).ConfigureAwait(false);        
            var result = await directoryProcessorService.ProcessDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = directoryInbound,
                Name = directoryInbound
            },inboundLibrary.LastScanAt,context.CancellationToken).ConfigureAwait(false);
            
            if (!result.IsSuccess)
            {
                Logger.Warning("Failed to Scan inbound library.");
            }
            jobDataMap.ScanStatus = ScanStatus.Idle;
            jobDataMap.FoundAlbumsCount = result.Data.NewAlbumsCount;
            jobDataMap.FoundArtistsCount = result.Data.NewArtistsCount;
            jobDataMap.FoundSongsCount = result.Data.NewSongsCount;
            await libraryService.CreateLibraryScanHistory(inboundLibrary, new LibraryScanHistory
            {
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
                DurationInMs = result.Data.DurationInMs,
                LibraryId = inboundLibrary.Id,
                FoundAlbumsCount = result.Data.NewAlbumsCount,
                FoundArtistsCount = result.Data.NewArtistsCount,
                FoundSongsCount = result.Data.NewSongsCount,
            }, context.CancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to Scan inbound library.");
        }
    }
}
