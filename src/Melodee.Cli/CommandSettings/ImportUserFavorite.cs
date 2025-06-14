using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class ImportUserFavorite : ImportSetting
{
    [Description("User api key")]
    [CommandArgument(1, "[USERAPIKEY]")]
    [Required]
    public string UserApiKey { get; init; } = string.Empty;

    [Description("Artist name column")]
    [CommandArgument(2, "[ARTIST]")]
    [Required]
    public string Artist { get; init; } = string.Empty;

    [Description("Album name column")]
    [CommandArgument(3, "[ALBUM]")]
    [Required]
    public string Album { get; init; } = string.Empty;

    [Description("Song name column")]
    [CommandArgument(4, "[SONG]")]
    [Required]
    public string Song { get; init; } = string.Empty;

    [Description("Don't actually do anything, just show what would happen.")]
    [CommandOption("--pretend")]
    [DefaultValue(false)]
    public bool IsPretend { get; init; }
}
