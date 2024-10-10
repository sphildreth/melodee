using System.Diagnostics;
using System.Text.Json;
using Melodee.Cli.CommandSettings;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Plugins.Discovery.Releases;
using Melodee.Plugins.MetaData.Track;
using Melodee.Plugins.Processor;
using Melodee.Plugins.Scripting;
using Melodee.Plugins.Validation;
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

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var config = new Configuration
        {
            PluginProcessOptions = new PluginProcessOptions
            {
                DoDeleteOriginal = !settings.CopyMode,
                DoOverrideExistingMelodeeDataFiles = settings.ForceMode
            },
            MediaConvertorOptions = new MediaConvertorOptions(),
            Scripting = new Scripting
            {
                PreDiscoveryScript = settings.PreDiscoveryScript
            },
            InboundDirectory = settings.Inbound,
            StagingDirectory = settings.Staging,
            LibraryDirectory = string.Empty
        };

        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(4))
            .AddColumn()
            .AddRow("[b]Copy Mode?[/]", $"{YesNo(!config.PluginProcessOptions.DoDeleteOriginal)}")
            .AddRow("[b]Force Mode?[/]", $"{YesNo(config.PluginProcessOptions.DoOverrideExistingMelodeeDataFiles)}")
            .AddRow("[b]PreDiscovery Script[/]", $"{config.Scripting.PreDiscoveryScript}")
            .AddRow("[b]Inbound[/]", $"{config.InboundDirectory}")
            .AddRow("[b]Staging[/]", $"{config.StagingDirectory}");
        
        AnsiConsole.Write(
            new Panel(grid)
                .Header("Configuration"));
        
        var validator = new ReleaseValidator(config);
        var processor = new DirectoryProcessor(
            new NullScript(config),
            new NullScript(config),
            validator,
            new ReleaseEditProcessor(config, 
                new ReleasesDiscoverer(validator, config), 
                new AtlMetaTag(new MetaTagsProcessor(config), config),
                validator),            
            config);
        var dirInfo = new DirectoryInfo(settings.Inbound);
        if (!dirInfo.Exists)
        {
            throw new Exception($"Directory [{settings.Inbound}] does not exist.");
        }

        var sw = Stopwatch.StartNew();

        Log.Debug("\ud83d\udcc1 Processing directory [{Inbound}]", settings.Inbound);

        var result = await processor.ProcessDirectoryAsync(new FileSystemDirectoryInfo
        {
            Path = dirInfo.FullName,
            Name = dirInfo.Name
        });

        sw.Stop();
        Log.Debug("ℹ️ Processed directory [{Inbound}] in [{ElapsedTime}]", settings.Inbound, sw.Elapsed);

        if (settings.Verbose)
        {
            AnsiConsole.Write(
                new Panel(new JsonText(JsonSerializer.Serialize(result)))
                    .Header("Process Result")
                    .Collapse()
                    .RoundedBorder()
                    .BorderColor(Color.Yellow));
        }

        // For console error codes, 0 is success.
        return result.IsSuccess ? 0 : 1;
    }

    private static string YesNo(bool value)
    {
        return value ? "Yes" : "No";
    }    
}
