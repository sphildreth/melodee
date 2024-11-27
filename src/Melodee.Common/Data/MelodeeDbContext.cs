using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data.Models;
using Melodee.Common.Models.OpenSubsonic.Enums;
using Melodee.Common.Enums;
using Microsoft.CodeAnalysis.Host;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Melodee.Common.Data;

public class MelodeeDbContext(DbContextOptions<MelodeeDbContext> options) : DbContext(options)
{
    public DbSet<Album> Albums { get; set; }

    public DbSet<AlbumDisc> AlbumDiscs { get; set; }

    public DbSet<Artist> Artists { get; set; }

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

    public DbSet<Share> Shares { get; set; }

    public DbSet<Song> Songs { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<UserAlbum> UserAlbums { get; set; }

    public DbSet<UserArtist> UserArtists { get; set; }

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
                    Description = "Files in this directory are scanned and Album information is gathered via processing.",
                    Path = "/storage/inbound/",
                    Type = (int)LibraryType.Inbound,
                    CreatedAt = now
                },
                new Library
                {
                    Id = 2,
                    Name = "Staging",
                    Description = "The staging directory to place processed files into (Inbound -> Staging -> Library).",
                    Path = "/storage/staging/",
                    Type = (int)LibraryType.Staging,
                    CreatedAt = now
                },
                new Library
                {
                    Id = 3,
                    Name = "Library",
                    Description = "The library directory to place processed, reviewed and ready to use music files into.",
                    Path = "/storage/library/",
                    Type = (int)LibraryType.Library,
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
                    Id = 3,
                    Key = SettingRegistry.ProcessingStagingDirectoryScanLimit,
                    Comment = "Maximum number of albums to scan when processing inbound directory.",
                    Value = "250",
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
                    Id = 7,
                    Category = (int)SettingCategory.Formatting,
                    Key = SettingRegistry.FormattingDateTimeDisplayFormatShort,
                    Comment = "Short Format to use when displaying full dates.",
                    Value = "yyyyMMdd HH\\:mm",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 8,
                    Category = (int)SettingCategory.Formatting,
                    Key = SettingRegistry.FormattingDateTimeDisplayActivityFormat,
                    Comment = "Format to use when displaying activity related dates (e.g. processing messages)",
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
                    Id = 10,
                    Category = (int)SettingCategory.Magic,
                    Key = SettingRegistry.MagicEnabled,
                    Comment = "Is Magic processing enabled.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 11,
                    Category = (int)SettingCategory.Magic,
                    Key = SettingRegistry.MagicDoRenumberSongs,
                    Comment = "Renumber songs when doing magic processing.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 12,
                    Category = (int)SettingCategory.Magic,
                    Key = SettingRegistry.MagicDoRemoveFeaturingArtistFromSongArtist,
                    Comment = "Remove featured artists from song artist when doing magic.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 13,
                    Category = (int)SettingCategory.Magic,
                    Key = SettingRegistry.MagicDoRemoveFeaturingArtistFromSongTitle,
                    Comment = "Remove featured artists from song title when doing magic.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 14,
                    Category = (int)SettingCategory.Magic,
                    Key = SettingRegistry.MagicDoReplaceSongsArtistSeparators,
                    Comment = "Replace song artist separators with standard ID3 separator ('/') when doing magic.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 15,
                    Category = (int)SettingCategory.Magic,
                    Key = SettingRegistry.MagicDoSetYearToCurrentIfInvalid,
                    Comment = "Set the song year to current year if invalid or missing when doing magic.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 16,
                    Category = (int)SettingCategory.Magic,
                    Key = SettingRegistry.MagicDoRemoveUnwantedTextFromAlbumTitle,
                    Comment = "Remove unwanted text from album title when doing magic.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 17,
                    Category = (int)SettingCategory.Magic,
                    Key = SettingRegistry.MagicDoRemoveUnwantedTextFromSongTitles,
                    Comment = "Remove unwanted text from song titles when doing magic.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 18,
                    Category = (int)SettingCategory.Conversion,
                    Key = SettingRegistry.ConversionEnabled,
                    Comment = "Enable Melodee to convert non-mp3 media files during processing.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 19,
                    Category = (int)SettingCategory.Conversion,
                    Key = SettingRegistry.ConversionBitrate,
                    Comment = "Bitrate to convert non-mp3 media files during processing.",
                    Value = "384",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 20,
                    Category = (int)SettingCategory.Conversion,
                    Key = SettingRegistry.ConversionVbrLevel,
                    Comment = "Vbr to convert non-mp3 media files during processing.",
                    Value = "4",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 21,
                    Category = (int)SettingCategory.Conversion,
                    Key = SettingRegistry.ConversionSamplingRate,
                    Comment = "Sampling rate to convert non-mp3 media files during processing.",
                    Value = "48000",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 22,
                    Category = (int)SettingCategory.PluginProcess,
                    Key = SettingRegistry.PluginEnabledCueSheet,
                    Comment = "Process of CueSheet files during processing.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 23,
                    Category = (int)SettingCategory.PluginProcess,
                    Key = SettingRegistry.PluginEnabledM3u,
                    Comment = "Process of M3U files during processing.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 24,
                    Category = (int)SettingCategory.PluginProcess,
                    Key = SettingRegistry.PluginEnabledNfo,
                    Comment = "Process of NFO files during processing.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 25,
                    Category = (int)SettingCategory.PluginProcess,
                    Key = SettingRegistry.PluginEnabledSimpleFileVerification,
                    Comment = "Process of Simple File Verification (SFV) files during processing.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 26,
                    Key = SettingRegistry.ProcessingArtistNameReplacements,
                    Comment = "Fragments of artist names to replace (JSON Dictionary).",
                    Value = "{'AC/DC': ['AC; DC', 'AC;DC', 'AC/ DC', 'AC DC'] , 'Love/Hate': ['Love; Hate', 'Love;Hate', 'Love/ Hate', 'Love Hate'] }",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 27,
                    Key = SettingRegistry.ProcessingDoUseCurrentYearAsDefaultOrigAlbumYearValue,
                    Comment = "If OrigAlbumYear [TOR, TORY, TDOR] value is invalid use current year.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 28,
                    Key = SettingRegistry.ProcessingDoDeleteOriginal,
                    Comment = "Delete original files when processing. When false a copy if made, else original is deleted after processed.",
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
                    Id = 31,
                    Key = SettingRegistry.ProcessingSkippedExtension,
                    Comment = "Extension to add to file to indicate other files in the same category where processed and this file was skipped during processing, leave blank to disable.",
                    Value = "_skipped",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 32,
                    Key = SettingRegistry.ProcessingDoOverrideExistingMelodeeDataFiles,
                    Comment = "When processing over write any existing Melodee data files, otherwise skip and leave in place.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 33,
                    Category = (int)SettingCategory.Imaging,
                    Key = SettingRegistry.ImagingDoLoadEmbeddedImages,
                    Comment = "Include any embedded images from media files into the Melodee data file.",
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
                    Id = 44,
                    Category = (int)SettingCategory.Validation,
                    Key = SettingRegistry.ValidationMaximumMediaNumber,
                    Comment = "The maximum value a media number can have for an album. The length of this is used for formatting song names.",
                    Value = "999",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 45,
                    Category = (int)SettingCategory.Validation,
                    Key = SettingRegistry.ValidationMaximumSongNumber,
                    Comment = "The maximum value a song number can have for an album. The length of this is used for formatting song names.",
                    Value = "9999",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 46,
                    Category = (int)SettingCategory.Validation,
                    Key = SettingRegistry.ValidationMinimumAlbumYear,
                    Comment = "Minimum allowed year for an album.",
                    Value = "1860",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 47,
                    Category = (int)SettingCategory.Validation,
                    Key = SettingRegistry.ValidationMaximumAlbumYear,
                    Comment = "Maximum allowed year for an album.",
                    Value = "2150",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 48,
                    Key = SettingRegistry.EncryptionPrivateKey,
                    Comment = "Private key used to encrypt/decrypt passwords for Subsonic authentication. Use https://generate-random.org/encryption-key-generator?count=1&bytes=32&cipher=aes-256-cbc&string=&password= to generate a new key.",
                    Value = "H+Kiik6VMKfTD2MesF1GoMjczTrD5RhuKckJ5+/UQWOdWajGcsEC3yEnlJ5eoy8Y",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 49,
                    Category = (int)SettingCategory.Api,
                    Key = SettingRegistry.OpenSubsonicServerSupportedVersion,
                    Comment = "OpenSubsonic server supported Subsonic API version.",
                    Value = "1.16.1",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 50,
                    Category = (int)SettingCategory.Api,
                    Key = SettingRegistry.OpenSubsonicServerType,
                    Comment = "OpenSubsonic server name.",
                    Value = "Melodee",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 51,
                    Category = (int)SettingCategory.Api,
                    Key = SettingRegistry.OpenSubsonicServerVersion,
                    Comment = "OpenSubsonic server actual version. [Ex: 1.2.3 (beta)]",
                    Value = "1.0.1 (beta)",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 52,
                    Category = (int)SettingCategory.Api,
                    Key = SettingRegistry.OpenSubsonicServerLicenseEmail,
                    Comment = "OpenSubsonic email to use in License responses.",
                    Value = "noreply@localhost.lan",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 53,
                    Key = SettingRegistry.DefaultsBatchSize,
                    Comment = $"Processing batching size. Allowed range is between [{MelodeeConfiguration.BatchSizeDefault}] and [{MelodeeConfiguration.BatchSizeMaximum}]. ",
                    Value = "250",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 60,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineBingImageEnabled,
                    Comment = "Use Bing search engine to find images for albums and artists.",
                    Value = "false",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 61,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineBingImageApiKey,
                    Comment = "Bing search ApiKey (Ocp-Apim-Subscription-Key), leave blank to disable.",
                    Value = "",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 62,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineUserAgent,
                    Comment = "User agent to send with Search engine requests.",
                    Value = "Mozilla/5.0 (X11; Linux x86_64; rv:131.0) Gecko/20100101 Firefox/131.0",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 63,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineDefaultPageSize,
                    Comment = "Default page size when performing a search engine search.",
                    Value = "20",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 70,
                    Category = (int)SettingCategory.Imaging,
                    Key = SettingRegistry.ImagingMaximumImageSize,
                    Comment = "Maximum image size allowed (WidthxHeight) for any image, if larger than will be resized to this image, leave blank to disable.",
                    Value = "1600x1600",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 71,
                    Category = (int)SettingCategory.Imaging,
                    Key = SettingRegistry.ImagingMaximumNumberOfAlbumImages,
                    Comment = "Maximum allowed number of images for an album, this includes all image types (Front, Rear, etc.), set to zero for unlimited.",
                    Value = "25",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 72,
                    Category = (int)SettingCategory.Imaging,
                    Key = SettingRegistry.ImagingMaximumNumberOfArtistImages,
                    Comment = "Maximum allowed number of images for an artist, set to zero for unlimited.",
                    Value = "25",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 73,
                    Category = (int)SettingCategory.PluginProcess,
                    Key = SettingRegistry.ProcessingDoDeleteComments,
                    Comment = "If true then all comments will be removed from media files.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 74,
                    Category = (int)SettingCategory.Transcoding,
                    Key = SettingRegistry.TranscodingDefault,
                    Comment = "Default format for transcoding.",
                    Value = "raw",
                    CreatedAt = now
                },                
                new Setting
                {
                    Id = 75,
                    Category = (int)SettingCategory.Transcoding,
                    Key = SettingRegistry.TranscodingCommandMp3,
                    Comment = "Default command to transcode MP3 for streaming.",
                    Value = $"{{ 'format': '{ TranscodingFormat.Mp3.ToString()}', 'bitrate: 192, 'command': 'ffmpeg -i %s -ss %t -map 0:a:0 -b:a %bk -v 0 -f mp3 -' }}",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 76,
                    Category = (int)SettingCategory.Transcoding,
                    Key = SettingRegistry.TranscodingCommandOpus,
                    Comment = "Default command to transcode using libopus for streaming.",
                    Value = $"{{ 'format': '{ TranscodingFormat.Opus.ToString()}', 'bitrate: 128, 'command': 'ffmpeg -i %s -ss %t -map 0:a:0 -b:a %bk -v 0 -c:a libopus -f opus -' }}",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 77,
                    Category = (int)SettingCategory.Transcoding,
                    Key = SettingRegistry.TranscodingCommandAac,
                    Comment = "Default command to transcode to aac for streaming.",
                    Value = $"{{ 'format': '{ TranscodingFormat.Aac.ToString()}', 'bitrate: 256, 'command': 'ffmpeg -i %s -ss %t -map 0:a:0 -b:a %bk -v 0 -c:a aac -f adts -' }}",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 78,
                    Category = (int)SettingCategory.Scrobbling,
                    Key = SettingRegistry.ScrobblingEnabled,
                    Comment = "Is scrobbling enabled.",
                    Value = "false",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 79,
                    Category = (int)SettingCategory.Scrobbling,
                    Key = SettingRegistry.ScrobblingLastFmEnabled,
                    Comment = "Is scrobbling to Last.fm enabled.",
                    Value = "false",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 80,
                    Category = (int)SettingCategory.Scrobbling,
                    Key = SettingRegistry.ScrobblingLastFmApiKey,
                    Comment = "ApiKey used to scrobble to last FM. See https://www.last.fm/api/authentication for more details.",
                    Value = "",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 81,
                    Category = (int)SettingCategory.Api,
                    Key = SettingRegistry.OpenSubsonicIndexesArtistLimit,
                    Comment = "Limit the number of artists to include in an indexes request, set to zero for 32k per index (really not recommended with tens of thousands of artists and mobile clients timeout downloading indexes, a user can find an artist by search)",
                    Value = "1000",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 82,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineMusicBrainzEnabled,
                    Comment = "Is MusicBrainz search engine enabled.",
                    Value = "true",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 83,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineMusicBrainzStoragePath,
                    Comment = "Storage path to hold MusicBrainz downloaded files and SQLite db.",
                    Value = "/melodee_test/search-engine-storage/musicbrainz/",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 84,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineMusicBrainzImportMaximumToProcess,
                    Comment = "Maximum number of batches import from MusicBrainz downloaded db dump (this setting is usually used during debugging), set to zero for unlimited.",
                    Value = "0",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 85,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineMusicBrainzImportBatchSize,
                    Comment = "Number of records to import from MusicBrainz downloaded db dump before commiting to local SQLite database.",
                    Value = "10000",
                    CreatedAt = now
                },
                new Setting
                {
                    Id = 86,
                    Category = (int)SettingCategory.SearchEngine,
                    Key = SettingRegistry.SearchEngineMusicBrainzImportLastImportTimestamp,
                    Comment = "Timestamp of when last MusicBrainz import was successful.",
                    Value = "",
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
        optionsBuilder.EnableSensitiveDataLogging();
    }
    
}
