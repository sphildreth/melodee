using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Jobs;
using Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Scanning;
using Melodee.Common.Services.SearchEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Rebus.Bus;
using Serilog;
using Spectre.Console.Cli;

namespace Melodee.Cli.Command;

/// <summary>
///     This runs the job that scans all the library type libraries
/// </summary>
public class LibraryScanCommand : CommandBase<LibraryScanSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, LibraryScanSettings settings)
    {
        using (var scope = CreateServiceProvider().CreateScope())
        {
           
            var job = new LibraryInsertJob
            (
                scope.ServiceProvider.GetRequiredService<ILogger>(),
                scope.ServiceProvider.GetRequiredService<IMelodeeConfigurationFactory>(),
                scope.ServiceProvider.GetRequiredService<LibraryService>(),
                scope.ServiceProvider.GetRequiredService<ISerializer>(),
                scope.ServiceProvider.GetRequiredService<IDbContextFactory<MelodeeDbContext>>(),
                scope.ServiceProvider.GetRequiredService<ArtistService>(),
                scope.ServiceProvider.GetRequiredService<AlbumService>(),
                scope.ServiceProvider.GetRequiredService<AlbumDiscoveryService>(),
                scope.ServiceProvider.GetRequiredService<DirectoryProcessorService>(),
                scope.ServiceProvider.GetRequiredService<IBus>()
            );            

            job.OnProcessingEvent += (_, e) => { Log.Information(e.ToString()); };

            var jobExecutionContext = new JobExecutionContext(CancellationToken.None);
            jobExecutionContext.Put("ForceMode", settings.ForceMode);
            jobExecutionContext.Put("Verbose", settings.Verbose);
            await job.Execute(jobExecutionContext);
            return 1;
        }
    }
}

internal class JobExecutionContext(CancellationToken cancellation) : IJobExecutionContext
{
    private readonly Dictionary<object, object> _dataMap = new();

    public void Put(object key, object objectValue)
    {
        if (!_dataMap.TryAdd(key, objectValue))
        {
            _dataMap[key] = objectValue;
        }
    }

    public object? Get(object key)
    {
        _dataMap.TryGetValue(key, out var value);
        return value;
    }

    public IScheduler Scheduler { get; } = null!;
    public ITrigger Trigger { get; } = null!;
    public ICalendar? Calendar { get; } = null!;
    public bool Recovering { get; } = false;
    public TriggerKey RecoveringTriggerKey { get; } = null!;
    public int RefireCount { get; } = 0;
    public JobDataMap MergedJobDataMap { get; } = null!;
    public IJobDetail JobDetail { get; } = new JobDetailImpl();
    public IJob JobInstance { get; } = null!;
    public DateTimeOffset FireTimeUtc { get; } = DateTimeOffset.MinValue;
    public DateTimeOffset? ScheduledFireTimeUtc { get; } = DateTimeOffset.MinValue;
    public DateTimeOffset? PreviousFireTimeUtc { get; } = DateTimeOffset.MinValue;
    public DateTimeOffset? NextFireTimeUtc { get; } = DateTimeOffset.MinValue;
    public string FireInstanceId { get; } = null!;
    public object? Result { get; set; }
    public TimeSpan JobRunTime { get; } = TimeSpan.Zero;
    public CancellationToken CancellationToken { get; } = cancellation;
}
