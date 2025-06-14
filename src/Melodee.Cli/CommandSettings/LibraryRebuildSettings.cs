using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class LibraryRebuildSettings : LibrarySettings
{
    [Description("Only create missing Melodee data files, when false, will recreate all files.")]
    [CommandOption("--only-missing")]
    [DefaultValue(true)]
    public bool CreateOnlyMissing { get; init; }

    [Description("Rebuild only this library path.")]
    [CommandArgument(1, "[only-path]")]
    public string? OnlyPath { get; init; }
}
