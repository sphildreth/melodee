using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Plugins.Validation;
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

/// <summary>
///     Validates a given album
/// </summary>
public class ValidateCommand : CommandBase<ValidateSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ValidateSettings settings)
    {
        var isValid = false;

        using (var scope = CreateServiceProvider().CreateScope())
        {
            var serializer = scope.ServiceProvider.GetRequiredService<ISerializer>();
            var configFactory = scope.ServiceProvider.GetRequiredService<IMelodeeConfigurationFactory>();

            var config = await configFactory.GetConfigurationAsync();

            var albumValidator = new AlbumValidator(config);

            Album? album = null;
            if (settings is { LibraryName: not null, Id: not null })
            {
                var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();

                var libraryListResult = await libraryService.ListAsync(new PagedRequest()).ConfigureAwait(false);
                var library = libraryListResult.Data.FirstOrDefault(x => x.Name == settings.LibraryName);
                if (library == null)
                {
                    Log.Logger.Error("Could not find library named {LibraryName}", settings.LibraryName);
                    return 0;
                }

                var albumDiscoveryService = scope.ServiceProvider.GetRequiredService<AlbumDiscoveryService>();
                await albumDiscoveryService.InitializeAsync();
                album = await albumDiscoveryService.AlbumByUniqueIdAsync(library.ToFileSystemDirectoryInfo(), settings.Id.Value);
            }
            else if (settings.ApiKey != null)
            {
                var albumService = scope.ServiceProvider.GetRequiredService<AlbumService>();
                var albumResult = await albumService.GetByApiKeyAsync(SafeParser.ToGuid(settings.ApiKey)!.Value).ConfigureAwait(false);
                if (albumResult.IsSuccess)
                {
                    album = await Album.DeserializeAndInitializeAlbumAsync(serializer, Path.Combine(albumResult.Data!.Directory, "melodee.json")).ConfigureAwait(false);
                }
            }
            else if (settings.PathToMelodeeDataFile != null)
            {
                album = await Album.DeserializeAndInitializeAlbumAsync(serializer, Path.Combine(settings.PathToMelodeeDataFile)).ConfigureAwait(false);
            }

            if (album != null)
            {
                var validationResult = albumValidator.ValidateAlbum(album);
                isValid = validationResult.IsSuccess;
                AnsiConsole.Write(
                    new Panel(new JsonText(serializer.Serialize(validationResult) ?? string.Empty))
                        .Header("Validation Result")
                        .Collapse()
                        .RoundedBorder()
                        .BorderColor(Color.Red));
            }

            return isValid ? 0 : 1;
        }
    }
}
