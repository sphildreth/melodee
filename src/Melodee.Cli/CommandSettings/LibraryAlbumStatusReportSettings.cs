using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class LibraryAlbumStatusReportSettings : LibrarySettings
{
    [Description("Output results (where applicable) in raw format versus pretty table.")]
    [CommandOption("--raw")]
    [DefaultValue(false)]
    public bool ReturnRaw { get; init; }
}
