using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Serialization;
using Melodee.Jobs;
using Melodee.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Services;
using Melodee.Services.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Spectre.Console;
using Spectre.Console.Cli;


namespace Melodee.Cli.Command;

public class JobRunMusicBrainzUpdateDatabaseJobCommand : AsyncCommand<JobSettings>
{
    public async override Task<int> ExecuteAsync(CommandContext context, JobSettings settings)
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
        services.AddScoped<IMusicBrainzRepository, SQLiteMusicBrainzRepository>();
        services.AddSingleton<IMelodeeConfigurationFactory, MelodeeConfigurationFactory>();
        services.AddSingleton(Log.Logger);
        
        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MelodeeDbContext>>();
            var melodeeConfigurationFactory = scope.ServiceProvider.GetRequiredService<IMelodeeConfigurationFactory>();
            var settingService = new SettingService(Log.Logger, cacheManager, dbFactory);
            var job = new MusicBrainzUpdateDatabaseJob
            (
                Log.Logger,
                melodeeConfigurationFactory,
                settingService,
                scope.ServiceProvider.GetRequiredService<IHttpClientFactory>(),
                scope.ServiceProvider.GetRequiredService<IMusicBrainzRepository>()
            );
            await job.Execute(new JobExecutionContext(CancellationToken.None));
            return 1;
        }
    }
}
