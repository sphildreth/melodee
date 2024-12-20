using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class ValidateSettings : Spectre.Console.Cli.CommandSettings
{
    [Description("Output verbose debug and timing results to console.")]
    [CommandOption("--verbose")]
    [DefaultValue(true)]
    public bool Verbose { get; init; }
    
    [Description("Name of Library.")]
    [CommandOption("--library")]    
    public string? LibraryName { get; init; }

    [Description("Id of Melodee Data File (melodee.json) file to validate.")]
    [CommandOption("--id")] 
    public Guid? Id { get; init; }
    
    [Description("ApiKey of Album to Validate.")]
    [CommandOption("--apiKey")]
    public string? ApiKey { get; init; }

    [Description("Path to Melodee Data File (melodee.json) file to Validate.")]
    [CommandOption("--file")]
    public string? PathToMelodeeDataFile { get; init; }
}
