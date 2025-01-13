using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace Melodee.Cli.Command;

public class LibraryMoveOkCommand : AsyncCommand<LibraryMoveOkSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, LibraryMoveOkSettings settings)
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
        services.AddSingleton<IMelodeeConfigurationFactory, MelodeeConfigurationFactory>();

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

            libraryService.OnProcessingProgressEvent += (sender, e) =>
            {
                switch (e.Type)
                {
                    case ProcessingEventType.Start:
                        if (e.Max == 0)
                        {
                            AnsiConsole.MarkupLine("[yellow]No albums found.[/]");
                        }
                        else
                        {
                            AnsiConsole.MarkupLine($"[blue]| {e.Max} albums to move.[/]");
                        }

                        break;

                    case ProcessingEventType.Processing:
                        if (e.Max > 0 && e.Current % 10 == 0)
                        {
                            AnsiConsole.MarkupLine($"[blue]- moved {e.Current} albums.[/]");
                        }

                        break;

                    case ProcessingEventType.Stop:
                        if (e.Max > 0)
                        {
                            AnsiConsole.MarkupLine("[green]= completed moving albums.[/]");
                        }

                        break;
                }
            };

            if (settings.LibraryName == settings.ToLibraryName)
            {
                AnsiConsole.MarkupLine("[red]Source and destination library are the same.[/]");
                return 1;
            }

            var result = await libraryService.MoveAlbumsFromLibraryToLibrary(settings.LibraryName,
                    settings.ToLibraryName,
                    b => b.Status == AlbumStatus.Ok,
                    settings.Verbose)
                .ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                AnsiConsole.Write(
                    new Panel(new JsonText(serializer.Serialize(result) ?? string.Empty))
                        .Header("Not successful")
                        .Collapse()
                        .RoundedBorder()
                        .BorderColor(Color.Yellow));
            }

            return result.IsSuccess ? 0 : 1;
        }
    }
}
