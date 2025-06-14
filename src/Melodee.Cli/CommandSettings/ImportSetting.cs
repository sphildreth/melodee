using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class ImportSetting : Spectre.Console.Cli.CommandSettings
{
    [Description("Output verbose debug and timing results to console.")]
    [CommandOption("--verbose")]
    [DefaultValue(true)]
    public bool Verbose { get; init; }

    [Description("CSV filename")]
    [CommandArgument(0, "[CSV]")]
    [Required]
    public string CsvFileName { get; init; } = string.Empty;
}
