using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class ShowTagsSettings : Spectre.Console.Cli.CommandSettings
{
    [Description("Media file to read tags.")]
    [CommandArgument(0, "[FILENAME]")]
    [Required]
    public string Filename { get; init; } = string.Empty;
    
    [Description("Only display given tag values in seperated comma format, not a sexy website.")]
    [CommandOption("-o|--onlytags")]
    public string? OnlyTags { get; init; }

    [Description("Output verbose debug and timing results to console.")]
    [CommandOption("--verbose")]
    [DefaultValue(true)]
    public bool Verbose { get; init; }
}
