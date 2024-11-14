using System.Diagnostics;
using Dapper;
using IdSharp.Common.Utils;
using Melodee.Common.Constants;
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
using dbModels = Melodee.Common.Data.Models;

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
    
    private static MetaTagIdentifier[] ContributorMetaTagIdentifiers => new[]
    {
        MetaTagIdentifier.Artist,
        MetaTagIdentifier.Composer,
        MetaTagIdentifier.Conductor,
        MetaTagIdentifier.Engineer,
        MetaTagIdentifier.InterpretedRemixedOrOtherwiseModifiedBy,
        MetaTagIdentifier.Lyricist,
        MetaTagIdentifier.MixDj,
        MetaTagIdentifier.MixEngineer,
        MetaTagIdentifier.MusicianCredit,
        MetaTagIdentifier.OriginalArtist,
        MetaTagIdentifier.OriginalLyricist,
        MetaTagIdentifier.Producer
    };    
    
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
        var totalAlbumsInserted = 0;
        var totalAlbumsUpdated = 0;
        var totalArtistsInserted = 0;
        var totalSongsInserted = 0;
        var totalSongsUpdated = 0;
        var dbAlbumIdsModifiedOrUpdated = new List<int>();
        var maxSongsToProcess = configuration.GetValue<int?>(SettingRegistry.ProcessingMaximumProcessingCount) ?? 0;
        
        var dataMap = context.JobDetail.JobDataMap;
        var jobDataMapLibraryScanHistory = (dbModels.LibraryScanHistory)dataMap.Get(nameof(dbModels.LibraryScanHistory));          
        
        await using (var scopedContext = await contextFactory.CreateDbContextAsync(context.CancellationToken).ConfigureAwait(false))
        {
            var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
            var stagingLibrary = await libraryService.GetStagingLibraryAsync(context.CancellationToken).ConfigureAwait(false);
            if (!stagingLibrary.IsSuccess)
            {
                Logger.Warning("[{JobName}] Unable to get staging library, skipping processing.", nameof(LibraryProcessJob));
                return;
            }

            jobDataMapLibraryScanHistory.ScanStatus = ScanStatus.InProcess;            
            
            var mediaFilePlugin = new AtlMetaTag(new MetaTagsProcessor(configuration, serializer), configuration);
            foreach (var library in libraries.Data.Where(x => x.TypeValue == LibraryType.Library))
            {
                if (library.IsLocked)
                {
                    Logger.Warning("[{JobName}] Skipped processing locked library [{LibraryName}]", nameof(LibraryProcessJob), library.Name);
                    continue;
                }
                if (totalSongsInserted + totalSongsUpdated > maxSongsToProcess && maxSongsToProcess > 0)
                {
                    Logger.Warning("[{JobName}] Maximum Processing Count reached. Stopping processing.", nameof(LibraryProcessJob));
                    break;
                }
                var libraryProcessStartTicks = Stopwatch.GetTimestamp();
                var libraryFileSystemDirectoryInfo = library.ToFileSystemDirectoryInfo();
                var dirs = new DirectoryInfo(library.Path).GetDirectories("*", SearchOption.AllDirectories);
                var lastScanAt = library.LastScanAt ?? now;
                // Get a list of modified directories in the Library; remember a library directory should only contain a single album in Melodee
                foreach (var dir in dirs.Where(d => d.LastWriteTime <= lastScanAt.ToDateTimeUtc()).ToArray())
                {
                    if (totalSongsInserted + totalSongsUpdated > maxSongsToProcess && maxSongsToProcess > 0)
                    {
                        Logger.Warning("[{JobName}] Maximum Processing Count reached. Stopping processing.", nameof(LibraryProcessJob));
                        break;
                    }                    
                    var dirFileSystemDirectoryInfo = new FileSystemDirectoryInfo
                    {
                        Path = dir.FullName,
                        Name = dir.Name
                    };
                    var allDirectoryFiles = dir.GetFiles("*", SearchOption.TopDirectoryOnly);
                    var melodeeFile = (await albumDiscoveryService
                                          .AllMelodeeAlbumDataFilesForDirectoryAsync(dirFileSystemDirectoryInfo, context.CancellationToken)
                                          .ConfigureAwait(false))
                                      .Data
                                      .FirstOrDefault() ??
                                      (await albumDiscoveryService
                                          .AlbumsForDirectoryAsync(dirFileSystemDirectoryInfo, new PagedRequest(), context.CancellationToken)
                                          .ConfigureAwait(false))
                                      .Data
                                      .FirstOrDefault();
                    if (melodeeFile == null)
                    {
                        Logger.Warning(
                            "[{JobName}] Unable to retrieve Melodee data file for directory [{DirectoryName}]",
                            nameof(LibraryProcessJob),
                            dir.FullName);
                        continue;
                    }

                    var mediaFiles = allDirectoryFiles.Where(x => FileHelper.IsFileMediaType(x.Extension)).ToArray();
                        
                    // Load metadata for all media files
                    var foundSongsMetaTagResults = new List<Song>();                    
                    foreach (var mediaFile in mediaFiles)
                    {
                        var songMetaTagResult = await mediaFilePlugin.ProcessFileAsync(libraryFileSystemDirectoryInfo, mediaFile.ToFileSystemInfo(), context.CancellationToken).ConfigureAwait(false);
                        if (!songMetaTagResult.IsSuccess)
                        {
                            Logger.Warning("[{JobName}] failed to load metadata for file [{FileName}].", nameof(LibraryProcessJob), mediaFile.Name);
                            continue;
                        }
                        foundSongsMetaTagResults.Add(songMetaTagResult.Data);
                    }
                    var dbAlbum = await scopedContext
                        .Albums
                        .Include(x => x.Artist)
                        .Include(x => x.Discs).ThenInclude(x => x.Songs)
                        .FirstOrDefaultAsync(a => a.LibraryId == library.Id && a.Directory == dir.Name, context.CancellationToken)
                        .ConfigureAwait(false);
                    if (dbAlbum != null && foundSongsMetaTagResults.Count == 0)
                    {
                        // Albums directory is empty or has no media files; delete album, delete folder.
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
                    var mediaCountValue = melodeeFile.MediaCountValue() < 1 ? 1 : melodeeFile.MediaCountValue();
                    var firstSongGroupedByArtist = foundSongsMetaTagResults.GroupBy(x => x.AlbumArtist()).FirstOrDefault()?.FirstOrDefault();
                    var artistName = melodeeFile.Artist() ?? firstSongGroupedByArtist?.AlbumArtist() ?? throw new Exception("Album artist is required.");
                    var dbArtistResult = await artistService.GetByMediaUniqueId(melodeeFile.ArtistUniqueId(), context.CancellationToken).ConfigureAwait(false);
                    if (!dbArtistResult.IsSuccess)
                    {
                        dbArtistResult = await artistService.GetByNameNormalized(artistName.ToNormalizedString() ?? artistName, context.CancellationToken).ConfigureAwait(false);
                    }
                    var dbArtist = dbArtistResult.Data;
                    if (!dbArtistResult.IsSuccess || dbArtist == null)
                    {
                        dbArtist = new dbModels.Artist
                        {
                            AlbumCount = 1,
                            CreatedAt = now,
                            MediaUniqueId = melodeeFile.ArtistUniqueId(),
                            MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess,
                            Name = artistName,
                            NameNormalized = artistName.ToNormalizedString() ?? artistName,
                            SongCount = melodeeFile.Songs?.Count() ?? 0,
                            SortName = artistName.CleanString(true)
                        };
                        await scopedContext.Artists.AddAsync(dbArtist, context.CancellationToken).ConfigureAwait(false);
                        await scopedContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
                        totalArtistsInserted++;
                    }

                    if (dbArtist.IsLocked)
                    {
                        Logger.Warning("[{JobName}] Skipped processing locked artist [{Artist}]", nameof(LibraryProcessJob), dbArtist);
                        continue;
                    }
                    
                    var albumTitle = melodeeFile.AlbumTitle() ?? throw new Exception("Album title is required.");
                    var albumDirectory = melodeeFile.AlbumDirectoryName(configuration.Configuration);
                    var dbAlbumDiscsToAdd = new List<dbModels.AlbumDisc>();
                    if (dbAlbum == null)
                    {
                        
                        dbAlbum = new dbModels.Album
                        {
                            AlbumStatus = (short)melodeeFile.Status,
                            AlbumType = (int)AlbumType.Album,
                            ArtistId = dbArtist.Id,
                            CreatedAt = now,
                            Directory = albumDirectory,
                            DiscCount = melodeeFile.MediaCountValue(),
                            Duration = melodeeFile.TotalDuration(),
                            Genres = melodeeFile.Genre() == null ? null : melodeeFile.Genre()!.Split('/'),
                            IsCompilation = melodeeFile.IsVariousArtistTypeAlbum(),
                            LibraryId = library.Id,                            
                            MediaUniqueId = melodeeFile.UniqueId,
                            MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess,
                            Name = albumTitle,
                            NameNormalized = albumTitle.ToNormalizedString() ?? albumTitle,
                            OriginalReleaseDate = melodeeFile.OriginalAlbumYear() == null ? null : new LocalDate(melodeeFile.OriginalAlbumYear()!.Value, 1, 1),
                            ReleaseDate = new LocalDate(melodeeFile.AlbumYear() ?? throw new Exception("Album year is required."), 1, 1),
                            SongCount = SafeParser.ToNumber<short>(melodeeFile.Songs?.Count() ?? 0),
                            SortName = albumTitle.CleanString(true)
                        };
                        await scopedContext.Albums.AddAsync(dbAlbum, context.CancellationToken).ConfigureAwait(false);
                        await scopedContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
                        for (short i = 1; i <= mediaCountValue; i++)
                        {
                            dbAlbumDiscsToAdd.Add(new dbModels.AlbumDisc
                            {
                                AlbumId = dbAlbum.Id,
                                DiscNumber = i,
                                SongCount = SafeParser.ToNumber<short>(melodeeFile.Songs?.Where(x => x.MediaNumber() == i).Count() ?? 0)
                            });
                        }
                    
                        await scopedContext.AlbumDiscs.AddRangeAsync(dbAlbumDiscsToAdd, context.CancellationToken).ConfigureAwait(false);
                        await scopedContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
                        dbAlbumDiscsToAdd.Clear();
                        dbAlbumIdsModifiedOrUpdated.Add(dbAlbum.Id);
                        totalAlbumsInserted++;
                    }
                    else if (dbAlbum.IsLocked)
                    {
                        Logger.Warning("[{JobName}] Skipped processing locked artist [{Album}]", nameof(LibraryProcessJob), dbAlbum);
                        continue;
                    }
                    else
                    {
                        // Update album metadata from found songs
                        dbAlbum.AlbumStatus = (short)melodeeFile.Status;
                        dbAlbum.AlbumType = (int)AlbumType.Album;
                        dbAlbum.ArtistId = dbArtist.Id;
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

                        dbAlbumIdsModifiedOrUpdated.Add(dbAlbum.Id);
                        
                        totalAlbumsUpdated++;
                    }
                    var mediaFileMediaNumbers = melodeeFile.Songs?.Select(x => x.MediaNumber()).Distinct().ToArray();
                    foreach (var mediaFileMediaNumber in mediaFileMediaNumbers ?? [])
                    {
                        var dbAlbumDiscForMediaNumber = dbAlbum.Discs.FirstOrDefault(x => x.DiscNumber == mediaFileMediaNumber);
                        if (dbAlbumDiscForMediaNumber != null)
                        {
                            dbAlbumDiscsToAdd.Add(new dbModels.AlbumDisc
                            {
                                AlbumId = dbAlbum.Id,
                                DiscNumber = mediaFileMediaNumber,
                                Title = melodeeFile.DiscSubtitle(mediaFileMediaNumber),
                                SongCount = SafeParser.ToNumber<short>(melodeeFile.Songs?.Where(x => x.MediaNumber() == mediaFileMediaNumber).Count() ?? 0)
                            });
                        }
                    }
                    if (dbAlbumDiscsToAdd.Any())
                    {
                        await scopedContext.AlbumDiscs.AddRangeAsync(dbAlbumDiscsToAdd, context.CancellationToken).ConfigureAwait(false);
                    }
                    await scopedContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);                    

                    var dbSongsToAdd = new List<dbModels.Song>();
                    foreach (var song in foundSongsMetaTagResults)
                    {
                        if (totalSongsInserted + totalSongsUpdated > maxSongsToProcess && maxSongsToProcess > 0)
                        {
                            Logger.Warning("[{JobName}] Maximum Processing Count reached. Stopping processing.", nameof(LibraryProcessJob));
                            break;
                        }
                        var mediaFile = mediaFiles.First(x => x.Name == song.File.Name);
                        var mediaFileHash = CRC32.Calculate(mediaFile);
                        var songTitle = song.Title();
                        if (songTitle.Nullify() == null)
                        {
                            Logger.Warning("[{JobName}] unable to add song [{SongName}] Song is missing Title.", nameof(LibraryProcessJob), song.File.FullName(libraryFileSystemDirectoryInfo));
                            continue;
                        }

                        var dbSong = await scopedContext.Songs.FirstOrDefaultAsync(x => x.MediaUniqueId == song.UniqueId, context.CancellationToken).ConfigureAwait(false);
                        
                        if (dbSong == null)
                        {
                            dbSongsToAdd.Add(new dbModels.Song
                            {
                                AlbumDiscId = dbAlbum.Discs.First(x => x.DiscNumber == song.MediaNumber()).Id,
                                BitDepth = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.BitDepth)?.Value),
                                BitRate = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.BitRate)?.Value),
                                BPM = song.MetaTagValue<int>(MetaTagIdentifier.Bpm),
                                ChannelCount = SafeParser.ToNumber<int?>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.Channels)?.Value),
                                CreatedAt = now,
                                Duration = song.Duration() ?? throw new Exception("Song duration is required."),
                                FileHash = mediaFileHash,
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
                            dbSong = dbSongsToAdd.Last();
                         
                            var dbContributorsToAdd = await GetContributorsForSong(song, dbArtist.Id, dbAlbum.Id, dbSong.Id, now, context.CancellationToken).ConfigureAwait(false);
                            if (dbContributorsToAdd.Length > 0)
                            {
                                Log.Debug("Added [{Count}] contributors to album [{Album}].", dbContributorsToAdd.Length, dbAlbum);
                                await scopedContext.Contributors.AddRangeAsync(dbContributorsToAdd, context.CancellationToken).ConfigureAwait(false);                                
                            }

                            totalSongsInserted++;
                        }
                        else
                        {
                            dbSong.AlbumDiscId = dbAlbum.Discs.First(x => x.DiscNumber == song.MediaNumber()).Id;
                            dbSong.BPM = song.MetaTagValue<int>(MetaTagIdentifier.Bpm);
                            dbSong.BitDepth = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.BitDepth)?.Value);
                            dbSong.BitRate = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.BitRate)?.Value);
                            dbSong.ChannelCount = SafeParser.ToNumber<int?>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.Channels)?.Value);
                            dbSong.Duration = song.Duration() ?? throw new Exception("Song duration is required.");
                            dbSong.FileHash = mediaFileHash;
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
                            
                            var dbContributors = await scopedContext
                                .Contributors.Include(x => x.Artist)
                                .Where(x => x.SongId == dbSong.Id)
                                .ToListAsync(context.CancellationToken)
                                .ConfigureAwait(false);
                            var dbContributorsForSong = await GetContributorsForSong(song, dbArtist.Id, dbAlbum.Id, dbSong.Id, now, context.CancellationToken).ConfigureAwait(false);
                            if (dbContributors.Count == 0 && dbContributorsForSong.Any())
                            {
                                // For each song contributor create new as there are none in database
                                Log.Debug("Added [{Count}] contributors to song [{Song}].", dbContributorsForSong.Length, dbSong);                                
                                await scopedContext.Contributors.AddRangeAsync(dbContributorsForSong, context.CancellationToken).ConfigureAwait(false);  
                            }
                            else
                            {
                                var dbContributorsToDelete = new List<dbModels.Contributor>();
                                foreach (var dbContributor in dbContributors)
                                {
                                    // If there isn't a contributor for the MetaTagIdentifier in the song, delete the database one.
                                    if (!dbContributor.IsLocked && dbContributorsForSong.All(x => x.MetaTagIdentifier != dbContributor.MetaTagIdentifier))
                                    {
                                        dbContributorsToDelete.Add(dbContributor);
                                    }
                                }
                                foreach (var songContributor in dbContributorsForSong)
                                {
                                    var dbContributor = dbContributorsToDelete.FirstOrDefault(x => x.MetaTagIdentifier == songContributor.MetaTagIdentifier && (x.ArtistId == songContributor.ArtistId || string.Equals(x.ContributorName, songContributor.ContributorName, StringComparison.OrdinalIgnoreCase)));
                                    if (dbContributor == null)
                                    {
                                        await scopedContext.Contributors.AddAsync(songContributor, context.CancellationToken).ConfigureAwait(false);  
                                    }
                                    else
                                    {
                                        // update db contributor
                                        if (!dbContributor.IsLocked)
                                        {
                                            dbContributor.ArtistId = songContributor.ArtistId;
                                            dbContributor.ContributorName = songContributor.ContributorName;
                                            dbContributor.LastUpdatedAt = now;
                                            dbContributor.Role = songContributor.Role;
                                            dbContributor.SubRole = songContributor.SubRole;
                                        }
                                    }
                                }
                                if (dbContributorsToDelete.Count > 0)
                                {
                                    scopedContext.Contributors.RemoveRange(dbContributorsToDelete);
                                }
                            }
                        }

                        totalSongsUpdated++;
                    }

                    if (dbSongsToAdd.Count != 0)
                    {
                        await scopedContext.Songs.AddRangeAsync(dbSongsToAdd, context.CancellationToken).ConfigureAwait(false);
                    }
                    
                    await scopedContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
                    
                    // Delete any songs not found in directory but in database
                    var orphanedDbSongs = (from a in dbAlbum.Discs.SelectMany(x => x.Songs)
                        join f in mediaFiles on a.FileName equals f.Name into af
                        from f in af.DefaultIfEmpty()
                        where f == null
                        select a.Id).ToArray();
                    await scopedContext.Songs.Where(x => orphanedDbSongs.Contains(x.Id)).ExecuteDeleteAsync(context.CancellationToken).ConfigureAwait(false);

                    var dbConn = scopedContext.Database.GetDbConnection();                    
                    var sql = """
                              with albumCounts as (
                              	select "ArtistId" as id, COUNT(*) as count
                              	from "Albums"
                              	group by "ArtistId"
                              )
                              UPDATE "Artists"
                              set "AlbumCount" = c.count, "LastUpdatedAt" = NOW()
                              from albumCounts c
                              where c.id = "Artists"."Id"
                              and c.id in @albumIds;
                              """;
                    await dbConn
                        .ExecuteAsync(sql, new { AlbumIds = string.Join(',', dbAlbumIdsModifiedOrUpdated) })
                        .ConfigureAwait(false);
                    
                    sql = """
                              with songCounts as (
                              	select a."ArtistId" as id, COUNT(s.*) as count
                              	from "Songs" s
                              	left join "AlbumDiscs" ad on (s."AlbumDiscId" = ad."Id")
                              	left join "Albums" a on (ad."AlbumId" = a."Id")
                              	group by a."ArtistId"
                              )
                              UPDATE "Artists"
                              set "SongCount" = c.count, "LastUpdatedAt" = NOW()
                              from songCounts c
                              where c.id = "Artists"."Id"
                              and c.id in (1,2,3);
                              """;
                    await dbConn
                        .ExecuteAsync(sql, new { AlbumIds = string.Join(',', dbAlbumIdsModifiedOrUpdated) })
                        .ConfigureAwait(false);

                    sql = """
                          with performerSongCounts as (
                          	select c."ArtistId" as id, COUNT(*) as count
                          	from "Contributors" c 
                          	group by c."ArtistId"
                          )
                          UPDATE "Artists"
                          set "SongCount" = c.count, "LastUpdatedAt" = NOW()
                          from performerSongCounts c
                          where c.id = "Artists"."Id"
                          and c.id in (1,2,3);
                          """;
                    await dbConn
                        .ExecuteAsync(sql, new { AlbumIds = string.Join(',', dbAlbumIdsModifiedOrUpdated) })
                        .ConfigureAwait(false);      
                    
                    library.LastScanAt = now;
                    library.LastUpdatedAt = now;
                    
                    var newLibraryScanHistory = new dbModels.LibraryScanHistory
                    {
                        LibraryId = library.Id,
                        CreatedAt = now,
                        DurationInMs = Stopwatch.GetElapsedTime(libraryProcessStartTicks).TotalMilliseconds,
                        FoundAlbumsCount = (totalAlbumsInserted + totalAlbumsUpdated),
                        FoundArtistsCount = totalArtistsInserted,
                        FoundSongsCount = (totalSongsInserted + totalSongsUpdated)
                    };
                    scopedContext.LibraryScanHistories.Add(newLibraryScanHistory);                    
                    await scopedContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);                    
                }
            }
        }
        
        jobDataMapLibraryScanHistory.ScanStatus = ScanStatus.Idle;
        jobDataMapLibraryScanHistory.FoundAlbumsCount = (totalAlbumsInserted + totalAlbumsUpdated);
        jobDataMapLibraryScanHistory.FoundArtistsCount = totalArtistsInserted ;
        jobDataMapLibraryScanHistory.FoundSongsCount = (totalSongsInserted + totalSongsUpdated);         

        Log.Debug("ℹ️ [{JobName}] Completed. Updated [{NumberOfAlbumsUpdated}] albums, [{NumberOfSongsUpdated}] songs in [{ElapsedTime}]",
            nameof(LibraryProcessJob), totalAlbumsUpdated, totalSongsUpdated, Stopwatch.GetElapsedTime(startTicks));
    }

    private async Task<dbModels.Contributor[]> GetContributorsForSong(Song song, int artistId, int albumId, int songId, Instant now, CancellationToken token)
    {
        var dbContributorsToAdd = new List<dbModels.Contributor>();
        foreach (var contributorTag in ContributorMetaTagIdentifiers)
        {
            var contributorForTag = await CreateContributorForSongAndTag(song, contributorTag, artistId, albumId, songId, now, null, null, null, token);
            if (contributorForTag != null)
            {
                dbContributorsToAdd.Add(contributorForTag);
            }
        }
        foreach (var tmclTag in song.Tags?.Where(x => x.Value != null && x.Value.ToString()!.StartsWith("TMCL:", StringComparison.InvariantCultureIgnoreCase)) ?? [])
        {
            var subRole = tmclTag.Value!.ToString()!.Substring(6).Trim();
            var contributorForTag = await CreateContributorForSongAndTag(song, tmclTag.Identifier, artistId, albumId, songId, now, null, subRole,null, token);
            if (contributorForTag != null)
            {
                dbContributorsToAdd.Add(contributorForTag);
            }
        }

        var songPublisherTag = song.MetaTagValue<string?>(MetaTagIdentifier.Publisher);
        if (songPublisherTag != null)
        {
            var publisherName = songPublisherTag.Trim();
            var publisherTag = await CreateContributorForSongAndTag(song, MetaTagIdentifier.Publisher, artistId, albumId, songId, now, null, null, publisherName, token);
            if (publisherTag != null)
            {
                dbContributorsToAdd.Add(publisherTag);
            }
        }
        return dbContributorsToAdd.ToArray();
    }
    
    private async Task<dbModels.Contributor?> CreateContributorForSongAndTag(
        Song song, 
        MetaTagIdentifier tag,
        int dbArtist,
        int dbAlbumId,
        int dbSongId,
        Instant now,
        string? role,
        string? subRole,
        string? contributorName,
        CancellationToken cancellationToken = default)
    {
        var tagValue = song.MetaTagValue<string?>(tag);
        if (tagValue != null)
        {
            var isContributorNameSet = contributorName.Nullify() != null;
            var artist = isContributorNameSet ? null : await artistService.GetByNameNormalized(tagValue, cancellationToken).ConfigureAwait(false);
            if ((artist?.IsSuccess ?? false) || isContributorNameSet)
            {
                var artistContributorId = artist?.Data?.Id;
                if (artistContributorId != dbArtist || isContributorNameSet)
                {
                    var roleValue = role ?? tag.GetEnumDescriptionValue();
                    return new dbModels.Contributor
                    {
                        AlbumId = dbAlbumId,
                        ArtistId = artistContributorId,
                        ContributorName = contributorName,
                        ContributorType = DetermineContributorType(tag),
                        CreatedAt = now,
                        MetaTagIdentifier = SafeParser.ToNumber<int>(tag),
                        Role = roleValue,
                        SongId = dbSongId,
                        SubRole = subRole,
                    };
                }
            }
            else
            {
                Logger.Warning("Unable to find '{Tag}' by name [{Name}]", tag.ToString(), tagValue);
            }
        }

        return null;
    }

    private int DetermineContributorType(MetaTagIdentifier tag)
    {
        switch (tag)
        {
            case MetaTagIdentifier.AlbumArtist:
            case MetaTagIdentifier.Artist:
            case MetaTagIdentifier.Artists:
            case MetaTagIdentifier.Composer:
            case MetaTagIdentifier.Conductor:
            case MetaTagIdentifier.MusicianCredit:
            case MetaTagIdentifier.OriginalArtist:
            case MetaTagIdentifier.OriginalLyricist:
                return SafeParser.ToNumber<int>(ContributorType.Performer);
            
            case MetaTagIdentifier.EncodedBy:
            case MetaTagIdentifier.Engineer:
            case MetaTagIdentifier.Group:
            case MetaTagIdentifier.InterpretedRemixedOrOtherwiseModifiedBy:
            case MetaTagIdentifier.InvolvedPeople:
            case MetaTagIdentifier.Lyricist:
            case MetaTagIdentifier.MixDj:
            case MetaTagIdentifier.MixEngineer:
            case MetaTagIdentifier.Producer:
                return SafeParser.ToNumber<int>(ContributorType.Production);
            
            case MetaTagIdentifier.Publisher:
                return SafeParser.ToNumber<int>(ContributorType.Publisher);            
        }

        return SafeParser.ToNumber<int>(ContributorType.NotSet);
    }
}
