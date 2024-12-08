using System.Net;
using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Serialization;
using Melodee.Jobs;
using Melodee.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Services;
using Melodee.Services.Caching;
using Melodee.Services.Scanning;
using Melodee.Services.SearchEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Serilog;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Melodee.Cli.Command;

/// <summary>
/// This runs the job that scans all the library type libraries 
/// </summary>
public class LibraryScanCommand : AsyncCommand<LibrarySetting>
{
    public override async Task<int> ExecuteAsync(CommandContext context, LibrarySetting settings)
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
        services.AddHttpClient();
        services.AddSingleton<IDbConnectionFactory>(opt => 
            new OrmLiteConnectionFactory(configuration.GetConnectionString("MusicBrainzConnection"), SqliteDialect.Provider));
        services.AddScoped<MusicBrainzRepository>();    
        services.AddSingleton<IMelodeeConfigurationFactory, MelodeeConfigurationFactory>();
        services.AddSingleton(Log.Logger);
        
        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MelodeeDbContext>>();
            var settingService = new SettingService(Log.Logger, cacheManager, dbFactory);
            var libraryService = new LibraryService(Log.Logger,
                cacheManager,
                dbFactory,
                settingService,
                serializer);
            
            var configurationFactory = new MelodeeConfigurationFactory(dbFactory);
            
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            
            var job = new LibraryProcessJob
            (
                Log.Logger,
                configurationFactory,
                libraryService,
                serializer,
                dbFactory,
                new ArtistService(Log.Logger, cacheManager, dbFactory),
                new AlbumService(Log.Logger, cacheManager, dbFactory),
                new AlbumDiscoveryService(Log.Logger, cacheManager, dbFactory, settingService, serializer),
                new DirectoryProcessorService(
                    Log.Logger, 
                    cacheManager, 
                    dbFactory, 
                    settingService, 
                    libraryService, 
                    serializer,
                    new MediaEditService(
                        Log.Logger, 
                        cacheManager, 
                        dbFactory, 
                        settingService, 
                        libraryService, 
                        new AlbumDiscoveryService(
                            Log.Logger, 
                            cacheManager, 
                            dbFactory, 
                            settingService, 
                            serializer), 
                        serializer, 
                        httpClientFactory
                    ),
                    new ArtistSearchEngineService
                    (
                        Log.Logger,
                        cacheManager,
                        serializer,
                        settingService,
                        dbFactory,
                        scope.ServiceProvider.GetRequiredService<MusicBrainzRepository>(),
                        httpClientFactory
                    ),
                    new AlbumImageSearchEngineService
                    (
                        Log.Logger,
                        cacheManager,
                        serializer,
                        settingService,
                        dbFactory,
                        scope.ServiceProvider.GetRequiredService<MusicBrainzRepository>(),
                        httpClientFactory
                    ),
                    httpClientFactory
                )
            );

            await job.Execute(new JobExecutionContext(CancellationToken.None));
            return 1;
        }
    }
}

internal class JobExecutionContext : IJobExecutionContext
{
    public JobExecutionContext(CancellationToken cancellation)
    {
        CancellationToken = cancellation;
    }
    
    public void Put(object key, object objectValue)
    {
        throw new NotImplementedException();
    }

    public object? Get(object key)
    {
        throw new NotImplementedException();
    }

    public IScheduler Scheduler { get; }
    public ITrigger Trigger { get; }
    public ICalendar? Calendar { get; }
    public bool Recovering { get; }
    public TriggerKey RecoveringTriggerKey { get; }
    public int RefireCount { get; }
    public JobDataMap MergedJobDataMap { get; }
    public IJobDetail JobDetail { get; } = new JobDetailImpl();
    public IJob JobInstance { get; }
    public DateTimeOffset FireTimeUtc { get; }
    public DateTimeOffset? ScheduledFireTimeUtc { get; }
    public DateTimeOffset? PreviousFireTimeUtc { get; }
    public DateTimeOffset? NextFireTimeUtc { get; }
    public string FireInstanceId { get; }
    public object? Result { get; set; }
    public TimeSpan JobRunTime { get; }
    public CancellationToken CancellationToken { get; }
}
