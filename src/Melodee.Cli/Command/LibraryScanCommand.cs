using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Jobs;
using Melodee.Common.MessageBus.EventHandlers;
using Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Caching;
using Melodee.Common.Services.Interfaces;
using Melodee.Common.Services.Scanning;
using Melodee.Common.Services.SearchEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Transport.InMem;
using Serilog;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Spectre.Console.Cli;

namespace Melodee.Cli.Command;

/// <summary>
///     This runs the job that scans all the library type libraries
/// </summary>
public class LibraryScanCommand : AsyncCommand<LibraryScanSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, LibraryScanSettings settings)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        var serializer = new Serializer(Log.Logger);

        var services = new ServiceCollection();
        services.AddDbContextFactory<MelodeeDbContext>(opt =>
            opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), o => o.UseNodaTime()));
        services.AddDbContextFactory<ArtistSearchEngineServiceDbContext>(opt 
            => opt.UseSqlite(configuration.GetConnectionString("ArtistSearchEngineConnection")));        
        services.AddHttpClient();
        services.AddSingleton<IDbConnectionFactory>(_ =>
            new OrmLiteConnectionFactory(configuration.GetConnectionString("MusicBrainzConnection"), SqliteDialect.Provider));
        services.AddScoped<IMusicBrainzRepository, SQLiteMusicBrainzRepository>();
        services.AddSingleton<IMelodeeConfigurationFactory, MelodeeConfigurationFactory>();
        services.AddSingleton(Log.Logger);
        services.AddSingleton(serializer);
        services.AddSingleton<ISerializer, Serializer>();
        services.AddSingleton<ICacheManager>(opt
            => new MemoryCacheManager(opt.GetRequiredService<ILogger>(),
                new TimeSpan(1, 0, 0, 0),
                opt.GetRequiredService<ISerializer>()));
        
        services.AddRebus(configure =>
        {
            return configure
                .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "melodee_bus"));
        });
        services.AddRebusHandler<AlbumUpdatedEventHandler>();
        services.AddRebusHandler<MelodeeAlbumReprocessEventHandler>();

        services.AddScoped<LibraryService>();
        services.AddScoped<AlbumService>();
        services.AddScoped<ArtistService>();
        services.AddScoped<AlbumDiscoveryService>();
        services.AddScoped<MediaEditService>();
        services.AddScoped<ArtistSearchEngineService>();
        services.AddScoped<SettingService>();
        services.AddScoped<AlbumImageSearchEngineService>();
        services.AddScoped<LibraryInsertJob>();
        services.AddScoped<DirectoryProcessorService>();
        
        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            // var job = new LibraryInsertJob
            // (
            //     scope.ServiceProvider.GetRequiredService<ILogger>(),
            //     scope.ServiceProvider.GetRequiredService<IMelodeeConfigurationFactory>(),
            //     scope.ServiceProvider.GetRequiredService<LibraryService>(),
            //     scope.ServiceProvider.GetRequiredService<ISerializer>(),
            //     scope.ServiceProvider.GetRequiredService<IDbContextFactory<MelodeeDbContext>>(),
            //     scope.ServiceProvider.GetRequiredService<ArtistService>(),                
            //     scope.ServiceProvider.GetRequiredService<AlbumService>(),
            //     scope.ServiceProvider.GetRequiredService<AlbumDiscoveryService>(),
            //     scope.ServiceProvider.GetRequiredService<DirectoryProcessorService>(),
            //     scope.ServiceProvider.GetRequiredService<IBus>()
            // );

            var job = scope.ServiceProvider.GetRequiredService<LibraryInsertJob>();
            
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
