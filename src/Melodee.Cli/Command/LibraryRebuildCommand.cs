using Melodee.Cli.CommandSettings;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace Melodee.Cli.Command;

/// <summary>
///     For the given library rebuild all of the Melodee data files from files in place (no conversion, no manipulations to
///     any files in place).
/// </summary>
public class LibraryRebuildCommand : CommandBase<LibraryRebuildSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, LibraryRebuildSettings settings)
    {
        using (var scope = CreateServiceProvider().CreateScope())
        {
            var serializer = scope.ServiceProvider.GetRequiredService<ISerializer>();
            var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();

            if (!settings.CreateOnlyMissing)
            {
                var cleanResult = await libraryService.CleanLibraryAsync(settings.LibraryName);
                if (!cleanResult.IsSuccess)
                {
                    AnsiConsole.Write(
                        new Panel(new JsonText(serializer.Serialize(cleanResult) ?? string.Empty))
                            .Header("Not successful")
                            .Collapse()
                            .RoundedBorder()
                            .BorderColor(Color.Yellow));
                }
            }

            var result = await libraryService.Rebuild(settings.LibraryName, settings.CreateOnlyMissing, settings.Verbose).ConfigureAwait(false);
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
