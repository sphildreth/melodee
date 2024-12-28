using Melodee.Cli.Command;
using Melodee.Cli.CommandSettings;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Melodee.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.AddBranch<ConfigurationSetSetting>("configuration", add =>
            {
                add.AddCommand<ConfigurationSetCommand>("set")
                    .WithDescription("Modify Melodee configuration.");
            });            
            config.AddBranch<JobSettings>("job", add =>
            {
                add.AddCommand<JobRunMusicBrainzUpdateDatabaseJobCommand>("musicbrainz-update")
                    .WithDescription("Run MusicBrainz update database job. This downloads MusicBrainz data dump and creates local database for Melodee when scanning metadata.");
            });
            config.AddBranch<LibrarySettings>("library", add =>
            {
                add.AddCommand<LibraryAlbumStatusReportCommand>("album-report")
                    .WithAlias("ar")
                    .WithDescription("Show report of albums found for library.");
                add.AddCommand<LibraryCleanCommand>("clean")
                    .WithAlias("c")
                    .WithDescription("Clean library and delete any folders without media files. CAUTION: Destructive!");
                add.AddCommand<ProcessInboundCommand>("process")
                    .WithAlias("p")
                    .WithDescription("Process media in given library into staging library.");
                add.AddCommand<LibraryMoveOkCommand>("move-ok")
                    .WithAlias("m")
                    .WithDescription("Move 'Ok' status albums into the given library.");
                add.AddCommand<LibraryScanCommand>("scan")
                    .WithAlias("s")
                    .WithDescription("Scan all non inbound and staging libraries for database updates from albums.");
                add.AddCommand<LibraryStatsCommand>("stats")
                    .WithAlias("ss")
                    .WithDescription("Show statistics for given library and library directory.");
            });
            config.AddBranch<ParseSettings>("parser", add =>
            {
                add.AddCommand<ParseCommand>("parse")
                    .WithDescription("Parse a given media file (CUE, NFO, SFV, etc.) and show results.");
            });
            config.AddBranch<ValidateSettings>("validate", add =>
            {
                add.AddCommand<ValidateCommand>("album")
                    .WithDescription("Validate a Melodee album data file (melodee.json).");
            });
            config.AddBranch<ShowTagsSettings>("tags", add =>
            {
                add.AddCommand<ShowTagsCommand>("show")
                    .WithDescription("Load given media file and show all known ID3 tags.");
            });
        });

        var version = $"1.0.0-rc2";
        AnsiConsole.MarkupLine($":musical_note: Melodee Command Line Interface v{version}");
        AnsiConsole.MarkupLine("");
        
        return app.Run(args);
    }
}
