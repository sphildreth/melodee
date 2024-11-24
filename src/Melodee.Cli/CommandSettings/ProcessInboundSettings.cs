using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;
using ValidationResult = Spectre.Console.ValidationResult;

namespace Melodee.Cli.CommandSettings;

public class ProcessInboundSettings : Spectre.Console.Cli.CommandSettings
{
    [Description("Name of library to process.")] 
    [CommandArgument(0, "[LIBRARY]")] 
    [Required]
    public string LibraryName { get; set; } = string.Empty;
 
    [Description("Copy or move files from library. If set then processed files are not deleted.")]
    [CommandOption("--copy")]
    [DefaultValue(true)]
    public bool CopyMode { get; init; }

    [Description("Override any existing Melodee data files.")]
    [CommandOption("--force")]
    [DefaultValue(true)]
    public bool ForceMode { get; init; }

    [Description("Script to run before Processing.")]
    [CommandArgument(0, "[PRESCRIPT]")]
    [DefaultValue(null)]
    public string? PreDiscoveryScript { get; set; }

    [Description("Output verbose debug and timing results to console.")]
    [CommandOption("--verbose")]
    [DefaultValue(true)]
    public bool Verbose { get; init; }


    public override ValidationResult Validate()
    {
        if (string.IsNullOrEmpty(LibraryName))
        {
            return ValidationResult.Error("Library name is required");
        }
        return ValidationResult.Success();
    }
}
