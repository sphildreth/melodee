using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Enums;
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
///     Generate some statistics for the given Library.
/// </summary>
public class LibraryStatsCommand : AsyncCommand<LibraryStatsSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, LibraryStatsSettings settings)
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
            var configFactory = scope.ServiceProvider.GetRequiredService<IMelodeeConfigurationFactory>();

            var libraryService = new LibraryService(Log.Logger,
                cacheManager,
                dbFactory,
                configFactory,
                serializer);
            var configurationFactory = new MelodeeConfigurationFactory(dbFactory);

            var result = await libraryService.Statistics(settings.LibraryName);

            if (!settings.ReturnRaw)
            {
                var table = new Table();
                table.AddColumn("Statistic");
                table.AddColumn("Data");
                table.AddColumn("Information");
                foreach (var stat in result.Data ?? [])
                {
                    if (!settings.ShowOnlyIssues || (settings.ShowOnlyIssues && stat.Type != StatisticType.Information))
                    {
                        table.AddRow(stat.Title, $"[{stat.DisplayColor}]{stat.Data?.ToString().EscapeMarkup() ?? string.Empty}[/]", stat.Message.EscapeMarkup());
                    }
                }

                AnsiConsole.Write(table);
                if (!settings.ShowOnlyIssues)
                {
                    if (result.Messages?.Any() ?? false)
                    {
                        AnsiConsole.WriteLine();
                        foreach (var message in result.Messages)
                        {
                            AnsiConsole.WriteLine(message.EscapeMarkup());
                        }

                        AnsiConsole.WriteLine();
                    }
                }
            }
            else
            {
                foreach (var stat in result.Data ?? [])
                {
                    if (!settings.ShowOnlyIssues || (settings.ShowOnlyIssues && stat.Type != StatisticType.Information))
                    {
                        Console.WriteLine($"{stat.Title}\t{stat.Data}\t{stat.Message}");
                    }
                }
            }

            return 1;
        }
    }
}
