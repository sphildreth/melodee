using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class LibraryMoveOkSetting : Spectre.Console.Cli.CommandSettings
{
    [Description("Name of library to process.")] 
    [CommandArgument(0, "[LIBRARY]")] 
    [Required]
    public string LibraryName { get; set; } = string.Empty;
    
    [Description("Name of library to move 'Ok' albums into.")] 
    [CommandArgument(0, "[TOLIBRARY]")] 
    [Required]
    public string ToLibraryName { get; set; } = string.Empty;
    
    [Description("Output verbose debug and timing results to console.")]
    [CommandOption("--verbose")]
    [DefaultValue(true)]
    public bool Verbose { get; init; }
}
