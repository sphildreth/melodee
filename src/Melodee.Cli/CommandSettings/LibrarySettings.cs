using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class LibrarySettings : Spectre.Console.Cli.CommandSettings
{
    [Description("Name of library to process.")] 
    [CommandArgument(0, "[NAME]")] 
    [Required]
    public string LibraryName { get; set; } = string.Empty;
    
    [Description("Output verbose debug and timing results to console.")]
    [CommandOption("--verbose")]
    [DefaultValue(true)]
    public bool Verbose { get; init; }
}
