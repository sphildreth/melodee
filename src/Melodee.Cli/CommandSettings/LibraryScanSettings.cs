using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class LibraryScanSettings : LibrarySettings
{
    [Description("Ignore last scan at date on Library.")]
    [CommandOption("--force")]
    [DefaultValue(false)]
    public bool ForceMode { get; init; }
}
