using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Jobs;
using Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Caching;
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
        var cacheManager = new MemoryCacheManager(Log.Logger, TimeSpan.FromDays(1), serializer);

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
        services.AddRebus(configure =>
        {
            return configure
                .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "melodee_bus"));
        });

        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MelodeeDbContext>>();
            var configFactory = scope.ServiceProvider.GetRequiredService<IMelodeeConfigurationFactory>();
            var bus = scope.ServiceProvider.GetRequiredService<IBus>();

            var libraryService = new LibraryService(Log.Logger,
                cacheManager,
                dbFactory,
                configFactory,
                serializer,
                bus);

            var configurationFactory = new MelodeeConfigurationFactory(dbFactory);

            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            var settingService = new SettingService(Log.Logger, cacheManager, configFactory, dbFactory);

            var job = new LibraryInsertJob
            (
                Log.Logger,
                configurationFactory,
                libraryService,
                serializer,
                dbFactory,
                new ArtistService(Log.Logger, cacheManager, configFactory, dbFactory, serializer, httpClientFactory),
                new AlbumService(Log.Logger, cacheManager, dbFactory),
                new AlbumDiscoveryService(Log.Logger, cacheManager, dbFactory, configFactory, serializer),
                new DirectoryProcessorService(
                    Log.Logger,
                    cacheManager,
                    dbFactory,
                    configFactory,
                    libraryService,
                    serializer,
                    new MediaEditService(
                        Log.Logger,
                        cacheManager,
                        dbFactory,
                        configFactory,
                        libraryService,
                        new AlbumDiscoveryService(
                            Log.Logger,
                            cacheManager,
                            dbFactory,
                            configFactory,
                            serializer),
                        serializer,
                        httpClientFactory
                    ),
                    new ArtistSearchEngineService
                    (
                        Log.Logger,
                        cacheManager,
                        settingService,
                        configFactory,
                        dbFactory,
                        scope.ServiceProvider.GetRequiredService<IDbContextFactory<ArtistSearchEngineServiceDbContext>>(),
                        scope.ServiceProvider.GetRequiredService<IMusicBrainzRepository>()
                    ),
                    new AlbumImageSearchEngineService
                    (
                        Log.Logger,
                        cacheManager,
                        serializer,
                        settingService,
                        configFactory,
                        dbFactory,
                        scope.ServiceProvider.GetRequiredService<IMusicBrainzRepository>(),
                        httpClientFactory
                    ),
                    httpClientFactory
                )
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
