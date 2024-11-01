using Melodee.Common.Constants;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Services;
using Melodee.Services.Scanning;
using Quartz;
using Serilog;

namespace Melodee.Jobs;

public sealed class MediaScanJob(
    ILogger logger,
    SettingService settingService,
    DirectoryProcessorService directoryProcessorService) : JobBase(logger, settingService), IJob
{
    private readonly DirectoryProcessorService _directoryProcessorService = directoryProcessorService;

    public async Task Execute(IJobExecutionContext context)
    {
        await _directoryProcessorService.InitializeAsync();

        var configuration = await SettingService.GetMelodeeConfigurationAsync();
        var directoryInbound = configuration.GetValue<string>(SettingRegistry.DirectoryInbound);
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
