using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class JobSettings : Spectre.Console.Cli.CommandSettings
{
    [Description("Output verbose debug and timing results to console.")]
    [CommandOption("--verbose")]
    [DefaultValue(true)]
    public bool Verbose { get; init; }
}
