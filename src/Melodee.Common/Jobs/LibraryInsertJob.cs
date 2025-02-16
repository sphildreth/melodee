using System.Collections.Concurrent;
using System.Diagnostics;
using Dapper;
using IdSharp.Common.Utils;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.MessageBus.Events;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Serialization;
using Melodee.Common.Services;
using Melodee.Common.Services.Models;
using Melodee.Common.Services.Scanning;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Quartz;
using Rebus.Bus;
using Serilog;
using SmartFormat;
using SearchOption = System.IO.SearchOption;
using dbModels = Melodee.Common.Data.Models;


namespace Melodee.Common.Jobs;

/// <summary>
///     Process non staging and inbound libraries and insert into database metadata found in existing melodee data files.
/// </summary>
[DisallowConcurrentExecution]
public class LibraryInsertJob(
    ILogger logger,
    IMelodeeConfigurationFactory configurationFactory,
    LibraryService libraryService,
    ISerializer serializer,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    ArtistService artistService,
    AlbumService albumService,
    AlbumDiscoveryService albumDiscoveryService,
    DirectoryProcessorToStagingService directoryProcessorToStagingService,
    IBus bus) : JobBase(logger, configurationFactory)
{
    private IAlbumValidator _albumValidator = null!;
    private int _batchSize;
    private IMelodeeConfiguration _configuration = null!;
    private JobDataMap _dataMap = null!;
    private string _duplicateAlbumPrefix = string.Empty;
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

            var forceMode = SafeParser.ToBoolean(context.Get("ForceMode"));

            DirectoryInfo? processingDirectory = null;

            _totalAlbumsInserted = 0;
            _totalArtistsInserted = 0;
            _totalSongsInserted = 0;
            _maxSongsToProcess = _configuration.GetValue<int?>(SettingRegistry.ProcessingMaximumProcessingCount) ?? 0;
            _batchSize = _configuration.BatchProcessingSize();
            var messagesForJobRun = new List<string>();
            var exceptionsForJobRun = new List<Exception>();

            await albumDiscoveryService.InitializeAsync(_configuration, context.CancellationToken).ConfigureAwait(false);
            await directoryProcessorToStagingService.InitializeAsync(_configuration, context.CancellationToken).ConfigureAwait(false);

            _ignorePerformers = MelodeeConfiguration.FromSerializedJsonArrayNormalized(_configuration.Configuration[SettingRegistry.ProcessingIgnoredPerformers], serializer);
            _ignorePublishers = MelodeeConfiguration.FromSerializedJsonArrayNormalized(_configuration.Configuration[SettingRegistry.ProcessingIgnoredPublishers], serializer);
            _ignoreProduction = MelodeeConfiguration.FromSerializedJsonArrayNormalized(_configuration.Configuration[SettingRegistry.ProcessingIgnoredProduction], serializer);

            _now = Instant.FromDateTimeUtc(DateTime.UtcNow);

            _duplicateAlbumPrefix = _configuration.GetValue<string>(SettingRegistry.ProcessingDuplicateAlbumPrefix) ?? "__duplicate_ ";

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

            var librariesToProcess = libraries.Data.Where(x => x.TypeValue == LibraryType.Storage).ToArray();
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
                    var lastScanAt = forceMode ? defaultNeverScannedDate : libraryIndex.library.LastScanAt ?? defaultNeverScannedDate;
                    if (_totalSongsInserted > _maxSongsToProcess && _maxSongsToProcess > 0)
                    {
                        Logger.Warning("[{JobName}] Maximum Processing Count reached. Stopping processing.", nameof(LibraryInsertJob));
                        break;
                    }

                    var allMelodeeFilesInLibrary = Directory.GetFiles(libraryIndex.library.Path, Album.JsonFileName, SearchOption.AllDirectories);
                    ConcurrentBag<FileInfo> melodeeFilesToProcessForLibrary = new();
                    var lastScanAtUtc = lastScanAt.ToDateTimeUtc();
                    Parallel.ForEach(allMelodeeFilesInLibrary, melodeeFile =>
                    {
                        var f = new FileInfo(melodeeFile);
                        if (f is { Directory: not null, Name.Length: > 3 } && (f.CreationTimeUtc >= lastScanAtUtc || f.LastWriteTimeUtc >= lastScanAtUtc))
                        {
                            melodeeFilesToProcessForLibrary.Add(f);
                        }
                    });
                    if (melodeeFilesToProcessForLibrary.Count == 0)
                    {
                        Logger.Information("[{JobName}] found no melodee files to process for library [{Library}].",
                            nameof(LibraryInsertJob),
                            libraryIndex.library.ToString());
                        continue;
                    }

                    var batches = (melodeeFilesToProcessForLibrary.Count + _batchSize - 1) / _batchSize;
                    Logger.Debug("[{JobName}] Found [{DirName}] melodee files to scan in [{Batches}] batches.",
                        nameof(LibraryInsertJob),
                        melodeeFilesToProcessForLibrary.Count,
                        batches);
                    for (var batch = 0; batch < batches; batch++)
                    {
                        var melodeeAlbumsForDirectory = new List<Album>();
                        foreach (var melodeeFileInfo in melodeeFilesToProcessForLibrary.Skip(_batchSize * batch).Take(_batchSize))
                        {
                            try
                            {
                                processingDirectory = melodeeFileInfo.Directory;

                                var allDirectoryFiles = melodeeFileInfo.Directory!.GetFiles("*", SearchOption.TopDirectoryOnly);
                                var mediaFiles = allDirectoryFiles.Where(x => FileHelper.IsFileMediaType(x.Extension)).ToArray();
                                if (mediaFiles.Length == 0)
                                {
                                    continue;
                                }

                                try
                                {
                                    var melodeeAlbum = await Album.DeserializeAndInitializeAlbumAsync(serializer, melodeeFileInfo.FullName, context.CancellationToken).ConfigureAwait(false);
                                    if (melodeeAlbum == null)
                                    {
                                        Logger.Warning("[{JobName}] Unable to load melodee file [{MelodeeFile}]",
                                            nameof(LibraryInsertJob),
                                            melodeeAlbum?.ToString() ?? melodeeFileInfo.FullName);
                                        continue;
                                    }
                                    var validationResult = _albumValidator.ValidateAlbum(melodeeAlbum);
                                    if (!validationResult.Data.IsValid)
                                    {
                                        Logger.Warning("[{JobName}] Invalid Melodee file [{MelodeeFile}] validation result [{ValidationResult}]",
                                            nameof(LibraryInsertJob),
                                            melodeeAlbum?.ToString() ?? melodeeFileInfo.FullName,
                                            validationResult.Data.ToString());
                                        await bus.SendLocal(new MelodeeAlbumReprocessEvent(melodeeAlbum!.Directory.FullName())).ConfigureAwait(false);
                                        if (File.Exists(melodeeAlbum!.MelodeeDataFileName!))
                                        {
                                            File.Delete(melodeeAlbum!.MelodeeDataFileName!);
                                        }
                                        continue;                                        
                                    }

                                    melodeeAlbumsForDirectory.Add(melodeeAlbum);
                                }
                                catch
                                {
                                    // The melodee data file won't load.
                                    var albumDirectoryToMove = melodeeFileInfo.Directory!.Parent;
                                    if (albumDirectoryToMove != null)
                                    {
                                        var moveDirectoryTo = Path.Combine(stagingLibrary.Data.Path, albumDirectoryToMove.Name);
                                        albumDirectoryToMove.MoveTo(moveDirectoryTo);
                                        var p = Path.Combine(moveDirectoryTo, Album.JsonFileName);
                                        if (File.Exists(p))
                                        {
                                            File.Delete(p);
                                        }
                                        Logger.Warning(
                                            "[{JobName}] Invalid Melodee File. Deleted and moved directory [{From}] to staging [{To}]",
                                            nameof(LibraryInsertJob),
                                            albumDirectoryToMove,
                                            moveDirectoryTo);
                                        await bus.SendLocal(new MelodeeAlbumReprocessEvent(moveDirectoryTo)).ConfigureAwait(false);                                        
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e, "[{JobName}] Error processing directory [{Dir}]", nameof(LibraryInsertJob), processingDirectory);
                            }
                        }

                        var processedArtistsResult = await ProcessArtistsAsync(bus, libraryIndex.library, melodeeAlbumsForDirectory, context.CancellationToken);
                        if (!processedArtistsResult)
                        {
                            continue;
                        }

                        var processedAlbumsResult = await ProcessAlbumsAsync(bus, melodeeAlbumsForDirectory, context.CancellationToken);
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
                                join "Albums" aa on (s."AlbumId" = aa."Id")	
                              	where aa."ArtistId" = a."Id"
                              ), "LastUpdatedAt" = NOW()
                              where "SongCount" <> (
                              	select COUNT(s.*)
                              	from "Songs" s
                                join "Albums" aa on (s."AlbumId" = aa."Id")	
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
                                  	join "Albums" aa on (s."AlbumId" = aa."Id") 
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
                            join "Albums" aa on (s."AlbumId" = aa."Id")
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
    private async Task<bool> ProcessAlbumsAsync(IBus bus, List<Album> melodeeAlbumsForDirectory, CancellationToken cancellationToken)
    {
        var currentAlbum = melodeeAlbumsForDirectory.FirstOrDefault();
        var currentSong = currentAlbum?.Songs?.FirstOrDefault();
        try
        {
            await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbAlbumsToAdd = new List<dbModels.Album>();
                foreach (var melodeeAlbum in melodeeAlbumsForDirectory)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    currentAlbum = melodeeAlbum;
                    var artistName = melodeeAlbum.Artist.Name.CleanStringAsIs() ?? throw new Exception("Album artist is required.");
                    var artistNormalizedName = artistName.ToNormalizedString() ?? artistName;
                    var dbArtistResult = await artistService.FindArtistAsync(melodeeAlbum.Artist.ArtistDbId, melodeeAlbum.Artist.Id, artistNormalizedName, melodeeAlbum.Artist.MusicBrainzId, cancellationToken).ConfigureAwait(false);
                    var dbArtistId = dbArtistResult.Data?.Id;
                    var dbArtist = dbArtistId == null ? null : await scopedContext.Artists.FirstOrDefaultAsync(x => x.Id == dbArtistId, cancellationToken).ConfigureAwait(false);
                    if (dbArtist == null)
                    {
                        Logger.Warning(
                            "Unable to find artist by id [{ArtistDbId}] apikey [{ApiKey}] nameNormalized [{NameNormalized}] musicBrainzId [{MbId}] artist for album [{AlbumUniqueId}].",
                            melodeeAlbum.Artist.ArtistDbId,
                            melodeeAlbum.Artist.Id,
                            artistNormalizedName,
                            melodeeAlbum.Artist.MusicBrainzId,
                            melodeeAlbum.Id);
                        continue;
                    }

                    var albumTitle = melodeeAlbum.AlbumTitle()?.CleanStringAsIs() ?? throw new Exception("Album title is required.");
                    var nameNormalized = albumTitle.ToNormalizedString() ?? albumTitle;
                    if (nameNormalized.Nullify() == null)
                    {
                        Logger.Warning("Album [{Album}] has invalid Album title, unable to generate NameNormalized.", melodeeAlbum);
                        continue;
                    }
                    var dbAlbumResult = await albumService.FindAlbumAsync(dbArtist.Id, melodeeAlbum, cancellationToken).ConfigureAwait(false);
                    var dbAlbum = dbAlbumResult.Data;
                    var albumDirectory = melodeeAlbum.AlbumDirectoryName(_configuration.Configuration);
                    if (dbAlbum != null)
                    {
                        Trace.WriteLine($"[{nameof(LibraryInsertJob)}] Artist [{dbArtist.Id}] Album [{dbAlbum.Name}] already exists in db. Skipping.");
                    }
                    else
                    {
                        var newAlbum = new dbModels.Album
                        {
                            AlbumStatus = (short)melodeeAlbum.Status,
                            AlbumType = SafeParser.ToNumber<short>(melodeeAlbum.AlbumType),
                            AmgId = melodeeAlbum.AmgId,
                            ApiKey = melodeeAlbum.Id,
                            Artist = dbArtist,
                            CreatedAt = _now,
                            Directory = albumDirectory,
                            DiscogsId = melodeeAlbum.DiscogsId,
                            Duration = melodeeAlbum.TotalDuration(),
                            Genres = melodeeAlbum.Genre() == null ? null : melodeeAlbum.Genre()!.Split('/'),
                            IsCompilation = melodeeAlbum.IsVariousArtistTypeAlbum(),
                            ItunesId = melodeeAlbum.ItunesId,
                            LastFmId = melodeeAlbum.LastFmId,
                            MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess,
                            MusicBrainzId = SafeParser.ToGuid(melodeeAlbum.MusicBrainzId),
                            Name = albumTitle,
                            NameNormalized = nameNormalized,
                            OriginalReleaseDate = melodeeAlbum.OriginalAlbumYear() == null ? null : SafeParser.ToLocalDate(melodeeAlbum.OriginalAlbumYear()!.Value),
                            ReleaseDate = SafeParser.ToLocalDate(melodeeAlbum.AlbumYear() ?? throw new Exception("Album year is required.")),
                            SongCount = SafeParser.ToNumber<short>(melodeeAlbum.Songs?.Count() ?? 0),
                            SortName = _configuration.RemoveUnwantedArticles(albumTitle.CleanString(true)),
                            SpotifyId = melodeeAlbum.SpotifyId,
                            WikiDataId = melodeeAlbum.WikiDataId
                        };
                        if (dbAlbumsToAdd.Any(x => x.Artist.Id == dbArtist.Id && x.NameNormalized == nameNormalized) ||
                            dbAlbumsToAdd.Any(x => x.MusicBrainzId != null && x.MusicBrainzId == newAlbum.MusicBrainzId) ||
                            dbAlbumsToAdd.Any(x => x.SpotifyId != null && x.SpotifyId == newAlbum.SpotifyId))
                        {
                            Logger.Warning("For artist [{Artist}] found duplicate album [{Album}]", dbArtist, newAlbum);
                            melodeeAlbum.Directory.AppendPrefix(_duplicateAlbumPrefix);
                            continue;
                        }

                        Logger.Debug("[{JobName}] Creating new album for ArtistId [{ArtistId}] Id [{Id}] NormalizedName [{Name}]",
                            nameof(LibraryInsertJob),
                            dbArtist.Id,
                            melodeeAlbum.Id,
                            nameNormalized);

                        var newAlbumSongs = new List<dbModels.Song>();
                        foreach (var song in melodeeAlbum.Songs ?? [])
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                newAlbumSongs.Clear();
                                break;
                            }
                            
                            currentSong = song;
                            var mediaFile = song.File.ToFileInfo(melodeeAlbum.Directory);
                            if (!mediaFile.Exists)
                            {
                                newAlbumSongs.Clear();
                                Logger.Warning("[{JobName}] Unable to find media file [{FileName}], deleting metadata album [{Album}] and triggering reprocess event.",
                                    nameof(LibraryInsertJob),
                                    mediaFile.FullName,
                                    melodeeAlbum.MelodeeDataFileName);
                                await bus.SendLocal(new MelodeeAlbumReprocessEvent(melodeeAlbum!.Directory.FullName())).ConfigureAwait(false);
                                if (File.Exists(melodeeAlbum!.MelodeeDataFileName!))
                                {
                                    File.Delete(melodeeAlbum!.MelodeeDataFileName!);
                                }
                                break;
                            }
                            var mediaFileHash = CRC32.Calculate(mediaFile);
                            var songTitle = song.Title()?.CleanStringAsIs() ?? throw new Exception("Song title is required.");
                            var s = new dbModels.Song
                            {
                                AlbumId = newAlbum.Id,
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
                                ChannelCount = song.ChannelCount(),
                                Genres = (song.Genre()?.Nullify() ?? melodeeAlbum.Genre()?.Nullify())?.Split('/'),
                                IsVbr = song.IsVbr(),
                                Lyrics = song.MetaTagValue<string>(MetaTagIdentifier.UnsynchronisedLyrics)?.CleanStringAsIs() ?? song.MetaTagValue<string>(MetaTagIdentifier.SynchronisedLyrics)?.CleanStringAsIs(),
                                MusicBrainzId = song.MetaTagValue<Guid?>(MetaTagIdentifier.MusicBrainzId),
                                PartTitles = song.MetaTagValue<string>(MetaTagIdentifier.SubTitle)?.CleanStringAsIs(),
                                SortOrder = song.SortOrder,
                                TitleSort = songTitle.CleanString(true)
                            };
                            newAlbumSongs.Add(s);
                            _totalSongsInserted++;
                        }
                        if (newAlbumSongs.Any())
                        {
                            newAlbum.Songs = newAlbumSongs;
                            dbAlbumsToAdd.Add(newAlbum);
                        }
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
                            var dbSong = dbAlbum.Songs.FirstOrDefault(x => x.ApiKey == song.Id);
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
    private async Task<bool> ProcessArtistsAsync(IBus bus, dbModels.Library library, List<Album> melodeeAlbumsForDirectory, CancellationToken cancellationToken)
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
                    var dbArtistResult = await artistService.FindArtistAsync(artist.ArtistDbId, artist.Id, artist.NameNormalized, artist.MusicBrainzId, cancellationToken).ConfigureAwait(false);
                    var dbArtistId = dbArtistResult.Data?.Id;
                    var dbArtist = dbArtistId == null ? null : await scopedContext.Artists.FirstOrDefaultAsync(x => x.Id == dbArtistId, cancellationToken).ConfigureAwait(false);
                    if (!dbArtistResult.IsSuccess || dbArtist == null)
                    {
                        Logger.Debug("[{JobName}] Creating new artist for NormalizedName [{Name}]",
                            nameof(LibraryInsertJob),
                            artist.NameNormalized);
                        var newArtistDirectory = artist.ToDirectoryName(_configuration.GetValue<int>(SettingRegistry.ProcessingMaximumArtistDirectoryNameLength));
                        dbArtistsToAdd.Add(new dbModels.Artist
                        {
                            AmgId = artist.AmgId,
                            ApiKey = artist.Id,
                            CreatedAt = _now,
                            Directory = newArtistDirectory,
                            DiscogsId = artist.DiscogsId,
                            ItunesId = artist.ItunesId,
                            LastFmId = artist.LastFmId,
                            LibraryId = library.Id,
                            MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess,
                            MusicBrainzId = SafeParser.ToGuid(artist.MusicBrainzId),
                            Name = artist.Name,
                            NameNormalized = artist.NameNormalized,
                            SortName = artist.SortName,
                            SpotifyId = artist.SpotifyId,
                            WikiDataId = artist.WikiDataId
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
            var artist = contributorNameValue == null ? null : await artistService.GetByNameNormalized(contributorNameValue.ToNormalizedString() ?? contributorName!, cancellationToken).ConfigureAwait(false);
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
