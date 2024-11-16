using Melodee.Cli.Command;
using Melodee.Cli.CommandSettings;
using Spectre.Console.Cli;

namespace Melodee.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.AddBranch<ProcessInboundSettings>("processor", add => { add.AddCommand<ProcessInboundCommand>("process"); });
            config.AddBranch<ParseSettings>("parser", add => { add.AddCommand<ParseCommand>("parse"); });
            config.AddBranch<ShowTagsSettings>("tags", add => { add.AddCommand<ShowTagsCommand>("show"); });
        });

        //:musical_note:
        //:musical_notes:
        //AnsiConsole.MarkupLine(":fire: :alien_monster: :sparkles:");
        // AnsiConsole.Markup("[white on blue]Melodee Command Line Interface[/]");
        //  AnsiConsole.MarkupLine("");

        return app.Run(args);
    }
}
