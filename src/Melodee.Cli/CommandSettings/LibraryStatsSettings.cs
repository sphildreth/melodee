using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class LibraryStatsSettings : LibrarySettings
{
    [Description("Output results (where applicable) in raw format versus pretty table.")]
    [CommandOption("--raw")]
    [DefaultValue(false)]
    public bool ReturnRaw { get; init; }

    [Description("Skip informational messages.")]
    [CommandOption("--borked")]
    [DefaultValue(false)]
    public bool ShowOnlyIssues { get; init; }
}
