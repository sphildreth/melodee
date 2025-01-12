using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Melodee.Cli.Command;

/// <summary>
///     Modifies configuration values.
/// </summary>
public class ConfigurationSetCommand : AsyncCommand<ConfigurationSetSetting>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ConfigurationSetSetting settings)
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
        services.AddSingleton<IDbConnectionFactory>(_ =>
            new OrmLiteConnectionFactory(configuration.GetConnectionString("MusicBrainzConnection"), SqliteDialect.Provider));
        services.AddScoped<IMusicBrainzRepository, SQLiteMusicBrainzRepository>();
        services.AddSingleton<IMelodeeConfigurationFactory, MelodeeConfigurationFactory>();
        services.AddSingleton(Log.Logger);
        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var settingService = new SettingService(
                Log.Logger,
                cacheManager,
                scope.ServiceProvider.GetRequiredService<IMelodeeConfigurationFactory>(),
                scope.ServiceProvider.GetRequiredService<IDbContextFactory<MelodeeDbContext>>());

            var config = await settingService.GetAsync(settings.Key).ConfigureAwait(false);
            if (!config.IsSuccess || config.Data == null)
            {
                AnsiConsole.MarkupLine($":warning: Unknown configuration key [{settings.Key}]");
                return 1;
            }

            if (settings.Remove)
            {
                config.Data.Value = string.Empty;
            }
            else
            {
                config.Data.Value = settings.Value;
            }

            var result = await settingService.UpdateAsync(config.Data).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                AnsiConsole.MarkupLine(":party_popper: Configuration updated.");
                return 0;
            }

            return 1;
        }
    }
}
