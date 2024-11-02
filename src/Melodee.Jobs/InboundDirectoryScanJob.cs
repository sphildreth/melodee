using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Services;
using Melodee.Services.Scanning;
using NodaTime;
using Quartz;
using Serilog;

namespace Melodee.Jobs;

/// <summary>
/// Scans inbound directory for media and puts processed into staging directory.
/// </summary>
[DisallowConcurrentExecution]
public sealed class InboundDirectoryScanJob(
    ILogger logger,
    SettingService settingService,
    LibraryService libraryService,
    DirectoryProcessorService directoryProcessorService) : JobBase(logger, settingService)
{
    private readonly DirectoryProcessorService _directoryProcessorService = directoryProcessorService;
    private readonly LibraryService _libraryService = libraryService;

    public override async Task Execute(IJobExecutionContext context)
    {
        var library = (await _libraryService.GetInboundLibraryAsync(context.CancellationToken).ConfigureAwait(false)).Data;
        var directoryInbound = library.Path;
        if (directoryInbound.Nullify() == null)
        {
            Logger.Warning("No inbound library configuration found.");
            return;
        }
        if (!library.NeedsScanning())
        {
            Logger.Debug($"Inbound library does not need scanning. Directory last scanned [{ library.LastScanAt }], Directory last write [{ library.LastWriteTime()}]");
            return;
        }
        await _directoryProcessorService.InitializeAsync(context.CancellationToken).ConfigureAwait(false);        
        var result = await _directoryProcessorService.ProcessDirectoryAsync(new FileSystemDirectoryInfo
        {
            Path = directoryInbound,
            Name = directoryInbound
        },context.CancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            Logger.Warning("Failed to Scan inbound library.");
        }
        await _libraryService.CreateLibraryScanHistory(library, new LibraryScanHistory
        {
            CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow),
            DurationInMs = result.Data.DurationInMs,
            LibraryId = library.Id,
            NewAlbumsCount = result.Data.NewAlbumsCount,
            NewArtistsCount = result.Data.NewArtistsCount,
            NewSongsCount = result.Data.NewSongsCount,
        }, context.CancellationToken).ConfigureAwait(false);
    }
}
