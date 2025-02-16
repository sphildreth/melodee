using System.Diagnostics;
using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Extensions;
using Melodee.Common.Metadata.Mpeg;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.Conversion.Image;
using Melodee.Common.Plugins.MetaData.Song;
using Melodee.Common.Plugins.Processor;
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace Melodee.Cli.Command;

public class ShowMpegInfoCommand : CommandBase<ShowMpegInfoSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ShowMpegInfoSettings settings)
    {
        using (var scope = CreateServiceProvider().CreateScope())
        {
            var serializer = scope.ServiceProvider.GetRequiredService<ISerializer>();
            var configFactory = scope.ServiceProvider.GetRequiredService<IMelodeeConfigurationFactory>();
            var config = await configFactory.GetConfigurationAsync();

            var imageValidator = new ImageValidator(config);
            var imageConvertor = new ImageConvertor(config);

            var fileInfo = new FileInfo(settings.Filename);
            if (!fileInfo.Exists)
            {
                throw new Exception($"Media file [{settings.Filename}] does not exist.");
            }

            if (fileInfo.Directory == null)
            {
                throw new Exception($"Media file directory [{settings.Filename}] does not exist.");
            }

            Trace.WriteLine($"\ud83d\udcdc Processing File [{settings.Filename}]");

            var mpeg = new Mpeg(fileInfo.FullName);
            await mpeg.ReadAsync().ConfigureAwait(false);

            AnsiConsole.Write(
                new Panel(new JsonText(serializer.Serialize(mpeg) ?? string.Empty))
                    .Header("MPEG Info")
                    .Collapse()
                    .RoundedBorder()
                    .BorderColor(mpeg.IsValid ? Color.Green : Color.Red));            
            
            return mpeg.IsValid ? 0 : 1;
        }
    }
}
