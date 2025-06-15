using System.Collections.Concurrent;
using System.Diagnostics;
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


    /// <summary>
    ///     This is raised when a Log event happens to return activity to caller.
    /// </summary>
    public event EventHandler<ProcessingEvent>? OnProcessingEvent;

    public override async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var startTicks = Stopwatch.GetTimestamp();
            _configuration = await ConfigurationFactory.GetConfigurationAsync(context.CancellationToken)
                .ConfigureAwait(false);
            _albumValidator = new AlbumValidator(_configuration);
            var libraries = await libraryService.ListAsync(new PagedRequest(), context.CancellationToken)
                .ConfigureAwait(false);
            if (!libraries.IsSuccess)
            {
                Logger.Warning("[{JobName}] Unable to get libraries, skipping processing.", nameof(LibraryInsertJob));
                return;
            }

            var forceMode = SafeParser.ToBoolean(context.Get(MelodeeJobExecutionContext.ForceMode));
            var scanJustDirectory = context.Get(MelodeeJobExecutionContext.ScanJustDirectory)?.ToString();

            DirectoryInfo? processingDirectory = null;

            _totalAlbumsInserted = 0;
            _totalArtistsInserted = 0;
            _totalSongsInserted = 0;
            _maxSongsToProcess = _configuration.GetValue<int?>(SettingRegistry.ProcessingMaximumProcessingCount) ?? 0;
            _batchSize = _configuration.BatchProcessingSize();
            var messagesForJobRun = new List<string>();
            var exceptionsForJobRun = new List<Exception>();

            await albumDiscoveryService.InitializeAsync(_configuration, context.CancellationToken)
                .ConfigureAwait(false);
            await directoryProcessorToStagingService.InitializeAsync(_configuration, context.CancellationToken)
                .ConfigureAwait(false);

            _ignorePerformers = MelodeeConfiguration.FromSerializedJsonArrayNormalized(
                _configuration.Configuration[SettingRegistry.ProcessingIgnoredPerformers], serializer);
            _ignorePublishers = MelodeeConfiguration.FromSerializedJsonArrayNormalized(
                _configuration.Configuration[SettingRegistry.ProcessingIgnoredPublishers], serializer);
            _ignoreProduction = MelodeeConfiguration.FromSerializedJsonArrayNormalized(
                _configuration.Configuration[SettingRegistry.ProcessingIgnoredProduction], serializer);

            _now = Instant.FromDateTimeUtc(DateTime.UtcNow);

            _duplicateAlbumPrefix = _configuration.GetValue<string>(SettingRegistry.ProcessingDuplicateAlbumPrefix) ??
                                    "__duplicate_ ";

            _dataMap = context.JobDetail.JobDataMap;
            var defaultNeverScannedDate = Instant.FromDateTimeUtc(DateTime.MinValue.ToUniversalTime());
            var stagingLibrary =
                await libraryService.GetStagingLibraryAsync(context.CancellationToken).ConfigureAwait(false);
            if (!stagingLibrary.IsSuccess)
            {
                messagesForJobRun.AddRange(stagingLibrary.Messages ?? []);
                exceptionsForJobRun.AddRange(stagingLibrary.Errors ?? []);
                Logger.Warning("[{JobName}] Unable to get staging library, skipping processing.",
                    nameof(LibraryInsertJob));
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

            var totalMelodeeFilesProcessed = 0;
            await using (var scopedContext =
                         await contextFactory.CreateDbContextAsync(context.CancellationToken).ConfigureAwait(false))
            {
                foreach (var libraryIndex in librariesToProcess.Select((library, index) => new { library, index }))
                {
                    if (libraryIndex.library.IsLocked)
                    {
                        Logger.Warning("[{JobName}] Skipped processing locked library [{LibraryName}]",
                            nameof(LibraryInsertJob), libraryIndex.library.Name);
                        continue;
                    }

                    if (_totalSongsInserted > _maxSongsToProcess && _maxSongsToProcess > 0)
                    {
                        Logger.Warning("[{JobName}] Maximum Processing Count reached. Stopping processing.",
                            nameof(LibraryInsertJob));
                        break;
                    }

                    var libraryProcessStartTicks = Stopwatch.GetTimestamp();
                    var lastScanAt = forceMode
                        ? defaultNeverScannedDate
                        : libraryIndex.library.LastScanAt ?? defaultNeverScannedDate;
                    if (_totalSongsInserted > _maxSongsToProcess && _maxSongsToProcess > 0)
                    {
                        Logger.Warning("[{JobName}] Maximum Processing Count reached. Stopping processing.",
                            nameof(LibraryInsertJob));
                        break;
                    }

                    ConcurrentBag<FileInfo> melodeeFilesToProcessForLibrary = new();
                    var lastScanAtUtc = lastScanAt.ToDateTimeUtc();
                    string[] allMelodeeFilesInLibrary = [];
                    if (scanJustDirectory.Nullify() != null)
                    {
                        var scanJustDir = scanJustDirectory!.ToFileSystemDirectoryInfo();
                        if (scanJustDir.Exists())
                        {
                            allMelodeeFilesInLibrary = Directory.GetFiles(scanJustDir.FullName(), Album.JsonFileName,
                                SearchOption.AllDirectories);
                        }
                    }
                    else
                    {
                        allMelodeeFilesInLibrary = Directory.GetFiles(libraryIndex.library.Path, Album.JsonFileName,
                            SearchOption.AllDirectories);
                    }

                    Parallel.ForEach(allMelodeeFilesInLibrary, melodeeFile =>
                    {
                        var f = new FileInfo(melodeeFile);
                        if (f is { Directory: not null, Name.Length: > 3 } && (f.CreationTimeUtc >= lastScanAtUtc ||
                                                                               f.LastWriteTimeUtc >= lastScanAtUtc))
                        {
                            melodeeFilesToProcessForLibrary.Add(f);
                        }
                    });
                    if (melodeeFilesToProcessForLibrary.Count == 0)
                    {
                        Logger.Information("[{JobName}] found no melodee files to process for directory [{PathName}].",
                            nameof(LibraryInsertJob),
                            scanJustDirectory.Nullify() ?? libraryIndex.library.Path);
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
                        foreach (var melodeeFileInfo in melodeeFilesToProcessForLibrary.Skip(_batchSize * batch)
                                     .Take(_batchSize))
                        {
                            try
                            {
                                processingDirectory = melodeeFileInfo.Directory;

                                var allDirectoryFiles =
                                    melodeeFileInfo.Directory!.GetFiles("*", SearchOption.TopDirectoryOnly);
                                var mediaFiles = allDirectoryFiles.Where(x => FileHelper.IsFileMediaType(x.Extension))
                                    .ToArray();
                                if (mediaFiles.Length == 0)
                                {
                                    continue;
                                }

                                try
                                {
                                    var melodeeAlbum = await Album.DeserializeAndInitializeAlbumAsync(serializer,
                                        melodeeFileInfo.FullName, context.CancellationToken).ConfigureAwait(false);
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
                                        Logger.Warning(
                                            "[{JobName}] Invalid Melodee file [{MelodeeFile}] validation result [{ValidationResult}]",
                                            nameof(LibraryInsertJob),
                                            melodeeAlbum?.ToString() ?? melodeeFileInfo.FullName,
                                            validationResult.Data.ToString());
                                        await bus.SendLocal(
                                                new MelodeeAlbumReprocessEvent(melodeeAlbum!.Directory.FullName()))
                                            .ConfigureAwait(false);
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
                                        var moveDirectoryTo = Path.Combine(stagingLibrary.Data.Path,
                                            albumDirectoryToMove.Name);
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
                                        await bus.SendLocal(new MelodeeAlbumReprocessEvent(moveDirectoryTo))
                                            .ConfigureAwait(false);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e, "[{JobName}] Error processing directory [{Dir}]",
                                    nameof(LibraryInsertJob), processingDirectory);
                            }
                        }

                        var processedArtistsResult = await ProcessArtistsAsync(bus, libraryIndex.library,
                            melodeeAlbumsForDirectory, context.CancellationToken);
                        if (!processedArtistsResult)
                        {
                            continue;
                        }

                        var processedAlbumsResult =
                            await ProcessAlbumsAsync(bus, melodeeAlbumsForDirectory, context.CancellationToken);
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

                        totalMelodeeFilesProcessed += melodeeAlbumsForDirectory.Count();
                    }

                    await libraryService.UpdateAggregatesAsync(libraryIndex.library.Id, context.CancellationToken)
                        .ConfigureAwait(false);

                    var newLibraryScanHistory = new dbModels.LibraryScanHistory
                    {
                        LibraryId = libraryIndex.library.Id,
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
                    "Processed [{0}] albums, [{1}] songs in [{2}]".FormatSmart(_totalAlbumsInserted,
                        _totalSongsInserted, Stopwatch.GetElapsedTime(startTicks))));

            foreach (var message in messagesForJobRun)
            {
                Log.Debug("[{JobName}] Message: [{Message}]", nameof(LibraryInsertJob), message);
            }

            foreach (var exception in exceptionsForJobRun)
            {
                Log.Error(exception, "[{JobName}] Processing Exception", nameof(LibraryInsertJob));
            }

            Log.Information(
                "ℹ️ [{JobName}] Completed. Processed [{NumberOfMelodeeAlbumsSeen}] melodee data albums and inserted [{NumberOfAlbumsUpdated}] db albums, [{NumberOfSongsUpdated}] db songs in [{ElapsedTime}]",
                nameof(LibraryInsertJob),
                totalMelodeeFilesProcessed,
                _totalAlbumsInserted,
                _totalSongsInserted,
                Stopwatch.GetElapsedTime(startTicks));
        }
        catch (Exception e)
        {
            Logger.Error(e, "[{JobName}] Processing Exception", nameof(LibraryInsertJob));
        }
    }

    /// <summary>
    ///     For all albums with songs, add to db albums
    /// </summary>
    private async Task<bool> ProcessAlbumsAsync(IBus bus, List<Album> melodeeAlbumsForDirectory,
        CancellationToken cancellationToken)
    {
        var currentAlbum = melodeeAlbumsForDirectory.FirstOrDefault();
        var currentSong = currentAlbum?.Songs?.FirstOrDefault();
        try
        {
            await using (var scopedContext =
                         await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbAlbumsToAdd = new List<dbModels.Album>();
                foreach (var melodeeAlbum in melodeeAlbumsForDirectory)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    currentAlbum = melodeeAlbum;
                    var artistName = melodeeAlbum.Artist.Name.CleanStringAsIs() ??
                                     throw new Exception("Album artist is required.");
                    var artistNormalizedName = artistName.ToNormalizedString() ?? artistName;
                    var dbArtistResult = await artistService.FindArtistAsync(melodeeAlbum.Artist.ArtistDbId,
                        melodeeAlbum.Artist.Id, artistNormalizedName, melodeeAlbum.Artist.MusicBrainzId,
                        melodeeAlbum.Artist.SpotifyId, cancellationToken).ConfigureAwait(false);
                    var dbArtistId = dbArtistResult.Data?.Id;
                    var dbArtist = dbArtistId == null
                        ? null
                        : await scopedContext.Artists.FirstOrDefaultAsync(x => x.Id == dbArtistId, cancellationToken)
                            .ConfigureAwait(false);
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

                    var albumTitle = melodeeAlbum.AlbumTitle()?.CleanStringAsIs() ??
                                     throw new Exception("Album title is required.");
                    var nameNormalized = albumTitle.ToNormalizedString() ?? albumTitle;
                    if (nameNormalized.Nullify() == null)
                    {
                        Logger.Warning("Album [{Album}] has invalid Album title, unable to generate NameNormalized.",
                            melodeeAlbum);
                        continue;
                    }

                    var dbAlbumResult = await albumService.FindAlbumAsync(dbArtist.Id, melodeeAlbum, cancellationToken)
                        .ConfigureAwait(false);
                    var dbAlbum = dbAlbumResult.Data;
                    var albumDirectory = melodeeAlbum.AlbumDirectoryName(_configuration.Configuration);
                    if (dbAlbum != null)
                    {
                        Trace.WriteLine(
                            $"[{nameof(LibraryInsertJob)}] Artist [{dbArtist.Id}] Album [{dbAlbum.Name}] already exists in db. Skipping.");
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
                            ImageCount = melodeeAlbum.Images?.Count(),
                            IsCompilation = melodeeAlbum.IsVariousArtistTypeAlbum(),
                            ItunesId = melodeeAlbum.ItunesId,
                            LastFmId = melodeeAlbum.LastFmId,
                            MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess,
                            MusicBrainzId = SafeParser.ToGuid(melodeeAlbum.MusicBrainzId),
                            Name = albumTitle,
                            NameNormalized = nameNormalized,
                            OriginalReleaseDate = melodeeAlbum.OriginalAlbumYear() == null
                                ? null
                                : SafeParser.ToLocalDate(melodeeAlbum.OriginalAlbumYear()!.Value),
                            ReleaseDate = SafeParser.ToLocalDate(melodeeAlbum.AlbumYear() ??
                                                                 throw new Exception("Album year is required.")),
                            SongCount = SafeParser.ToNumber<short>(melodeeAlbum.Songs?.Count() ?? 0),
                            SortName = _configuration.RemoveUnwantedArticles(albumTitle.CleanString(true)),
                            SpotifyId = melodeeAlbum.SpotifyId,
                            WikiDataId = melodeeAlbum.WikiDataId
                        };
                        if (dbAlbumsToAdd.Any(x => x.Artist.Id == dbArtist.Id && x.NameNormalized == nameNormalized) ||
                            dbAlbumsToAdd.Any(x =>
                                x.MusicBrainzId != null && x.MusicBrainzId == newAlbum.MusicBrainzId) ||
                            dbAlbumsToAdd.Any(x => x.SpotifyId != null && x.SpotifyId == newAlbum.SpotifyId))
                        {
                            Logger.Warning("For artist [{Artist}] found duplicate album [{Album}]", dbArtist, newAlbum);
                            melodeeAlbum.Directory.AppendPrefix(_duplicateAlbumPrefix);
                            continue;
                        }

                        Logger.Debug(
                            "[{JobName}] Creating new album for ArtistId [{ArtistId}] Id [{Id}] NormalizedName [{Name}] Directory [{Directory}]",
                            nameof(LibraryInsertJob),
                            dbArtist.Id,
                            melodeeAlbum.Id,
                            nameNormalized,
                            melodeeAlbum.Directory.FullName());

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
                                Logger.Warning(
                                    "[{JobName}] Unable to find media file [{FileName}], deleting metadata album [{Album}] and triggering reprocess event.",
                                    nameof(LibraryInsertJob),
                                    mediaFile.FullName,
                                    melodeeAlbum.MelodeeDataFileName);
                                await bus.SendLocal(new MelodeeAlbumReprocessEvent(melodeeAlbum!.Directory.FullName()))
                                    .ConfigureAwait(false);
                                if (File.Exists(melodeeAlbum!.MelodeeDataFileName!))
                                {
                                    File.Delete(melodeeAlbum!.MelodeeDataFileName!);
                                }

                                break;
                            }

                            var mediaFileHash = CRC32.Calculate(mediaFile);
                            var songTitle = song.Title()?.CleanStringAsIs() ??
                                            throw new Exception("Song title is required.");
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
                                Lyrics = song.MetaTagValue<string>(MetaTagIdentifier.UnsynchronisedLyrics)
                                             ?.CleanStringAsIs() ??
                                         song.MetaTagValue<string>(MetaTagIdentifier.SynchronisedLyrics)
                                             ?.CleanStringAsIs(),
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
                        await scopedContext.Albums.AddRangeAsync(dbAlbumsToAdd, cancellationToken)
                            .ConfigureAwait(false);
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
                                dbContributorsToAdd.AddRange(await song.GetContributorsForSong(
                                    _now,
                                    artistService,
                                    dbAlbum.ArtistId,
                                    dbAlbum.Id,
                                    dbSong.Id,
                                    _ignorePerformers,
                                    _ignoreProduction,
                                    _ignorePublishers,
                                    cancellationToken));
                            }
                        }

                        if (!dbAlbum.IsCompilation)
                        {
                            // Some Contributor types are one per song and some are one per album.
                            // For the ones that are one per album, ensure there is only one.
                            var uniqueContributors = new HashSet<(string Name, ContributorType Type)>();
                            var contributorsToRemove = new List<dbModels.Contributor>();

                            foreach (var contributor in dbContributorsToAdd.Where(x =>
                                         x.AlbumId == dbAlbum.Id && x.ContributorTypeValue.RestrictToOnePerAlbum()))
                            {
                                var key = (contributor.ContributorName ?? string.Empty, contributor.ContributorTypeValue);

                                if (!uniqueContributors.Add(key))
                                {
                                    // This is a duplicate, so mark it for removal
                                    contributorsToRemove.Add(contributor);
                                }
                            }

                            foreach (var contributor in contributorsToRemove)
                            {
                                dbContributorsToAdd.Remove(contributor);
                            }
                        }

                        // For all contributors that are type RestrictToOnePerAlbum, if every song has the same contributor, then remove all but first and set the first to the album.
                        var songContributorsToRestrictToOnePerAlbum = dbContributorsToAdd
                            .Where(x => x.AlbumId == dbAlbum.Id && x.ContributorTypeValue.RestrictToOnePerAlbum())
                            .GroupBy(x => x.ContributorName);
                        foreach (var songContributorToRestrictToOnePerAlbum in songContributorsToRestrictToOnePerAlbum
                                     .Where(x => x.Count() == dbAlbum.SongCount))
                        {
                            dbContributorsToAdd.RemoveAll(x =>
                                x.AlbumId == dbAlbum.Id && x.ContributorTypeValue.RestrictToOnePerAlbum() &&
                                x.ContributorName == songContributorToRestrictToOnePerAlbum.Key);
                            var firstContributor = songContributorToRestrictToOnePerAlbum.First();
                            dbContributorsToAdd.Add(new dbModels.Contributor
                            {
                                AlbumId = dbAlbum.Id,
                                ArtistId = firstContributor.ArtistId,
                                ContributorName = firstContributor.ContributorName,
                                ContributorType = firstContributor.ContributorType,
                                CreatedAt = _now,
                                Role = firstContributor.Role,
                                SongId = null
                            });
                        }
                    }

                    if (dbContributorsToAdd.Count > 0)
                    {
                        try
                        {
                            await scopedContext.Contributors.AddRangeAsync(dbContributorsToAdd, cancellationToken)
                                .ConfigureAwait(false);
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
            Logger.Error(e, "[{JobName}] [{MethodName}] Processing album [{Album}] song [{Song}]",
                nameof(LibraryInsertJob), nameof(ProcessAlbumsAsync), currentAlbum, currentSong);
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
    private async Task<bool> ProcessArtistsAsync(IBus bus, dbModels.Library library,
        List<Album> melodeeAlbumsForDirectory, CancellationToken cancellationToken)
    {
        Artist? currentArtist = null;
        Artist? lastAddedArtist = null;

        try
        {
            await using (var scopedContext =
                         await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
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
                    var dbArtistResult = await artistService.FindArtistAsync(artist.ArtistDbId, artist.Id,
                            artist.NameNormalized, artist.MusicBrainzId, artist.SpotifyId, cancellationToken)
                        .ConfigureAwait(false);
                    var dbArtistId = dbArtistResult.Data?.Id;
                    var dbArtist = dbArtistId == null
                        ? null
                        : await scopedContext.Artists.FirstOrDefaultAsync(x => x.Id == dbArtistId, cancellationToken)
                            .ConfigureAwait(false);
                    if (!dbArtistResult.IsSuccess || dbArtist == null)
                    {
                        lastAddedArtist = artist;

                        var newArtistDirectory = artist.ToDirectoryName(
                            _configuration.GetValue<int>(SettingRegistry.ProcessingMaximumArtistDirectoryNameLength));

                        Logger.Debug(
                            "[{JobName}] Creating new artist for NormalizedName [{Name}] MusicBrainzId [{MusicBrainzId}] with directory [{Directory}] for albums [{Album}]",
                            nameof(LibraryInsertJob),
                            artist.NameNormalized,
                            artist.MusicBrainzId?.ToString(),
                            newArtistDirectory,
                            string.Empty.AddTags(melodeeAlbumsForDirectory
                                .Where(x => x.Artist.NameNormalized == artist.NameNormalized)
                                .Select(x => x.MelodeeDataFileName)));

                        dbArtistsToAdd.Add(new dbModels.Artist
                        {
                            AmgId = artist.AmgId?.CleanStringAsIs(),
                            ApiKey = artist.Id,
                            CreatedAt = _now,
                            Directory = newArtistDirectory,
                            DiscogsId = artist.DiscogsId?.CleanStringAsIs(),
                            ItunesId = artist.ItunesId?.CleanStringAsIs(),
                            LastFmId = artist.LastFmId?.CleanStringAsIs(),
                            LibraryId = library.Id,
                            MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess,
                            MusicBrainzId = artist.MusicBrainzId,
                            Name = artist.Name.CleanStringAsIs() ?? artist.Name,
                            NameNormalized = artist.NameNormalized,
                            SortName = artist.SortName?.CleanStringAsIs() ?? artist.SortName,
                            SpotifyId = artist.SpotifyId?.CleanStringAsIs(),
                            WikiDataId = artist.WikiDataId?.CleanStringAsIs()
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
            Logger.Error(e, "[{JobName}] [{MethodName}] error processing artist [{Artist}]", nameof(LibraryInsertJob),
                nameof(ProcessArtistsAsync), serializer.Serialize(lastAddedArtist ?? currentArtist));
        }

        return false;
    }
}
