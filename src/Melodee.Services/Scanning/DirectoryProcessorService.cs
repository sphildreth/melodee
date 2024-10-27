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
    ISerializer serializer,
    MediaEditService mediaEditService)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private bool _initialized;
    private IMelodeeConfiguration _configuration = new MelodeeConfiguration([]);
    private IEnumerable<IConversionPlugin> _conversionPlugins = [];
    private IEnumerable<IDirectoryPlugin> _directoryPlugins= [];

    private IScriptPlugin _preDiscoveryScript = new NullScript();
    private IScriptPlugin _postDiscoveryScript = new NullScript();
    private IAlbumValidator _albumValidator = new AlbumValidator(new MelodeeConfiguration([]));
    
    private  IEnumerable<ISongPlugin> _songPlugins = [];
    private bool _stopProcessingTriggered;

    private string DirectoryInbound => SafeParser.ToString(_configuration.Configuration[SettingRegistry.DirectoryInbound]);
    
    private string DirectoryStaging => SafeParser.ToString(_configuration.Configuration[SettingRegistry.DirectoryStaging]);
    
    private DirectoryInfo DirectoryInboundInfo => new DirectoryInfo(DirectoryInbound);
    
    private DirectoryInfo DirectoryStagingInfo => new DirectoryInfo(DirectoryStaging);
    
    private FileSystemDirectoryInfo DirectoryStagingFileSystemDirectoryInfo => DirectoryStagingInfo.ToDirectorySystemInfo();
    
    private FileSystemDirectoryInfo DirectoryInboundFileSystemDirectoryInfo => DirectoryInboundInfo.ToDirectorySystemInfo();

    /// <summary>
    /// Used for Unit testing.
    /// </summary>
    /// <param name="configuration"></param>
    public void SetConfiguration(IMelodeeConfiguration configuration) => _configuration = configuration;

    public async Task InitializeAsync(CancellationToken token = default)
    {
        _configuration = await settingService.GetMelodeeConfigurationAsync(token).ConfigureAwait(false);
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
            new CueSheet(_songPlugins, _configuration),
            new SimpleFileVerification(_songPlugins, _albumValidator, _configuration),
            new M3UPlaylist(_songPlugins, _albumValidator, _configuration),
            new Nfo(_configuration)
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
        _initialized = true;
    }

    private void CheckInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Albums discovery service is not initialized.");
        }
    }

    public async Task<OperationResult<DirectoryProcessorResult>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        CheckInitialized();
            
        var processingMessages = new List<string>();
        var processingErrors = new List<Exception>();
        var numberOfAlbumJsonFilesProcessed = 0;
        var conversionPluginsProcessedFileCount = 0;
        var directoryPluginProcessedFileCount = 0;
        var numberOfAlbumFilesProcessed = 0;

        var result = new DirectoryProcessorResult
        {
            NumberOfConversionPluginsProcessed = 0,
            NumberOfDirectoryPluginProcessed = 0,
            NumberOfAlbumFilesProcessed = 0
        };
        

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
        if (!DirectoryStagingInfo.Exists)
        {
            return new OperationResult<DirectoryProcessorResult>
            {
                Errors = new[]
                {
                    new Exception($"Staging Directory [{DirectoryStaging}] not found.")
                },
                Data = result
            };
        }

        // Run PreDiscovery script
        if (!SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.ScriptingEnabled]) && _preDiscoveryScript.IsEnabled)
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

        var directoriesToProcess = fileSystemDirectoryInfo.GetFileSystemDirectoryInfosToProcess(SearchOption.AllDirectories).ToList();
        if (directoriesToProcess.Count > 1)
        {
            OnProcessingStart?.Invoke(this, directoriesToProcess.Count);
            LogAndRaiseEvent(LogEventLevel.Debug, "\u251c Found [{0}] directories to process", null, directoriesToProcess.Count);
        }

        foreach (var directoryInfoToProcess in directoriesToProcess.Take(SafeParser.ToNumber<int>(_configuration.Configuration[SettingRegistry.ProcessingMaximumProcessingCount])))
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
                            if (SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.ProcessingDoOverrideExistingMelodeeDataFiles]))
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
                            var maximumSongNumber = SafeParser.ToNumber<int>(_configuration.Configuration[SettingRegistry.ValidationMaximumSongNumber]); 
                            album.Songs.Where(x => x.SongNumber() < 1).Each((x, i) => { album.SetSongTagValue(x.SongId, MetaTagIdentifier.TrackNumber, maximumSongNumber + i + 1); });
                            foreach (var song in album.Songs)
                            {
                                if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                                {
                                    break;
                                }

                                song.File.Name = song.ToSongFileName();
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
                if (SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.ProcessingDoMoveMelodeeDataFileToStagingDirectory]))
                {
                    foreach (var albumKvp in albumAndJsonFile)
                    {
                        if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                        {
                            break;
                        }

                        var album = albumKvp.Key;
                        var albumDirInfo = new DirectoryInfo(Path.Combine(DirectoryStaging, album.ToDirectoryName()));
                        if (!albumDirInfo.Exists)
                        {
                            albumDirInfo.Create();
                        }

                        album.Images?.Where(x => x.FileInfo != null).Each((image, index) =>
                        {
                            var newImageFileName = Path.Combine(albumDirInfo.FullName, $"{(index + 1).ToStringPadLeft(2)}-{image.PictureIdentifier}.jpg");
                            File.Copy(image.FileInfo!.FullOriginalName(directoryInfoToProcess), newImageFileName, true);
                            if (SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.ProcessingDoDeleteOriginal]))
                            {
                                File.Delete(image.FileInfo!.FullOriginalName(directoryInfoToProcess));
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

                                var newSongFileName = Path.Combine(albumDirInfo.FullName, song.File.Name);
                                if (SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.ProcessingDoDeleteOriginal]))
                                {
                                    song.File.MoveFile(directoryInfoToProcess, newSongFileName);
                                }
                                else
                                {
                                    File.Copy(song.File.FullOriginalName(directoryInfoToProcess), newSongFileName, true);
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
                        album.Status = _albumValidator.ValidateAlbum(album).Data.AlbumStatus;
                        var serialized = serializer.Serialize(album);
                        var jsonName = album.ToMelodeeJsonName(true);
                        if (jsonName.Nullify() != null)
                        {
                            await File.WriteAllTextAsync(Path.Combine(albumDirInfo.FullName, jsonName), serialized, cancellationToken);
                            File.Delete(albumKvp.Value);
                            if (SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.MagicEnabled]))
                            {
                                using (Operation.At(LogEventLevel.Debug).Time("ProcessDirectoryAsync \ud83e\ude84 DoMagic [{DirectoryInfo}]", albumDirInfo.Name))
                                {
                                    await mediaEditService.DoMagic(album.UniqueId, cancellationToken);
                                }
                            }
                        }
                        else
                        {
                            processingMessages.Add($"Unable to determine JsonName for Album [{album}]");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogAndRaiseEvent(LogEventLevel.Error, "Processing Directory [{0}]", e, directoryInfoToProcess.ToString());
                processingErrors.Add(e);
                if (!SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.ProcessingDoContinueOnDirectoryProcessingErrors]))
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
        if (!SafeParser.ToBoolean(_configuration.Configuration[SettingRegistry.ScriptingEnabled]) && _postDiscoveryScript.IsEnabled)
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
        
        DirectoryStagingFileSystemDirectoryInfo.DeleteAllEmptyDirectories();
        DirectoryInboundFileSystemDirectoryInfo.DeleteAllEmptyDirectories();

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
                    NumberOfConversionPluginsProcessed = numberOfAlbumFilesProcessed,
                    NumberOfDirectoryPluginProcessed = directoryPluginProcessedFileCount,
                    NumberOfAlbumFilesProcessed = numberOfAlbumJsonFilesProcessed
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

                foreach (var song in songs)
                {
                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                    {
                        break;
                    }

                    var foundAlbum = albums.FirstOrDefault(x => x.UniqueId == song.AlbumUniqueId);
                    if (foundAlbum != null)
                    {
                        albums.Remove(foundAlbum);
                        albums.Add(foundAlbum.MergeSongs(songs));
                    }
                    else
                    {
                        var songTotal = song.SongTotalNumber();
                        if (songTotal < 1)
                        {
                            songTotal = songs.Count;
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
                        var genres = songs
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
                            Images = songs.Where(x => x.Images != null)
                                .SelectMany(x => x.Images!)
                                .DistinctBy(x => x.CrcHash).ToArray(),
                            OriginalDirectory = fileSystemDirectoryInfo,
                            Tags = newAlbumTags,
                            Songs = songs.OrderBy(x => x.SortOrder).ToArray(),
                            ViaPlugins = viaPlugins.Distinct().ToArray()
                        });
                        if (albums.Count > SafeParser.ToNumber<int>(_configuration.Configuration[SettingRegistry.ProcessingMaximumProcessingCount]))
                        {
                            _stopProcessingTriggered = true;
                            break;
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
