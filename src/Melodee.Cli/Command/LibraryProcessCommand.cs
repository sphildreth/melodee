using System.Diagnostics;
using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Scanning;
using Melodee.Common.Utility;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace Melodee.Cli.Command;

public class ProcessInboundCommand : CommandBase<LibraryProcessSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, LibraryProcessSettings settings)
    {
        // var font = FigletFont.Load("Fonts/Elite.flf");        
        //
        // AnsiConsole.Write(
        //     new FigletText(font, "Processing")
        //         .LeftJustified()
        //         .Color(Color.Purple3));        

        if (settings.Verbose)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }

        using (var scope = CreateServiceProvider().CreateScope())
        {
            var serializer = scope.ServiceProvider.GetRequiredService<ISerializer>();
            var melodeeConfigurationFactory = scope.ServiceProvider.GetRequiredService<IMelodeeConfigurationFactory>();
            var melodeeConfiguration = await melodeeConfigurationFactory.GetConfigurationAsync().ConfigureAwait(false);

            var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();

            var libraryToProcess = (await libraryService.ListAsync(new PagedRequest())).Data?.FirstOrDefault(x => string.Equals(x.Name, settings.LibraryName, StringComparison.OrdinalIgnoreCase));
            if (libraryToProcess == null)
            {
                throw new Exception($"Library with name [{settings.LibraryName}] not found.");
            }

            var directoryInbound = libraryToProcess.Path;
            var directoryStaging = (await libraryService.GetStagingLibraryAsync().ConfigureAwait(false)).Data!.Path;

            var grid = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(4))
                .AddColumn()
                .AddRow("[b]Copy Mode?[/]", $"{YesNo(!SafeParser.ToBoolean(melodeeConfiguration.Configuration[SettingRegistry.ProcessingDoDeleteOriginal]))}")
                .AddRow("[b]Force Mode?[/]", $"{YesNo(SafeParser.ToBoolean(melodeeConfiguration.Configuration[SettingRegistry.ProcessingDoOverrideExistingMelodeeDataFiles]))}")
                .AddRow("[b]PreDiscovery Script[/]", $"{SafeParser.ToString(melodeeConfiguration.Configuration[SettingRegistry.ScriptingPreDiscoveryScript])}")
                .AddRow("[b]Inbound[/]", $"{directoryInbound.EscapeMarkup()}")
                .AddRow("[b]Staging[/]", $"{directoryStaging.EscapeMarkup()}");

            AnsiConsole.Write(
                new Panel(grid)
                    .Header("Configuration"));

            var processor = scope.ServiceProvider.GetRequiredService<DirectoryProcessorToStagingService>();
            var dirInfo = new DirectoryInfo(libraryToProcess.Path);
            if (!dirInfo.Exists)
            {
                throw new Exception($"Directory [{libraryToProcess.Path}] does not exist.");
            }

            var startTicks = Stopwatch.GetTimestamp();

            Log.Debug("\ud83d\udcc1 Processing library [{Inbound}]", libraryToProcess.ToString());

            await processor.InitializeAsync();

            var result = await processor.ProcessDirectoryAsync(libraryToProcess.ToFileSystemDirectoryInfo(), settings.ForceMode ? null : libraryToProcess.LastScanAt, settings.ProcessLimit);

            Log.Debug("ℹ️ Processed library [{Inbound}] in [{ElapsedTime}]", libraryToProcess.ToString(), Stopwatch.GetElapsedTime(startTicks));

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
