using System.Diagnostics;
using Dapper;
using IdSharp.Common.Utils;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Song;
using Melodee.Plugins.Processor;
using Melodee.Services;
using Melodee.Services.Interfaces;
using Melodee.Services.Models;
using Melodee.Services.Scanning;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Quartz;
using Serilog;
using SmartFormat;
using SearchOption = System.IO.SearchOption;
using dbModels = Melodee.Common.Data.Models;

namespace Melodee.Jobs;

/// <summary>
///     Process non staging and inbound libraries and update database with updated metadata found in existing melodee data
///     files.
/// </summary>
[DisallowConcurrentExecution]
public class LibraryProcessJob(
    ILogger logger,
    ISettingService settingService,
    ILibraryService libraryService,
    ISerializer serializer,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    ArtistService artistService,
    AlbumService albumService,
    AlbumDiscoveryService albumDiscoveryService,
    DirectoryProcessorService directoryProcessorService) : JobBase(logger, settingService)
{
    private readonly List<int> _dbAlbumIdsModifiedOrUpdated = new();
    private readonly List<int> _dbArtistsIdsModifiedOrUpdated = new();
    private int _batchSize;
    private IMelodeeConfiguration _configuration;
    private JobDataMap _dataMap;
    private int _maxSongsToProcess;
    private AtlMetaTag _mediaFilePlugin;
    private Instant _now;
    private int _totalAlbumsInserted;
    private int _totalAlbumsUpdated;
    private int _totalArtistsInserted;
    private int _totalArtistsUpdated;
    private int _totalSongsInserted;
    private int _totalSongsUpdated;

    private static MetaTagIdentifier[] ContributorMetaTagIdentifiers =>
    [
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
    ];

    /// <summary>
    ///     This is raised when a Log event happens to return activity to caller.
    /// </summary>
    public event EventHandler<ProcessingEvent>? OnProcessingEvent;

    public override async Task Execute(IJobExecutionContext context)
    {
        var startTicks = Stopwatch.GetTimestamp();
        _configuration = await SettingService.GetMelodeeConfigurationAsync(context.CancellationToken).ConfigureAwait(false);
        var libraries = await libraryService.ListAsync(new PagedRequest(), context.CancellationToken).ConfigureAwait(false);
        if (!libraries.IsSuccess)
        {
            Logger.Warning("[{JobName}] Unable to get libraries, skipping processing.", nameof(LibraryProcessJob));
            return;
        }

        DirectoryInfo? processingDirectory = null;

        _totalAlbumsInserted = 0;
        _totalAlbumsUpdated = 0;
        _totalArtistsInserted = 0;
        _totalSongsInserted = 0;
        _totalSongsUpdated = 0;
        _maxSongsToProcess = _configuration.GetValue<int?>(SettingRegistry.ProcessingMaximumProcessingCount) ?? 0;
        _batchSize = _configuration.BatchProcessingSize();
        var messagesForJobRun = new List<string>();
        var exceptionsForJobRun = new List<Exception>();

        Trace.Listeners.Clear();
        Trace.Listeners.Add(new ConsoleTraceListener());

        await albumDiscoveryService.InitializeAsync(_configuration, context.CancellationToken).ConfigureAwait(false);
        await directoryProcessorService.InitializeAsync(_configuration, context.CancellationToken).ConfigureAwait(false);

        ISongPlugin[] songPlugins =
        [
            new AtlMetaTag(new MetaTagsProcessor(_configuration, serializer), _configuration)
        ];
        _mediaFilePlugin = new AtlMetaTag(new MetaTagsProcessor(_configuration, serializer), _configuration);
        _now = Instant.FromDateTimeUtc(DateTime.UtcNow);

        _dataMap = context.JobDetail.JobDataMap;
        var defaultNeverScannedDate = Instant.FromDateTimeUtc(DateTime.MinValue.ToUniversalTime());
        var stagingLibrary = await libraryService.GetStagingLibraryAsync(context.CancellationToken).ConfigureAwait(false);
        if (!stagingLibrary.IsSuccess)
        {
            messagesForJobRun.AddRange(stagingLibrary.Messages);
            exceptionsForJobRun.AddRange(stagingLibrary.Errors);
            Logger.Warning("[{JobName}] Unable to get staging library, skipping processing.", nameof(LibraryProcessJob));
            return;
        }

        var librariesToProcess = libraries.Data.Where(x => x.TypeValue == LibraryType.Library).ToArray();
        _dataMap.Put(JobMapNameRegistry.ScanStatus, ScanStatus.InProcess.ToString());
        OnProcessingEvent?.Invoke(
            this,
            new ProcessingEvent(ProcessingEventType.Start,
                nameof(LibraryProcessJob),
                librariesToProcess.Count(),
                0,
                "Started library processing libraries."));
        await using (var scopedContext = await contextFactory.CreateDbContextAsync(context.CancellationToken).ConfigureAwait(false))
        {
            foreach (var library in librariesToProcess)
            {
                if (library.IsLocked)
                {
                    Logger.Warning("[{JobName}] Skipped processing locked library [{LibraryName}]", nameof(LibraryProcessJob), library.Name);
                    continue;
                }

                if (_totalSongsInserted + _totalSongsUpdated > _maxSongsToProcess && _maxSongsToProcess > 0)
                {
                    Logger.Warning("[{JobName}] Maximum Processing Count reached. Stopping processing.", nameof(LibraryProcessJob));
                    break;
                }

                var libraryProcessStartTicks = Stopwatch.GetTimestamp();
                var dirs = new DirectoryInfo(library.Path).GetDirectories("*", SearchOption.AllDirectories);
                var lastScanAt = library.LastScanAt ?? defaultNeverScannedDate;
                if (_totalSongsInserted + _totalSongsUpdated > _maxSongsToProcess && _maxSongsToProcess > 0)
                {
                    Logger.Warning("[{JobName}] Maximum Processing Count reached. Stopping processing.", nameof(LibraryProcessJob));
                    break;
                }

                var melodeeFilesForDirectory = new List<Album>();
                // Get a list of modified directories in the Library; remember a library directory should only contain a single album in Melodee
                var allDirsForLibrary = dirs.Where(d => d.LastWriteTime >= lastScanAt.ToDateTimeUtc() && d.Name.Length > 3).ToArray();
                var batches = (allDirsForLibrary.Length + _batchSize - 1) / _batchSize;
                for (var batch = 0; batch < batches; batch++)
                {
                    foreach (var dir in allDirsForLibrary.Skip(_batchSize * batch).Take(_batchSize))
                    {
                        try
                        {
                            processingDirectory = dir;

                            var dirFileSystemDirectoryInfo = new FileSystemDirectoryInfo
                            {
                                Path = dir.FullName,
                                Name = dir.Name
                            };
                            var allDirectoryFiles = dir.GetFiles("*", SearchOption.TopDirectoryOnly);
                            var mediaFiles = allDirectoryFiles.Where(x => FileHelper.IsFileMediaType(x.Extension)).ToArray();

                            // Don't continue if there are no media files in the directory.
                            if (mediaFiles.Length == 0)
                            {
                                continue;
                            }

                            // See if an existing melodee file exists in the directory and if so load it
                            var melodeeFile = (await albumDiscoveryService
                                    .AllMelodeeAlbumDataFilesForDirectoryAsync(dirFileSystemDirectoryInfo, context.CancellationToken)
                                    .ConfigureAwait(false))
                                .Data?
                                .FirstOrDefault();
                            if (melodeeFile == null)
                            {
                                melodeeFile = (await directoryProcessorService.AllAlbumsForDirectoryAsync(
                                        dirFileSystemDirectoryInfo,
                                        songPlugins.ToArray(),
                                        _configuration,
                                        context.CancellationToken)
                                    .ConfigureAwait(false)).Data.Item1.FirstOrDefault();
                                if (melodeeFile == null)
                                {
                                    Logger.Warning("[{JobName}] Unable to find Melodee file for directory [{DirName}]", nameof(LibraryProcessJob), dirFileSystemDirectoryInfo);
                                    continue;
                                }
                            }

                            if (!melodeeFile.IsValid(_configuration.Configuration).Item1)
                            {
                                Logger.Warning("[{JobName}] Invalid Melodee file [{Status}]", nameof(LibraryProcessJob), melodeeFile.ToString());
                                continue;
                            }

                            melodeeFilesForDirectory.Add(melodeeFile);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e, "[{JobName}] Error processing directory [{Dir}]", nameof(LibraryProcessJob), processingDirectory);
                        }
                    }
                }

                await ProcessArtistsAsync(library, melodeeFilesForDirectory, context.CancellationToken);
                await ProcessAlbumsAsync(library, melodeeFilesForDirectory, context.CancellationToken);

                var batchCount = (_dbAlbumIdsModifiedOrUpdated.Count + _batchSize - 1) / _batchSize;
                var dbConn = scopedContext.Database.GetDbConnection();
                for (var updateCountBatch = 0; updateCountBatch < batchCount; updateCountBatch++)
                {
                    var skipValue = updateCountBatch * _batchSize;
                    var joinedDbIds = string.Join(',', _dbAlbumIdsModifiedOrUpdated.Skip(skipValue).Take(_batchSize));

                    var sql = """
                              with albumCounts as (
                              	select "ArtistId" as id, COUNT(*) as count
                              	FROM "Albums"
                              	WHERE "LibraryId" = {0}
                              	group by "ArtistId"
                              )
                              UPDATE "Artists"
                              SET "AlbumCount" = c.count, "LastUpdatedAt" = NOW()
                              from albumCounts c
                              WHERE c.id = "Artists"."Id"
                              AND c.id in ({1});
                              """;
                    await dbConn
                        .ExecuteAsync(sql.FormatSmart(library.Id, joinedDbIds), context.CancellationToken)
                        .ConfigureAwait(false);

                    sql = """
                          with songCounts as (
                          	select a."ArtistId" as id, COUNT(s.*) as count
                          	FROM "Songs" s
                          	left join "AlbumDiscs" ad on (s."AlbumDiscId" = ad."Id")
                          	left join "Albums" a on (ad."AlbumId" = a."Id")
                          	WHERE a."LibraryId" = {0}
                          	group by a."ArtistId"
                          )
                          UPDATE "Artists"
                          SET "SongCount" = c.count, "LastUpdatedAt" = NOW()
                          from songCounts c
                          WHERE c.id = "Artists"."Id"
                          AND c.id in ({1});
                          """;
                    await dbConn
                        .ExecuteAsync(sql.FormatSmart(library.Id, joinedDbIds), context.CancellationToken)
                        .ConfigureAwait(false);

                    sql = """
                          with performerSongCounts as (
                          	select c."ArtistId" as id, COUNT(*) as count
                          	from "Contributors" c 
                          	left join "Songs" s on (c."SongId" = s."Id")
                            left join "AlbumDiscs" ad on (s."AlbumDiscId" = ad."Id")
                            left join "Albums" a on (ad."AlbumId" = a."Id")
                            WHERE a."LibraryId" = {0}
                          	group by c."ArtistId"
                          )
                          UPDATE "Artists"
                          set "SongCount" = c.count, "LastUpdatedAt" = NOW()
                          from performerSongCounts c
                          where c.id = "Artists"."Id"
                          and c.id in ({1});
                          """;
                    await dbConn
                        .ExecuteAsync(sql.FormatSmart(library.Id, joinedDbIds), context.CancellationToken)
                        .ConfigureAwait(false);
                }

                var dbLibrary = await scopedContext.Libraries.FirstAsync(x => x.Id == library.Id).ConfigureAwait(false);
                dbLibrary.LastScanAt = _now;
                dbLibrary.LastUpdatedAt = _now;
                var newLibraryScanHistory = new dbModels.LibraryScanHistory
                {
                    LibraryId = dbLibrary.Id,
                    CreatedAt = _now,
                    DurationInMs = Stopwatch.GetElapsedTime(libraryProcessStartTicks).TotalMilliseconds,
                    FoundAlbumsCount = _totalAlbumsInserted + _totalAlbumsUpdated,
                    FoundArtistsCount = _totalArtistsInserted,
                    FoundSongsCount = _totalSongsInserted + _totalSongsUpdated
                };
                scopedContext.LibraryScanHistories.Add(newLibraryScanHistory);
                await scopedContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
            }
        }

        _dataMap.Put(JobMapNameRegistry.ScanStatus, ScanStatus.Idle.ToString());
        _dataMap.Put(JobMapNameRegistry.Count, _totalAlbumsInserted + _totalAlbumsUpdated + _totalArtistsInserted + _totalSongsInserted + _totalSongsUpdated);

        OnProcessingEvent?.Invoke(
            this,
            new ProcessingEvent(ProcessingEventType.Stop,
                nameof(LibraryProcessJob),
                0,
                0,
                "Processed [{0}] albums, [{1}] songs in [{2}]".FormatSmart(_totalAlbumsUpdated + _totalAlbumsInserted, _totalSongsUpdated + _totalSongsInserted, Stopwatch.GetElapsedTime(startTicks))));

        foreach (var message in messagesForJobRun)
        {
            Log.Debug("[{JobName}] Message: [{Message}]", nameof(LibraryProcessJob), message);
        }

        foreach (var exception in exceptionsForJobRun)
        {
            Log.Error(exception, "[{JobName}] Processing Exception", nameof(LibraryProcessJob));
        }

        Log.Debug("ℹ️ [{JobName}] Completed. Processed [{NumberOfAlbumsUpdated}] albums, [{NumberOfSongsUpdated}] songs in [{ElapsedTime}]", nameof(LibraryProcessJob), _totalAlbumsUpdated + _totalAlbumsInserted, _totalSongsUpdated + _totalSongsInserted, Stopwatch.GetElapsedTime(startTicks));
    }

    /// <summary>
    ///     For all albums with songs, add/update the db albums
    /// </summary>
    private async Task ProcessAlbumsAsync(dbModels.Library library, List<Album> melodeeFilesForDirectory, CancellationToken cancellationToken)
    {
        _dbAlbumIdsModifiedOrUpdated.Clear();
        await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbAlbumsToAdd = new List<dbModels.Album>();
            foreach (var melodeeFile in melodeeFilesForDirectory)
            {
                var artistName = melodeeFile.Artist()?.CleanStringAsIs() ?? throw new Exception("Album artist is required.");
                var artistNormalizedName = artistName.ToNormalizedString() ?? artistName;
                var dbArtistResult = await artistService.GetByMediaUniqueId(melodeeFile.ArtistUniqueId(), cancellationToken).ConfigureAwait(false);
                if (!dbArtistResult.IsSuccess)
                {
                    dbArtistResult = await artistService.GetByNameNormalized(artistNormalizedName, cancellationToken).ConfigureAwait(false);
                }

                var dbArtist = await scopedContext.Artists.FirstOrDefaultAsync(x => x.Id == dbArtistResult.Data!.Id, cancellationToken).ConfigureAwait(false);
                if (dbArtist == null)
                {
                    Logger.Warning("Unable to find artist [{ArtistUniqueId}] Artist for album [{AlbumUniqeId}].", melodeeFile.ArtistUniqueId(), melodeeFile.UniqueId);
                    continue;
                }

                var albumTitle = melodeeFile.AlbumTitle()?.CleanStringAsIs() ?? throw new Exception("Album title is required.");
                var nameNormalized = albumTitle.ToNormalizedString() ?? albumTitle;
                var dbAlbumResult = await albumService.GetByMediaUniqueId(melodeeFile.UniqueId, cancellationToken).ConfigureAwait(false);
                if (!dbAlbumResult.IsSuccess)
                {
                    dbAlbumResult = await albumService.GetByArtistIdAndNameNormalized(dbArtist.Id, nameNormalized, cancellationToken).ConfigureAwait(false);
                }

                var dbAlbum = dbAlbumResult.Data;

                var albumDirectory = melodeeFile.AlbumDirectoryName(_configuration.Configuration);
                if (dbAlbum == null)
                {
                    var newAlbum = new dbModels.Album
                    {
                        AlbumStatus = (short)melodeeFile.Status,
                        AlbumType = (int)AlbumType.Album,
                        Artist = dbArtist,
                        CreatedAt = _now,
                        Directory = albumDirectory,
                        DiscCount = melodeeFile.MediaCountValue(),
                        Duration = melodeeFile.TotalDuration(),
                        Genres = melodeeFile.Genre() == null ? null : melodeeFile.Genre()!.Split('/'),
                        IsCompilation = melodeeFile.IsVariousArtistTypeAlbum(),
                        LibraryId = library.Id,
                        MediaUniqueId = melodeeFile.UniqueId,
                        MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess,
                        Name = albumTitle,
                        NameNormalized = nameNormalized,
                        OriginalReleaseDate = melodeeFile.OriginalAlbumYear() == null ? null : new LocalDate(melodeeFile.OriginalAlbumYear()!.Value, 1, 1),
                        ReleaseDate = new LocalDate(melodeeFile.AlbumYear() ?? throw new Exception("Album year is required."), 1, 1),
                        SongCount = SafeParser.ToNumber<short>(melodeeFile.Songs?.Count() ?? 0),
                        SortName = albumTitle.CleanString(true)
                    };
                    for (short i = 1; i <= melodeeFile.MediaCountValue(); i++)
                    {
                        newAlbum.Discs.Add(new dbModels.AlbumDisc
                        {
                            DiscNumber = i,
                            Title = melodeeFile.DiscSubtitle(i),
                            SongCount = SafeParser.ToNumber<short>(melodeeFile.Songs?.Where(x => x.MediaNumber() == i).Count() ?? 0)
                        });
                    }

                    foreach (var disc in newAlbum.Discs)
                    {
                        var songsForDisc = melodeeFile.Songs?.Where(x => x.MediaNumber() == disc.DiscNumber).ToArray() ?? [];
                        foreach (var song in songsForDisc)
                        {
                            var mediaFile = song.File.ToFileInfo(melodeeFile.Directory);
                            var mediaFileHash = CRC32.Calculate(mediaFile);
                            var songTitle = song.Title()?.CleanStringAsIs() ?? throw new Exception("Song title is required.");

                            //         var dbContributors = await scopedContext
                            //             .Contributors.Include(x => x.Artist)
                            //             .Where(x => x.SongId == dbSong.Id)
                            //             .ToListAsync(context.CancellationToken)
                            //             .ConfigureAwait(false);
                            //         var dbContributorsForSong = await GetContributorsForSong(song, dbArtist.Id, dbAlbum.Id, dbSong.Id, now, context.CancellationToken).ConfigureAwait(false);
                            //         if (dbContributors.Count == 0 && dbContributorsForSong.Any())
                            //         {
                            //             dbContributorsToAdd.AddRange(dbContributorsForSong);
                            //         }
                            //         else
                            //         {
                            //             var dbContributorsToDelete = new List<dbModels.Contributor>();
                            //             foreach (var dbContributor in dbContributors)
                            //             {
                            //                 // If there isn't a contributor for the MetaTagIdentifier in the song, delete the database one.
                            //                 if (!dbContributor.IsLocked && dbContributorsForSong.All(x => x.MetaTagIdentifier != dbContributor.MetaTagIdentifier))
                            //                 {
                            //                     dbContributorsToDelete.Add(dbContributor);
                            //                 }
                            //             }
                            //
                            //             foreach (var songContributor in dbContributorsForSong)
                            //             {
                            //                 var dbContributor = dbContributorsToDelete.FirstOrDefault(x => x.MetaTagIdentifier == songContributor.MetaTagIdentifier && (x.ArtistId == songContributor.ArtistId || string.Equals(x.ContributorName, songContributor.ContributorName, StringComparison.OrdinalIgnoreCase)));
                            //                 if (dbContributor == null)
                            //                 {
                            //                     dbContributorsToAdd.Add(songContributor);
                            //                 }
                            //                 else
                            //                 {
                            //                     // update db contributor
                            //                     var updatedRole = songContributor.Role.CleanStringAsIs();
                            //                     if (updatedRole != null && !dbContributor.IsLocked)
                            //                     {
                            //                         dbContributor.ArtistId = songContributor.ArtistId;
                            //                         dbContributor.ContributorName = songContributor.ContributorName;
                            //                         dbContributor.LastUpdatedAt = now;
                            //                         dbContributor.Role = updatedRole;
                            //                         dbContributor.SubRole = songContributor.SubRole?.CleanStringAsIs();
                            //                     }
                            //                 }
                            //             }                            
                            //                  var dbContributorsForSong = await GetContributorsForSong(song, dbArtist.Id, dbAlbum.Id, dbSong.Id, now, context.CancellationToken).ConfigureAwait(false);

                            disc.Songs.Add(new dbModels.Song
                            {
                                AlbumDiscId = disc.Id,
                                BitDepth = song.BitDepth(),
                                BitRate = song.BitRate(),
                                BPM = song.MetaTagValue<int>(MetaTagIdentifier.Bpm),
                                ChannelCount = song.ChannelCount(),
                                ContentType = song.ContentType(),
                                CreatedAt = _now,
                                Duration = song.Duration() ?? throw new Exception("Song duration is required."),
                                FileHash = mediaFileHash,
                                FileName = mediaFile.Name,
                                FileSize = mediaFile.Length,
                                Genres = melodeeFile.Genre()?.Length < 1 ? null : song.Genre()!.Split('/'),
                                IsVbr = song.IsVbr(),
                                Lyrics = song.MetaTagValue<string>(MetaTagIdentifier.UnsynchronisedLyrics)?.CleanStringAsIs() ?? song.MetaTagValue<string>(MetaTagIdentifier.SynchronisedLyrics)?.CleanStringAsIs(),
                                MediaUniqueId = song.UniqueId,
                                PartTitles = song.MetaTagValue<string>(MetaTagIdentifier.SubTitle)?.CleanStringAsIs(),
                                SamplingRate = song.SamplingRate(),
                                SortOrder = song.SortOrder,
                                Title = songTitle,
                                TitleNormalized = songTitle.ToNormalizedString() ?? songTitle,
                                TitleSort = songTitle!.CleanString(true),
                                SongNumber = song.SongNumber()
                            });
                            _totalSongsInserted++;
                        }
                    }

                    dbAlbumsToAdd.Add(newAlbum);
                }
            }

            if (dbAlbumsToAdd.Count > 0)
            {
                try
                {
                    await scopedContext.Albums.AddRangeAsync(dbAlbumsToAdd, cancellationToken).ConfigureAwait(false);
                    await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Unable to insert albums into db.");
                }

                _totalAlbumsInserted += dbAlbumsToAdd.Count;
                _dbAlbumIdsModifiedOrUpdated.AddRange(dbAlbumsToAdd.Select(x => x.Id));
                UpdateDataMap();
            }

            var albumsToUpdate = (from a in melodeeFilesForDirectory
                    join addedAlbum in dbAlbumsToAdd on a.UniqueId equals addedAlbum.MediaUniqueId into aa
                    from album in aa.DefaultIfEmpty()
                    where album is null
                    select a)
                .ToArray();
            foreach (var album in albumsToUpdate)
            {
                var albumDirectory = album.AlbumDirectoryName(_configuration.Configuration);
                var artistName = album.Artist()?.CleanStringAsIs() ?? throw new Exception("Album artist is required.");
                var artistNormalizedName = artistName.ToNormalizedString() ?? artistName;
                var dbArtistResult = await artistService.GetByMediaUniqueId(album.ArtistUniqueId(), cancellationToken).ConfigureAwait(false);
                if (!dbArtistResult.IsSuccess)
                {
                    dbArtistResult = await artistService.GetByNameNormalized(artistNormalizedName, cancellationToken).ConfigureAwait(false);
                }

                var dbArtist = await scopedContext.Artists.FirstOrDefaultAsync(x => x.Id == dbArtistResult.Data!.Id, cancellationToken).ConfigureAwait(false);
                if (dbArtist == null)
                {
                    Logger.Warning("Unable to find artist [{ArtistUniqueId}] Artist for album [{AlbumUniqueId}].", album.ArtistUniqueId(), album.UniqueId);
                    continue;
                }

                if (dbArtist.IsLocked)
                {
                    Logger.Warning("[{JobName}] Skipped processing locked artist [{ArtistId}]", nameof(LibraryProcessJob), dbArtist);
                    continue;
                }

                var albumTitle = album.AlbumTitle()?.CleanStringAsIs() ?? throw new Exception("Album title is required.");
                var nameNormalized = albumTitle.ToNormalizedString() ?? albumTitle;
                var dbAlbumResult = await albumService.GetByMediaUniqueId(album.UniqueId, cancellationToken).ConfigureAwait(false);
                if (!dbAlbumResult.IsSuccess)
                {
                    dbAlbumResult = await albumService.GetByArtistIdAndNameNormalized(dbArtist.Id, nameNormalized, cancellationToken).ConfigureAwait(false);
                }

                if (dbAlbumResult.Data == null)
                {
                    Logger.Warning("Unable to find album [{AlbumUniqueId}] for artist [{ArtistUniqueId}].", album.UniqueId, album.ArtistUniqueId());
                    continue;
                }

                if (dbAlbumResult.Data!.IsLocked)
                {
                    Logger.Warning("[{JobName}] Skipped processing locked album [{AlbumId}]", nameof(LibraryProcessJob), dbAlbumResult.Data);
                    continue;
                }

                var dbAlbum = await scopedContext.Albums.FirstAsync(x => x.Id == dbAlbumResult.Data!.Id, cancellationToken).ConfigureAwait(false);
                dbAlbum.AlbumStatus = (short)album.Status;
                dbAlbum.AlbumType = (int)AlbumType.Album;
                dbAlbum.ArtistId = dbArtist.Id;
                dbAlbum.Directory = albumDirectory;
                dbAlbum.DiscCount = album.MediaCountValue();
                dbAlbum.Duration = album.TotalDuration();
                dbAlbum.Genres = album.Genre() == null ? null : album.Genre()!.Split('/');
                dbAlbum.IsCompilation = album.IsVariousArtistTypeAlbum();
                dbAlbum.LastUpdatedAt = _now;
                dbAlbum.MediaUniqueId = album.UniqueId;
                dbAlbum.MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess;
                dbAlbum.Name = albumTitle;
                dbAlbum.NameNormalized = albumTitle.ToNormalizedString() ?? albumTitle;
                dbAlbum.OriginalReleaseDate = album.OriginalAlbumYear() == null ? null : new LocalDate(album.OriginalAlbumYear()!.Value, 1, 1);
                dbAlbum.ReleaseDate = new LocalDate(album.AlbumYear() ?? throw new Exception("Album year is required."), 1, 1);
                dbAlbum.SongCount = SafeParser.ToNumber<short>(album.Songs?.Count() ?? 0);
                dbAlbum.SortName = albumTitle.CleanString(true);
                dbAlbum.MediaUniqueId = album.UniqueId;
                dbAlbum.Name = albumTitle;
                dbAlbum.NameNormalized = nameNormalized;
                dbAlbum.SortName = albumTitle.CleanString(true);

                // TODO handle media disc changes (add a new one, removed one, etc.)
                _dbAlbumIdsModifiedOrUpdated.Add(dbAlbum.Id);
            }

            if (albumsToUpdate.Any())
            {
                await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                _totalAlbumsUpdated += albumsToUpdate.Length;
                UpdateDataMap();
            }

            // TODO
            // // Delete any songs not found in directory but in database
            // var orphanedDbSongs = (from a in dbAlbum.Discs.SelectMany(x => x.Songs)
            //     join f in mediaFiles on a.FileName equals f.Name into af
            //     from f in af.DefaultIfEmpty()
            //     where f == null
            //     select a.Id).ToArray();
            // if (orphanedDbSongs.Length > 0)
            // {
            //     await scopedContext.Songs.Where(x => orphanedDbSongs.Contains(x.Id)).ExecuteDeleteAsync(context.CancellationToken).ConfigureAwait(false);
            // }
            //             
            // await scopedContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);            
        }
    }

    private void UpdateDataMap()
    {
        _dataMap.Put(
            JobMapNameRegistry.Count,
            _totalAlbumsInserted +
            _totalAlbumsUpdated +
            _totalArtistsInserted +
            _totalArtistsUpdated +
            _totalSongsInserted +
            _totalSongsUpdated);
    }

    /// <summary>
    ///     For given albums, add/update the db album and db song artists.
    /// </summary>
    private async Task ProcessArtistsAsync(dbModels.Library library, List<Album> melodeeFilesForDirectory, CancellationToken cancellationToken)
    {
        _dbArtistsIdsModifiedOrUpdated.Clear();
        await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var artists = melodeeFilesForDirectory
                .Select(x => new
                {
                    UniqueId = x.ArtistUniqueId(),
                    Name = x.Artist()?.CleanStringAsIs() ?? string.Empty,
                    NormalizedName = x.Artist()?.ToNormalizedString() ?? x.Artist()?.CleanStringAsIs() ?? string.Empty,
                    SortName = x.Artist().CleanString(true) ?? string.Empty
                })
                .Where(x => x.Name.Nullify() != null && x.NormalizedName.Nullify() != null && x.SortName.Nullify() != null)
                .Distinct()
                .OrderBy(x => x.Name)
                .ToArray();
            var dbArtistsToAdd = new List<dbModels.Artist>();
            foreach (var artist in artists)
            {
                var dbArtistResult = await artistService.GetByMediaUniqueId(artist.UniqueId, cancellationToken).ConfigureAwait(false);
                if (!dbArtistResult.IsSuccess)
                {
                    dbArtistResult = await artistService.GetByNameNormalized(artist.NormalizedName, cancellationToken).ConfigureAwait(false);
                }

                var dbArtist = dbArtistResult.Data;
                if (!dbArtistResult.IsSuccess || dbArtist == null)
                {
                    dbArtistsToAdd.Add(new dbModels.Artist
                    {
                        CreatedAt = _now,
                        MediaUniqueId = artist.UniqueId,
                        MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess,
                        Name = artist.Name,
                        NameNormalized = artist.NormalizedName,
                        SortName = artist.SortName
                    });
                }
            }

            if (dbArtistsToAdd.Count > 0)
            {
                await scopedContext.Artists.AddRangeAsync(dbArtistsToAdd, cancellationToken).ConfigureAwait(false);
                await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                _totalArtistsInserted += dbArtistsToAdd.Count;
                _dbArtistsIdsModifiedOrUpdated.AddRange(dbArtistsToAdd.Select(x => x.Id));
                UpdateDataMap();
            }

            var artistsToUpdate = (from a in artists
                    join addedArtist in dbArtistsToAdd on a.UniqueId equals addedArtist.MediaUniqueId into aa
                    from artist in aa.DefaultIfEmpty()
                    where artist is null
                    select a)
                .ToArray();
            foreach (var artist in artistsToUpdate)
            {
                var dbArtistResult = await artistService.GetByMediaUniqueId(artist.UniqueId, cancellationToken).ConfigureAwait(false);
                if (!dbArtistResult.IsSuccess)
                {
                    dbArtistResult = await artistService.GetByNameNormalized(artist.NormalizedName, cancellationToken).ConfigureAwait(false);
                }

                if (dbArtistResult.Data!.IsLocked)
                {
                    Logger.Warning("[{JobName}] Skipped processing locked artist [{ArtistId}]", nameof(LibraryProcessJob), dbArtistResult.Data);
                    continue;
                }

                var dbArtist = await scopedContext.Artists.FirstAsync(x => x.Id == dbArtistResult.Data!.Id, cancellationToken).ConfigureAwait(false);
                dbArtist.MediaUniqueId = artist.UniqueId;
                dbArtist.Name = artist.Name;
                dbArtist.NameNormalized = artist.NormalizedName;
                dbArtist.SortName = artist.SortName;
                dbArtist.LastUpdatedAt = _now;
                _dbArtistsIdsModifiedOrUpdated.Add(dbArtist.Id);
            }

            if (artistsToUpdate.Any())
            {
                await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                _totalArtistsUpdated += artistsToUpdate.Length;
                UpdateDataMap();
            }
        }
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
            var contributorForTag = await CreateContributorForSongAndTag(song, tmclTag.Identifier, artistId, albumId, songId, now, null, subRole, null, token);
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
            var isContributorNameSet = contributorName.Nullify()?.CleanStringAsIs() != null;
            var artist = isContributorNameSet ? null : await artistService.GetByNameNormalized(tagValue, cancellationToken).ConfigureAwait(false);
            if ((artist?.IsSuccess ?? false) || isContributorNameSet)
            {
                var artistContributorId = artist?.Data?.Id;
                if (artistContributorId != dbArtist || isContributorNameSet)
                {
                    return new dbModels.Contributor
                    {
                        AlbumId = dbAlbumId,
                        ArtistId = artistContributorId,
                        ContributorName = contributorName,
                        ContributorType = DetermineContributorType(tag),
                        CreatedAt = now,
                        MetaTagIdentifier = SafeParser.ToNumber<int>(tag),
                        Role = role?.CleanStringAsIs() ?? tag.GetEnumDescriptionValue(),
                        SongUniqueId = song.UniqueId,
                        SongId = dbSongId,
                        SubRole = subRole?.CleanStringAsIs()
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

    private static int DetermineContributorType(MetaTagIdentifier tag)
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
