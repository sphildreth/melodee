using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
using Melodee.Common.Models.OpenSubsonic.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NodaTime;

namespace Melodee.Common.Data;

public class MelodeeDbContext(DbContextOptions<MelodeeDbContext> options) : DbContext(options)
{
    public DbSet<Album> Albums { get; set; }

    public DbSet<Artist> Artists { get; set; }

    public DbSet<ArtistRelation> ArtistRelation { get; set; }

    public DbSet<Bookmark> Bookmarks { get; set; }

    public DbSet<Contributor> Contributors { get; set; }

    public DbSet<Library> Libraries { get; set; }

    public DbSet<LibraryScanHistory> LibraryScanHistories { get; set; }

    public DbSet<Player> Players { get; set; }

    public DbSet<Playlist> Playlists { get; set; }

    public DbSet<PlaylistSong> PlaylistSong { get; set; }

    public DbSet<PlayQueue> PlayQues { get; set; }

    public DbSet<RadioStation> RadioStations { get; set; }

    public DbSet<Setting> Settings { get; set; }

    public DbSet<SearchHistory> SearchHistories { get; set; }

    public DbSet<Share> Shares { get; set; }

    public DbSet<ShareActivity> ShareActivities { get; set; }

    public DbSet<Song> Songs { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<UserAlbum> UserAlbums { get; set; }

    public DbSet<UserArtist> UserArtists { get; set; }

    public DbSet<UserPin> UserPins { get; set; }

    public DbSet<UserSong> UserSongs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);

        modelBuilder.Entity<Library>(s =>
        {
            s.HasData(new Library
                {
                    Id = 1,
                    Name = "Inbound",
                    Description =
                        "Files in this directory are scanned and Album information is gathered via processing.",
                    Path = "/storage/inbound/",
                    Type = (int)LibraryType.Inbound,
                    CreatedAt = now
                },
                new Library
                {
                    Id = 2,
                    Name = "Staging",
                    Description =
                        "The staging directory to place processed files into (Inbound -> Staging -> Library).",
                    Path = "/storage/staging/",
                    Type = (int)LibraryType.Staging,
                    CreatedAt = now
                },
                new Library
                {
                    Id = 3,
                    Name = "Storage",
                    Description =
                        "The library directory to place processed, reviewed and ready to use music files into.",
                    Path = "/storage/library/",
                    Type = (int)LibraryType.Storage,
                    CreatedAt = now
                },
                new Library
                {
                    Id = 4,
                    Name = "User Images",
                    Description = "Library where user images are stored.",
                    Path = "/storage/images/users/",
                    Type = (int)LibraryType.UserImages,
                    CreatedAt = now
                },
                new Library
                {
                    Id = 5,
                    Name = "Playlist Data",
                    Description = "Library where playlist data is stored.",
                    Path = "/storage/playlists/",
                    Type = (int)LibraryType.Playlist,
                    CreatedAt = now
                });
        });

