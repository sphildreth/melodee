using Melodee.Cli.CommandSettings;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Plugins.Processor;
using Melodee.Plugins.Scripting;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace Melodee.Cli.Command;

public class ProcessInboundCommand : AsyncCommand<ProcessInboundSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ProcessInboundSettings settings)
    {
        // var grid = new Grid()
        //     .AddColumn(new GridColumn().NoWrap().PadRight(4))
        //     .AddColumn()
        //     .AddRow("[b]Enrichers[/]", string.Join(", ", AnsiConsole.Profile.Enrichers))
        //     .AddRow("[b]Color system[/]", $"{AnsiConsole.Profile.Capabilities.ColorSystem}")
        //     .AddRow("[b]Unicode?[/]", $"{YesNo(AnsiConsole.Profile.Capabilities.Unicode)}")
        //     .AddRow("[b]Supports ansi?[/]", $"{YesNo(AnsiConsole.Profile.Capabilities.Ansi)}")
        //     .AddRow("[b]Supports links?[/]", $"{YesNo(AnsiConsole.Profile.Capabilities.Links)}")
        //     .AddRow("[b]Legacy console?[/]", $"{YesNo(AnsiConsole.Profile.Capabilities.Legacy)}")
        //     .AddRow("[b]Interactive?[/]", $"{YesNo(AnsiConsole.Profile.Capabilities.Interactive)}")
        //     .AddRow("[b]Terminal?[/]", $"{YesNo(AnsiConsole.Profile.Out.IsTerminal)}")
        //     .AddRow("[b]Buffer width[/]", $"{AnsiConsole.Console.Profile.Width}")
        //     .AddRow("[b]Buffer height[/]", $"{AnsiConsole.Console.Profile.Height}")
        //     .AddRow("[b]Encoding[/]", $"{AnsiConsole.Console.Profile.Encoding.EncodingName}");
        //
        // AnsiConsole.Write(
        //     new Panel(grid)
        //         .Header("Information"));

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
                DoDeleteOriginal = !settings.CopyMode
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
        var processor = new DirectoryProcessor(
            new NullScript(config),
            new NullScript(config),
            config);
        var dirInfo = new System.IO.DirectoryInfo(settings.Inbound);
        if (!dirInfo.Exists)
        {
            throw new Exception($"Directory [{settings.Inbound}] does not exist.");
        }

        Log.Debug("Processing directory [{Inbound}]", settings.Inbound);
        
        var result = await processor.ProcessDirectoryAsync(new FileSystemDirectoryInfo
        {
            Path = dirInfo.FullName,
            Name = dirInfo.Name
        });
        
        AnsiConsole.Write(
            new Panel(new JsonText(System.Text.Json.JsonSerializer.Serialize(result)))
                .Header("Process Result")
                .Collapse()
                .RoundedBorder()
                .BorderColor(Color.Yellow));
        
        return 1;
    }

    // private static string YesNo(bool value)
    // {
    //     return value ? "Yes" : "No";
    // }    
}