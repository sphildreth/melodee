using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class ProcessInboundSettings : Spectre.Console.Cli.CommandSettings
{
    [Description("The inbound directory holding music files to process.")]
    [CommandArgument(0, "[INBOUND]")]
    public string Inbound { get; set; }
    
    [Description("The staging directory to place processed files into.")]
    [CommandArgument(0, "[STAGING]")]
    public string Staging { get; set; }
    
    [Description("Copy or move files from inbound. If set then processed files are not deleted.")]
    [CommandOption("--copy")]
    [DefaultValue(true)]
    public bool CopyMode { get; init; }    
    
    [Description("Script to run before Processing.")]
    [CommandArgument(0, "[PRESCRIPT]")]
    [DefaultValue(null)]
    public string? PreDiscoveryScript { get; set; }
    
    public override ValidationResult Validate()
    {
        if (string.IsNullOrEmpty(Inbound))
        {
            return ValidationResult.Error("Inbound directory is required");
        }
        if (string.IsNullOrEmpty(Staging))
        {
            return ValidationResult.Error("Staging directory is required");
        }
        return ValidationResult.Success();
    }    
}