        modelBuilder.Entity<Setting>(s =>
        {
            s.HasData(
                new Setting
                {
                    Id = 1,
                    Key = SettingRegistry.FilteringLessThanSongCount,
                    Comment = "Add a default filter to show only albums with this or less number of songs.",
                    Value = "3",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 2,
                    Key = SettingRegistry.FilteringLessThanDuration,
                    Comment = "Add a default filter to show only albums with this or less duration.",
                    Value = "720000",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 4,
                    Key = SettingRegistry.DefaultsPageSize,
                    Comment = "Default page size when view including pagination.",
                    Value = "100",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 6,
                    Key = SettingRegistry.UserInterfaceToastAutoCloseTime,
                    Comment = "Amount of time to display a Toast then auto-close (in milliseconds.)",
                    Value = "2000",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 300,
                    Category = (int)SettingCategory.Formatting,
                    Key = SettingRegistry.FormattingDateTimeDisplayFormatShort,
                    Comment = "Short Format to use when displaying full dates.",
                    Value = "yyyyMMdd HH\\:mm",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 301,
                    Category = (int)SettingCategory.Formatting,
                    Key = SettingRegistry.FormattingDateTimeDisplayActivityFormat,
                    Comment = "Format to use when displaying activity related dates (e.g., processing messages)",
                    Value = MelodeeConfiguration.FormattingDateTimeDisplayActivityFormatDefault,
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 9,
                    Key = SettingRegistry.ProcessingIgnoredArticles,
                    Comment = "List of ignored articles when scanning media (pipe delimited).",
                    Value = "THE|EL|LA|LOS|LAS|LE|LES|OS|AS|O|A",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 500,
                    Category = (int)SettingCategory.Magic,
                    Key = SettingRegistry.MagicEnabled,
                    Comment = "Is Magic processing enabled.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 501,
                    Category = (int)SettingCategory.Magic,
                    Key = SettingRegistry.MagicDoRenumberSongs,
                    Comment = "Renumber songs when doing magic processing.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 502,
                    Category = (int)SettingCategory.Magic,
                    Key = SettingRegistry.MagicDoRemoveFeaturingArtistFromSongArtist,
                    Comment = "Remove featured artists from song artist when doing magic.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 503,
                    Category = (int)SettingCategory.Magic,
                    Key = SettingRegistry.MagicDoRemoveFeaturingArtistFromSongTitle,
                    Comment = "Remove featured artists from song title when doing magic.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 504,
                    Category = (int)SettingCategory.Magic,
                    Key = SettingRegistry.MagicDoReplaceSongsArtistSeparators,
                    Comment = "Replace song artist separators with standard ID3 separator ('/') when doing magic.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 505,
                    Category = (int)SettingCategory.Magic,
                    Key = SettingRegistry.MagicDoSetYearToCurrentIfInvalid,
                    Comment = "Set the song year to current year if invalid or missing when doing magic.",
                    Value = "false",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 506,
                    Category = (int)SettingCategory.Magic,
                    Key = SettingRegistry.MagicDoRemoveUnwantedTextFromAlbumTitle,
                    Comment = "Remove unwanted text from album title when doing magic.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 507,
                    Category = (int)SettingCategory.Magic,
                    Key = SettingRegistry.MagicDoRemoveUnwantedTextFromSongTitles,
                    Comment = "Remove unwanted text from song titles when doing magic.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 200,
                    Category = (int)SettingCategory.Conversion,
                    Key = SettingRegistry.ConversionEnabled,
                    Comment = "Enable Melodee to convert non-mp3 media files during processing.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 201,
                    Category = (int)SettingCategory.Conversion,
                    Key = SettingRegistry.ConversionBitrate,
                    Comment = "Bitrate to convert non-mp3 media files during processing.",
                    Value = "384",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 202,
                    Category = (int)SettingCategory.Conversion,
                    Key = SettingRegistry.ConversionVbrLevel,
                    Comment = "Vbr to convert non-mp3 media files during processing.",
                    Value = "4",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 203,
                    Category = (int)SettingCategory.Conversion,
                    Key = SettingRegistry.ConversionSamplingRate,
                    Comment = "Sampling rate to convert non-mp3 media files during processing.",
                    Value = "48000",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 700,
                    Category = (int)SettingCategory.PluginProcess,
                    Key = SettingRegistry.PluginEnabledCueSheet,
                    Comment = "Process of CueSheet files during processing.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 701,
                    Category = (int)SettingCategory.PluginProcess,
                    Key = SettingRegistry.PluginEnabledM3u,
                    Comment = "Process of M3U files during processing.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 702,
                    Category = (int)SettingCategory.PluginProcess,
                    Key = SettingRegistry.PluginEnabledNfo,
                    Comment = "Process of NFO files during processing.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 703,
                    Category = (int)SettingCategory.PluginProcess,
                    Key = SettingRegistry.PluginEnabledSimpleFileVerification,
                    Comment = "Process of Simple File Verification (SFV) files during processing.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 704,
                    Category = (int)SettingCategory.PluginProcess,
                    Key = SettingRegistry.ProcessingDoDeleteComments,
                    Comment = "If true then all comments will be removed from media files.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 26,
                    Key = SettingRegistry.ProcessingArtistNameReplacements,
                    Comment = "Fragments of artist names to replace (JSON Dictionary).",
                    Value =
                        "{'AC/DC': ['AC; DC', 'AC;DC', 'AC/ DC', 'AC DC'] , 'Love/Hate': ['Love; Hate', 'Love;Hate', 'Love/ Hate', 'Love Hate'] }",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 27,
                    Key = SettingRegistry.ProcessingDoUseCurrentYearAsDefaultOrigAlbumYearValue,
                    Comment = "If OrigAlbumYear [TOR, TORY, TDOR] value is invalid use current year.",
                    Value = "false",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 28,
                    Key = SettingRegistry.ProcessingDoDeleteOriginal,
                    Comment =
                        "Delete original files when processing. When false a copy if made, else original is deleted after processed.",
                    Value = "false",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 29,
                    Key = SettingRegistry.ProcessingConvertedExtension,
                    Comment = "Extension to add to file when converted, leave blank to disable.",
                    Value = "_converted",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 30,
                    Key = SettingRegistry.ProcessingProcessedExtension,
                    Comment = "Extension to add to file when processed, leave blank to disable.",
                    Value = "_processed",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 32,
                    Key = SettingRegistry.ProcessingDoOverrideExistingMelodeeDataFiles,
                    Comment =
                        "When processing over write any existing Melodee data files, otherwise skip and leave in place.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 34,
                    Key = SettingRegistry.ProcessingMaximumProcessingCount,
                    Comment = "The maximum number of files to process, set to zero for unlimited.",
                    Value = "0",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 35,
                    Key = SettingRegistry.ProcessingMaximumAlbumDirectoryNameLength,
                    Comment = "Maximum allowed length of album directory name.",
                    Value = "255",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 36,
                    Key = SettingRegistry.ProcessingMaximumArtistDirectoryNameLength,
                    Comment = "Maximum allowed length of artist directory name.",
                    Value = "255",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 37,
                    Key = SettingRegistry.ProcessingAlbumTitleRemovals,
                    Comment = "Fragments to remove from album titles (JSON array).",
                    Value = "['^', '~', '#']",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 38,
                    Key = SettingRegistry.ProcessingSongTitleRemovals,
                    Comment = "Fragments to remove from song titles (JSON array).",
                    Value = "[';', '(Remaster)', 'Remaster']",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 39,
                    Key = SettingRegistry.ProcessingDoContinueOnDirectoryProcessingErrors,
                    Comment = "Continue processing if an error is encountered.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 41,
                    Key = SettingRegistry.ScriptingEnabled,
                    Comment = "Is scripting enabled.",
                    Value = "false",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 42,
                    Key = SettingRegistry.ScriptingPreDiscoveryScript,
                    Comment = "Script to run before processing the inbound directory, leave blank to disable.",
                    Value = "",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 43,
                    Key = SettingRegistry.ScriptingPostDiscoveryScript,
                    Comment = "Script to run after processing the inbound directory, leave blank to disable.",
                    Value = "",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 45,
                    Key = SettingRegistry.ProcessingIgnoredPerformers,
                    Comment = "Don't create performer contributors for these performer names.",
                    Value = "",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 46,
                    Key = SettingRegistry.ProcessingIgnoredProduction,
                    Comment = "Don't create production contributors for these production names.",
                    Value = "['www.t.me;pmedia_music']",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 47,
                    Key = SettingRegistry.ProcessingIgnoredPublishers,
                    Comment = "Don't create publisher contributors for these artist names.",
                    Value = "['P.M.E.D.I.A','PMEDIA','PMEDIA GROUP']",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 49,
                    Key = SettingRegistry.EncryptionPrivateKey,
                    Comment =
                        "Private key used to encrypt/decrypt passwords for Subsonic authentication. Use https://generate-random.org/encryption-key-generator?count=1&bytes=32&cipher=aes-256-cbc&string=&password= to generate a new key.",
                    Value = "H+Kiik6VMKfTD2MesF1GoMjczTrD5RhuKckJ5+/UQWOdWajGcsEC3yEnlJ5eoy8Y",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 50,
                    Key = SettingRegistry.ProcessingDuplicateAlbumPrefix,
                    Comment =
                        "Prefix to apply to indicate an album directory is a duplicate album for an artist. If left blank the default of '__duplicate_' will be used.",
                    Value = "_duplicate_ ",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1300,
                    Category = (int)SettingCategory.Validation,
                    Key = SettingRegistry.ValidationMaximumSongNumber,
                    Comment = "The maximum value a song number can have for an album.",
                    Value = "9999",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1301,
                    Category = (int)SettingCategory.Validation,
                    Key = SettingRegistry.ValidationMinimumAlbumYear,
                    Comment = "Minimum allowed year for an album.",
                    Value = "1860",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1302,
                    Category = (int)SettingCategory.Validation,
                    Key = SettingRegistry.ValidationMaximumAlbumYear,
                    Comment = "Maximum allowed year for an album.",
                    Value = "2150",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1303,
                    Category = (int)SettingCategory.Validation,
                    Key = SettingRegistry.ValidationMinimumSongCount,
                    Comment =
                        "Minimum number of songs an album has to have to be considered valid, set to 0 to disable check.",
                    Value = "3",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1304,
                    Category = (int)SettingCategory.Validation,
                    Key = SettingRegistry.ValidationMinimumAlbumDuration,
                    Comment =
                        "Minimum duration of an album to be considered valid (in minutes), set to 0 to disable check.",
                    Value = "10",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 100,
                    Category = (int)SettingCategory.Api,
                    Key = SettingRegistry.OpenSubsonicServerSupportedVersion,
                    Comment = "OpenSubsonic server supported Subsonic API version.",
                    Value = "1.16.1",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 101,
                    Category = (int)SettingCategory.Api,
                    Key = SettingRegistry.OpenSubsonicServerType,
                    Comment = "OpenSubsonic server name.",
                    Value = "Melodee",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 102,
                    Category = (int)SettingCategory.Api,
                    Key = SettingRegistry.OpenSubsonicServerVersion,
                    Comment = "OpenSubsonic server actual version. [Ex: 1.2.3 (beta)]",
                    Value = "1.0.1 (beta)",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 103,
                    Category = (int)SettingCategory.Api,
                    Key = SettingRegistry.OpenSubsonicServerLicenseEmail,
                    Comment = "OpenSubsonic email to use in License responses.",
                    Value = "noreply@localhost.lan",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 104,
                    Category = (int)SettingCategory.Api,
                    Key = SettingRegistry.OpenSubsonicIndexesArtistLimit,
                    Comment =
                        "Limit the number of artists to include in an indexes request, set to zero for 32k per index (really not recommended with tens of thousands of artists and mobile clients timeout downloading indexes, a user can find an artist by search)",
                    Value = "1000",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 53,
                    Key = SettingRegistry.DefaultsBatchSize,
                    Comment =
                        $"Processing batching size. Allowed range is between [{MelodeeConfiguration.BatchSizeDefault}] and [{MelodeeConfiguration.BatchSizeMaximum}]. ",
                    Value = "250",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 54,
                    Key = SettingRegistry.ProcessingFileExtensionsToDelete,
                    Comment =
                        "When processing folders immediately delete any files with these extensions. (JSON array).",
                    Value = "['log', 'lnk', 'lrc', 'doc']",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 902,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineUserAgent,
                    Comment = "User agent to send with Search engine requests.",
                    Value = "Mozilla/5.0 (X11; Linux x86_64; rv:131.0) Gecko/20100101 Firefox/131.0",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 903,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineDefaultPageSize,
                    Comment = "Default page size when performing a search engine search.",
                    Value = "20",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 904,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineMusicBrainzEnabled,
                    Comment = "Is MusicBrainz search engine enabled.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 905,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineMusicBrainzStoragePath,
                    Comment = "Storage path to hold MusicBrainz downloaded files and SQLite db.",
                    Value = "/melodee_test/search-engine-storage/musicbrainz/",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 906,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineMusicBrainzImportMaximumToProcess,
                    Comment =
                        "Maximum number of batches import from MusicBrainz downloaded db dump (this setting is usually used during debugging), set to zero for unlimited.",
                    Value = "0",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 907,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineMusicBrainzImportBatchSize,
                    Comment =
                        "Number of records to import from MusicBrainz downloaded db dump before commiting to local SQLite database.",
                    Value = "50000",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 908,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineMusicBrainzImportLastImportTimestamp,
                    Comment = "Timestamp of when last MusicBrainz import was successful.",
                    Value = "",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 910,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineSpotifyEnabled,
                    Comment = "Is Spotify search engine enabled.",
                    Value = "false",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 911,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineSpotifyApiKey,
                    Comment = "ApiKey used used with Spotify. See https://developer.spotify.com/ for more details.",
                    Value = "",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 912,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineSpotifyClientSecret,
                    Comment = "Shared secret used with Spotify. See https://developer.spotify.com/ for more details.",
                    Value = "",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 913,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineSpotifyAccessToken,
                    Comment =
                        "Token obtained from Spotify using the ApiKey and the Secret, this json contains expiry information.",
                    Value = "",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 914,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineITunesEnabled,
                    Comment = "Is ITunes search engine enabled.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 915,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineLastFmEnabled,
                    Comment = "Is LastFM search engine enabled.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 916,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineMaximumAllowedPageSize,
                    Comment = "When performing a search engine search, the maximum allowed page size.",
                    Value = "1000",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 917,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineArtistSearchDatabaseRefreshInDays,
                    Comment =
                        "Refresh albums for artists from search engine database every x days, set to zero to not refresh.",
                    Value = "14",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 918,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineDeezerEnabled,
                    Comment = "Is Deezer search engine enabled.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 400,
                    Category = (int)SettingCategory.Imaging,
                    Key = SettingRegistry.ImagingDoLoadEmbeddedImages,
                    Comment = "Include any embedded images from media files into the Melodee data file.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 401,
                    Category = (int)SettingCategory.Imaging,
                    Key = SettingRegistry.ImagingSmallSize,
                    Comment = "Small image size (square image, this is both width and height).",
                    Value = "300",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 402,
                    Category = (int)SettingCategory.Imaging,
                    Key = SettingRegistry.ImagingMediumSize,
                    Comment = "Medium image size (square image, this is both width and height).",
                    Value = "600",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 403,
                    Category = (int)SettingCategory.Imaging,
                    Key = SettingRegistry.ImagingLargeSize,
                    Comment =
                        "Large image size (square image, this is both width and height), if larger than will be resized to this image, leave blank to disable.",
                    Value = "1600",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 404,
                    Category = (int)SettingCategory.Imaging,
                    Key = SettingRegistry.ImagingMaximumNumberOfAlbumImages,
                    Comment =
                        "Maximum allowed number of images for an album, this includes all image types (Front, Rear, etc.), set to zero for unlimited.",
                    Value = "25",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 405,
                    Category = (int)SettingCategory.Imaging,
                    Key = SettingRegistry.ImagingMaximumNumberOfArtistImages,
                    Comment = "Maximum allowed number of images for an artist, set to zero for unlimited.",
                    Value = "25",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 406,
                    Category = (int)SettingCategory.Imaging,
                    Key = SettingRegistry.ImagingMinimumImageSize,
                    Comment = "Images under this size are considered invalid, set to zero to disable.",
                    Value = "300",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1200,
                    Category = (int)SettingCategory.Transcoding,
                    Key = SettingRegistry.TranscodingDefault,
                    Comment = "Default format for transcoding.",
                    Value = "raw",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1201,
                    Category = (int)SettingCategory.Transcoding,
                    Key = SettingRegistry.TranscodingCommandMp3,
                    Comment = "Default command to transcode MP3 for streaming.",
                    Value =
                        $"{{ 'format': '{TranscodingFormat.Mp3.ToString()}', 'bitrate: 192, 'command': 'ffmpeg -i %s -ss %t -map 0:a:0 -b:a %bk -v 0 -f mp3 -' }}",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1202,
                    Category = (int)SettingCategory.Transcoding,
                    Key = SettingRegistry.TranscodingCommandOpus,
                    Comment = "Default command to transcode using libopus for streaming.",
                    Value =
                        $"{{ 'format': '{TranscodingFormat.Opus.ToString()}', 'bitrate: 128, 'command': 'ffmpeg -i %s -ss %t -map 0:a:0 -b:a %bk -v 0 -c:a libopus -f opus -' }}",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1203,
                    Category = (int)SettingCategory.Transcoding,
                    Key = SettingRegistry.TranscodingCommandAac,
                    Comment = "Default command to transcode to aac for streaming.",
                    Value =
                        $"{{ 'format': '{TranscodingFormat.Aac.ToString()}', 'bitrate: 256, 'command': 'ffmpeg -i %s -ss %t -map 0:a:0 -b:a %bk -v 0 -c:a aac -f adts -' }}",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1000,
                    Category = (int)SettingCategory.Scrobbling,
                    Key = SettingRegistry.ScrobblingEnabled,
                    Comment = "Is scrobbling enabled.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1001,
                    Category = (int)SettingCategory.Scrobbling,
                    Key = SettingRegistry.ScrobblingLastFmEnabled,
                    Comment = "Is scrobbling to Last.fm enabled.",
                    Value = "false",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1002,
                    Category = (int)SettingCategory.Scrobbling,
                    Key = SettingRegistry.ScrobblingLastFmApiKey,
                    Comment =
                        "ApiKey used used with last FM. See https://www.last.fm/api/authentication for more details.",
                    Value = "",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1003,
                    Category = (int)SettingCategory.Scrobbling,
                    Key = SettingRegistry.ScrobblingLastFmSharedSecret,
                    Comment =
                        "Shared secret used with last FM. See https://www.last.fm/api/authentication for more details.",
                    Value = "",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1100,
                    Category = (int)SettingCategory.System,
                    Key = SettingRegistry.SystemBaseUrl,
                    Comment =
                        "Base URL for Melodee to use when building shareable links and image urls (e.g., 'https://server.domain.com:8080', 'http://server.domain.com').",
                    Value = MelodeeConfiguration.RequiredNotSetValue,
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1101,
                    Category = (int)SettingCategory.System,
                    Key = SettingRegistry.SystemIsDownloadingEnabled,
                    Comment = "Is downloading enabled.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1400,
                    Category = (int)SettingCategory.Jobs,
                    Key = SettingRegistry.JobsArtistHousekeepingCronExpression,
                    Comment =
                        "Cron expression to run the artist housekeeping job, set empty to disable. Default of '0 0 0/1 1/1 * ? *' will run every hour. See https://www.freeformatter.com/cron-expression-generator-quartz.html",
                    Value = "0 0 0/1 1/1 * ? *",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1401,
                    Category = (int)SettingCategory.Jobs,
                    Key = SettingRegistry.JobsLibraryProcessCronExpression,
                    Comment =
                        "Cron expression to run the library process job, set empty to disable. Default of '0 */10 * ? * *' Every 10 minutes. See https://www.freeformatter.com/cron-expression-generator-quartz.html",
                    Value = "0 */10 * ? * *",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1402,
                    Category = (int)SettingCategory.Jobs,
                    Key = SettingRegistry.JobsLibraryInsertCronExpression,
                    Comment =
                        "Cron expression to run the library scan job, set empty to disable. Default of '0 0 0 * * ?' will run every day at 00:00. See https://www.freeformatter.com/cron-expression-generator-quartz.html",
                    Value = "0 0 0 * * ?",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1403,
                    Category = (int)SettingCategory.Jobs,
                    Key = SettingRegistry.JobsMusicBrainzUpdateDatabaseCronExpression,
                    Comment =
                        "Cron expression to run the musicbrainz database house keeping job, set empty to disable. Default of '0 0 12 1 * ?' will run first day of the month. See https://www.freeformatter.com/cron-expression-generator-quartz.html",
                    Value = "0 0 12 1 * ?",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 1404,
                    Category = (int)SettingCategory.Jobs,
                    Key = SettingRegistry.JobsArtistSearchEngineHousekeepingCronExpression,
                    Comment =
                        "Cron expression to run the artist search engine house keeping job, set empty to disable. Default of '0 0 0 * * ?' will run every day at 00:00. See https://www.freeformatter.com/cron-expression-generator-quartz.html",
                    Value = "0 0 0 * * ?",
                    CreatedAt = now
                }
            );
        });

        // sph; left here for example of GIN FTS. More info here https://www.npgsql.org/efcore/mapping/full-text-search.html?tabs=pg12%2Cv5
        // modelBuilder.Entity<Song>()
        //     .HasIndex(s => new
        //     {
        //         SongTitle= s.Title, 
        //         AlbumTitle= s.AlbumDisc.Title, 
        //         ArtistName = s.AlbumDisc.Album.Name
        //     })
        //     .HasMethod("GIN")
        //     .IsTsVectorExpressionIndex("english");          

        // modelBuilder.Entity<User>()
        //     .HasGeneratedTsVectorColumn(u => u.SearchVector, "english", u => new { u.Email, u.UserName })
        //     .HasIndex(u => u.Email)
        //     .HasMethod("GIN");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        optionsBuilder.EnableSensitiveDataLogging();
    }
}
