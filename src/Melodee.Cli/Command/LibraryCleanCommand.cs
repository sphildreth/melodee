using Melodee.Cli.CommandSettings;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace Melodee.Cli.Command;

/// <summary>
///     Clean library of folders that don't add value.
/// </summary>
public class LibraryCleanCommand : CommandBase<LibraryCleanSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, LibraryCleanSettings settings)
    {
        using (var scope = CreateServiceProvider().CreateScope())
        {
            var serializer = scope.ServiceProvider.GetRequiredService<ISerializer>();
            var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();
            var result = await libraryService.CleanLibraryAsync(settings.LibraryName);
            if (settings.Verbose)
            {
                AnsiConsole.Write(
                    new Panel(new JsonText(serializer.Serialize(result) ?? string.Empty))
                        .Header("Process Result")
                        .Collapse()
                        .RoundedBorder()
                        .BorderColor(Color.Yellow));
            }

            return 1;
        }
    }
}
