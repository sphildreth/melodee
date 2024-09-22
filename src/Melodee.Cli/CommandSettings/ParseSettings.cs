using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class ParseSettings : Spectre.Console.Cli.CommandSettings
{
    [Description("The filename of the file to parse.")]
    [CommandArgument(0, "[FILENAME]")]
    [Required]
    public string Filename { get; init; } = string.Empty;

    [Description("Output verbose debug and timing results to console.")]
    [CommandOption("--verbose")]
    [DefaultValue(true)]
    public bool Verbose { get; init; }
}
