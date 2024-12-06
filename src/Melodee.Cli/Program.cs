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
           
            config.AddBranch<LibrarySetting>("library", add =>
            {
                add.AddCommand<ProcessInboundCommand>("process")
                    .WithAlias("p")
                    .WithDescription("Process media in given library into staging library.");
                add.AddCommand<LibraryMoveOkCommand>("move-ok")
                    .WithAlias("m")
                    .WithDescription("Move 'Ok' status processed media into the given library.");
                add.AddCommand<LibraryScanCommand>("scan")
                    .WithAlias("s")
                    .WithDescription("Scan all non inbound and staging libraries for database updates from processed media.");

            });          
            config.AddBranch<ParseSettings>("parser", add =>
            {
                add.AddCommand<ParseCommand>("parse")
                    .WithDescription("Parse a given media file (CUE, NFO, SFV, etc.) and show results.");
            });
            config.AddBranch<ShowTagsSettings>("tags", add =>
            {
                add.AddCommand<ShowTagsCommand>("show")
                    .WithDescription("Load given media file and show all known ID3 tags.");
            });
        });

        //:musical_note:
        //:musical_notes:
        //AnsiConsole.MarkupLine(":fire: :alien_monster: :sparkles:");
        // AnsiConsole.Markup("[white on blue]Melodee Command Line Interface[/]");
        //  AnsiConsole.MarkupLine("");

        return app.Run(args);
    }
}
