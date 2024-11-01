using Melodee.Services;
using Serilog;

namespace Melodee.Jobs;

public abstract class JobBase(
    ILogger logger,
    SettingService settingService)
{
    protected ILogger Logger { get; } = logger;

    protected SettingService SettingService { get; } = settingService;
}
