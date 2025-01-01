using Melodee.Common.Configuration;
using Quartz;
using Serilog;

namespace Melodee.Common.Jobs;

public abstract class JobBase(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory) : IJob
{
    protected ILogger Logger { get; } = logger;

    protected IMelodeeConfigurationFactory ConfigurationFactory { get; } = configurationFactory;

    public abstract Task Execute(IJobExecutionContext context);
}
