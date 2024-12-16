using System.Diagnostics;
using Dapper;
using IdSharp.Common.Utils;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.MessageBus;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Melodee.Plugins.Conversion.Image;
using Melodee.Plugins.MetaData.Song;
using Melodee.Plugins.Processor;
using Melodee.Plugins.Validation;
using Melodee.Services;
using Melodee.Services.Interfaces;
using Melodee.Services.Models;
using Melodee.Services.Scanning;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Quartz;
using Serilog;
using ServiceStack;
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
    IMelodeeConfigurationFactory configurationFactory,
    ILibraryService libraryService,
    ISerializer serializer,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    ArtistService artistService,
    AlbumService albumService,
    AlbumDiscoveryService albumDiscoveryService,
    DirectoryProcessorService directoryProcessorService,
    ImageConvertor imageConvertor,
    IImageValidator imageValidator) : JobBase(logger, configurationFactory)
{
    private readonly List<int> _dbAlbumIdsModifiedOrUpdated = [];
    private readonly List<int> _dbArtistsIdsModifiedOrUpdated = [];
    private int _batchSize;
    private IMelodeeConfiguration _configuration = null!;
    private JobDataMap _dataMap = null!;
    private int _maxSongsToProcess;
    private Instant _now;
    private int _totalAlbumsInserted;
    private int _totalAlbumsUpdated;
    private int _totalArtistsInserted;
    private int _totalArtistsUpdated;
    private int _totalSongsInserted;
    private int _totalSongsUpdated;
    private string[] _ignorePerformers = [];
    private string[] _ignoreProduction = [];
    private string[] _ignorePublishers = [];

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
        try
        {
            var startTicks = Stopwatch.GetTimestamp();
            _configuration = await ConfigurationFactory.GetConfigurationAsync(context.CancellationToken).ConfigureAwait(false);
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

            _ignorePerformers = MelodeeConfiguration.FromSerializedJsonArrayNormalized(_configuration.Configuration[SettingRegistry.ProcessingIgnoredPerformers], serializer);
            _ignorePublishers = MelodeeConfiguration.FromSerializedJsonArrayNormalized(_configuration.Configuration[SettingRegistry.ProcessingIgnoredPublishers], serializer);
            _ignoreProduction = MelodeeConfiguration.FromSerializedJsonArrayNormalized(_configuration.Configuration[SettingRegistry.ProcessingIgnoredProduction], serializer);

            ISongPlugin[] songPlugins =
            [
                new AtlMetaTag(new MetaTagsProcessor(_configuration, serializer), imageConvertor, imageValidator, _configuration)
            ];
            _now = Instant.FromDateTimeUtc(DateTime.UtcNow);

            _dataMap = context.JobDetail.JobDataMap;
            var defaultNeverScannedDate = Instant.FromDateTimeUtc(DateTime.MinValue.ToUniversalTime());
            var stagingLibrary = await libraryService.GetStagingLibraryAsync(context.CancellationToken).ConfigureAwait(false);
            if (!stagingLibrary.IsSuccess)
            {
                messagesForJobRun.AddRange(stagingLibrary.Messages ?? []);
                exceptionsForJobRun.AddRange(stagingLibrary.Errors ?? []);
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
                                            Enumerable.ToArray(songPlugins),
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
                        OnProcessingEvent?.Invoke(
                            this,
                            new ProcessingEvent(ProcessingEventType.Start,
                                nameof(LibraryProcessJob),
                                librariesToProcess.Count(),
                                0,
                                $"Batch [{batch}] of [{batches}] for library [{library.Name}]."));                        
                    }

                    var processedArtistsResult = await ProcessArtistsAsync(library, melodeeFilesForDirectory, context.CancellationToken);
                    if (!processedArtistsResult)
                    {
                        continue;
                    }

                    var processedAlbumsResult = await ProcessAlbumsAsync(melodeeFilesForDirectory, context.CancellationToken);
                    if (!processedAlbumsResult)
                    {
                        continue;
                    }

                    var batchCount = (_dbAlbumIdsModifiedOrUpdated.Count + _batchSize - 1) / _batchSize;
                    var dbConn = scopedContext.Database.GetDbConnection();
                    for (var updateCountBatch = 0; updateCountBatch < batchCount; updateCountBatch++)
                    {
                        var skipValue = updateCountBatch * _batchSize;
                        var joinedDbIds = string.Join(',', _dbAlbumIdsModifiedOrUpdated.Skip(skipValue).Take(_batchSize));

                        var sql = """
                                  UPDATE "Artists" a
                                  SET "AlbumCount" = (select COUNT(*) from "Albums" where "ArtistId" = a."Id"), "LastUpdatedAt" = NOW()
                                  where "AlbumCount" <> (select COUNT(*) from "Albums" where "ArtistId" = a."Id");
                                                                 
                                  UPDATE "Artists" a
                                  SET "SongCount" = (
                                  	select COUNT(s.*)
                                  	from "Songs" s 
                                  	join "AlbumDiscs" ad on (s."AlbumDiscId" = ad."Id")
                                    join "Albums" aa on (ad."AlbumId" = aa."Id")	
                                  	where aa."ArtistId" = a."Id"
                                  ), "LastUpdatedAt" = NOW()
                                  where "SongCount" <> (
                                  	select COUNT(s.*)
                                  	from "Songs" s 
                                  	join "AlbumDiscs" ad on (s."AlbumDiscId" = ad."Id")
                                    join "Albums" aa on (ad."AlbumId" = aa."Id")	
                                  	where aa."ArtistId" = a."Id"
                                  );

                                  UPDATE "Libraries" l 
                                  set "ArtistCount" = (select count(*) from "Artists" where "LibraryId" = l."Id"),
                                      "AlbumCount" = (select count(aa.*) 
                                      	from "Albums" aa 
                                      	join "Artists" a on (a."Id" = aa."ArtistId") 
                                      	where a."LibraryId" = l."Id"),
                                      "SongCount" = (select count(s.*) 
                                      	from "Songs" s
                                      	join "AlbumDiscs" ad on (s."AlbumDiscId" = ad."Id")
                                      	join "Albums" aa on (ad."AlbumId" = aa."Id") 
                                      	join "Artists" a on (a."Id" = aa."ArtistId") 
                                      	where a."LibraryId" = l."Id"),
                                  	"LastUpdatedAt" = now()
                                  where l."Id" = @libraryId;
                                  """;
                        await dbConn
                            .ExecuteAsync(sql, new { libraryId = library.Id })
                            .ConfigureAwait(false);

                        sql = """
                              with performerSongCounts as (
                              	select c."ArtistId" as id, COUNT(*) as count
                              	from "Contributors" c 
                              	join "Songs" s on (c."SongId" = s."Id")
                                join "AlbumDiscs" ad on (s."AlbumDiscId" = ad."Id")
                                join "Albums" aa on (ad."AlbumId" = aa."Id")
                                join "Artists" a on (a."Id" = aa."ArtistId") 
                                WHERE a."LibraryId" = @libraryId
                              	group by c."ArtistId"
                              )
                              UPDATE "Artists"
                              set "SongCount" = c.count, "LastUpdatedAt" = NOW()
                              from performerSongCounts c
                              where c.id = "Artists"."Id";
                              """;
                        await dbConn
                            .ExecuteAsync(sql, new { libraryId = library.Id })
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
        catch (Exception e)
        {
            Logger.Error(e, "[{JobName}] Processing Exception", nameof(LibraryProcessJob));
        }
    }

    /// <summary>
    ///     For all albums with songs, add/update the db albums
    /// </summary>
    private async Task<bool> ProcessAlbumsAsync(List<Album> melodeeAlbumsForDirectory, CancellationToken cancellationToken)
    {
        var currentAlbum = melodeeAlbumsForDirectory.FirstOrDefault();
        var currentSong = currentAlbum?.Songs?.FirstOrDefault();
        try
        {
            _dbAlbumIdsModifiedOrUpdated.Clear();
            await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbAlbumsToAdd = new List<dbModels.Album>();
                foreach (var melodeeAlbum in melodeeAlbumsForDirectory)
                {
                    currentAlbum = melodeeAlbum;
                    var artistName = melodeeAlbum.Artist.Name.CleanStringAsIs() ?? throw new Exception("Album artist is required.");
                    var artistNormalizedName = artistName.ToNormalizedString() ?? artistName;
                    var dbArtistResult = await artistService.GetByMediaUniqueId(melodeeAlbum.Artist.UniqueId(), cancellationToken).ConfigureAwait(false);
                    if (!dbArtistResult.IsSuccess)
                    {
                        dbArtistResult = await artistService.GetByNameNormalized(artistNormalizedName, cancellationToken).ConfigureAwait(false);
                    }

                    var dbArtist = await scopedContext.Artists.FirstOrDefaultAsync(x => x.Id == dbArtistResult.Data!.Id, cancellationToken).ConfigureAwait(false);
                    if (dbArtist == null)
                    {
                        Logger.Warning("Unable to find artist [{ArtistUniqueId}] Artist for album [{AlbumUniqueId}].", melodeeAlbum.Artist.UniqueId(), melodeeAlbum.UniqueId);
                        continue;
                    }

                    var albumTitle = melodeeAlbum.AlbumTitle()?.CleanStringAsIs() ?? throw new Exception("Album title is required.");
                    var nameNormalized = albumTitle.ToNormalizedString() ?? albumTitle;
                    var dbAlbumResult = await albumService.GetByMediaUniqueId(melodeeAlbum.UniqueId(), cancellationToken).ConfigureAwait(false);
                    if (!dbAlbumResult.IsSuccess)
                    {
                        dbAlbumResult = await albumService.GetByArtistIdAndNameNormalized(dbArtist.Id, nameNormalized, cancellationToken).ConfigureAwait(false);
                    }

                    var dbAlbum = dbAlbumResult.Data;

                    var albumDirectory = melodeeAlbum.AlbumDirectoryName(_configuration.Configuration);
                    if (dbAlbum == null)
                    {
                        var newAlbum = new dbModels.Album
                        {
                            AlbumStatus = (short)melodeeAlbum.Status,
                            AlbumType = (int)AlbumType.Album,
                            Artist = dbArtist,
                            CreatedAt = _now,
                            Directory = albumDirectory,
                            DiscCount = melodeeAlbum.MediaCountValue(),
                            Duration = melodeeAlbum.TotalDuration(),
                            Genres = melodeeAlbum.Genre() == null ? null : melodeeAlbum.Genre()!.Split('/'),
                            IsCompilation = melodeeAlbum.IsVariousArtistTypeAlbum(),
                            MusicBrainzId = SafeParser.ToGuid(melodeeAlbum.MusicBrainzId),
                            MediaUniqueId = melodeeAlbum.UniqueId(),
                            MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess,
                            Name = albumTitle,
                            NameNormalized = nameNormalized,
                            OriginalReleaseDate = melodeeAlbum.OriginalAlbumYear() == null ? null : new LocalDate(melodeeAlbum.OriginalAlbumYear()!.Value, 1, 1),
                            ReleaseDate = new LocalDate(melodeeAlbum.AlbumYear() ?? throw new Exception("Album year is required."), 1, 1),
                            SongCount = SafeParser.ToNumber<short>(melodeeAlbum.Songs?.Count() ?? 0),
                            SortName = _configuration.RemoveUnwantedArticles(albumTitle.CleanString(true))
                        };
                        for (short i = 1; i <= melodeeAlbum.MediaCountValue(); i++)
                        {
                            newAlbum.Discs.Add(new dbModels.AlbumDisc
                            {
                                DiscNumber = i,
                                Title = melodeeAlbum.DiscSubtitle(i),
                                SongCount = SafeParser.ToNumber<short>(melodeeAlbum.Songs?.Where(x => x.MediaNumber() == i).Count() ?? 0)
                            });
                        }

                        foreach (var disc in newAlbum.Discs)
                        {
                            var songsForDisc = melodeeAlbum.Songs?.Where(x => x.MediaNumber() == disc.DiscNumber).ToArray() ?? [];
                            foreach (var song in songsForDisc)
                            {
                                currentSong = song;
                                var mediaFile = song.File.ToFileInfo(melodeeAlbum.Directory) ?? throw new Exception("Song File is required.");
                                var mediaFileHash = CRC32.Calculate(mediaFile);
                                var songTitle = song.Title()?.CleanStringAsIs() ?? throw new Exception("Song title is required.");
                                disc.Songs.Add(new dbModels.Song
                                {
                                    BitDepth = song.BitDepth(),
                                    BitRate = song.BitRate(),
                                    BPM = song.MetaTagValue<int>(MetaTagIdentifier.Bpm),
                                    ContentType = song.ContentType(),
                                    CreatedAt = _now,
                                    Duration = song.Duration() ?? throw new Exception("Song duration is required."),
                                    FileHash = mediaFileHash,
                                    FileName = mediaFile.Name,
                                    FileSize = mediaFile.Length,
                                    SamplingRate = song.SamplingRate(),
                                    Title = songTitle,
                                    TitleNormalized = songTitle.ToNormalizedString() ?? songTitle,
                                    SongNumber = song.SongNumber(),
                                    AlbumDiscId = disc.Id,
                                    ChannelCount = song.ChannelCount(),
                                    Genres = melodeeAlbum.Genre()?.Nullify() == null ? null : song.Genre()!.Split('/'),
                                    IsVbr = song.IsVbr(),
                                    Lyrics = song.MetaTagValue<string>(MetaTagIdentifier.UnsynchronisedLyrics)?.CleanStringAsIs() ?? song.MetaTagValue<string>(MetaTagIdentifier.SynchronisedLyrics)?.CleanStringAsIs(),
                                    MediaUniqueId = song.UniqueId,
                                    MusicBrainzId = song.MetaTagValue<Guid>(MetaTagIdentifier.MusicBrainzId),
                                    PartTitles = song.MetaTagValue<string>(MetaTagIdentifier.SubTitle)?.CleanStringAsIs(),
                                    SortOrder = song.SortOrder,
                                    TitleSort = songTitle.CleanString(true)
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

                    var dbContributorsToAdd = new List<dbModels.Contributor>();
                    foreach (var dbAlbum in dbAlbumsToAdd)
                    {
                        var melodeeAlbum = melodeeAlbumsForDirectory.First(x => x.UniqueId() == dbAlbum.MediaUniqueId);
                        foreach (var song in melodeeAlbum.Songs ?? [])
                        {
                            var dbSong = dbAlbum.Discs.SelectMany(x => x.Songs).FirstOrDefault(x => x.MediaUniqueId == song.UniqueId);
                            if (dbSong != null)
                            {
                                var contributorsForSong = await GetContributorsForSong(song, dbAlbum.ArtistId, dbAlbum.Id, dbSong.Id, cancellationToken);
                                foreach (var cfs in contributorsForSong)
                                {
                                    if (!dbContributorsToAdd.Any(x => x.AlbumId == cfs.AlbumId && 
                                                                                (x.ArtistId == cfs.ArtistId || x.ContributorName == cfs.ContributorName) && 
                                                                                x.MetaTagIdentifier == cfs.MetaTagIdentifier))
                                    {
                                        dbContributorsToAdd.Add(cfs);
                                    }
                                }
                            }
                        }
                    }

                    if (dbContributorsToAdd.Count > 0)
                    {
                        try
                        {
                            await scopedContext.Contributors.AddRangeAsync(dbContributorsToAdd, cancellationToken).ConfigureAwait(false);
                            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e, "Unable to insert album contributors into db.");
                        }
                    }
                }

                var albumsToUpdate = (from a in melodeeAlbumsForDirectory
                        join addedAlbum in dbAlbumsToAdd on a.UniqueId() equals addedAlbum.MediaUniqueId into aa
                        from album in aa.DefaultIfEmpty()
                        where album is null
                        select a)
                    .ToArray();
                foreach (var album in albumsToUpdate)
                {
                    var albumDirectory = album.AlbumDirectoryName(_configuration.Configuration);
                    var artistName = album.Artist.Name.CleanStringAsIs() ?? throw new Exception("Album artist is required.");
                    var artistNormalizedName = artistName.ToNormalizedString() ?? artistName;
                    var dbArtistResult = await artistService.GetByMediaUniqueId(album.Artist.UniqueId(), cancellationToken).ConfigureAwait(false);
                    if (!dbArtistResult.IsSuccess)
                    {
                        dbArtistResult = await artistService.GetByNameNormalized(artistNormalizedName, cancellationToken).ConfigureAwait(false);
                    }

                    var dbArtist = await scopedContext.Artists.FirstOrDefaultAsync(x => x.Id == dbArtistResult.Data!.Id, cancellationToken).ConfigureAwait(false);
                    if (dbArtist == null)
                    {
                        Logger.Warning("Unable to find artist [{ArtistUniqueId}] Artist for album [{AlbumUniqueId}].", album.Artist.UniqueId(), album.UniqueId);
                        continue;
                    }

                    if (dbArtist.IsLocked)
                    {
                        Logger.Warning("[{JobName}] Skipped processing locked artist [{ArtistId}]", nameof(LibraryProcessJob), dbArtist);
                        continue;
                    }

                    var albumTitle = album.AlbumTitle()?.CleanStringAsIs() ?? throw new Exception("Album title is required.");
                    var nameNormalized = albumTitle.ToNormalizedString() ?? albumTitle;
                    var dbAlbumResult = await albumService.GetByMediaUniqueId(album.UniqueId(), cancellationToken).ConfigureAwait(false);
                    if (!dbAlbumResult.IsSuccess)
                    {
                        dbAlbumResult = await albumService.GetByArtistIdAndNameNormalized(dbArtist.Id, nameNormalized, cancellationToken).ConfigureAwait(false);
                    }

                    if (dbAlbumResult.Data == null)
                    {
                        Logger.Warning("Unable to find album [{AlbumUniqueId}] for artist [{ArtistUniqueId}].", album.UniqueId, album.Artist.UniqueId());
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
                    dbAlbum.MediaUniqueId = album.UniqueId();
                    dbAlbum.MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess;
                    dbAlbum.Name = albumTitle;
                    dbAlbum.NameNormalized = albumTitle.ToNormalizedString() ?? albumTitle;
                    dbAlbum.OriginalReleaseDate = album.OriginalAlbumYear() == null ? null : new LocalDate(album.OriginalAlbumYear()!.Value, 1, 1);
                    dbAlbum.ReleaseDate = new LocalDate(album.AlbumYear() ?? throw new Exception("Album year is required."), 1, 1);
                    dbAlbum.SongCount = SafeParser.ToNumber<short>(album.Songs?.Count() ?? 0);
                    dbAlbum.SortName = albumTitle.CleanString(true);
                    dbAlbum.MediaUniqueId = album.UniqueId();
                    dbAlbum.Name = albumTitle;
                    dbAlbum.NameNormalized = nameNormalized;
                    dbAlbum.SortName = albumTitle.CleanString(true);

                    //TODO handle media disc changes (add a new one, removed one, etc.)
                    _dbAlbumIdsModifiedOrUpdated.Add(dbAlbum.Id);
                }

                if (albumsToUpdate.Any())
                {
                    await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    _totalAlbumsUpdated += albumsToUpdate.Length;
                    UpdateDataMap();
                }

                //TODO
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

            return true;
        }
        catch (Exception e)
        {
            Logger.Error(e, "[{JobName}] [{MethodName}] Processing album [{Album}] song [{Song}]", nameof(LibraryProcessJob), nameof(ProcessAlbumsAsync), currentAlbum, currentSong);
        }

        return false;
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
    private async Task<bool> ProcessArtistsAsync(dbModels.Library library, List<Album> melodeeAlbumsForDirectory, CancellationToken cancellationToken)
    {
        Artist? currentArtist = null;

        try
        {
            _dbArtistsIdsModifiedOrUpdated.Clear();
            await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var artists = melodeeAlbumsForDirectory
                    .Select(x => x.Artist)
                    .Where(x => x.IsValid())
                    .Distinct()
                    .OrderBy(x => x.Name)
                    .ToArray();
                var dbArtistsToAdd = new List<dbModels.Artist>();
                foreach (var artist in artists)
                {
                    currentArtist = artist;
                    OperationResult<dbModels.Artist?> dbArtistResult = new OperationResult<dbModels.Artist?> { Data = null };
                    if (artist.MusicBrainzId != null)
                    {
                        dbArtistResult = await artistService.GetByMusicBrainzIdAsync(SafeParser.ToGuid(artist.MusicBrainzId)!.Value, cancellationToken).ConfigureAwait(false);
                    }

                    if (!dbArtistResult.IsSuccess)
                    {
                        dbArtistResult = await artistService.GetByMediaUniqueId(artist.UniqueId(), cancellationToken).ConfigureAwait(false);
                        if (!dbArtistResult.IsSuccess)
                        {
                            dbArtistResult = await artistService.GetByNameNormalized(artist.NameNormalized, cancellationToken).ConfigureAwait(false);
                        }
                    }

                    var dbArtist = dbArtistResult.Data;
                    if (!dbArtistResult.IsSuccess || dbArtist == null)
                    {
                        var newArtistDirectory = artist.ToDirectoryName(_configuration.GetValue<int>(SettingRegistry.ProcessingMaximumArtistDirectoryNameLength));
                        if (dbArtistsToAdd.All(x => x.MediaUniqueId != artist.UniqueId()))
                        {
                            dbArtistsToAdd.Add(new dbModels.Artist
                            {
                                Directory = newArtistDirectory,
                                CreatedAt = _now,
                                LibraryId = library.Id,
                                MediaUniqueId = artist.UniqueId(),
                                MusicBrainzId = SafeParser.ToGuid(artist.MusicBrainzId),
                                MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess,
                                Name = artist.Name,
                                NameNormalized = artist.NameNormalized,
                                SortName = artist.SortName
                            });
                        }
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
                        join addedArtist in dbArtistsToAdd on a.UniqueId() equals addedArtist.MediaUniqueId into aa
                        from artist in aa.DefaultIfEmpty()
                        where artist is null
                        select a)
                    .ToArray();
                foreach (var artist in artistsToUpdate)
                {
                    var dbArtistResult = await artistService.GetByMediaUniqueId(artist.UniqueId(), cancellationToken).ConfigureAwait(false);
                    if (!dbArtistResult.IsSuccess)
                    {
                        dbArtistResult = await artistService.GetByNameNormalized(artist.NameNormalized, cancellationToken).ConfigureAwait(false);
                    }

                    if (dbArtistResult.Data!.IsLocked)
                    {
                        Logger.Warning("[{JobName}] Skipped processing locked artist [{ArtistId}]", nameof(LibraryProcessJob), dbArtistResult.Data);
                        continue;
                    }

                    var dbArtist = await scopedContext.Artists.FirstAsync(x => x.Id == dbArtistResult.Data!.Id, cancellationToken).ConfigureAwait(false);
                    var newArtistDirectory = artist.ToDirectoryName(_configuration.GetValue<int>(SettingRegistry.ProcessingMaximumArtistDirectoryNameLength));
                    if (!string.Equals(newArtistDirectory, dbArtist.Directory, StringComparison.OrdinalIgnoreCase))
                    {
                        // directory has changed then; move artist folder
                        if (Directory.Exists(newArtistDirectory))
                        {
                            Logger.Warning("[{JobName}] Artist [{Artist}] directory [{NewDir}] has changed [{OldDir}] but directory exists. Skipping artist update.",
                                nameof(LibraryProcessJob), artist, newArtistDirectory, dbArtist.Directory);
                            continue;
                        }

                        MediaEditService.MoveDirectory(dbArtist.Directory, newArtistDirectory);
                        dbArtist.Directory = newArtistDirectory;
                    }

                    dbArtist.MediaUniqueId = artist.UniqueId();
                    dbArtist.Name = artist.Name;
                    dbArtist.NameNormalized = artist.NameNormalized;
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

            return true;
        }
        catch (Exception e)
        {
            Logger.Error(e, "[{JobName}] [{MethodName}] error processing artist [{Artist}]", nameof(LibraryProcessJob), nameof(ProcessArtistsAsync), currentArtist);
        }

        return false;
    }

    private async Task<dbModels.Contributor[]> GetContributorsForSong(Song song, int artistId, int albumId, int songId, CancellationToken token)
    {
        var dbContributorsToAdd = new List<dbModels.Contributor>();
        foreach (var contributorTag in ContributorMetaTagIdentifiers)
        {
            var tagValue = song.MetaTagValue<string?>(contributorTag)?.CleanStringAsIs();
            if (!dbContributorsToAdd.Any(x => (x.ArtistId == artistId || x.ContributorName == tagValue) && x.MetaTagIdentifierValue == contributorTag))
            {
                var contributorForTag = await CreateContributorForSongAndTag(song, contributorTag, artistId, albumId, songId, _now, null, null, token);
                if (contributorForTag != null)
                {
                    dbContributorsToAdd.Add(contributorForTag);
                }
            }
        }

        foreach (var tmclTag in song.Tags?.Where(x => x.Value != null && x.Value.ToString()!.StartsWith("TMCL:", StringComparison.OrdinalIgnoreCase)) ?? [])
        {
            var subRole = tmclTag.Value!.ToString()!.Substring(6).Trim();
            var tagValue = song.MetaTagValue<string?>(tmclTag.Identifier)?.CleanStringAsIs();
            if (!dbContributorsToAdd.Any(x => (x.ArtistId == artistId || x.ContributorName == tagValue) &&
                                                         x.MetaTagIdentifierValue == tmclTag.Identifier))
            {
                var contributorForTag = await CreateContributorForSongAndTag(song, tmclTag.Identifier, artistId, albumId, songId, _now, subRole, null, token);
                if (contributorForTag != null)
                {
                    dbContributorsToAdd.Add(contributorForTag);
                }
            }
        }
        var songPublisherTag = song.MetaTagValue<string?>(MetaTagIdentifier.Publisher);
        if (songPublisherTag != null)
        {
            var publisherName = songPublisherTag.CleanStringAsIs();
            if (!dbContributorsToAdd.Any(x => x.ContributorName == publisherName && x.MetaTagIdentifierValue == MetaTagIdentifier.Publisher))
            {
                var publisherTag = await CreateContributorForSongAndTag(song, MetaTagIdentifier.Publisher, artistId, albumId, null, _now, null, publisherName, token);
                if (publisherTag != null && dbContributorsToAdd.All(x => x.ContributorTypeValue != ContributorType.Publisher))
                {
                    dbContributorsToAdd.Add(publisherTag);
                }
            }
        }
        return dbContributorsToAdd.ToArray();
    }

    private async Task<dbModels.Contributor?> CreateContributorForSongAndTag(
        Song song,
        MetaTagIdentifier tag,
        int dbArtist,
        int dbAlbumId,
        int? dbSongId,
        Instant now,
        string? subRole,
        string? contributorName,
        CancellationToken cancellationToken = default)
    {
        var contributorNameValue = contributorName.Nullify()?.CleanStringAsIs() ?? song.MetaTagValue<string?>(tag)?.CleanStringAsIs();
        if (contributorNameValue.Nullify() != null)
        {
            var artist = contributorNameValue == null ? null : await artistService.GetByNameNormalized(contributorNameValue, cancellationToken).ConfigureAwait(false);
            var contributorType = DetermineContributorType(tag);
            if (DoMakeContributorForTageTypeAndValue(contributorType, tag, contributorNameValue))
            {
                return new dbModels.Contributor
                {
                    AlbumId = dbAlbumId,
                    ArtistId = artist?.Data?.Id,
                    ContributorName = contributorNameValue,
                    ContributorType = SafeParser.ToNumber<int>(contributorType),
                    CreatedAt = now,
                    MetaTagIdentifier = SafeParser.ToNumber<int>(tag),
                    Role = tag.GetEnumDescriptionValue(),
                    SongUniqueId = song.UniqueId,
                    SongId = dbSongId,
                    SubRole = subRole?.CleanStringAsIs()
                };
            }
        }
        return null;
    }

    private bool DoMakeContributorForTageTypeAndValue(ContributorType type, MetaTagIdentifier tag, string? contributorName)
    {
        switch (type)
        {
            case ContributorType.Performer:
                if (_ignorePerformers.Contains(contributorName.ToNormalizedString()))
                {
                    return false;
                }

                break;

            case ContributorType.Production:
                if (_ignoreProduction.Contains(contributorName.ToNormalizedString()))
                {
                    return false;
                }

                break;

            case ContributorType.Publisher:
                if (_ignorePublishers.Contains(contributorName.ToNormalizedString()))
                {
                    return false;
                }

                break;
        }

        return true;
    }

    private static ContributorType DetermineContributorType(MetaTagIdentifier tag)
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
                return ContributorType.Performer;

            case MetaTagIdentifier.EncodedBy:
            case MetaTagIdentifier.Engineer:
            case MetaTagIdentifier.Group:
            case MetaTagIdentifier.InterpretedRemixedOrOtherwiseModifiedBy:
            case MetaTagIdentifier.InvolvedPeople:
            case MetaTagIdentifier.Lyricist:
            case MetaTagIdentifier.MixDj:
            case MetaTagIdentifier.MixEngineer:
            case MetaTagIdentifier.Producer:
                return ContributorType.Production;

            case MetaTagIdentifier.Publisher:
                return ContributorType.Publisher;
        }

        return ContributorType.NotSet;
    }
}
