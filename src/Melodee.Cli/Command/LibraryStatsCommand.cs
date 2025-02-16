using Melodee.Cli.CommandSettings;
using Melodee.Common.Enums;
using Melodee.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Melodee.Cli.Command;

/// <summary>
///     Generate some statistics for the given Library.
/// </summary>
public class LibraryStatsCommand : CommandBase<LibraryStatsSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, LibraryStatsSettings settings)
    {
        using (var scope = CreateServiceProvider().CreateScope())
        {
            var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();
            var result = await libraryService.Statistics(settings.LibraryName);
            if (!settings.ReturnRaw)
            {
                var table = new Table();
                table.AddColumn("Statistic");
                table.AddColumn("Data");
                table.AddColumn("Information");
                foreach (var stat in result.Data ?? [])
                {
                    if (!settings.ShowOnlyIssues || (settings.ShowOnlyIssues && stat.Type != StatisticType.Information))
                    {
                        table.AddRow(stat.Title, $"[{stat.DisplayColor}]{stat.Data?.ToString().EscapeMarkup() ?? string.Empty}[/]", stat.Message.EscapeMarkup());
                    }
                }

                AnsiConsole.Write(table);
                if (!settings.ShowOnlyIssues)
                {
                    if (result.Messages?.Any() ?? false)
                    {
                        AnsiConsole.WriteLine();
                        foreach (var message in result.Messages)
                        {
                            AnsiConsole.WriteLine(message.EscapeMarkup());
                        }

                        AnsiConsole.WriteLine();
                    }
                }
            }
            else
            {
                foreach (var stat in result.Data ?? [])
                {
                    if (!settings.ShowOnlyIssues || (settings.ShowOnlyIssues && stat.Type != StatisticType.Information))
                    {
                        AnsiConsole.WriteLine($"{stat.Title}\t{stat.Data}\t{stat.Message}");
                    }
                }
            }

            return 1;
        }
    }
}
