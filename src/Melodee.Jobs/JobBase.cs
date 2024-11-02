using Melodee.Services;
using Quartz;
using Serilog;

namespace Melodee.Jobs;

public abstract class JobBase(
    ILogger logger,
    SettingService settingService) : IJob
{
    protected ILogger Logger { get; } = logger;

    protected SettingService SettingService { get; } = settingService;
    
    public abstract Task Execute(IJobExecutionContext context);
}
