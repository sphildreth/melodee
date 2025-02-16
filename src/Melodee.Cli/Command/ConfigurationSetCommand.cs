using Melodee.Cli.CommandSettings;
using Melodee.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Melodee.Cli.Command;

/// <summary>
///     Modifies configuration values.
/// </summary>
public class ConfigurationSetCommand : CommandBase<ConfigurationSetSetting>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ConfigurationSetSetting settings)
    {
        using (var scope = CreateServiceProvider().CreateScope())
        {
            var settingService = scope.ServiceProvider.GetRequiredService<SettingService>();

            var config = await settingService.GetAsync(settings.Key).ConfigureAwait(false);
            if (!config.IsSuccess || config.Data == null)
            {
                AnsiConsole.MarkupLine($":warning: Unknown configuration key [{settings.Key}]");
                return 1;
            }

            if (settings.Remove)
            {
                config.Data.Value = string.Empty;
            }
            else
            {
                config.Data.Value = settings.Value;
            }

            var result = await settingService.UpdateAsync(config.Data).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                AnsiConsole.MarkupLine(":party_popper: Configuration updated.");
                return 0;
            }

            return 1;
        }
    }
}
