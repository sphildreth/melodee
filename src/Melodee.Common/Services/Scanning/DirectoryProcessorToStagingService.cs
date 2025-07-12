using System.Collections.Concurrent;
using System.Diagnostics;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.SpecialArtists;
using Melodee.Common.Plugins.Conversion;
using Melodee.Common.Plugins.Conversion.Image;
using Melodee.Common.Plugins.Conversion.Media;
using Melodee.Common.Plugins.MetaData.Directory;
using Melodee.Common.Plugins.MetaData.Directory.Nfo;
using Melodee.Common.Plugins.MetaData.Song;
using Melodee.Common.Plugins.Processor;
using Melodee.Common.Plugins.Processor.Models;
using Melodee.Common.Plugins.Scripting;
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Serialization;
using Melodee.Common.Services.Caching;
using Melodee.Common.Services.SearchEngines;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using SixLabors.ImageSharp;
using SmartFormat;
using ImageInfo = Melodee.Common.Models.ImageInfo;

namespace Melodee.Common.Services.Scanning;

/// <summary>
///     Take a given directory and process all the directories in it putting processed files into the staging library.
/// </summary>
public sealed class DirectoryProcessorToStagingService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory,
    LibraryService libraryService,
    ISerializer serializer,
    MediaEditService mediaEditService,
    ArtistSearchEngineService artistSearchEngineService,
    AlbumImageSearchEngineService albumImageSearchEngineService,
    IHttpClientFactory httpClientFactory)
    : ServiceBase(logger, cacheManager, contextFactory), IDisposable
{
    private IAlbumNamesInDirectoryPlugin _albumNamesInDirectoryPlugin = null!;
    private IAlbumValidator _albumValidator = new AlbumValidator(new MelodeeConfiguration([]));
    private IMelodeeConfiguration _configuration = new MelodeeConfiguration([]);

    /// <summary>
    ///     These plugins convert media from various formats into configured formats.
    /// </summary>
    private IEnumerable<IConversionPlugin> _conversionPlugins = [];

    /// <summary>
    ///     These plugins translate various files into albums.
    /// </summary>
    private IEnumerable<IDirectoryPlugin> _directoryPlugins = [];

    private string _directoryStaging = null!;
    private int _duplicateThreshold;
    private ImageConvertor _imageConvertor = new(new MelodeeConfiguration([]));
    private IImageValidator _imageValidator = new ImageValidator(new MelodeeConfiguration([]));
    private bool _initialized;
    private int _maxAlbumProcessingCount;

    /// <summary>
    ///     These plugins create albums from media files.
    /// </summary>
    private IEnumerable<IDirectoryPlugin> _mediaAlbumCreatorPlugins = [];

    private IScriptPlugin _postDiscoveryScript = new NullScript();

    private IScriptPlugin _preDiscoveryScript = new NullScript();

    private ISongPlugin[] _songPlugins = [];

    private bool _stopProcessingTriggered;

    private readonly SemaphoreSlim _processingThrottle = new(Environment.ProcessorCount);

    public async Task InitializeAsync(IMelodeeConfiguration? configuration = null, CancellationToken token = default)
    {
        if (_initialized)
        {
            return;
        }

        _configuration = configuration ?? await configurationFactory.GetConfigurationAsync(token).ConfigureAwait(false);

        _maxAlbumProcessingCount = _configuration.GetValue<int>(SettingRegistry.ProcessingMaximumProcessingCount,
            value => value < 1 ? int.MaxValue : value);

        _duplicateThreshold = _configuration.GetValue<int?>(SettingRegistry.ImagingDuplicateThreshold) ??
                              MelodeeConfiguration.DefaultImagingDuplicateThreshold;

        _directoryStaging = (await libraryService.GetStagingLibraryAsync(token).ConfigureAwait(false)).Data.Path;

        _albumValidator = new AlbumValidator(_configuration);
        _imageValidator = new ImageValidator(_configuration);
        _imageConvertor = new ImageConvertor(_configuration);
        _songPlugins =
        [
            new AtlMetaTag(new MetaTagsProcessor(_configuration, serializer), _imageConvertor, _imageValidator,
                _configuration),
            new IdSharpMetaTag(new MetaTagsProcessor(_configuration, serializer), _configuration)
        ];
        _albumNamesInDirectoryPlugin = new AtlMetaTag(new MetaTagsProcessor(_configuration, serializer),
            _imageConvertor, _imageValidator, _configuration);

        _conversionPlugins =
        [
            new ImageConvertor(_configuration),
            new MediaConvertor(_configuration)
        ];

        _directoryPlugins =
        [
            new CueSheet(serializer, _songPlugins, _albumValidator, _configuration)
            {
                IsEnabled = _configuration.GetValue<bool>(SettingRegistry.PluginEnabledCueSheet)
            },
            new SimpleFileVerification(serializer, _songPlugins, _albumValidator, _configuration)
            {
                IsEnabled = _configuration.GetValue<bool>(SettingRegistry.PluginEnabledSimpleFileVerification)
            },
            new M3UPlaylist(serializer, _songPlugins, _albumValidator, _configuration)
            {
                IsEnabled = _configuration.GetValue<bool>(SettingRegistry.PluginEnabledM3u)
            },
            new Nfo(serializer, _albumValidator, _configuration)
            {
                IsEnabled = _configuration.GetValue<bool>(SettingRegistry.PluginEnabledNfo)
            }
        ];

        _mediaAlbumCreatorPlugins =
        [
            new Mp3Files(_songPlugins, _albumValidator, serializer, Logger, _configuration)
        ];

        var preDiscoveryScript = _configuration.GetValue<string>(SettingRegistry.ScriptingPreDiscoveryScript).Nullify();
        if (preDiscoveryScript != null)
        {
            _preDiscoveryScript = new PreDiscoveryScript(_configuration);
        }

        var postDiscoveryScript =
            _configuration.GetValue<string>(SettingRegistry.ScriptingPostDiscoveryScript).Nullify();
        if (postDiscoveryScript != null)
        {
            _postDiscoveryScript = new PostDiscoveryScript(_configuration);
        }

        await mediaEditService.InitializeAsync(configuration, token).ConfigureAwait(false);
        await artistSearchEngineService.InitializeAsync(configuration, token).ConfigureAwait(false);

        _initialized = true;
    }

    private void CheckInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Directory processor service is not initialized.");
        }
    }

    public async Task<OperationResult<DirectoryProcessorResult>> ProcessDirectoryAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo, Instant? lastProcessDate, int? maxAlbumsToProcess,
        CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var processingMessages = new ConcurrentBag<string>();
        var processingErrors = new ConcurrentBag<Exception>();
        var numberOfAlbumJsonFilesProcessed = 0;
        var numberOfValidAlbumsProcessed = 0;
        var numberOfAlbumsProcessed = 0;

        var conversionPluginsProcessedFileCount = 0;
        var directoryPluginProcessedFileCount = 0;
        var numberOfAlbumFilesProcessed = 0;

        // Use HashSet for faster lookups and deduplication
        var artistsIdsSeen = new ConcurrentBag<long?>();
        var albumsIdsSeen = new ConcurrentBag<long?>();
        var songsIdsSeen = new ConcurrentBag<Guid>();

        var result = new DirectoryProcessorResult
        {
            DurationInMs = 0,
            NewAlbumsCount = 0,
            NewArtistsCount = 0,
            NewSongsCount = 0,
            NumberOfAlbumFilesProcessed = 0,
            NumberOfConversionPluginsProcessed = 0,
            NumberOfConversionPluginsProcessedFileCount = 0,
            NumberOfDirectoryPluginProcessed = 0,
            NumberOfValidAlbumsProcessed = 0,
            NumberOfAlbumsProcessed = 0
        };

        _maxAlbumProcessingCount = maxAlbumsToProcess ?? _maxAlbumProcessingCount;

        var startTicks = Stopwatch.GetTimestamp();

        // Ensure directory to process exists
        Trace.WriteLine($"Ensuring processing path [{fileSystemDirectoryInfo.Path}] exists...");
        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (!dirInfo.Exists)
        {
            return new OperationResult<DirectoryProcessorResult>
            {
                Errors =
                [
                    new Exception($"Directory [{fileSystemDirectoryInfo}] not found.")
                ],
                Data = result
            };
        }

        // Ensure that staging directory exists
        Trace.WriteLine($"Ensuring staging path [{_directoryStaging}] exists...");
        if (!Directory.Exists(_directoryStaging))
        {
            return new OperationResult<DirectoryProcessorResult>
            {
                Errors =
                [
                    new Exception($"Staging Directory [{_directoryStaging}] not found.")
                ],
                Data = result
            };
        }

        // Run PreDiscovery script
        if (!_configuration.GetValue<bool>(SettingRegistry.ScriptingEnabled) && _preDiscoveryScript.IsEnabled)
        {
            LogAndRaiseEvent(LogEventLevel.Debug, "Executing _preDiscoveryScript [{0}]", null,
                _preDiscoveryScript.DisplayName);
            var preDiscoveryScriptResult = new OperationResult<bool>
            {
                Data = false
            };
            try
            {
                preDiscoveryScriptResult = await _preDiscoveryScript
                    .ProcessAsync(fileSystemDirectoryInfo, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                LogAndRaiseEvent(LogEventLevel.Error, "PreDiscoveryScript [{0}]", e, _preDiscoveryScript.DisplayName);
                preDiscoveryScriptResult.AddError(e);
            }

            if (!preDiscoveryScriptResult.IsSuccess)
            {
                return new OperationResult<DirectoryProcessorResult>(preDiscoveryScriptResult.Messages)
                {
                    Errors = preDiscoveryScriptResult.Errors,
                    Data = result
                };
            }
        }

        foreach (var dirName in Directory.EnumerateDirectories(fileSystemDirectoryInfo.Path))
        {
            if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
            {
                break;
            }

            if (Path.GetExtension(dirName).Nullify() != null)
            {
                var newDirName = dirName.Replace(".", "_");
                if (newDirName != dirName)
                {
                    Directory.Move(dirName, newDirName);
                    Logger.Debug("[{Name}] renamed directory from [{Old}] to [{New}]",
                        nameof(DirectoryProcessorToStagingService),
                        dirName,
                        newDirName);
                }
            }
        }

        var directoriesToProcess = fileSystemDirectoryInfo
            .GetFileSystemDirectoryInfosToProcess(lastProcessDate, SearchOption.AllDirectories).ToList();
        if (directoriesToProcess.Count > 0)
        {
            OnProcessingStart?.Invoke(this, directoriesToProcess.Count);
            LogAndRaiseEvent(LogEventLevel.Debug, "\u251c Found [{0}] directories to process", null,
                directoriesToProcess.Count);
        }

        // Process directories in parallel with controlled concurrency
        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Environment.ProcessorCount // Limit parallel execution to CPU core count
        };

        try
        {
            await Parallel.ForEachAsync(directoriesToProcess, parallelOptions, async (directoryInfoToProcess, ct) =>
            {
                // Check for cancellation before acquiring semaphore
                ct.ThrowIfCancellationRequested();
                
                await _processingThrottle.WaitAsync(ct);
                try
                {
                    await ProcessSingleDirectoryAsync(directoryInfoToProcess, processingMessages, processingErrors,
                        artistsIdsSeen, albumsIdsSeen, songsIdsSeen, ct);
                }
                finally
                {
                    _processingThrottle.Release();
                    OnDirectoryProcessed?.Invoke(this, directoryInfoToProcess);
                }
            });
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation gracefully - this is expected behavior
            LogAndRaiseEvent(LogEventLevel.Debug, "Processing was cancelled");
        }

        // Run PostDiscovery script
        if (!_configuration.GetValue<bool>(SettingRegistry.ScriptingEnabled) && _postDiscoveryScript.IsEnabled)
        {
            var postDiscoveryScriptResult = new OperationResult<bool>
            {
                Data = false
            };
            try
            {
                postDiscoveryScriptResult = await _postDiscoveryScript
                    .ProcessAsync(fileSystemDirectoryInfo, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                LogAndRaiseEvent(LogEventLevel.Error, "PostDiscoveryScript [{0}]", e, _postDiscoveryScript.DisplayName);
                postDiscoveryScriptResult.AddError(e);
            }

            if (!postDiscoveryScriptResult.IsSuccess)
            {
                return new OperationResult<DirectoryProcessorResult>(postDiscoveryScriptResult.Messages)
                {
                    Errors = postDiscoveryScriptResult.Errors,
                    Data = result
                };
            }
        }

        fileSystemDirectoryInfo.DeleteAllEmptyDirectories();

        LogAndRaiseEvent(LogEventLevel.Debug, "Processing Complete!");

        return new OperationResult<DirectoryProcessorResult>(processingMessages)
        {
            Errors = processingErrors.ToArray(),
            Data = new DirectoryProcessorResult
            {
                DurationInMs = Stopwatch.GetElapsedTime(startTicks).TotalMilliseconds,
                NewAlbumsCount = albumsIdsSeen.Distinct().Count(),
                NewArtistsCount = artistsIdsSeen.Distinct().Count(),
                NewSongsCount = songsIdsSeen.Distinct().Count(),
                NumberOfAlbumFilesProcessed = numberOfAlbumJsonFilesProcessed,
                NumberOfConversionPluginsProcessed = numberOfAlbumFilesProcessed,
                NumberOfConversionPluginsProcessedFileCount = conversionPluginsProcessedFileCount,
                NumberOfDirectoryPluginProcessed = directoryPluginProcessedFileCount,
                NumberOfValidAlbumsProcessed = numberOfValidAlbumsProcessed,
                NumberOfAlbumsProcessed = numberOfAlbumsProcessed
            }
        };
    }

    /// <summary>
    ///     This is raised when a Log event happens to return activity to caller.
    /// </summary>
    public event EventHandler<string>? OnProcessingEvent;

    /// <summary>
    ///     This is raised when the number of directories to process is known.
    /// </summary>
    public event EventHandler<int>? OnProcessingStart;

    /// <summary>
    ///     This is raised when a new Album is processed put into the Staging directory.
    /// </summary>
    public event EventHandler<FileSystemDirectoryInfo>? OnDirectoryProcessed;

    private void LogAndRaiseEvent(LogEventLevel logLevel, string messageTemplate, Exception? exception = null,
        params object[] args)
    {
        if (exception != null)
        {
            Log.Error(exception, messageTemplate, args);
        }
        else
        {
            Log.Write(logLevel, messageTemplate, args);
        }

        var eventMessage = messageTemplate;
        if (args.Length > 0)
        {
            try
            {
                eventMessage = Smart.Format(eventMessage, args);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
        }

        OnProcessingEvent?.Invoke(this, exception?.ToString() ?? eventMessage);
    }

    public void Dispose()
    {
        _processingThrottle.Dispose();
    }

    private async Task ProcessSingleDirectoryAsync(
        FileSystemDirectoryInfo directoryInfoToProcess,
        ConcurrentBag<string> processingMessages,
        ConcurrentBag<Exception> processingErrors,
        ConcurrentBag<long?> artistsIdsSeen,
        ConcurrentBag<long?> albumsIdsSeen,
        ConcurrentBag<Guid> songsIdsSeen,
        CancellationToken cancellationToken)
    {
        // Add performance monitoring
        using var operation = Operation.At(LogEventLevel.Debug)
            .Time("ProcessSingleDirectoryAsync for directory [{DirectoryName}]", directoryInfoToProcess.Name);
        
        Trace.WriteLine($"DirectoryInfoToProcess: [{directoryInfoToProcess}]");
        try
        {
            var dontDeleteExistingMelodeeFiles = _configuration.GetValue<bool>(SettingRegistry.ProcessingDontDeleteExistingMelodeeDataFiles);
            
            if (!dontDeleteExistingMelodeeFiles)
            {
                // Optimized batch delete operations
                var melodeeFiles = directoryInfoToProcess.MelodeeJsonFiles().Select(f => f.FullName).ToList();
                if (melodeeFiles.Count > 0)
                {
                    var deletedCount = await OptimizedFileOperations.DeleteFilesAsync(melodeeFiles, cancellationToken)
                        .ConfigureAwait(false);
                    LogAndRaiseEvent(LogEventLevel.Debug, "Deleted [{0}] existing Melodee files", null, deletedCount);
                }
            }

            if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
            {
                return;
            }

            // Use optimized file enumeration for memory efficiency
            var fileCount = 0;
            await foreach (var fileInfo in OptimizedFileOperations.EnumerateFilesAsync(
                directoryInfoToProcess.Path, "*", SearchOption.TopDirectoryOnly, cancellationToken))
            {
                fileCount++;
                if (fileCount > 10000) break; // Prevent counting very large directories
            }

            LogAndRaiseEvent(LogEventLevel.Debug, "\u251c Processing [{0}] Number of files to process [{1}]", null,
                directoryInfoToProcess.Name, fileCount);

            // Run all enabled IDirectoryPlugins to convert MetaData files into Album json files.
            // e.g. Build Album json file for M3U or NFO or SFV, etc.
            foreach (var plugin in _directoryPlugins.Where(x => x.IsEnabled).OrderBy(x => x.SortOrder))
            {
                if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                {
                    break;
                }

                var pluginResult = await plugin.ProcessDirectoryAsync(directoryInfoToProcess, cancellationToken)
                    .ConfigureAwait(false);
                if (!pluginResult.IsSuccess && pluginResult.Type != OperationResponseType.NotFound)
                {
                    // ConcurrentBag doesn't have AddRange, so add items individually
                    if (pluginResult.Errors != null)
                    {
                        foreach (var error in pluginResult.Errors)
                        {
                            processingErrors.Add(error);
                        }
                    }
                    if (pluginResult.Messages != null)
                    {
                        foreach (var message in pluginResult.Messages)
                        {
                            processingMessages.Add(message);
                        }
                    }
                    if (plugin.StopProcessing)
                    {
                        Logger.Debug("Received stop processing from [{PluginName}] on Directory [{DirectoryName}]",
                            plugin.DisplayName, directoryInfoToProcess);
                        break;
                    }

                    continue;
                }

                if (plugin.StopProcessing)
                {
                    Logger.Debug("Received stop processing from [{PluginName}] on Directory [{DirectoryName}]",
                        plugin.DisplayName, directoryInfoToProcess);
                    break;
                }
            }

            // Run Enabled Conversion scripts on each file in directory
            // e.g. Convert FLAC to MP3, Convert non JPEG files into JPEGs, etc.
            await foreach (var fileInfo in OptimizedFileOperations.EnumerateFilesAsync(
                directoryInfoToProcess.Path, "*", SearchOption.TopDirectoryOnly, cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                {
                    break;
                }

                var fsi = fileInfo.ToFileSystemInfo();
                foreach (var plugin in _conversionPlugins.Where(x => x.IsEnabled).OrderBy(x => x.SortOrder))
                {
                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                    {
                        break;
                    }

                    if (plugin.DoesHandleFile(directoryInfoToProcess, fsi))
                    {
                        var pluginResult = await plugin
                            .ProcessFileAsync(directoryInfoToProcess, fsi, cancellationToken).ConfigureAwait(false);
                        if (!pluginResult.IsSuccess)
                        {
                            // ConcurrentBag doesn't have AddRange, so add items individually
                            if (pluginResult.Errors != null)
                            {
                                foreach (var error in pluginResult.Errors)
                                {
                                    processingErrors.Add(error);
                                }
                            }
                            if (pluginResult.Messages != null)
                            {
                                foreach (var message in pluginResult.Messages)
                                {
                                    processingMessages.Add(message);
                                }
                            }
                        }
                    }

                    if (plugin.StopProcessing)
                    {
                        break;
                    }
                }
            }

            // If no albums were created by previous plugins, create from media files
            if (!directoryInfoToProcess.MelodeeJsonFiles().Any())
            {
                foreach (var plugin in _mediaAlbumCreatorPlugins.Where(x => x.IsEnabled).OrderBy(x => x.SortOrder))
                {
                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                    {
                        break;
                    }

                    await plugin.ProcessDirectoryAsync(directoryInfoToProcess, cancellationToken)
                        .ConfigureAwait(false);
                    if (plugin.StopProcessing)
                    {
                        Logger.Debug("Received stop processing from [{PluginName}] on Directory [{DirectoryName}]",
                            plugin.DisplayName, directoryInfoToProcess);
                        break;
                    }
                }
            }

            var albumsForDirectory = new List<Album>();
            foreach (var melodeeJsonFile in directoryInfoToProcess.MelodeeJsonFiles())
            {
                if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                {
                    break;
                }

                try
                {
                    var album = await Album
                        .DeserializeAndInitializeAlbumAsync(serializer, melodeeJsonFile.FullName, cancellationToken)
                        .ConfigureAwait(false);
                    if (album != null)
                    {
                        album.MelodeeDataFileName = melodeeJsonFile.FullName;
                        albumsForDirectory.Add(album);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error loading Album json file [{0}]", melodeeJsonFile.FullName);
                }
            }

            // For each Album json find all image files and add to Album to be moved below to staging directory.
            Trace.WriteLine("Loading images for album...");
            await ProcessAlbumsAsync(directoryInfoToProcess, albumsForDirectory, processingMessages, artistsIdsSeen, albumsIdsSeen, songsIdsSeen, cancellationToken);
        }
        catch (Exception e)
        {
            LogAndRaiseEvent(LogEventLevel.Error, "Processing Directory [{0}]", e,
                directoryInfoToProcess.ToString());
            processingErrors.Add(e);
        }
    }

    private async Task ProcessAlbumsAsync(
        FileSystemDirectoryInfo directoryInfoToProcess,
        List<Album> albumsForDirectory,
        ConcurrentBag<string> processingMessages,
        ConcurrentBag<long?> artistsIdsSeen,
        ConcurrentBag<long?> albumsIdsSeen,
        ConcurrentBag<Guid> songsIdsSeen,
        CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient();
        var numberOfValidAlbumsProcessed = 0;
        var numberOfAlbumsProcessed = 0;

        foreach (var album in albumsForDirectory.Take(_maxAlbumProcessingCount))
        {
            if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
            {
                break;
            }

            try
            {
                album.Images = (await album.FindImages(_albumNamesInDirectoryPlugin, _duplicateThreshold,
                    _imageConvertor, _imageValidator,
                    _configuration.GetValue<bool>(SettingRegistry.ProcessingDoDeleteOriginal),
                    cancellationToken).ConfigureAwait(false)).ToArray();

                album.Artist = new Artist(album.Artist.Name,
                    album.Artist.NameNormalized,
                    album.Artist.SortName,
                    (await album.FindArtistImages(_imageConvertor,
                            _imageValidator,
                            _configuration.GetValue<bool>(SettingRegistry.ProcessingDoDeleteOriginal),
                            _configuration.GetValue<bool>(SettingRegistry.ProcessingDoDeleteOriginal),
                            cancellationToken)
                        .ConfigureAwait(false)).ToArray());

                if (album.IsSoundTrackTypeAlbum() && album.Songs != null)
                {
                    // If the album has different artists and is soundtrack then ensure artist is set to special VariousArtists
                    var songsGroupedByArtist = album.Songs.GroupBy(x => x.AlbumArtist()).ToArray();
                    if (songsGroupedByArtist.Length > 1)
                    {
                        album.Artist = new VariousArtist();
                        foreach (var song in album.Songs)
                        {
                            album.SetSongTagValue(song.Id, MetaTagIdentifier.AlbumArtist, album.Artist.Name);
                        }
                    }
                }
                else if (album.IsOriginalCastTypeAlbum() && album.Songs != null)
                {
                    // If the album has different artists and is Original Cast type then ensure artist is set to special Theater
                    // NOTE: Remember Original Cast Type albums with a single composer/artist is attributed to that composer/artist (e.g. Stephen Schwartz - Wicked)
                    var songsGroupedByArtist = album.Songs.GroupBy(x => x.AlbumArtist()).ToArray();
                    if (songsGroupedByArtist.Length > 1)
                    {
                        album.Artist = new Theater();
                        foreach (var song in album.Songs)
                        {
                            album.SetSongTagValue(song.Id, MetaTagIdentifier.AlbumArtist, album.Artist.Name);
                        }
                    }
                }

                var albumDirectorySystemInfo = new FileSystemDirectoryInfo
                {
                    Path = Path.Combine(_directoryStaging, album.ToDirectoryName()),
                    Name = album.ToDirectoryName()
                };
                albumDirectorySystemInfo.EnsureExists();

                // Collect all file operations for batch processing
                var filesToCopy = new List<(string source, string destination)>();
                var deleteOriginal = _configuration.GetValue<bool>(SettingRegistry.ProcessingDoDeleteOriginal);

                // Prepare image file operations
                var albumImagesToMove = album.Images?.Where(x => x.FileInfo?.OriginalName != null) ?? [];
                var artistImageToMove = album.Artist.Images?.Where(x => x.FileInfo?.OriginalName != null) ?? [];
                
                foreach (var image in albumImagesToMove.Concat(artistImageToMove).OrderBy(x => x.SortOrder))
                {
                    var oldImageFileName = Path.Combine((image.DirectoryInfo ?? album.Directory).FullName(),
                        image.FileInfo!.OriginalName!);
                    if (!File.Exists(oldImageFileName))
                    {
                        Logger.Warning("Unable to find image by original name [{OriginalName}]",
                            oldImageFileName);
                        continue;
                    }

                    var newImageFileName = Path.Combine(albumDirectorySystemInfo.FullName(), image.FileInfo.Name);
                    if (!string.Equals(oldImageFileName, newImageFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        filesToCopy.Add((oldImageFileName, newImageFileName));
                        image.FileInfo!.Name = Path.GetFileName(newImageFileName);
                    }
                }

                // Prepare song file operations
                if (album.Songs != null)
                {
                    foreach (var song in album.Songs.Where(x => x.File.OriginalName != null))
                    {
                        if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                        {
                            break;
                        }

                        if (song.File.OriginalName != null)
                        {
                            var oldSongFilename = Path.Combine(album.OriginalDirectory.FullName(),
                                song.File.OriginalName!);
                            if (!File.Exists(oldSongFilename))
                            {
                                continue;
                            }

                            var newSongFileName = Path.Combine(albumDirectorySystemInfo.FullName(),
                                song.ToSongFileName(albumDirectorySystemInfo));
                            if (!string.Equals(oldSongFilename, newSongFileName,
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                filesToCopy.Add((oldSongFilename, newSongFileName));
                                song.File.Name = Path.GetFileName(newSongFileName);
                            }
                        }
                    }
                }

                // Perform batch file operations if any files need to be copied
                if (filesToCopy.Count > 0)
                {
                    using (Operation.At(LogEventLevel.Debug)
                        .Time("Copying [{FileCount}] files for album [{AlbumName}]", filesToCopy.Count, album.AlbumTitle()))
                    {
                        var copiedCount = await OptimizedFileOperations.CopyFilesAsync(
                            filesToCopy, 
                            deleteOriginal, 
                            bufferSize: 2 * 1024 * 1024, // 2MB buffer for media files
                            cancellationToken).ConfigureAwait(false);
                        
                        LogAndRaiseEvent(LogEventLevel.Debug, "Copied [{0}] files for album [{1}]", null, 
                            copiedCount, album.AlbumTitle());
                    }
                }

                if (album.Songs != null)
                {
                    if ((album.Tags ?? []).Any(x => x.WasModified) ||
                        album.Songs!.Any(x => (x.Tags ?? []).Any(y => y.WasModified)))
                    {
                        Trace.WriteLine("Running plugins on songs with modified tags...");

                        foreach (var songPlugin in _songPlugins)
                        {
                            foreach (var song in album.Songs.Where(x =>
                                         x.Tags?.Any(t => t.WasModified) ?? false))
                            {
                                if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                                {
                                    break;
                                }

                                using (Operation.At(LogEventLevel.Debug)
                                           .Time(
                                               "ProcessDirectoryAsync :: Updating song [{Name}] with plugin [{DisplayName}]",
                                               song.File.Name, songPlugin.DisplayName))
                                {
                                    try
                                    {
                                        await songPlugin
                                            .UpdateSongAsync(albumDirectorySystemInfo, song, cancellationToken)
                                            .ConfigureAwait(false);
                                    }
                                    catch (Exception e)
                                    {
                                        Logger.Error(e,
                                            "Error updating song [{Name}] with plugin [{DisplayName}]",
                                            song.File.Name, songPlugin.DisplayName);
                                    }
                                }
                            }
                        }
                    }
                }

                album.Directory = albumDirectorySystemInfo;

                // See if artist can be found using ArtistSearchEngine to populate metadata, set UniqueId and MusicBrainzId
                Trace.WriteLine("Querying for artist...");
                var searchRequest = album.Artist.ToArtistQuery([
                    new KeyValue((album.AlbumYear() ?? 0).ToString(),
                        album.AlbumTitle().ToNormalizedString() ?? album.AlbumTitle())
                ]);
                var artistSearchResult = await artistSearchEngineService.DoSearchAsync(searchRequest,
                        1,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (artistSearchResult.IsSuccess)
                {
                    var artistFromSearch =
                        artistSearchResult.Data.OrderByDescending(x => x.Rank).FirstOrDefault();
                    if (artistFromSearch != null)
                    {
                        album.Artist = album.Artist with
                        {
                            AmgId = album.Artist.AmgId ?? artistFromSearch.AmgId,
                            ArtistDbId = album.Artist.ArtistDbId ?? artistFromSearch.Id,
                            DiscogsId = album.Artist.DiscogsId ?? artistFromSearch.DiscogsId,
                            ItunesId = album.Artist.ItunesId ?? artistFromSearch.ItunesId,
                            LastFmId = album.Artist.LastFmId ?? artistFromSearch.LastFmId,
                            MusicBrainzId = album.Artist.MusicBrainzId ?? artistFromSearch.MusicBrainzId,
                            Name = album.Artist.Name.Nullify() ?? artistFromSearch.Name,
                            NameNormalized = album.Artist.NameNormalized.Nullify() ??
                                             artistFromSearch.Name.ToNormalizedString() ??
                                             artistFromSearch.Name,
                            OriginalName =
                            artistFromSearch.Name != album.Artist.Name ? album.Artist.Name : null,
                            SearchEngineResultUniqueId = album.Artist.SearchEngineResultUniqueId is null or < 1
                                ? artistFromSearch.UniqueId
                                : album.Artist.SearchEngineResultUniqueId,
                            SortName = album.Artist.SortName.Nullify() ?? artistFromSearch.SortName,
                            SpotifyId = album.Artist.SpotifyId ?? artistFromSearch.SpotifyId,
                            WikiDataId = album.Artist.WikiDataId ?? artistFromSearch.WikiDataId
                        };

                        if (artistFromSearch.Releases?.FirstOrDefault() != null)
                        {
                            var searchResultRelease = artistFromSearch.Releases.FirstOrDefault(x =>
                                x.Year == album.AlbumYear() &&
                                x.NameNormalized == album.AlbumTitle().ToNormalizedString());
                            if (searchResultRelease != null)
                            {
                                album.AlbumDbId = album.AlbumDbId ?? searchResultRelease.Id;
                                album.AlbumType = album.AlbumType == AlbumType.NotSet
                                    ? searchResultRelease.AlbumType
                                    : album.AlbumType;

                                // Artist result should override any in place for Album as its more specific and likely more accurate
                                album.MusicBrainzId = searchResultRelease.MusicBrainzId;
                                album.SpotifyId = searchResultRelease.SpotifyId;

                                if (!album.HasValidAlbumYear(_configuration.Configuration))
                                {
                                    album.SetTagValue(MetaTagIdentifier.RecordingYear,
                                        searchResultRelease.Year.ToString());
                                }
                            }
                        }

                        album.Status = AlbumStatus.Ok;

                        LogAndRaiseEvent(LogEventLevel.Debug,
                            $"[{nameof(DirectoryProcessorToStagingService)}] Using artist from search engine query [{searchRequest}] result [{artistFromSearch}]");
                    }
                    else
                    {
                        LogAndRaiseEvent(LogEventLevel.Warning,
                            $"[{nameof(DirectoryProcessorToStagingService)}] No result from search engine for artist [{searchRequest}]");
                    }
                }

                Trace.WriteLine("Testing for album images...");
                // If album has no images then see if ImageSearchEngine can find any
                if (album.Images?.Count() == 0)
                {
                    Trace.WriteLine("Querying for album image...");
                    var albumImageSearchRequest = album.ToAlbumQuery();
                    var albumImageSearchResult = await albumImageSearchEngineService.DoSearchAsync(
                            albumImageSearchRequest,
                            1,
                            cancellationToken)
                        .ConfigureAwait(false);
                    if (albumImageSearchResult.IsSuccess)
                    {
                        var imageSearchResult = albumImageSearchResult.Data.OrderByDescending(x => x.Rank)
                            .FirstOrDefault();
                        if (imageSearchResult != null)
                        {
                            album.AmgId ??= imageSearchResult.AmgId;
                            album.DiscogsId ??= imageSearchResult.DiscogsId;
                            album.ItunesId ??= imageSearchResult.ItunesId;
                            album.LastFmId ??= imageSearchResult.LastFmId;
                            album.SpotifyId ??= imageSearchResult.SpotifyId;
                            album.WikiDataId ??= imageSearchResult.WikiDataId;

                            album.Artist.AmgId ??= imageSearchResult.ArtistAmgId;
                            album.Artist.DiscogsId ??= imageSearchResult.ArtistDiscogsId;
                            album.Artist.ItunesId ??= imageSearchResult.ArtistItunesId;
                            album.Artist.LastFmId ??= imageSearchResult.ArtistLastFmId;
                            album.Artist.SpotifyId ??= imageSearchResult.ArtistSpotifyId;
                            album.Artist.WikiDataId ??= imageSearchResult.ArtistWikiDataId;

                            if (!album.HasValidAlbumYear(_configuration.Configuration) &&
                                imageSearchResult.ReleaseDate != null)
                            {
                                album.SetTagValue(MetaTagIdentifier.RecordingYear,
                                    imageSearchResult.ReleaseDate.ToString());
                            }

                            var albumImageFromSearchFileName = Path.Combine(albumDirectorySystemInfo.FullName(),
                                albumDirectorySystemInfo
                                    .GetNextFileNameForType(Data.Models.Album.FrontImageType).Item1);
                            if (await httpClient.DownloadFileAsync(
                                    imageSearchResult.MediaUrl,
                                    albumImageFromSearchFileName,
                                    async (_, newFileInfo, _) =>
                                        (await _imageValidator.ValidateImage(newFileInfo,
                                            PictureIdentifier.Front, cancellationToken)).Data.IsValid,
                                    cancellationToken).ConfigureAwait(false))
                            {
                                var newImageInfo = new FileInfo(albumImageFromSearchFileName);
                                var imageInfo = await Image
                                    .IdentifyAsync(albumImageFromSearchFileName, cancellationToken)
                                    .ConfigureAwait(false);
                                album.Images = new List<ImageInfo>
                                {
                                    new()
                                    {
                                        FileInfo = newImageInfo.ToFileSystemInfo(),
                                        PictureIdentifier = PictureIdentifier.Front,
                                        CrcHash = Crc32.Calculate(newImageInfo),
                                        Width = imageInfo.Width,
                                        Height = imageInfo.Height,
                                        SortOrder = 1,
                                        WasEmbeddedInSong = false
                                    }
                                };
                                LogAndRaiseEvent(LogEventLevel.Debug,
                                    $"[{nameof(DirectoryProcessorToStagingService)}] Downloaded album image [{imageSearchResult.MediaUrl}]");
                            }
                        }
                        else
                        {
                            LogAndRaiseEvent(LogEventLevel.Warning,
                                $"[{nameof(DirectoryProcessorToStagingService)}] No result from album search engine for album [{albumImageSearchRequest}]");
                        }
                    }
                }

                album.RenumberImages();

                var isMagicEnabled = _configuration.GetValue<bool>(SettingRegistry.MagicEnabled);

                Trace.WriteLine("Validating album...");
                var validationResult = _albumValidator.ValidateAlbum(album);
                album.ValidationMessages = validationResult.Data.Messages ?? [];
                album.Status = validationResult.Data.AlbumStatus;
                album.StatusReasons = validationResult.Data.AlbumStatusReasons;

                var serialized = serializer.Serialize(album);
                var jsonName = album.ToMelodeeJsonName(_configuration, true);
                if (jsonName.Nullify() != null)
                {
                    await File.WriteAllTextAsync(Path.Combine(albumDirectorySystemInfo.FullName(), jsonName),
                        serialized, cancellationToken).ConfigureAwait(false);

                    artistsIdsSeen.Add(album.Artist.ArtistUniqueId());
                    // ConcurrentBag doesn't have AddRange, so add items individually
                    if (album.Songs?.Where(x => x.SongArtistUniqueId() != null) != null)
                    {
                        foreach (var artistId in album.Songs.Where(x => x.SongArtistUniqueId() != null).Select(x => x.SongArtistUniqueId()))
                        {
                            artistsIdsSeen.Add(artistId);
                        }
                    }
                    albumsIdsSeen.Add(album.ArtistAlbumUniqueId());
                    // ConcurrentBag doesn't have AddRange, so add items individually
                    if (album.Songs != null)
                    {
                        foreach (var songId in album.Songs.Select(x => x.Id))
                        {
                            songsIdsSeen.Add(songId);
                        }
                    }

                    var albumCouldBeMagicfied = album;
                    if (isMagicEnabled)
                    {
                        await mediaEditService.DoMagic(album, cancellationToken).ConfigureAwait(false);
                        albumCouldBeMagicfied = await Album.DeserializeAndInitializeAlbumAsync(serializer,
                                Path.Combine(albumDirectorySystemInfo.FullName(), jsonName), cancellationToken)
                            .ConfigureAwait(false) ?? album;
                    }

                    if (albumCouldBeMagicfied.IsValid)
                    {
                        numberOfValidAlbumsProcessed++;
                        LogAndRaiseEvent(LogEventLevel.Debug,
                            $"[{nameof(DirectoryProcessorToStagingService)}] \ud83d\udc4d Found valid album [{albumCouldBeMagicfied}]");
                        if (numberOfValidAlbumsProcessed >= _maxAlbumProcessingCount)
                        {
                            LogAndRaiseEvent(LogEventLevel.Debug,
                                $"[{nameof(DirectoryProcessorToStagingService)}] \ud83d\uded1 Stopped processing directory [{directoryInfoToProcess}], processing.maximumProcessingCount is set to [{_maxAlbumProcessingCount}]");
                            _stopProcessingTriggered = true;
                            break;
                        }
                    }
                    else
                    {
                        LogAndRaiseEvent(LogEventLevel.Debug,
                            $"[{nameof(DirectoryProcessorToStagingService)}] \ud83d\ude3f Found invalid album [{albumCouldBeMagicfied}]");
                    }

                    if (_configuration.GetValue<bool>(SettingRegistry.ProcessingDoDeleteOriginal) &&
                        album.MelodeeDataFileName != null)
                    {
                        File.Delete(album.MelodeeDataFileName);
                    }
                }
                else
                {
                    processingMessages.Add($"Unable to determine JsonName for Album [{album}]");
                }

                numberOfAlbumsProcessed++;
            }
            catch (Exception e)
            {
                LogAndRaiseEvent(LogEventLevel.Error,
                    $"[{nameof(DirectoryProcessorToStagingService)}] Error processing album in directory [{directoryInfoToProcess}]",
                    e);
            }
        }
    }
}
