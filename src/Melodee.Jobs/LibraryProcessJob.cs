using System.Diagnostics;
using Melodee.Common.Data;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Song;
using Melodee.Plugins.Processor;
using Melodee.Services;
using Melodee.Services.Scanning;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Quartz;
using Serilog;
using SearchOption = System.IO.SearchOption;

namespace Melodee.Jobs;

/// <summary>
/// Process non staging and inbound libraries and update database with updated metadata.
/// </summary>
[DisallowConcurrentExecution]
public class LibraryProcessJob(
    ILogger logger,
    SettingService settingService,
    LibraryService libraryService,
    ISerializer serializer,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    ArtistService artistService,
    AlbumDiscoveryService albumDiscoveryService) : JobBase(logger, settingService)
{
    public override async Task Execute(IJobExecutionContext context)
    {
        var startTicks = Stopwatch.GetTimestamp();
        var configuration = await SettingService.GetMelodeeConfigurationAsync(context.CancellationToken).ConfigureAwait(false);
        var libraries = await libraryService.ListAsync(new PagedRequest(), context.CancellationToken).ConfigureAwait(false);
        if (!libraries.IsSuccess)
        {
            Logger.Warning("[{JobName}] Unable to get libraries, skipping processing.", nameof(LibraryProcessJob));
            return;
        }
        var totalAlbumsUpdated = 0;
        var totalSongsUpdated = 0;        
        await using (var scopedContext = await contextFactory.CreateDbContextAsync(context.CancellationToken).ConfigureAwait(false))
        {
            var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
            var stagingLibrary = await libraryService.GetStagingLibraryAsync(context.CancellationToken).ConfigureAwait(false);
            if (!stagingLibrary.IsSuccess)
            {
                Logger.Warning("[{JobName}] Unable to get staging library, skipping processing.", nameof(LibraryProcessJob));
                return;
            }
            var mediaFilePlugin = new AtlMetaTag(new MetaTagsProcessor(configuration, serializer), configuration);
            foreach (var library in libraries.Data.Where(x => x.TypeValue == LibraryType.Library))
            {
                if (library.IsLocked)
                {
                    Logger.Warning("[{JobName}] Skipped processing locked library [{LibraryName}]", nameof(LibraryProcessJob), library.Name);
                    continue;
                }
                var dirs = new DirectoryInfo(library.Path).GetDirectories("*", SearchOption.AllDirectories);
                var lastScanAt = library.LastScanAt ?? now;
                // Get a list of modified directories in the Library; remember a library directory only contains a single album in Melodee
                foreach (var dir in dirs.Where(d => d.LastWriteTime <= lastScanAt.ToDateTimeUtc()).ToArray())
                {
                    var libraryFileSystemDirectoryInfo = library.ToFileSystemDirectoryInfo();
                    var dirFileSystemDirectoryInfo = new FileSystemDirectoryInfo
                    {
                        Path = dir.FullName,
                        Name = dir.Name
                    };
                    var allDirectoryFiles = dir.GetFiles("*", SearchOption.TopDirectoryOnly);
                    var melodeeFile = (await albumDiscoveryService.AllMelodeeAlbumDataFilesForDirectoryAsync(dirFileSystemDirectoryInfo, context.CancellationToken).ConfigureAwait(false)).Data.FirstOrDefault() ?? (await albumDiscoveryService.AlbumsForDirectoryAsync(dirFileSystemDirectoryInfo, new PagedRequest(), context.CancellationToken).ConfigureAwait(false)).Data.FirstOrDefault();
                    if (melodeeFile == null)
                    {
                        Logger.Warning("[{JobName}] Unable to retrieve Melodee data file for directory [{DirectoryName}]", nameof(LibraryProcessJob), dir.FullName);
                        continue;
                    }
                    var mediaFiles = allDirectoryFiles.Where(x => FileHelper.IsFileMediaType(x.Extension)).ToArray();
                    var dbAlbum = await scopedContext
                        .Albums
                        .Include(x => x.Artist)
                        .Include(x => x.Discs).ThenInclude(x => x.Songs)
                        .FirstOrDefaultAsync(a => a.Directory == dir.Name, context.CancellationToken);
                    if (dbAlbum != null && mediaFiles.Length == 0)
                    {
                        // Albums directory is empty or has no media files, delete album, delete folder.
                        await scopedContext
                            .Libraries
                            .Where(x => x.Id == library.Id)
                            .ExecuteDeleteAsync(context.CancellationToken)
                            .ConfigureAwait(false);
                        await scopedContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
                        dir.Delete(true);
                        Logger.Warning("[{JobName}] Deleted empty album directory [{DirName}]", nameof(LibraryProcessJob), dir.Name);
                        continue;
                    }
                    if (dbAlbum == null)
                    {
                        MediaEditService.MoveDirectory(dir.FullName, stagingLibrary.Data.Path, null);
                        Logger.Debug("[{JobName}] Moved album directory [{DirName}] to staging library.", nameof(LibraryProcessJob),dir.FullName);
                        continue;
                    }
                    if (dbAlbum.IsLocked || dbAlbum.Artist.IsLocked)
                    {
                        // don't do anything with locked albums or locked artists.
                        continue;
                    }
                    if (dbAlbum.Discs.Count == 0)
                    {
                        var dbAlbumDiscsToAdd = new List<Common.Data.Models.AlbumDisc>();
                        var mediaCountValue = dbAlbum.DiscCount < 1 ? 1 : dbAlbum.DiscCount;
                        for (short i = 1; i <= mediaCountValue; i++)
                        {
                            dbAlbumDiscsToAdd.Add(new Common.Data.Models.AlbumDisc
                            {
                                AlbumId = dbAlbum.Id,
                                DiscNumber = i,
                                SongCount = SafeParser.ToNumber<short>(dbAlbum.SongCount ?? 0)
                            });
                        }
                        await scopedContext.AlbumDiscs.AddRangeAsync(dbAlbumDiscsToAdd, context.CancellationToken).ConfigureAwait(false);
                        await scopedContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);   
                    }
                    var foundSongsMetaTagResults = new List<Song>();
                    var dbSongsToAdd = new List<Common.Data.Models.Song>();
                    foreach (var mediaFile in mediaFiles)
                    {
                        var dbSong = dbAlbum.Discs?.SelectMany(x => x.Songs).FirstOrDefault(x => x.FileName == mediaFile.Name);
                        if (dbSong != null && dbSong.FileHash == Crc32.Calculate(mediaFile))
                        {
                            // File hasn't changed skip.
                            continue;
                        }
                        if (dbSong?.IsLocked ?? false)
                        {
                            // Don't do anything with locked songs
                            continue;
                        }
                        var songMetaTagResult = await mediaFilePlugin.ProcessFileAsync(libraryFileSystemDirectoryInfo, mediaFile.ToFileSystemInfo(), context.CancellationToken).ConfigureAwait(false);
                        if (!songMetaTagResult.IsSuccess)
                        {
                            Logger.Warning("[{JobName}] failed to load metadata for file [{FileName}].", nameof(LibraryProcessJob), mediaFile.Name); 
                            continue;
                        }
                        var song = songMetaTagResult.Data;
                        foundSongsMetaTagResults.Add(song);
                        var songTitle = song.Title();
                        if (songTitle.Nullify() == null)
                        {
                            Logger.Warning("[{JobName}] unable to add song [{SongName}] Song is missing Title.", nameof(LibraryProcessJob), mediaFile.FullName);
                            continue;
                        }                        
                        if (dbSong == null)
                        {
                            dbSongsToAdd.Add(new Common.Data.Models.Song
                            {
                                AlbumDiscId = dbAlbum.Discs!.First(x => x.DiscNumber == song.MediaNumber()).Id,
                                BitDepth = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.BitDepth)?.Value),
                                BitRate = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.BitRate)?.Value),
                                BPM = song.MetaTagValue<int>(MetaTagIdentifier.Bpm),
                                ChannelCount = SafeParser.ToNumber<int?>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.Channels)?.Value),
                                CreatedAt = now,
                                Duration = song.Duration() ?? throw new Exception("Song duration is required."),
                                FileHash = Crc32.Calculate(mediaFile),
                                FileName = mediaFile.Name,
                                FileSize = mediaFile.Length,
                                Genres = dbAlbum.Genres?.Length < 1 ? null : song.Genre()!.Split('/'),
                                Lyrics = song.MetaTagValue<string>(MetaTagIdentifier.UnsynchronisedLyrics) ?? song.MetaTagValue<string>(MetaTagIdentifier.SynchronisedLyrics),
                                MediaUniqueId = song.UniqueId,
                                PartTitles = song.MetaTagValue<string>(MetaTagIdentifier.SubTitle),
                                SamplingRate = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.SampleRate)?.Value),
                                SortOrder = song.SortOrder,
                                Title = songTitle!,
                                TitleNormalized = songTitle!.ToNormalizedString() ?? songTitle!,
                                TitleSort = songTitle!.CleanString(true),
                                SongNumber = song.SongNumber()
                            });
                            // Add contributors
                            
                        }
                        else
                        {
                            dbSong.AlbumDiscId = dbAlbum.Discs!.First(x => x.DiscNumber == song.MediaNumber()).Id;
                            dbSong.BPM = song.MetaTagValue<int>(MetaTagIdentifier.Bpm);
                            dbSong.BitDepth = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.BitDepth)?.Value);
                            dbSong.BitRate = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.BitRate)?.Value);
                            dbSong.ChannelCount = SafeParser.ToNumber<int?>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.Channels)?.Value);
                            dbSong.Duration = song.Duration() ?? throw new Exception("Song duration is required.");
                            dbSong.FileHash = Crc32.Calculate(mediaFile);
                            dbSong.FileName = mediaFile.Name;
                            dbSong.FileSize = mediaFile.Length;
                            dbSong.Genres = dbAlbum.Genres?.Length < 1 ? null : song.Genre()!.Split('/');
                            dbSong.LastUpdatedAt = now;
                            dbSong.Lyrics = song.MetaTagValue<string>(MetaTagIdentifier.UnsynchronisedLyrics) ?? song.MetaTagValue<string>(MetaTagIdentifier.SynchronisedLyrics);
                            dbSong.MediaUniqueId = song.UniqueId;
                            dbSong.PartTitles = song.MetaTagValue<string>(MetaTagIdentifier.SubTitle);
                            dbSong.SamplingRate = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.SampleRate)?.Value);
                            dbSong.SongNumber = song.SongNumber();
                            dbSong.SortOrder = song.SortOrder;
                            dbSong.Title = songTitle!;
                            dbSong.TitleNormalized = songTitle!.ToNormalizedString() ?? songTitle!;
                            dbSong.TitleSort = songTitle!.CleanString(true);
                            
                            // Update contributors
                        }

                        totalSongsUpdated++;
                    }
                    if (dbSongsToAdd.Count != 0)
                    {
                        await scopedContext.Songs.AddRangeAsync(dbSongsToAdd, context.CancellationToken).ConfigureAwait(false);
                    }
                    await scopedContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
                    
                    // - Delete any songs not found in directory but in database
                    var orphanedDbSongs = (from a in dbAlbum.Discs?.SelectMany(x => x.Songs)
                        join f in mediaFiles on a.FileName equals f.Name into af
                        from f in af.DefaultIfEmpty()
                        where f == null
                        select a.Id).ToArray();
                    await scopedContext.Songs.Where(x => orphanedDbSongs.Contains(x.Id)).ExecuteDeleteAsync(context.CancellationToken).ConfigureAwait(false);

                    // - Update album metadata from found songs
                    var firstSongGroupedByArtist = foundSongsMetaTagResults.GroupBy(x => x.AlbumArtist()).FirstOrDefault()?.FirstOrDefault();
                    var artistName = melodeeFile.Artist() ?? firstSongGroupedByArtist?.AlbumArtist() ?? throw new Exception("Album artist is required.");
                    
                    var dbArtistResult = await artistService.GetByMediaUniqueId(melodeeFile.ArtistUniqueId(),context.CancellationToken).ConfigureAwait(false);
                    if (!dbArtistResult.IsSuccess)
                    {
                        dbArtistResult = await artistService.GetByNameNormalized(artistName.ToNormalizedString() ?? artistName, context.CancellationToken).ConfigureAwait(false);
                    }

                    if (!dbArtistResult.IsSuccess || dbArtistResult.Data == null)
                    {
                        Logger.Warning("[{JobName}] unable to find db artist for album artist [{ArtistName}] for directory [{DirectoryName}].", nameof(LibraryProcessJob), artistName, dir.FullName);
                        continue;
                    }
                    var albumTitle = melodeeFile.AlbumTitle() ?? throw new Exception("Album title is required.");
                    var albumDirectory = melodeeFile.AlbumDirectoryName(configuration.Configuration);
                    dbAlbum.AlbumStatus = (short)melodeeFile.Status;
                    dbAlbum.AlbumType = (int)AlbumType.Album;
                    dbAlbum.ArtistId = dbArtistResult.Data.Id;
                    dbAlbum.Directory = albumDirectory;
                    dbAlbum.DiscCount = melodeeFile.MediaCountValue();
                    dbAlbum.Duration = melodeeFile.TotalDuration();
                    dbAlbum.Genres = melodeeFile.Genre() == null ? null : melodeeFile.Genre()!.Split('/');
                    dbAlbum.IsCompilation = melodeeFile.IsVariousArtistTypeAlbum();
                    dbAlbum.LastUpdatedAt = now;                    
                    dbAlbum.MediaUniqueId = melodeeFile.UniqueId;
                    dbAlbum.MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess;
                    dbAlbum.Name = albumTitle;
                    dbAlbum.NameNormalized = albumTitle.ToNormalizedString() ?? albumTitle;
                    dbAlbum.OriginalReleaseDate = melodeeFile.OriginalAlbumYear() == null ? null : new LocalDate(melodeeFile.OriginalAlbumYear()!.Value, 1, 1);
                    dbAlbum.ReleaseDate = new LocalDate(melodeeFile.AlbumYear() ?? throw new Exception("Album year is required."), 1, 1);
                    dbAlbum.SongCount = SafeParser.ToNumber<short>(melodeeFile.Songs?.Count() ?? 0);
                    dbAlbum.SortName = albumTitle.CleanString(true);

                    totalAlbumsUpdated++;
                    
                    library.LastScanAt = now;
                    library.LastUpdatedAt = now;
                    
                    await scopedContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);                    
                }
            }
        }
        Log.Debug("ℹ️ [{JobName}] Completed. Updated [{NumberOfAlbumsUpdated}] albums, [{NumberOfSongsUpdated}] songs in [{ElapsedTime}]",
            nameof(LibraryProcessJob), totalAlbumsUpdated, totalSongsUpdated, Stopwatch.GetElapsedTime(startTicks));
        
    }
}
