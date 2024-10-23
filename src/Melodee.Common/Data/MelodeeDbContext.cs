using Melodee.Common.Constants;
using Melodee.Common.Data.Contants;
using Melodee.Common.Data.Models;
using Melodee.Common.Enums;
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
    
    public DbSet<Scrobble> Scrobbles { get; set; }
    
    public DbSet<Share> Shares { get; set; }
    
    public DbSet<Song> Songs { get; set; }
    
    public DbSet<User> Users { get; set; }
    
    public DbSet<UserAlbum> UserAlbums { get; set; }
    
    public DbSet<UserArtist> UserArtists { get; set; }
    
    public DbSet<UserSong> UserSongs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        modelBuilder.Entity<Setting>(s =>
        {
            s.HasData([
                new Setting
                {
                    Key = SettingRegistry.DirectoryInbound,
                    Comment = "Files in this directory are scanned and Album information is gathered.",
                    Value = "/storage/inbound",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.DirectoryLibrary,
                    Comment = "This is the main storage library (holds all previously scanned/edited/approved Albums. This is directory is where the API gets media file data from.",
                    Value = "/storage/library",
                    CreatedAt = now,
                },                
                new Setting
                {
                    Key = SettingRegistry.DirectoryStaging,
                    Comment = "When one is happy with scan results one can select Albums (and all associated files) to move to this directory in the proper directory structure.",
                    Value = "/storage/staging",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.FilteringLessThanSongCount,
                    Comment = "Add a default filter to show only albums with this or less number of songs.",
                    Value = "3",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.FilteringLessThanConfiguredDuration,
                    Comment = "Add a default filter to show only albums with this or less duration.",
                    Value = "720000",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ProcessingStagingDirectoryScanLimit,
                    Comment = "Maximum number of albums to scan when processing inbound directory.",
                    Value = "250",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.DefaultsPageSize,
                    Comment = "Default page size when view including pagination.",
                    Value = "100",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ProcessingMoveMelodeeJsonDataFileToLibrary,
                    Comment = "When true then move the Melodee.json data file when moving Albums, otherwise delete.",
                    Value = "false",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.UserInterfaceToastAutoCloseTime,
                    Comment = "Amount of time to display a Toast then auto-close (in milliseconds.)",
                    Value = "2000",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.FormattingDateTimeDisplayFormatShort,
                    Comment = "Short Format to use when displaying full dates.",
                    Value = "yyyyMMdd HH:mm",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.FormattingDateTimeDisplayActivityFormat,
                    Comment = "Format to use when displaying activity related dates (e.g. processing messages)",
                    Value = "HH:mm:ss.fff",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ProcessingIgnoredArticles,
                    Comment = "List of ignored articles when scanning media (pipe delimited).",
                    Value = "THE|EL|LA|LOS|LAS|LE|LES|OS|AS|O|A",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.MagicEnabled,
                    Comment = "Is Magic processing enabled.",
                    Value = "true",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.MagicDoRenumberSongs,
                    Comment = "Renumber songs when doing magic processing.",
                    Value = "true",
                    CreatedAt = now,
                }, 
                new Setting
                {
                    Key = SettingRegistry.MagicDoRemoveFeaturingArtistFromSongsArtist,
                    Comment = "Remove featured artists from song artists when doing magic.",
                    Value = "true",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.MagicDoRemoveFeaturingArtistFromSongTitle,
                    Comment = "Remove featured artists from song title when doing magic.",
                    Value = "true",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.MagicDoReplaceSongsArtistSeparators,
                    Comment = "Replace song artist separators with standard ID3 separator ('/') when doing magic.",
                    Value = "true",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.MagicDoSetYearToCurrentIfInvalid,
                    Comment = "Set the song year to current year if invalid or missing when doing magic.",
                    Value = "true",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.MagicDoRemoveUnwantedTextFromAlbumTitle,
                    Comment = "Remove unwanted text from album title when doing magic.",
                    Value = "true",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ConversionEnabled,
                    Comment = "Enable Melodee to convert non-mp3 media files during processing.",
                    Value = "true",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ConversionBitrate,
                    Comment = "Bitrate to convert non-mp3 media files during processing.",
                    Value = "384",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ConversionVbrLevel,
                    Comment = "Vbr to convert non-mp3 media files during processing.",
                    Value = "4",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ConversionSamplingRate,
                    Comment = "Sampling rate to convert non-mp3 media files during processing.",
                    Value = "48000",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ProcessingArtistNameReplacements,
                    Comment = "Fragments of artist names to replace.",
                    Value = "[{'AC/DC', ['AC; DC', 'AC;DC', 'AC/ DC', 'AC DC'] }, {'Love/Hate', ['Love; Hate', 'Love;Hate', 'Love/ Hate', 'Love Hate'] }]",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ProcessingDoUseCurrentYearAsDefaultOrigAlbumYearValue,
                    Comment = "If OrigAlbumYear [TOR, TORY, TDOR] value is invalid use current year.",
                    Value = "true",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ProcessingDoDeleteOriginal,
                    Comment = "Delete original files when processing. When false a copy if made, else original is deleted after processed.",
                    Value = "true",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ProcessingConvertedExtension,
                    Comment = "Extension to add to file when converted, leave blank to disable.",
                    Value = "_converted",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ProcessingProcessedExtension,
                    Comment = "Extension to add to file when processed, leave blank to disable.",
                    Value = "_processed",
                    CreatedAt = now,
                }, 
                new Setting
                {
                    Key = SettingRegistry.ProcessingSkippedExtension,
                    Comment = "Extension to add to file to indicate other files in the same category where processed and this file was skipped during processing, leave blank to disable.",
                    Value = "_skipped",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ProcessingDoOverrideExistingMelodeeDataFiles,
                    Comment = "When processing over write any existing Melodee data files, otherwise skip and leave in place.",
                    Value = "true",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ProcessingDoLoadEmbeddedImages,
                    Comment = "Include any embedded images from media files into the Melodee data file.",
                    Value = "true",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ProcessingMaximumProcessingCount,
                    Comment = "The maximum number of files to process, set to zero for infinite.",
                    Value = "0",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ProcessingMaximumAlbumDirectoryNameLength,
                    Comment = "Maximum allowed length of album directory name.",
                    Value = "255",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ProcessingMaximumAlbumDirectoryNameLength,
                    Comment = "Maximum allowed length of album directory name.",
                    Value = "255",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ProcessingAlbumTitleRemovals,
                    Comment = "Fragments to remove from album titles.",
                    Value = "['^', '~', '#']",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ProcessingSongTitleRemovals,
                    Comment = "Fragments to remove from song titles.",
                    Value = "[';', '(Remaster)', 'Remaster']",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ProcessingDoContinueOnDirectoryProcessingErrors,
                    Comment = "Continue processing if an error is encountered.",
                    Value = "true",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ProcessingDoMoveMelodeeDataFileToStagingDirectory,
                    Comment = "When true then move Album Melodee json files to the Staging directory.",
                    Value = "true",
                    CreatedAt = now,
                },              
                new Setting
                {
                    Key = SettingRegistry.ScriptingEnabled,
                    Comment = "Is scripting enabled.",
                    Value = "false",
                    CreatedAt = now,
                },                
                new Setting
                {
                    Key = SettingRegistry.ScriptingPostDiscoveryScript,
                    Comment = "Script to run before processing the inbound directory, leave blank to disable.",
                    Value = "",
                    CreatedAt = now,
                },                
                new Setting
                {
                    Key = SettingRegistry.ScriptingPostDiscoveryScript,
                    Comment = "Script to run after processing the inbound directory, leave blank to disable.",
                    Value = "",
                    CreatedAt = now,
                },
                
                new Setting
                {
                    Key = SettingRegistry.ValidationMaximumMediaNumber,
                    Comment = "The maximum value a media number can have for an album.",
                    Value = "500",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ValidationMaximumSongNumber,
                    Comment = "The maximum value a song number can have for an album.",
                    Value = "1000",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ValidationMinimumAlbumYear,
                    Comment = "Minimum allowed year for an album.",
                    Value = "1860",
                    CreatedAt = now,
                },
                new Setting
                {
                    Key = SettingRegistry.ValidationMaximumAlbumYear,
                    Comment = "Maximum allowed year for an album.",
                    Value = "2150",
                    CreatedAt = now,
                },                
            ]);            
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
    }

}
