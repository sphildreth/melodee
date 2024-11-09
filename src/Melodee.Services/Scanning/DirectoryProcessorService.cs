using System.Diagnostics;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Melodee.Plugins.Conversion;
using Melodee.Plugins.Conversion.Image;
using Melodee.Plugins.Conversion.Media;
using Melodee.Plugins.MetaData.Directory;
using Melodee.Plugins.MetaData.Song;
using Melodee.Plugins.Processor;
using Melodee.Plugins.Processor.Models;
using Melodee.Plugins.Scripting;
using Melodee.Plugins.Validation;
using Melodee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using SixLabors.ImageSharp;
using SmartFormat;
using ImageInfo = Melodee.Common.Models.ImageInfo;

namespace Melodee.Services.Scanning;

/// <summary>
///     Take a given directory and process all the directories in it.
/// </summary>
public sealed class DirectoryProcessorService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    SettingService settingService,
    LibraryService libraryService,
    ISerializer serializer,
    MediaEditService mediaEditService)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private readonly LibraryService _libraryService = libraryService;
    private readonly MediaEditService _mediaEditService = mediaEditService;
    private bool _initialized;
    private IMelodeeConfiguration _configuration = new MelodeeConfiguration([]);
    private IEnumerable<IConversionPlugin> _conversionPlugins = [];
    private IEnumerable<IDirectoryPlugin> _directoryPlugins = [];

    private IScriptPlugin _preDiscoveryScript = new NullScript();
    private IScriptPlugin _postDiscoveryScript = new NullScript();
    private IAlbumValidator _albumValidator = new AlbumValidator(new MelodeeConfiguration([]));

    private IEnumerable<ISongPlugin> _songPlugins = [];
    private bool _stopProcessingTriggered;

    private string _directoryStaging = null!;

    public async Task InitializeAsync(IMelodeeConfiguration? configuration = null, CancellationToken token = default)
    {
        if (_initialized)
        {
            return;
        }
        _configuration = configuration ?? await settingService.GetMelodeeConfigurationAsync(token).ConfigureAwait(false);

        _directoryStaging = configuration?.GetValue<string?>(SettingRegistry.DirectoryStaging) ?? (await _libraryService.GetStagingLibraryAsync(token)).Data.Path;

        _songPlugins = new[]
        {
            new AtlMetaTag(new MetaTagsProcessor(_configuration, serializer), _configuration)
        };

        _conversionPlugins = new IConversionPlugin[]
        {
            new ImageConvertor(_configuration),
            new MediaConvertor(_configuration)
        };

        _albumValidator = new AlbumValidator(_configuration);

        _directoryPlugins = new IDirectoryPlugin[]
        {
            new CueSheet(_songPlugins, _configuration)
            {
                IsEnabled = _configuration.GetValue<bool>(SettingRegistry.PluginEnabledCueSheet)
            },
            new SimpleFileVerification(_songPlugins, _albumValidator, _configuration)
            {
                IsEnabled = _configuration.GetValue<bool>(SettingRegistry.PluginEnabledSimpleFileVerification)
            },
            new M3UPlaylist(_songPlugins, _albumValidator, _configuration)
            {
                IsEnabled = _configuration.GetValue<bool>(SettingRegistry.PluginEnabledM3u)
            },
            new Nfo(_configuration)
            {
                IsEnabled = _configuration.GetValue<bool>(SettingRegistry.PluginEnabledNfo)
            }
        };
        var preDiscoveryScript = _configuration.GetValue<string>(SettingRegistry.ScriptingPreDiscoveryScript).Nullify();
        if (preDiscoveryScript != null)
        {
            _preDiscoveryScript = new PreDiscoveryScript(_configuration);
        }

        var postDiscoveryScript = _configuration.GetValue<string>(SettingRegistry.ScriptingPostDiscoveryScript).Nullify();
        if (postDiscoveryScript != null)
        {
            _postDiscoveryScript = new PostDiscoveryScript(_configuration);
        }

        await _mediaEditService.InitializeAsync(configuration, token).ConfigureAwait(false);

        _initialized = true;
    }

    private void CheckInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Directory processor service is not initialized.");
        }
    }

    public async Task<OperationResult<DirectoryProcessorResult>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, Instant? lastProcessDate, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var processingMessages = new List<string>();
        var processingErrors = new List<Exception>();
        var numberOfAlbumJsonFilesProcessed = 0;
        var conversionPluginsProcessedFileCount = 0;
        var directoryPluginProcessedFileCount = 0;
        var numberOfAlbumFilesProcessed = 0;

        var artistsUniqueIdsSeen = new List<long>();
        var albumsUniqueIdsSeen = new List<long>();
        var songsUniqueIdsSeen = new List<long>();

        var result = new DirectoryProcessorResult
        {
            NumberOfConversionPluginsProcessed = 0,
            NumberOfDirectoryPluginProcessed = 0,
            NumberOfAlbumFilesProcessed = 0,
            NewArtistsCount = 0,
            NewAlbumsCount = 0,
            NewSongsCount = 0,
            DurationInMs = 0
        };

        var startTicks = Stopwatch.GetTimestamp();

        // Ensure directory to process exists
        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (!dirInfo.Exists)
        {
            return new OperationResult<DirectoryProcessorResult>
            {
                Errors = new[]
                {
                    new Exception($"Directory [{fileSystemDirectoryInfo}] not found.")
                },
                Data = result
            };
        }

        // Ensure that staging directory exists
        if (!Directory.Exists(_directoryStaging))
        {
            return new OperationResult<DirectoryProcessorResult>
            {
                Errors = new[]
                {
                    new Exception($"Staging Directory [{_directoryStaging}] not found.")
                },
                Data = result
            };
        }

        // Run PreDiscovery script
        if (!_configuration.GetValue<bool>(SettingRegistry.ScriptingEnabled) && _preDiscoveryScript.IsEnabled)
        {
            LogAndRaiseEvent(LogEventLevel.Debug, "Executing _preDiscoveryScript [{0}]", null, _preDiscoveryScript.DisplayName);
            var preDiscoveryScriptResult = new OperationResult<bool>
            {
                Data = false
            };
            try
            {
                preDiscoveryScriptResult = await _preDiscoveryScript.ProcessAsync(fileSystemDirectoryInfo, cancellationToken);
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

        var directoriesToProcess = fileSystemDirectoryInfo.GetFileSystemDirectoryInfosToProcess(lastProcessDate, SearchOption.AllDirectories).ToList();
        if (directoriesToProcess.Count > 0)
        {
            OnProcessingStart?.Invoke(this, directoriesToProcess.Count);
            LogAndRaiseEvent(LogEventLevel.Debug, "\u251c Found [{0}] directories to process", null, directoriesToProcess.Count);
        }

        foreach (var directoryInfoToProcess in directoriesToProcess.Take(_configuration.GetValue<int>(SettingRegistry.ProcessingMaximumProcessingCount, value => value < 1 ? int.MaxValue : value)))
        {
            try
            {
                if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                {
                    break;
                }

                var allFilesInDirectory = directoryInfoToProcess.FileInfosForExtension("*").ToArray();

                LogAndRaiseEvent(LogEventLevel.Debug, "\u251c Processing [{0}] Number of files to process [{1}]", null, directoryInfoToProcess.Name, allFilesInDirectory.Length);

                // Run Enabled Conversion scripts on each file in directory
                // e.g. Convert FLAC to MP3, Convert non JPEG files into JPEGs, etc.
                foreach (var fileSystemInfo in allFilesInDirectory)
                {
                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                    {
                        break;
                    }

                    var fsi = fileSystemInfo.ToFileSystemInfo();
                    foreach (var plugin in _conversionPlugins.Where(x => x.IsEnabled).OrderBy(x => x.SortOrder))
                    {
                        if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                        {
                            break;
                        }

                        if (plugin.DoesHandleFile(directoryInfoToProcess, fsi))
                        {
                            using (Operation.At(LogEventLevel.Debug).Time("[{Plugin}] Processing [{File}] ", plugin.DisplayName, fileSystemInfo.Name))
                            {
                                var pluginResult = await plugin.ProcessFileAsync(directoryInfoToProcess, fsi, cancellationToken);
                                if (!pluginResult.IsSuccess)
                                {
                                    return new OperationResult<DirectoryProcessorResult>(pluginResult.Messages)
                                    {
                                        Errors = pluginResult.Errors,
                                        Data = result
                                    };
                                }

                                conversionPluginsProcessedFileCount++;
                            }
                        }

                        if (plugin.StopProcessing)
                        {
                            break;
                        }
                    }
                }

                // Run all enabled IDirectoryPlugins to convert MetaData files into Album json files.
                // e.g. Build Album json file for M3U or NFO or SFV, etc.
                foreach (var plugin in _directoryPlugins.Where(x => x.IsEnabled).OrderBy(x => x.SortOrder))
                {
                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                    {
                        break;
                    }

                    var pluginResult = await plugin.ProcessDirectoryAsync(directoryInfoToProcess, cancellationToken);
                    if (!pluginResult.IsSuccess && pluginResult.Type != OperationResponseType.NotFound)
                    {
                        processingErrors.AddRange(pluginResult.Errors);
                        if (plugin.StopProcessing)
                        {
                            break;
                        }

                        continue;
                    }

                    directoryPluginProcessedFileCount += pluginResult.Data;

                    if (plugin.StopProcessing)
                    {
                        break;
                    }
                }

                // Check if any Album json files exist in given directory, if none then create from Song files.
                var albumJsonFiles = directoryInfoToProcess.FileInfosForExtension(Album.JsonFileName);
                if (!albumJsonFiles.Any())
                {
                    var albumsForDirectory = await AllAlbumsForDirectoryAsync(directoryInfoToProcess, cancellationToken);
                    if (!albumsForDirectory.IsSuccess)
                    {
                        return new OperationResult<DirectoryProcessorResult>(albumsForDirectory.Messages)
                        {
                            Errors = albumsForDirectory.Errors,
                            Data = result
                        };
                    }

                    numberOfAlbumFilesProcessed = albumsForDirectory.Data.Item2;

                    foreach (var albumForDirectory in albumsForDirectory.Data.Item1.ToArray())
                    {
                        if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                        {
                            break;
                        }

                        var mergedAlbum = albumForDirectory;

                        var serialized = string.Empty;
                        var albumDataName = Path.Combine(directoryInfoToProcess.Path, albumForDirectory.ToMelodeeJsonName());
                        if (File.Exists(albumDataName))
                        {
                            if (_configuration.GetValue<bool>(SettingRegistry.ProcessingDoOverrideExistingMelodeeDataFiles))
                            {
                                File.Delete(albumDataName);
                            }
                            else
                            {
                                var existingAlbum = serializer.Deserialize<Album?>(await File.ReadAllTextAsync(albumDataName, cancellationToken));
                                if (existingAlbum != null)
                                {
                                    mergedAlbum = mergedAlbum.Merge(existingAlbum);
                                }
                            }
                        }

                        try
                        {
                            serialized = serializer.Serialize(mergedAlbum);
                        }
                        catch (Exception e)
                        {
                            LogAndRaiseEvent(LogEventLevel.Error, "Error processing [{0}]", e, fileSystemDirectoryInfo);
                        }

                        await File.WriteAllTextAsync(albumDataName, serialized, cancellationToken);
                        numberOfAlbumJsonFilesProcessed++;
                    }
                }

                // Find all Album json files in given directory, if none bail.
                albumJsonFiles = directoryInfoToProcess.FileInfosForExtension(Album.JsonFileName).ToArray();
                if (!albumJsonFiles.Any())
                {
                    return new OperationResult<DirectoryProcessorResult>($"No Albums found in given directory [{directoryInfoToProcess}]")
                    {
                        Data = result
                    };
                }

                // For each Album json find all image files and add to Album to be moved below to staging directory.
                var albumAndJsonFile = new Dictionary<Album, string>();
                foreach (var albumJsonFile in albumJsonFiles)
                {
                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                    {
                        break;
                    }

                    try
                    {
                        var album = serializer.Deserialize<Album>(await File.ReadAllTextAsync(albumJsonFile.FullName, cancellationToken));
                        if (album == null)
                        {
                            return new OperationResult<DirectoryProcessorResult>($"Invalid Album json file [{albumJsonFile.FullName}]")
                            {
                                Data = result
                            };
                        }

                        var albumImages = new List<ImageInfo>();
                        var foundAlbumImages = (await FindImagesForAlbum(album, cancellationToken)).ToArray();
                        if (foundAlbumImages.Length != 0)
                        {
                            foreach (var foundAlbumImage in foundAlbumImages)
                            {
                                if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                                {
                                    break;
                                }

                                if (!albumImages.Any(x => x.IsCrcHashMatch(foundAlbumImage.CrcHash)))
                                {
                                    albumImages.Add(foundAlbumImage);
                                }
                            }
                        }

                        album.Images = albumImages.ToArray();
                        if (album.Songs != null)
                        {
                            // Set SongNumber to invalid range if SongNumber is missing
                            var maximumSongNumber = _configuration.GetValue<int>(SettingRegistry.ValidationMaximumSongNumber);
                            album.Songs.Where(x => x.SongNumber() < 1).Each((x, i) => { album.SetSongTagValue(x.SongId, MetaTagIdentifier.TrackNumber, maximumSongNumber + i + 1); });
                            foreach (var song in album.Songs)
                            {
                                if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                                {
                                    break;
                                }

                                song.File.Name = song.ToSongFileName(_configuration.Configuration);
                            }
                        }

                        albumAndJsonFile.Add(album, albumJsonFile.FullName);
                    }
                    catch (Exception e)
                    {
                        processingErrors.Add(e);
                        LogAndRaiseEvent(LogEventLevel.Error, "Unable to load Album json [{0}]", e, albumJsonFile.FullName);
                    }
                }

                // Create directory and move files for each found Album in staging directory.
                foreach (var albumKvp in albumAndJsonFile)
                {
                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                    {
                        break;
                    }

                    var album = albumKvp.Key;
                    var albumDirInfo = new DirectoryInfo(Path.Combine(_directoryStaging, album.ToDirectoryName()));
                    if (!albumDirInfo.Exists)
                    {
                        albumDirInfo.Create();
                    }

                    album.Images?.Where(x => x.FileInfo != null).Each((image, index) =>
                    {
                        var oldImageFileName = image.FileInfo!.FullOriginalName(directoryInfoToProcess);
                        var newImageFileName = Path.Combine(albumDirInfo.FullName, $"{(index + 1).ToStringPadLeft(2)}-{image.PictureIdentifier}.jpg");
                        if (!string.Equals(oldImageFileName, newImageFileName, StringComparison.OrdinalIgnoreCase))
                        {
                            File.Copy(oldImageFileName, newImageFileName, true);                            
                            if (_configuration.GetValue<bool>(SettingRegistry.ProcessingDoDeleteOriginal))
                            {
                                File.Delete(image.FileInfo!.FullOriginalName(directoryInfoToProcess));
                            }
                        }
                    });

                    if (album.Songs != null)
                    {
                        foreach (var song in album.Songs)
                        {
                            if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                            {
                                break;
                            }

                            var oldSongFilename = song.File.FullOriginalName(directoryInfoToProcess);
                            var newSongFileName = Path.Combine(albumDirInfo.FullName, song.File.Name);
                            if(!string.Equals(oldSongFilename, newSongFileName, StringComparison.OrdinalIgnoreCase))
                            {
                                File.Copy(oldSongFilename, newSongFileName, true);
                                if (_configuration.GetValue<bool>(SettingRegistry.ProcessingDoDeleteOriginal))
                                {
                                    try
                                    {
                                        File.Delete(oldSongFilename);
                                    }
                                    catch (Exception e)
                                    {
                                        Logger.Warning(e, "Error deleting original file [{0}]", oldSongFilename);
                                    }
                                }
                            }
                        }

                        if ((album.Tags ?? Array.Empty<MetaTag<object?>>()).Any(x => x.WasModified) ||
                            album.Songs!.Any(x => (x.Tags ?? Array.Empty<MetaTag<object?>>()).Any(y => y.WasModified)))
                        {
                            var albumDirectorySystemInfo = albumDirInfo.ToDirectorySystemInfo();
                            // Set the value then change to NeedsAttention
                            foreach (var songPlugin in _songPlugins)
                            {
                                foreach (var song in album.Songs)
                                {
                                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                                    {
                                        break;
                                    }

                                    await songPlugin.UpdateSongAsync(albumDirectorySystemInfo, song, cancellationToken);
                                }
                            }

                            album.Status = AlbumStatus.NeedsAttention;
                        }
                    }

                    album.Directory = albumDirInfo.ToDirectorySystemInfo();
                    var validationResult = _albumValidator.ValidateAlbum(album);
                    album.ValidationMessages = validationResult.Data.Messages ?? [];
                    album.Status = validationResult.Data.AlbumStatus;
                    var serialized = serializer.Serialize(album);
                    var jsonName = album.ToMelodeeJsonName(true);
                    if (jsonName.Nullify() != null)
                    {
                        if (_configuration.GetValue<bool>(SettingRegistry.ProcessingDoMoveMelodeeDataFileToStagingDirectory))
                        {
                            await File.WriteAllTextAsync(Path.Combine(albumDirInfo.FullName, jsonName), serialized, cancellationToken);
                            File.Delete(albumKvp.Value);
                        }

                        if (_configuration.GetValue<bool>(SettingRegistry.ProcessingDoDeleteOriginal))
                        {
                            File.Delete(albumKvp.Value);
                        }

                        if (_configuration.GetValue<bool>(SettingRegistry.MagicEnabled))
                        {
                            using (Operation.At(LogEventLevel.Debug).Time("ProcessDirectoryAsync \ud83e\ude84 DoMagic [{DirectoryInfo}]", albumDirInfo.Name))
                            {
                                await _mediaEditService.DoMagic(album.UniqueId, cancellationToken);
                            }
                        }

                        artistsUniqueIdsSeen.Add(album.ArtistUniqueId());
                        artistsUniqueIdsSeen.AddRange(album.Songs?.Where(x => x.SongArtistUniqueId() != null).Select(x => x.SongArtistUniqueId() ?? 0) ?? []);
                        albumsUniqueIdsSeen.Add(album.UniqueId);
                        songsUniqueIdsSeen.AddRange(album.Songs?.Select(x => x.UniqueId) ?? []);
                    }
                    else
                    {
                        processingMessages.Add($"Unable to determine JsonName for Album [{album}]");
                    }
                }
            }
            catch (Exception e)
            {
                LogAndRaiseEvent(LogEventLevel.Error, "Processing Directory [{0}]", e, directoryInfoToProcess.ToString());
                processingErrors.Add(e);
                if (!_configuration.GetValue<bool>(SettingRegistry.ProcessingDoContinueOnDirectoryProcessingErrors))
                {
                    return new OperationResult<DirectoryProcessorResult>
                    {
                        Errors = processingErrors,
                        Data = result
                    };
                }
            }

            OnDirectoryProcessed?.Invoke(this, directoryInfoToProcess);
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
                postDiscoveryScriptResult = await _postDiscoveryScript.ProcessAsync(fileSystemDirectoryInfo, cancellationToken);
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
        LogAndRaiseEvent(LogEventLevel.Information, "Processing Complete!");

        processingMessages.Add($"Directory Plugin(s) process count [{directoryPluginProcessedFileCount}]");
        processingMessages.Add($"Conversion Plugin(s) process count [{conversionPluginsProcessedFileCount}]");
        processingMessages.Add($"Song Plugin(s) process count [{numberOfAlbumFilesProcessed}]");
        processingMessages.Add($"Album process count [{numberOfAlbumJsonFilesProcessed}]");

        return new OperationResult<DirectoryProcessorResult>(processingMessages)
        {
            Errors = processingErrors.ToArray(),
            Data = new DirectoryProcessorResult
            {
                DurationInMs = Stopwatch.GetElapsedTime(startTicks).TotalMilliseconds,
                NewAlbumsCount = albumsUniqueIdsSeen.Distinct().Count(),
                NewArtistsCount = artistsUniqueIdsSeen.Distinct().Count(),
                NewSongsCount = songsUniqueIdsSeen.Distinct().Count(),
                NumberOfAlbumFilesProcessed = numberOfAlbumJsonFilesProcessed,
                NumberOfConversionPluginsProcessed = numberOfAlbumFilesProcessed,
                NumberOfDirectoryPluginProcessed = directoryPluginProcessedFileCount
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

    public void StopProcessing()
    {
        _stopProcessingTriggered = true;
    }

    private void LogAndRaiseEvent(LogEventLevel logLevel, string messageTemplate, Exception? exception = null, params object[] args)
    {
        if (exception != null)
        {
            Log.Write(logLevel, messageTemplate, exception);
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
                Console.WriteLine(e);
            }
        }

        OnProcessingEvent?.Invoke(this, exception?.ToString() ?? eventMessage);
    }

    private static async Task<IEnumerable<ImageInfo>> FindImagesForAlbum(Album album, CancellationToken cancellationToken = default)
    {
        var imageInfos = new List<ImageInfo>();
        var imageFiles = ImageHelper.ImageFilesInDirectory(album.OriginalDirectory.Path, SearchOption.TopDirectoryOnly);
        var index = 0;
        foreach (var imageFile in imageFiles)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var fileInfo = new FileInfo(imageFile);
            if (album.IsFileForAlbum(fileInfo) && !AlbumValidator.IsImageAProofType(imageFile))
            {
                if (ImageHelper.IsAlbumImage(fileInfo) ||
                    ImageHelper.IsArtistImage(fileInfo) ||
                    ImageHelper.IsArtistSecondaryImage(fileInfo) ||
                    ImageHelper.IsAlbumSecondaryImage(fileInfo))
                {
                    var pictureIdentifier = PictureIdentifier.NotSet;
                    if (ImageHelper.IsAlbumImage(fileInfo))
                    {
                        pictureIdentifier = PictureIdentifier.Front;
                    }
                    else if (ImageHelper.IsAlbumSecondaryImage(fileInfo))
                    {
                        pictureIdentifier = PictureIdentifier.SecondaryFront;
                    }
                    else if (ImageHelper.IsArtistImage(fileInfo))
                    {
                        pictureIdentifier = PictureIdentifier.Band;
                    }
                    else if (ImageHelper.IsArtistSecondaryImage(fileInfo))
                    {
                        pictureIdentifier = PictureIdentifier.BandSecondary;
                    }

                    var imageInfo = await Image.LoadAsync(fileInfo.FullName, cancellationToken);
                    var fileInfoFileSystemInfo = fileInfo.ToFileSystemInfo();
                    imageInfos.Add(new ImageInfo
                    {
                        CrcHash = Crc32.Calculate(fileInfo),
                        FileInfo = new FileSystemFileInfo
                        {
                            Name = $"{(index + 1).ToStringPadLeft(2)}-{pictureIdentifier}.jpg",
                            Size = fileInfoFileSystemInfo.Size,
                            OriginalName = fileInfo.Name
                        },
                        PictureIdentifier = pictureIdentifier,
                        Width = imageInfo.Width,
                        Height = imageInfo.Height,
                        SortOrder = 0
                    });
                }

                index++;
            }
        }

        return imageInfos;
    }

    public async Task<OperationResult<(IEnumerable<Album>, int)>> AllAlbumsForDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var albums = new List<Album>();
        var messages = new List<string>();
        var viaPlugins = new List<string>();
        var songs = new List<Song>();

        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (dirInfo.Exists)
        {
            using (Operation.At(LogEventLevel.Debug).Time("AllAlbumsForDirectoryAsync [{directoryInfo}]", fileSystemDirectoryInfo.Name))
            {
                foreach (var fileSystemInfo in dirInfo.EnumerateFileSystemInfos("*.*", SearchOption.TopDirectoryOnly))
                {
                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                    {
                        break;
                    }

                    var fsi = fileSystemInfo.ToFileSystemInfo();
                    foreach (var plugin in _songPlugins.OrderBy(x => x.SortOrder))
                    {
                        if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                        {
                            break;
                        }

                        if (plugin.DoesHandleFile(fileSystemDirectoryInfo, fsi))
                        {
                            using (Operation.At(LogEventLevel.Debug).Time("File [{File}] Plugin [{Plugin}]",
                                       fileSystemInfo.Name, plugin.DisplayName))
                            {
                                var pluginResult = await plugin.ProcessFileAsync(fileSystemDirectoryInfo, fsi, cancellationToken);
                                if (pluginResult.IsSuccess)
                                {
                                    songs.Add(pluginResult.Data);
                                    viaPlugins.Add(plugin.DisplayName);
                                }

                                messages.AddRange(pluginResult.Messages);
                            }
                        }

                        if (plugin.StopProcessing)
                        {
                            break;
                        }
                    }
                }

                foreach (var songsGroupedByAlbum in songs.GroupBy(x => x.AlbumUniqueId))
                {
                    foreach (var song in songsGroupedByAlbum)
                    {
                        if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                        {
                            break;
                        }

                        var foundAlbum = albums.FirstOrDefault(x => x.UniqueId == songsGroupedByAlbum.Key);
                        if (foundAlbum != null)
                        {
                            albums.Remove(foundAlbum);
                            albums.Add(foundAlbum.MergeSongs([song]));
                        }
                        else
                        {
                            var songTotal = song.SongTotalNumber();
                            if (songTotal < 1)
                            {
                                songTotal = songsGroupedByAlbum.Count();
                            }

                            var newAlbumTags = new List<MetaTag<object?>>
                            {
                                new()
                                {
                                    Identifier = MetaTagIdentifier.Album, Value = song.AlbumTitle(), SortOrder = 1
                                },
                                new()
                                {
                                    Identifier = MetaTagIdentifier.AlbumArtist, Value = song.AlbumArtist(), SortOrder = 2
                                },
                                new()
                                {
                                    Identifier = MetaTagIdentifier.DiscNumber, Value = song.MediaNumber(), SortOrder = 4
                                },
                                new()
                                {
                                    Identifier = MetaTagIdentifier.DiscTotal, Value = song.MediaNumber(), SortOrder = 4
                                },
                                new()
                                {
                                    Identifier = MetaTagIdentifier.OrigAlbumYear, Value = song.AlbumYear(),
                                    SortOrder = 100
                                },
                                new() { Identifier = MetaTagIdentifier.SongTotal, Value = songTotal, SortOrder = 101 }
                            };
                            var genres = songsGroupedByAlbum
                                .SelectMany(x => x.Tags ?? Array.Empty<MetaTag<object?>>())
                                .Where(x => x.Identifier == MetaTagIdentifier.Genre);
                            newAlbumTags.AddRange(genres
                                .GroupBy(x => x.Value)
                                .Select((genre, i) => new MetaTag<object?>
                                {
                                    Identifier = MetaTagIdentifier.Genre,
                                    Value = genre.Key,
                                    SortOrder = 5 + i
                                }));
                            albums.Add(new Album
                            {
                                Images = songsGroupedByAlbum.Where(x => x.Images != null)
                                    .SelectMany(x => x.Images!)
                                    .DistinctBy(x => x.CrcHash).ToArray(),
                                OriginalDirectory = fileSystemDirectoryInfo,
                                Tags = newAlbumTags,
                                Songs = songsGroupedByAlbum.OrderBy(x => x.SortOrder).ToArray(),
                                ViaPlugins = viaPlugins.Distinct().ToArray()
                            });
                            if (albums.Count > _configuration.GetValue<int>(SettingRegistry.ProcessingMaximumProcessingCount, value => value < 1 ? int.MaxValue : value))
                            {
                                _stopProcessingTriggered = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        return new OperationResult<(IEnumerable<Album>, int)>(messages)
        {
            Data = (albums, songs.Count)
        };
    }
}
