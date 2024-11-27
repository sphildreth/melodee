using Melodee.Common.Configuration;
using Melodee.Services.Interfaces;
using Quartz;
using Serilog;

namespace Melodee.Jobs;

public abstract class JobBase(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory) : IJob
{
    protected ILogger Logger { get; } = logger;

    protected IMelodeeConfigurationFactory ConfigurationFactory { get; } = configurationFactory;

    public abstract Task Execute(IJobExecutionContext context);
}
