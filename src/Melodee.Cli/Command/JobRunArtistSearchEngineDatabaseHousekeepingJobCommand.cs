using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Jobs;
using Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Caching;
using Melodee.Common.Services.SearchEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Spectre.Console.Cli;

namespace Melodee.Cli.Command;

public class JobRunArtistSearchEngineDatabaseHousekeepingJobCommand : AsyncCommand<JobSettings>
{
        public override async Task<int> ExecuteAsync(CommandContext context, JobSettings settings)
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
        services.AddSingleton<IDbConnectionFactory>(opt =>
            new OrmLiteConnectionFactory(configuration.GetConnectionString("MusicBrainzConnection"), SqliteDialect.Provider));
        services.AddScoped<IMusicBrainzRepository, SQLiteMusicBrainzRepository>();
        services.AddSingleton<IMelodeeConfigurationFactory, MelodeeConfigurationFactory>();
        services.AddSingleton(Log.Logger);

        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MelodeeDbContext>>();
            var melodeeConfigurationFactory = scope.ServiceProvider.GetRequiredService<IMelodeeConfigurationFactory>();
            var settingService = new SettingService(Log.Logger, cacheManager, melodeeConfigurationFactory, dbFactory);
            var musicBrainzRepository = new SQLiteMusicBrainzRepository(
                Log.Logger,
                melodeeConfigurationFactory,
                scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>());            
            var artistSearchEngineService = new ArtistSearchEngineService(
                Log.Logger,
                cacheManager,
                settingService,
                melodeeConfigurationFactory,
                dbFactory,
                scope.ServiceProvider.GetRequiredService<IDbContextFactory<ArtistSearchEngineServiceDbContext>>(),
                musicBrainzRepository);            
            var job = new ArtistSearchEngineRepositoryHousekeepingJob
            (
                Log.Logger,
                melodeeConfigurationFactory,
                artistSearchEngineService,
                scope.ServiceProvider.GetRequiredService<IDbContextFactory<ArtistSearchEngineServiceDbContext>>()
            );
            var jc = new JobExecutionContext(CancellationToken.None);
            if (settings.BatchSize != null)
            {
                jc.Put(JobMapNameRegistry.BatchSize, settings.BatchSize);
            }
            await job.Execute(jc);
            return 1;
        }
    }
}
