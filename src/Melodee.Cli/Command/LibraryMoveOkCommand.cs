using Melodee.Cli.CommandSettings;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Serialization;
using Melodee.Plugins.Validation;
using Melodee.Services;
using Melodee.Services.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Melodee.Cli.Command;

public class LibraryMoveOkCommand : AsyncCommand<LibraryMoveOkSetting>
{
    public override async Task<int> ExecuteAsync(CommandContext context, LibraryMoveOkSetting settings)
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

        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MelodeeDbContext>>();
            var settingService = new SettingService(Log.Logger, cacheManager, dbFactory);
            var melodeeConfiguration = await settingService.GetMelodeeConfigurationAsync().ConfigureAwait(false);
            
            var libraryService = new LibraryService(Log.Logger,
                cacheManager,
                dbFactory,
                settingService,
                serializer,
                new ImageValidator(melodeeConfiguration));

            libraryService.OnProcessingProgressEvent += (sender, e) =>
            {
                switch (e.Type)
                {
                    case ProcessingEventType.Start:
                        if (e.Max == 0)
                        {
                            AnsiConsole.MarkupLine($"[yellow]No albums found.[/]");
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
                            AnsiConsole.MarkupLine($"[green]= completed moving albums.[/]");
                        }
                        break;
                }
            };
            
            var result = await libraryService.MoveAlbumsFromLibraryToLibrary(settings.LibraryName,
                    settings.ToLibraryName,
                    b => b.Status == AlbumStatus.Ok,
                    settings.Verbose)
                .ConfigureAwait(false);          

            return result.IsSuccess ? 0 : 1;
        }
    }
}
