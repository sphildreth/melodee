using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Services;
using Melodee.Services.Scanning;
using Quartz;
using Serilog;

namespace Melodee.Jobs;

/// <summary>
/// Scans inbound directory for media and puts processed into staging directory.
/// </summary>
public sealed class InboundDirectoryScanJob(
    ILogger logger,
    SettingService settingService,
    LibraryService libraryService,
    DirectoryProcessorService directoryProcessorService) : JobBase(logger, settingService), IJob
{
    private readonly DirectoryProcessorService _directoryProcessorService = directoryProcessorService;
    private readonly LibraryService _libraryService = libraryService;

    public async Task Execute(IJobExecutionContext context)
    {
        await _directoryProcessorService.InitializeAsync();

        var configuration = await SettingService.GetMelodeeConfigurationAsync();
        var directoryInbound = (await _libraryService.GetInboundLibraryAsync()).Data!.Path;
        if (directoryInbound.Nullify() == null)
        {
            Logger.Warning("No directory inbound configuration found.");
            return;
        }
        var result = await _directoryProcessorService.ProcessDirectoryAsync(new FileSystemDirectoryInfo
        {
            Path = directoryInbound!,
            Name = directoryInbound!
        });
        if (!result.IsSuccess)
        {
            Logger.Warning("Failed to Scan inbound directory.");
        }
    }
}
