using System.Diagnostics;
using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Melodee.Services;
using Melodee.Services.Caching;
using Melodee.Services.Scanning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace Melodee.Cli.Command;

public class ProcessInboundCommand : AsyncCommand<ProcessInboundSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ProcessInboundSettings settings)
    {
        // var font = FigletFont.Load("Fonts/Elite.flf");        
        //
        // AnsiConsole.Write(
        //     new FigletText(font, "Processing")
        //         .LeftJustified()
        //         .Color(Color.Purple3));        

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
            
            var config = new MelodeeConfiguration(await settingService.GetAllSettingsAsync().ConfigureAwait(false));

            var libraryToProcess = (await libraryService.ListAsync(new PagedRequest())).Data?.FirstOrDefault(x => x.Name == settings.LibraryName);
            if (libraryToProcess == null)
            {
                throw new Exception($"Library with name [{settings.LibraryName}] not found."); }

            var directoryInbound = libraryToProcess.Path;
            var directoryStaging = (await libraryService.GetStagingLibraryAsync().ConfigureAwait(false)).Data!.Path;

            var grid = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(4))
                .AddColumn()
                .AddRow("[b]Copy Mode?[/]", $"{YesNo(!SafeParser.ToBoolean(config.Configuration[SettingRegistry.ProcessingDoDeleteOriginal]))}")
                .AddRow("[b]Force Mode?[/]", $"{YesNo(SafeParser.ToBoolean(config.Configuration[SettingRegistry.ProcessingDoOverrideExistingMelodeeDataFiles]))}")
                .AddRow("[b]PreDiscovery Script[/]", $"{SafeParser.ToString(config.Configuration[SettingRegistry.ScriptingPreDiscoveryScript])}")
                .AddRow("[b]Inbound[/]", $"{directoryInbound}")
                .AddRow("[b]Staging[/]", $"{directoryStaging}");

            AnsiConsole.Write(
                new Panel(grid)
                    .Header("Configuration"));

            var processor = new DirectoryProcessorService(
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
                    scope.ServiceProvider.GetRequiredService<IHttpClientFactory>()
                )
            );
            var dirInfo = new DirectoryInfo(libraryToProcess.Path);
            if (!dirInfo.Exists)
            {
                throw new Exception($"Directory [{libraryToProcess.Path}] does not exist.");
            }

            var startTicks = Stopwatch.GetTimestamp();

            Log.Debug("\ud83d\udcc1 Processing library [{Inbound}]", libraryToProcess);

            await processor.InitializeAsync();

            var result = await processor.ProcessDirectoryAsync(libraryToProcess.ToFileSystemDirectoryInfo(), settings.ForceMode ? null : libraryToProcess.LastScanAt);

            Log.Debug("ℹ️ Processed library [{Inbound}] in [{ElapsedTime}]", libraryToProcess, Stopwatch.GetElapsedTime(startTicks));

            if (settings.Verbose)
            {
                AnsiConsole.Write(
                    new Panel(new JsonText(serializer.Serialize(result) ?? string.Empty))
                        .Header("Process Result")
                        .Collapse()
                        .RoundedBorder()
                        .BorderColor(Color.Yellow));
            }

            // For console error codes, 0 is success.
            return result.IsSuccess ? 0 : 1;
        }
    }

    private static string YesNo(bool value)
    {
        return value ? "Yes" : "No";
    }
}
