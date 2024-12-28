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
using SmartFormat;
using SearchOption = System.IO.SearchOption;
using dbModels = Melodee.Common.Data.Models;

namespace Melodee.Jobs;

/// <summary>
///     Process non staging and inbound libraries and insert into database metadata found in existing melodee data files.
/// </summary>
[DisallowConcurrentExecution]
public class LibraryInsertJob(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    ILibraryService libraryService,
    ISerializer serializer,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    ArtistService artistService,
    AlbumService albumService,
    AlbumDiscoveryService albumDiscoveryService,
    DirectoryProcessorService directoryProcessorService,
    IImageValidator imageValidator) : JobBase(logger, configurationFactory)
{
    private int _batchSize;
    private IMelodeeConfiguration _configuration = null!;
    private IAlbumValidator _albumValidator = null!;
    private JobDataMap _dataMap = null!;
    private string[] _ignorePerformers = [];
    private string[] _ignoreProduction = [];
    private string[] _ignorePublishers = [];
    private int _maxSongsToProcess;
    private Instant _now;
    private int _totalAlbumsInserted;
    private int _totalArtistsInserted;
    private int _totalSongsInserted;

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
            _albumValidator = new AlbumValidator(_configuration);
            var libraries = await libraryService.ListAsync(new PagedRequest(), context.CancellationToken).ConfigureAwait(false);
            if (!libraries.IsSuccess)
            {
                Logger.Warning("[{JobName}] Unable to get libraries, skipping processing.", nameof(LibraryInsertJob));
                return;
            }

            var imageConvertor = new ImageConvertor(_configuration);

            DirectoryInfo? processingDirectory = null;

            _totalAlbumsInserted = 0;
            _totalArtistsInserted = 0;
            _totalSongsInserted = 0;
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
                Logger.Warning("[{JobName}] Unable to get staging library, skipping processing.", nameof(LibraryInsertJob));
                return;
            }

            var librariesToProcess = libraries.Data.Where(x => x.TypeValue == LibraryType.Library).ToArray();
            _dataMap.Put(JobMapNameRegistry.ScanStatus, ScanStatus.InProcess.ToString());
            OnProcessingEvent?.Invoke(
                this,
                new ProcessingEvent(ProcessingEventType.Start,
                    nameof(LibraryInsertJob),
                    librariesToProcess.Count(),
                    0,
                    "Started library processing libraries."));
            await using (var scopedContext = await contextFactory.CreateDbContextAsync(context.CancellationToken).ConfigureAwait(false))
            {
                foreach (var libraryIndex in librariesToProcess.Select((library, index) => new { library, index }))
                {
                    if (libraryIndex.library.IsLocked)
                    {
                        Logger.Warning("[{JobName}] Skipped processing locked library [{LibraryName}]", nameof(LibraryInsertJob), libraryIndex.library.Name);
                        continue;
                    }

                    if (_totalSongsInserted > _maxSongsToProcess && _maxSongsToProcess > 0)
                    {
                        Logger.Warning("[{JobName}] Maximum Processing Count reached. Stopping processing.", nameof(LibraryInsertJob));
                        break;
                    }

                    var libraryProcessStartTicks = Stopwatch.GetTimestamp();
                    var dirs = new DirectoryInfo(libraryIndex.library.Path).GetDirectories("*", SearchOption.AllDirectories);
                    var lastScanAt = libraryIndex.library.LastScanAt ?? defaultNeverScannedDate;
                    if (_totalSongsInserted > _maxSongsToProcess && _maxSongsToProcess > 0)
                    {
                        Logger.Warning("[{JobName}] Maximum Processing Count reached. Stopping processing.", nameof(LibraryInsertJob));
                        break;
                    }

                    // Get a list of modified directories in the Library; remember a library artists album directory should only contain a single album in Melodee
                    var allDirsForLibrary = dirs.Where(d => d.LastWriteTime >= lastScanAt.ToDateTimeUtc() && d.Name.Length > 3).ToArray();
                    var batches = (allDirsForLibrary.Length + _batchSize - 1) / _batchSize;
                    for (var batch = 0; batch < batches; batch++)
                    {
                        var melodeeFilesForDirectory = new List<Album>();
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
                                            _albumValidator,
                                            songPlugins.ToArray(),
                                            _configuration,
                                            context.CancellationToken)
                                        .ConfigureAwait(false)).Data.Item1.FirstOrDefault();
                                    if (melodeeFile == null)
                                    {
                                        Logger.Warning("[{JobName}] Unable to find Melodee file for directory [{DirName}]", nameof(LibraryInsertJob), dirFileSystemDirectoryInfo);
                                        continue;
                                    }
                                }

                                if (!_albumValidator.ValidateAlbum(melodeeFile).Data.IsValid)
                                {
                                    Logger.Warning("[{JobName}] Invalid Melodee file [{Status}]", nameof(LibraryInsertJob), melodeeFile.ToString());
                                    continue;
                                }

                                melodeeFilesForDirectory.Add(melodeeFile);
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e, "[{JobName}] Error processing directory [{Dir}]", nameof(LibraryInsertJob), processingDirectory);
                            }
                        }

                        var processedArtistsResult = await ProcessArtistsAsync(libraryIndex.library, melodeeFilesForDirectory, context.CancellationToken);
                        if (!processedArtistsResult)
                        {
                            continue;
                        }

                        var processedAlbumsResult = await ProcessAlbumsAsync(melodeeFilesForDirectory, context.CancellationToken);
                        if (!processedAlbumsResult)
                        {
                            continue;
                        }

                        OnProcessingEvent?.Invoke(
                            this,
                            new ProcessingEvent(ProcessingEventType.Processing,
                                nameof(LibraryInsertJob),
                                batches,
                                batch,
                                $"Batch [{batch}] of [{batches}] for library [{libraryIndex.library.Name}]."));
                    }

                    var dbConn = scopedContext.Database.GetDbConnection();
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
                        .ExecuteAsync(sql, new { libraryId = libraryIndex.library.Id })
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
                        .ExecuteAsync(sql, new { libraryId = libraryIndex.library.Id })
                        .ConfigureAwait(false);

                    var dbLibrary = await scopedContext.Libraries.FirstAsync(x => x.Id == libraryIndex.library.Id).ConfigureAwait(false);
                    dbLibrary.LastScanAt = _now;
                    dbLibrary.LastUpdatedAt = _now;
                    var newLibraryScanHistory = new dbModels.LibraryScanHistory
                    {
                        LibraryId = dbLibrary.Id,
                        CreatedAt = _now,
                        DurationInMs = Stopwatch.GetElapsedTime(libraryProcessStartTicks).TotalMilliseconds,
                        FoundAlbumsCount = _totalAlbumsInserted,
                        FoundArtistsCount = _totalArtistsInserted,
                        FoundSongsCount = _totalSongsInserted
                    };
                    scopedContext.LibraryScanHistories.Add(newLibraryScanHistory);
                    await scopedContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);

                    OnProcessingEvent?.Invoke(
                        this,
                        new ProcessingEvent(ProcessingEventType.Processing,
                            nameof(LibraryInsertJob),
                            librariesToProcess.Length,
                            libraryIndex.index,
                            $"Library [{libraryIndex.library.Name}]."));
                }
            }

            _dataMap.Put(JobMapNameRegistry.ScanStatus, ScanStatus.Idle.ToString());
            _dataMap.Put(JobMapNameRegistry.Count, _totalAlbumsInserted + _totalArtistsInserted + _totalSongsInserted);

            OnProcessingEvent?.Invoke(
                this,
                new ProcessingEvent(ProcessingEventType.Stop,
                    nameof(LibraryInsertJob),
                    0,
                    0,
                    "Processed [{0}] albums, [{1}] songs in [{2}]".FormatSmart(_totalAlbumsInserted, _totalSongsInserted, Stopwatch.GetElapsedTime(startTicks))));

            foreach (var message in messagesForJobRun)
            {
                Log.Debug("[{JobName}] Message: [{Message}]", nameof(LibraryInsertJob), message);
            }

            foreach (var exception in exceptionsForJobRun)
            {
                Log.Error(exception, "[{JobName}] Processing Exception", nameof(LibraryInsertJob));
            }

            Log.Debug("ℹ️ [{JobName}] Completed. Processed [{NumberOfAlbumsUpdated}] albums, [{NumberOfSongsUpdated}] songs in [{ElapsedTime}]", nameof(LibraryInsertJob), _totalAlbumsInserted, _totalSongsInserted, Stopwatch.GetElapsedTime(startTicks));
        }
        catch (Exception e)
        {
            Logger.Error(e, "[{JobName}] Processing Exception", nameof(LibraryInsertJob));
        }
    }

    /// <summary>
    ///     For all albums with songs, add to db albums
    /// </summary>
    private async Task<bool> ProcessAlbumsAsync(List<Album> melodeeAlbumsForDirectory, CancellationToken cancellationToken)
    {
        var currentAlbum = melodeeAlbumsForDirectory.FirstOrDefault();
        var currentSong = currentAlbum?.Songs?.FirstOrDefault();
        try
        {
            await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                // var artists = melodeeAlbumsForDirectory
                //     .Select(x => x.Artist)
                //     .Where(x => x.IsValid())
                //     .DistinctBy(x => x.NameNormalized)
                //     .OrderBy(x => x.Name)
                //     .ToArray();

                var dbAlbumsToAdd = new List<dbModels.Album>();
                var stopProcessingAlbum = false;
                foreach (var melodeeAlbum in melodeeAlbumsForDirectory)
                {
                    if (stopProcessingAlbum)
                    {
                        stopProcessingAlbum = false;
                        continue;
                    }

                    currentAlbum = melodeeAlbum;
                    var artistName = melodeeAlbum.Artist.Name.CleanStringAsIs() ?? throw new Exception("Album artist is required.");
                    var artistNormalizedName = artistName.ToNormalizedString() ?? artistName;
                    var dbArtistResult = await artistService.GetByApiKeyAsync(melodeeAlbum.Artist.Id, cancellationToken).ConfigureAwait(false);
                    if (!dbArtistResult.IsSuccess)
                    {
                        dbArtistResult = await artistService.GetByNameNormalized(artistNormalizedName, cancellationToken).ConfigureAwait(false);
                    }

                    var dbArtist = await scopedContext.Artists.FirstOrDefaultAsync(x => x.Id == dbArtistResult.Data!.Id, cancellationToken).ConfigureAwait(false);
                    if (dbArtist == null)
                    {
                        Logger.Warning("Unable to find artist [{ArtistUniqueId}] Artist for album [{AlbumUniqueId}].", melodeeAlbum.Artist.Id, melodeeAlbum.Id);
                        continue;
                    }

                    var albumTitle = melodeeAlbum.AlbumTitle()?.CleanStringAsIs() ?? throw new Exception("Album title is required.");
                    var nameNormalized = albumTitle.ToNormalizedString() ?? albumTitle;
                    if (nameNormalized.Nullify() == null)
                    {
                        Logger.Warning("Album [{Album}] has invalid Album title, unable to generate NameNormalized.", melodeeAlbum);
                        continue;
                    }

                    var dbAlbumResult = await albumService.GetByApiKeyAsync(melodeeAlbum.Id, cancellationToken).ConfigureAwait(false);
                    if (!dbAlbumResult.IsSuccess)
                    {
                        var albumMusicBrainzId = SafeParser.ToGuid(melodeeAlbum.MusicBrainzId);
                        if (albumMusicBrainzId != null)
                        {
                            dbAlbumResult = await albumService.GetByMusicBrainzIdAsync(albumMusicBrainzId.Value, cancellationToken).ConfigureAwait(false);
                        }

                        if (!dbAlbumResult.IsSuccess)
                        {
                            dbAlbumResult = await albumService.GetByArtistIdAndNameNormalized(dbArtist.Id, nameNormalized, cancellationToken).ConfigureAwait(false);
                        }
                    }

                    var dbAlbum = dbAlbumResult.Data;

                    var albumDirectory = melodeeAlbum.AlbumDirectoryName(_configuration.Configuration);
                    if (dbAlbum == null)
                    {
                        var newAlbum = new dbModels.Album
                        {
                            ApiKey = melodeeAlbum.Id,
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
                            MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess,
                            Name = albumTitle,
                            NameNormalized = nameNormalized,
                            OriginalReleaseDate = melodeeAlbum.OriginalAlbumYear() == null ? null : new LocalDate(melodeeAlbum.OriginalAlbumYear()!.Value, 1, 1),
                            ReleaseDate = new LocalDate(melodeeAlbum.AlbumYear() ?? throw new Exception("Album year is required."), 1, 1),
                            SongCount = SafeParser.ToNumber<short>(melodeeAlbum.Songs?.Count() ?? 0),
                            SortName = _configuration.RemoveUnwantedArticles(albumTitle.CleanString(true))
                        };
                        if (dbAlbumsToAdd.Any(x => x.Artist.Id == dbArtist.Id && x.NameNormalized == nameNormalized))
                        {
                            Logger.Warning("For artist [{Artist}] found duplicate album [{Album}]", dbArtist, newAlbum);
                            melodeeAlbum.Directory.AppendPrefix("_duplicate");
                            continue;
                        }

                        Logger.Debug("[{JobName}] Creating new album for ArtistId [{ArtistId}] Id [{Id}] MusicbrainzId [{MusicBrainzId}] NormalizedName [{Name}]",
                            nameof(LibraryInsertJob),
                            dbArtist.Id,
                            melodeeAlbum.Id,
                            melodeeAlbum.MusicBrainzId,
                            nameNormalized);
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
                            if (stopProcessingAlbum)
                            {
                                break;
                            }

                            var songsForDisc = melodeeAlbum.Songs?.Where(x => x.MediaNumber() == disc.DiscNumber).ToArray() ?? [];
                            foreach (var song in songsForDisc)
                            {
                                currentSong = song;
                                var mediaFile = song.File.ToFileInfo(melodeeAlbum.Directory) ?? throw new Exception("Song File is required.");
                                var mediaFileHash = CRC32.Calculate(mediaFile);
                                if (mediaFileHash == null)
                                {
                                    Logger.Warning("[{JobName}] Unable to calculate CRC for Song file [{FileName}",
                                        nameof(LibraryInsertJob), mediaFile.FullName);
                                    stopProcessingAlbum = true;
                                    break;
                                }

                                var songTitle = song.Title()?.CleanStringAsIs() ?? throw new Exception("Song title is required.");
                                var s = new dbModels.Song
                                {
                                    ApiKey = song.Id,
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
                                    Genres = (song.Genre()?.Nullify() ?? melodeeAlbum.Genre()?.Nullify())?.Split('/'),
                                    IsVbr = song.IsVbr(),
                                    Lyrics = song.MetaTagValue<string>(MetaTagIdentifier.UnsynchronisedLyrics)?.CleanStringAsIs() ?? song.MetaTagValue<string>(MetaTagIdentifier.SynchronisedLyrics)?.CleanStringAsIs(),
                                    MusicBrainzId = song.MetaTagValue<Guid?>(MetaTagIdentifier.MusicBrainzId),
                                    PartTitles = song.MetaTagValue<string>(MetaTagIdentifier.SubTitle)?.CleanStringAsIs(),
                                    SortOrder = song.SortOrder,
                                    TitleSort = songTitle.CleanString(true)
                                };
                                disc.Songs.Add(s);

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
                    UpdateDataMap();

                    var dbContributorsToAdd = new List<dbModels.Contributor>();
                    foreach (var dbAlbum in dbAlbumsToAdd)
                    {
                        var melodeeAlbum = melodeeAlbumsForDirectory.First(x => x.Id == dbAlbum.ApiKey);
                        foreach (var song in melodeeAlbum.Songs ?? [])
                        {
                            var dbSong = dbAlbum.Discs.SelectMany(x => x.Songs).FirstOrDefault(x => x.ApiKey == song.Id);
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
            }

            return true;
        }
        catch (Exception e)
        {
            Logger.Error(e, "[{JobName}] [{MethodName}] Processing album [{Album}] song [{Song}]", nameof(LibraryInsertJob), nameof(ProcessAlbumsAsync), currentAlbum, currentSong);
        }

        return false;
    }

    private void UpdateDataMap()
    {
        _dataMap.Put(
            JobMapNameRegistry.Count,
            _totalAlbumsInserted +
            _totalArtistsInserted +
            _totalSongsInserted);
    }

    /// <summary>
    ///     For given albums, add to the db album and db song artists.
    /// </summary>
    private async Task<bool> ProcessArtistsAsync(dbModels.Library library, List<Album> melodeeAlbumsForDirectory, CancellationToken cancellationToken)
    {
        Artist? currentArtist = null;

        try
        {
            await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var artists = melodeeAlbumsForDirectory
                    .Select(x => x.Artist)
                    .Where(x => x.IsValid())
                    .DistinctBy(x => x.NameNormalized)
                    .OrderBy(x => x.Name)
                    .ToArray();
                var dbArtistsToAdd = new List<dbModels.Artist>();
                foreach (var artist in artists)
                {
                    currentArtist = artist;
                    var dbArtistResult = await artistService.GetByApiKeyAsync(artist.Id, cancellationToken).ConfigureAwait(false);
                    if (!dbArtistResult.IsSuccess)
                    {
                        var artistMusicBrainzId = SafeParser.ToGuid(artist.MusicBrainzId);
                        if (artistMusicBrainzId != null)
                        {
                            dbArtistResult = await artistService.GetByMusicBrainzIdAsync(artistMusicBrainzId.Value, cancellationToken).ConfigureAwait(false);
                        }

                        if (!dbArtistResult.IsSuccess)
                        {
                            dbArtistResult = await artistService.GetByNameNormalized(artist.NameNormalized, cancellationToken).ConfigureAwait(false);
                        }
                    }

                    var dbArtist = dbArtistResult.Data;
                    if (!dbArtistResult.IsSuccess || dbArtist == null)
                    {
                        Logger.Debug("[{JobName}] Creating new artist for Id [{Id}] MusicbrainzId [{MusicBrainzId}] NormalizedName [{Name}]",
                            nameof(LibraryInsertJob),
                            artist.Id,
                            artist.MusicBrainzId,
                            artist.NameNormalized);
                        var newArtistDirectory = artist.ToDirectoryName(_configuration.GetValue<int>(SettingRegistry.ProcessingMaximumArtistDirectoryNameLength));
                        dbArtistsToAdd.Add(new dbModels.Artist
                        {
                            ApiKey = artist.Id,
                            Directory = newArtistDirectory,
                            CreatedAt = _now,
                            LibraryId = library.Id,
                            MusicBrainzId = SafeParser.ToGuid(artist.MusicBrainzId),
                            MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess,
                            Name = artist.Name,
                            NameNormalized = artist.NameNormalized,
                            SortName = artist.SortName
                        });
                    }
                }

                if (dbArtistsToAdd.Count > 0)
                {
                    await scopedContext.Artists.AddRangeAsync(dbArtistsToAdd, cancellationToken).ConfigureAwait(false);
                    await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    _totalArtistsInserted += dbArtistsToAdd.Count;
                    UpdateDataMap();
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Logger.Error(e, "[{JobName}] [{MethodName}] error processing artist [{Artist}]", nameof(LibraryInsertJob), nameof(ProcessArtistsAsync), currentArtist);
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
                var contributorForTag = await CreateContributorForSongAndTag(song, contributorTag, albumId, songId, _now, null, null, token);
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
                var contributorForTag = await CreateContributorForSongAndTag(song, tmclTag.Identifier, albumId, songId, _now, subRole, null, token);
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
                var publisherTag = await CreateContributorForSongAndTag(song, MetaTagIdentifier.Publisher, albumId, null, _now, null, publisherName, token);
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
            if (DoMakeContributorForTageTypeAndValue(contributorType, contributorNameValue))
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
                    SongId = dbSongId,
                    SubRole = subRole?.CleanStringAsIs()
                };
            }
        }

        return null;
    }

    private bool DoMakeContributorForTageTypeAndValue(ContributorType type, string? contributorName)
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
