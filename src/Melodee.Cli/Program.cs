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
            config.AddBranch<ShowMpegInfoSettings>("file", add =>
            {
                add.AddCommand<ShowMpegInfoCommand>("mpeg")
                    .WithDescription("Load given file and show MPEG info and if Melodee thinks this is a valid MPEG file.");
            });
            config.AddBranch<ImportSetting>("import", add =>
            {
                add.AddCommand<ImportUserFavoriteCommand>("user-favorite-songs")
                    .WithDescription("Import user favorite songs from a given CSV file.");
            });
            config.AddBranch<JobSettings>("job", add =>
            {
                add.AddCommand<JobRunArtistSearchEngineDatabaseHousekeepingJobCommand>("artistsearchengine-refresh")
                    .WithDescription("Run artist search engine refresh job. This updates the local database of artists albums from search engines.");
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
                add.AddCommand<LibraryPurgeCommand>("purge")
                    .WithDescription("Purge library, deleting artists, albums, album songs and resetting library stats. CAUTION: Destructive!");
                add.AddCommand<LibraryMoveOkCommand>("move-ok")
                    .WithAlias("m")
                    .WithDescription("Move 'Ok' status albums into the given library.");
                add.AddCommand<LibraryRebuildCommand>("rebuild")
                    .WithAlias("r")
                    .WithDescription("Rebuild melodee metadata albums in the given library.");
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
                    .WithDescription("Validate a metadata album data file (melodee.json).");
            });
            config.AddBranch<ShowTagsSettings>("tags", add =>
            {
                add.AddCommand<ShowTagsCommand>("show")
                    .WithDescription("Load given media file and show all known ID3 tags.");
            });
        });


        var doShowVersion = args.Length < 4 || (args.Length > 3 && args[2] == "--verbose" && args[3]?.ToUpper() != "FALSE");
        if (doShowVersion)
        {
            var version = typeof(Program).Assembly.GetName().Version;
            AnsiConsole.MarkupLine($":musical_note: Melodee Command Line Interface v{version}");
            AnsiConsole.MarkupLine("");
        }

        return app.Run(args);
    }
}
