using Melodee.Services;
using Quartz;
using Serilog;

namespace Melodee.Jobs;

public abstract class JobBase(
    ILogger logger,
    ISettingService settingService) : IJob
{
    protected ILogger Logger { get; } = logger;

    protected ISettingService SettingService { get; } = settingService;
    
    public abstract Task Execute(IJobExecutionContext context);
}
