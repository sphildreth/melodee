using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;
using ValidationResult = Spectre.Console.ValidationResult;

namespace Melodee.Cli.CommandSettings;

public class LibraryProcessSettings : LibrarySettings
{
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

    public override ValidationResult Validate()
    {
        if (string.IsNullOrEmpty(LibraryName))
        {
            return ValidationResult.Error("Library name is required");
        }
        return ValidationResult.Success();
    }
}
