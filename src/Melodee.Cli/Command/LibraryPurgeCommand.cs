using Melodee.Cli.CommandSettings;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Extensions;
using Melodee.Common.Jobs;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Scanning;
using Melodee.Common.Services.SearchEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Rebus.Bus;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace Melodee.Cli.Command;

/// <summary>
///     This runs the library purge command that erases everything in a library
/// </summary>
public class LibraryPurgeCommand : CommandBase<LibraryScanSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, LibraryScanSettings settings)
    {
        using (var scope = CreateServiceProvider().CreateScope())
        {
            var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();
            var libraries = await libraryService.ListAsync(new PagedRequest { PageSize = short.MaxValue }).ConfigureAwait(false);
            var library = libraries.Data.FirstOrDefault(x => x.Name.ToNormalizedString() == settings.LibraryName.ToNormalizedString());
            if (library == null)
            {
                AnsiConsole.Write(
                    new Panel("Invalid library name.")
                        .Header("Purge Failed")
                        .Collapse()
                        .RoundedBorder()
                        .BorderColor(Color.Red));
                return 0;
            }

            var result = await libraryService.PurgeLibraryAsync(library.Id);

            var serializer = scope.ServiceProvider.GetRequiredService<ISerializer>();

            if (!result.IsSuccess && settings.Verbose)
            {
                AnsiConsole.Write(
                    new Panel(new JsonText(serializer.Serialize(result) ?? string.Empty))
                        .Header("Failed")
                        .Collapse()
                        .RoundedBorder()
                        .BorderColor(Color.Yellow));
                return 0;
            }

            if (settings.Verbose)
            {
                AnsiConsole.Write(
                    new Panel(new JsonText(serializer.Serialize(result) ?? string.Empty))
                        .Header("Successful")
                        .Collapse()
                        .RoundedBorder()
                        .BorderColor(Color.Green));
            }
            else
            {
                AnsiConsole.MarkupLine("[green]Successful[/]");
            }

            return 1;
        }
    }
}
