using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class ShowMpegInfoSettings : Spectre.Console.Cli.CommandSettings
{
    [Description("Media file to read MPEG info.")]
    [CommandArgument(0, "[FILENAME]")]
    [Required]
    public string Filename { get; init; } = string.Empty;
}
