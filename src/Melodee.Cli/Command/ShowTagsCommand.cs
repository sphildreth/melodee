using System.Diagnostics;
using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.Conversion.Image;
using Melodee.Common.Plugins.MetaData.Song;
using Melodee.Common.Plugins.Processor;
using Melodee.Common.Plugins.Validation;
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

public class ShowTagsCommand : AsyncCommand<ShowTagsSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ShowTagsSettings settings)
    {
        // Read the given file and display the all the processed tags.

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
            var settingService = new SettingService(
                Log.Logger,
                cacheManager,
                scope.ServiceProvider.GetRequiredService<IMelodeeConfigurationFactory>(),
                scope.ServiceProvider.GetRequiredService<IDbContextFactory<MelodeeDbContext>>());
            var config = new MelodeeConfiguration(await settingService.GetAllSettingsAsync().ConfigureAwait(false));

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

            Log.Debug("\ud83d\udcdc Processing File [{MediaFilename}]", settings.Filename);

            var isValid = false;

            var metaTag = new AtlMetaTag(new MetaTagsProcessor(config, serializer), imageConvertor, imageValidator, config);
            var tagResult = await metaTag.ProcessFileAsync(fileInfo.Directory!.ToDirectorySystemInfo(), FileSystemInfoExtensions.ToFileSystemInfo(fileInfo));

            if (settings.OnlyTags.Nullify() != null)
            {
                var onlyTags = new Dictionary<string, object?>();
                foreach (var tag in settings.OnlyTags!.Split(','))
                {
                    var t = tag.ToNormalizedString() ?? string.Empty;
                    var value = tagResult.Data?.Tags?.FirstOrDefault(x => x.IdentifierDescription.ToNormalizedString()?.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false)?.Value;
                    onlyTags.TryAdd(t, value);
                }

                AnsiConsole.Write(
                    new Panel(new JsonText(serializer.Serialize(onlyTags) ?? string.Empty))
                        .Header("Only Tags")
                        .Collapse()
                        .RoundedBorder()
                        .BorderColor(Color.Yellow));
            }
            else if (settings.Verbose)
            {
                AnsiConsole.Write(
                    new Panel(new JsonText(serializer.Serialize(tagResult) ?? string.Empty))
                        .Header("File Info")
                        .Collapse()
                        .RoundedBorder()
                        .BorderColor(Color.Yellow));
            }

            return isValid ? 0 : 1;
        }
    }
}
