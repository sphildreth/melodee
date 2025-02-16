using System.ComponentModel;
using Spectre.Console.Cli;

namespace Melodee.Cli.CommandSettings;

public class JobSettings : Spectre.Console.Cli.CommandSettings
{
    [Description("Output verbose debug and timing results to console.")]
    [CommandOption("--verbose")]
    [DefaultValue(true)]
    public bool Verbose { get; init; }

    [Description("Use this value for any batch size, overwriting default batch size in configuration.")]
    [CommandOption("-b|--batchsize")]
    public int? BatchSize { get; init; }
}
