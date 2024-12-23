using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class LibrarySetting : Spectre.Console.Cli.CommandSettings
{
    [Description("Name of library to process.")] 
    [CommandArgument(0, "[LIBRARY]")] 
    [Required]
    public string LibraryName { get; set; } = string.Empty;
    
    [Description("Output results (where applicable) in raw format versus pretty table.")]
    [CommandOption("--raw")]
    [DefaultValue(false)]
    public bool ReturnRaw { get; init; }    
    
    [Description("Only output issues, skip information messages.")]
    [CommandOption("--borked")]
    [DefaultValue(false)]
    public bool ShowOnlyIssues { get; init; }     
    
    [Description("Output verbose debug and timing results to console.")]
    [CommandOption("--verbose")]
    [DefaultValue(true)]
    public bool Verbose { get; init; }
}
