using System.Diagnostics;
using Melodee.Cli.CommandSettings;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Plugins.MetaData.Directory;
using Melodee.Plugins.MetaData.Track;
using Melodee.Plugins.Processor;
using Melodee.Plugins.Scripting;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace Melodee.Cli.Command;

public class ParseCommand : AsyncCommand<ParseSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ParseSettings settings)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var config = new Configuration
        {
            PluginProcessOptions = new PluginProcessOptions
            {
                DoDeleteOriginal = false,
                DoOverrideExistingMelodeeDataFiles = false
            },
            MediaConvertorOptions = new MediaConvertorOptions(),
            Scripting = new Scripting
            {
                PreDiscoveryScript = string.Empty
            },
            InboundDirectory = string.Empty,
            StagingDirectory = string.Empty,
            LibraryDirectory = string.Empty
        };
        
        var fileInfo = new System.IO.FileInfo(settings.Filename);
        if (!fileInfo.Exists)
        {
            throw new Exception($"Parse File [{settings.Filename}] does not exist.");
        }
        
        var sw = System.Diagnostics.Stopwatch.StartNew();
        Log.Debug("\u250d Parsing File [{NfoFilename}]", settings.Filename);

        bool isValid = false;
        
        var sfv = new SimpleFileVerification(
            new []
            {
                new AtlMetaTag(new MetaTagsProcessor(config), config)
            }, config);
        if (sfv.DoesHandleFile(fileInfo.Directory.ToDirectorySystemInfo(), fileInfo.ToFileSystemInfo()))
        {
            try
            {
                var svfResult = await sfv.ProcessDirectoryAsync(fileInfo.Directory.ToDirectorySystemInfo());

                sw.Stop();
                Log.Debug("\u2515 Processed SFV File [{NfoFilename}] in [{ElapsedTime}]", settings.Filename, sw.Elapsed);

                if (settings.Verbose)
                {
                    AnsiConsole.Write(
                        new Panel(new JsonText(System.Text.Json.JsonSerializer.Serialize(svfResult)))
                            .Header("Parse Result")
                            .Collapse()
                            .RoundedBorder()
                            .BorderColor(Color.Yellow));
                }
                isValid = svfResult.IsSuccess;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }        
        
        var m3u = new M3UPlaylist(
            new []
            {
                new AtlMetaTag(new MetaTagsProcessor(config), config)
            }, config);
        if (m3u.DoesHandleFile(fileInfo.Directory.ToDirectorySystemInfo(), fileInfo.ToFileSystemInfo()))
        {
            try
            {
                var svfResult = await m3u.ProcessDirectoryAsync(fileInfo.Directory.ToDirectorySystemInfo());

                sw.Stop();
                Log.Debug("\u2515 Processed M3U File [{NfoFilename}] in [{ElapsedTime}]", settings.Filename, sw.Elapsed);

                if (settings.Verbose)
                {
                    AnsiConsole.Write(
                        new Panel(new JsonText(System.Text.Json.JsonSerializer.Serialize(svfResult)))
                            .Header("Parse Result")
                            .Collapse()
                            .RoundedBorder()
                            .BorderColor(Color.Yellow));
                }
                isValid = svfResult.IsSuccess;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }          
        
        var nfo = new Nfo(config);
        if (nfo.DoesHandleFile(fileInfo.Directory.ToDirectorySystemInfo(), fileInfo.ToFileSystemInfo()))
        {
            try
            {
                var nfoParserResult = await nfo.ReleaseForNfoFileAsync(fileInfo, fileInfo.Directory.ToDirectorySystemInfo());

                sw.Stop();
                Log.Debug("\u2515 Processed Nfo File [{NfoFilename}] in [{ElapsedTime}]", settings.Filename, sw.Elapsed);

                if (settings.Verbose)
                {
                    AnsiConsole.Write(
                        new Panel(new JsonText(System.Text.Json.JsonSerializer.Serialize(nfoParserResult)))
                            .Header("Parse Result")
                            .Collapse()
                            .RoundedBorder()
                            .BorderColor(Color.Yellow));
                }
                isValid = nfoParserResult.IsValid();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        return isValid ? 0 : 1;
    }
}
