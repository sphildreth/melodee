using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class LibraryMoveOkSettings : LibrarySettings
{
    [Description("Name of library to move 'Ok' albums into.")]
    [CommandArgument(0, "[TOLIBRARY]")]
    [Required]
    public string ToLibraryName { get; set; } = string.Empty;
}
