using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;
using ValidationResult = Spectre.Console.ValidationResult;

namespace Melodee.Cli.CommandSettings;

public class ProcessInboundSettings : Spectre.Console.Cli.CommandSettings
{
    [Description("The inbound directory holding music files to process.")]
    [CommandArgument(0, "[INBOUND]")]
    [Required]
    public string Inbound { get; set; } = string.Empty;

    [Description("The staging directory to place processed files into.")]
    [CommandArgument(0, "[STAGING]")]
    [Required]
    public string Staging { get; set; } = string.Empty;

    [Description("Copy or move files from inbound. If set then processed files are not deleted.")]
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
