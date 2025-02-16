using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class LibraryRebuild : LibrarySettings
{
    [Description("Maximum number of albums to process and then quit, null is unlimited.")]
    [CommandArgument(0, "[LIMIT]")]
    [DefaultValue(null)]
    public int? ProcessLimit { get; init; }
}
