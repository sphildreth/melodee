using System.Text.Json;
using System.Text.Json.Serialization;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.Conversion;
using Melodee.Plugins.Conversion.Image;
using Melodee.Plugins.Conversion.Media;
using Melodee.Plugins.MetaData.Directory;
using Melodee.Plugins.MetaData.Track;
using Melodee.Plugins.Processor.Models;
using Melodee.Plugins.Scripting;
using Melodee.Plugins.Validation;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using SixLabors.ImageSharp;
using SmartFormat;
using Configuration = Melodee.Common.Models.Configuration.Configuration;
using ImageInfo = Melodee.Common.Models.ImageInfo;

namespace Melodee.Plugins.Processor;

/// <summary>
///     Take a given directory and process all the directories in it.
/// </summary>
public sealed class DirectoryProcessor : IDirectoryProcessorPlugin
{
    private readonly Configuration _configuration;
    private readonly IEnumerable<IConversionPlugin> _conversionPlugins;
    private readonly IEnumerable<IDirectoryPlugin> _directoryPlugins;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IScriptPlugin _postDiscoveryScript;
    private readonly IScriptPlugin _preDiscoveryScript;
    private readonly IReleaseValidator _releaseValidator;
    private readonly IReleaseEditProcessor _releaseEditProcessor;
    private readonly IEnumerable<ITrackPlugin> _trackPlugins;
    private bool _stopProcessingTriggered;

    public DirectoryProcessor(
        IScriptPlugin preDiscoveryScript,
        IScriptPlugin postDiscoveryScript,
        IReleaseValidator releaseValidator,
        IReleaseEditProcessor releaseEditProcessor,
        Configuration configuration)
    {
        _configuration = configuration;

        _preDiscoveryScript = preDiscoveryScript;
        _postDiscoveryScript = postDiscoveryScript;
        _releaseValidator = releaseValidator;
        _releaseEditProcessor = releaseEditProcessor;

        _trackPlugins = new[]
        {
            new AtlMetaTag(new MetaTagsProcessor(_configuration), _configuration)
        };

        _conversionPlugins = new IConversionPlugin[]
        {
            new ImageConvertor(_configuration),
            new MediaConvertor(_configuration)
        };

        _directoryPlugins = new IDirectoryPlugin[]
        {
            new CueSheet(_trackPlugins, _configuration),
            new SimpleFileVerification(_trackPlugins, releaseValidator, _configuration),
            new M3UPlaylist(_trackPlugins, releaseValidator, _configuration),
            new Nfo(_configuration)
        };
    }

    public string Id => "9BF95E5A-2EB5-4E28-820A-6F3B857356BD";

    public string DisplayName => nameof(DirectoryProcessor);

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; } = 0;

    public async Task<OperationResult<DirectoryProcessorResult>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var processingMessages = new List<string>();
        var processingErrors = new List<Exception>();
        var numberOfReleaseJsonFilesProcessed = 0;
        var conversionPluginsProcessedFileCount = 0;
        var directoryPluginProcessedFileCount = 0;
        var numberOfReleaseFilesProcessed = 0;

        var result = new DirectoryProcessorResult
        {
            NumberOfConversionPluginsProcessed = 0,
            NumberOfDirectoryPluginProcessed = 0,
            NumberOfReleaseFilesProcessed = 0
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
        var stagingInfo = new DirectoryInfo(_configuration.StagingDirectory);
        if (!stagingInfo.Exists)
        {
            return new OperationResult<DirectoryProcessorResult>
            {
                Errors = new[]
                {
                    new Exception($"Staging Directory [{_configuration.StagingDirectory}] not found.")
                },
                Data = result
            };
        }

        // Run PreDiscovery script
        if (!_configuration.Scripting.Disabled && _preDiscoveryScript.IsEnabled)
        {
            LogAndRaiseEvent(LogEventLevel.Debug, "Executing PreDiscoveryScript [{0}]", null, _preDiscoveryScript.DisplayName);
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

        foreach (var directoryInfoToProcess in directoriesToProcess.Take(_configuration.PluginProcessOptions.MaximumProcessingCountValue))
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

                // Run all enabled IDirectoryPlugins to convert MetaData files into Release json files.
                // e.g. Build Release json file for M3U or NFO or SFV, etc.
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

                // Check if any Release json files exist in given directory, if none then create from track files.
                var releaseJsonFiles = directoryInfoToProcess.FileInfosForExtension(Release.JsonFileName);
                if (!releaseJsonFiles.Any())
                {
                    var releasesForDirectory = await AllReleasesForDirectoryAsync(directoryInfoToProcess, cancellationToken);
                    if (!releasesForDirectory.IsSuccess)
                    {
                        return new OperationResult<DirectoryProcessorResult>(releasesForDirectory.Messages)
                        {
                            Errors = releasesForDirectory.Errors,
                            Data = result
                        };
                    }

                    numberOfReleaseFilesProcessed = releasesForDirectory.Data.Item2;

                    foreach (var releaseForDirectory in releasesForDirectory.Data.Item1.ToArray())
                    {
                        if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                        {
                            break;
                        }

                        var mergedRelease = releaseForDirectory;

                        var serialized = string.Empty;
                        var releaseDataName = Path.Combine(directoryInfoToProcess.Path, releaseForDirectory.ToMelodeeJsonName());
                        if (File.Exists(releaseDataName))
                        {
                            if (_configuration.PluginProcessOptions.DoOverrideExistingMelodeeDataFiles)
                            {
                                File.Delete(releaseDataName);
                            }
                            else
                            {
                                var existingRelease = JsonSerializer.Deserialize<Release?>(await File.ReadAllTextAsync(releaseDataName, cancellationToken));
                                if (existingRelease != null)
                                {
                                    mergedRelease = mergedRelease.Merge(existingRelease);
                                }
                            }
                        }

                        try
                        {
                            serialized = JsonSerializer.Serialize(mergedRelease);
                        }
                        catch (Exception e)
                        {
                            LogAndRaiseEvent(LogEventLevel.Error, "Error processing [{0}]", e, fileSystemDirectoryInfo);
                        }

                        await File.WriteAllTextAsync(releaseDataName, serialized, cancellationToken);
                        numberOfReleaseJsonFilesProcessed++;
                    }
                }

                // Find all release json files in given directory, if none bail.
                releaseJsonFiles = directoryInfoToProcess.FileInfosForExtension(Release.JsonFileName).ToArray();
                if (!releaseJsonFiles.Any())
                {
                    return new OperationResult<DirectoryProcessorResult>($"No Releases found in given directory [{directoryInfoToProcess}]")
                    {
                        Data = result
                    };
                }

                // For each Release json find all image files and add to Release to be moved below to staging directory.
                var releaseAndJsonFile = new Dictionary<Release, string>();
                foreach (var releaseJsonFile in releaseJsonFiles)
                {
                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                    {
                        break;
                    }

                    try
                    {
                        var release = JsonSerializer.Deserialize<Release>(await File.ReadAllTextAsync(releaseJsonFile.FullName, cancellationToken));
                        if (release == null)
                        {
                            return new OperationResult<DirectoryProcessorResult>($"Invalid Release json file [{releaseJsonFile.FullName}]")
                            {
                                Data = result
                            };
                        }

                        var releaseImages = new List<ImageInfo>();
                        var foundReleaseImages = (await FindImagesForRelease(release, cancellationToken)).ToArray();
                        if (foundReleaseImages.Length != 0)
                        {
                            foreach (var foundReleaseImage in foundReleaseImages)
                            {
                                if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                                {
                                    break;
                                }

                                if (!releaseImages.Any(x => x.IsCrcHashMatch(foundReleaseImage.CrcHash)))
                                {
                                    releaseImages.Add(foundReleaseImage);
                                }
                            }
                        }

                        release.Images = releaseImages.ToArray();
                        if (release.Tracks != null)
                        {
                            // Set TrackNumber to invalid range if TrackNumber is missing
                            release.Tracks.Where(x => x.TrackNumber() < 1).Each((x, i) => { release.SetTrackTagValue(x.TrackId, MetaTagIdentifier.TrackNumber, _configuration.ValidationOptions.MaximumTrackNumber + i + 1); });
                            foreach (var track in release.Tracks)
                            {
                                if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                                {
                                    break;
                                }

                                track.File.Name = track.ToTrackFileName();
                            }
                        }
                        releaseAndJsonFile.Add(release, releaseJsonFile.FullName);
                    }
                    catch (Exception e)
                    {
                        processingErrors.Add(e);
                        LogAndRaiseEvent(LogEventLevel.Error, "Unable to load release json [{0}]", e, releaseJsonFile.FullName);
                    }
                }

                // Create directory and move files for each found release in staging directory.
                if (_configuration.PluginProcessOptions.DoMoveToStagingDirectory)
                {
                    foreach (var releaseKvp in releaseAndJsonFile)
                    {
                        if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                        {
                            break;
                        }

                        var release = releaseKvp.Key;
                        var releaseDirInfo = new DirectoryInfo(Path.Combine(_configuration.StagingDirectory, release.ToDirectoryName()));
                        if (!releaseDirInfo.Exists)
                        {
                            releaseDirInfo.Create();
                        }

                        release.Images?.Where(x => x.FileInfo != null).Each((image, index) =>
                        {
                            var newImageFileName = Path.Combine(releaseDirInfo.FullName, $"{(index + 1).ToStringPadLeft(2)}-{image.PictureIdentifier}.jpg");
                            File.Copy(image.FileInfo!.FullOriginalName(directoryInfoToProcess), newImageFileName, true);
                            if (_configuration.PluginProcessOptions.DoDeleteOriginal)
                            {
                                File.Delete(image.FileInfo!.FullOriginalName(directoryInfoToProcess));
                            }
                        });

                        if (release.Tracks != null)
                        {
                            foreach (var track in release.Tracks)
                            {
                                if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                                {
                                    break;
                                }

                                var newTrackFileName = Path.Combine(releaseDirInfo.FullName, track.File.Name);
                                if (_configuration.PluginProcessOptions.DoDeleteOriginal)
                                {
                                    track.File.MoveFile(directoryInfoToProcess, newTrackFileName);
                                }
                                else
                                {
                                    File.Copy(track.File.FullOriginalName(directoryInfoToProcess), newTrackFileName, true);
                                }
                            }

                            if ((release.Tags ?? Array.Empty<MetaTag<object?>>()).Any(x => x.WasModified) ||
                                release.Tracks!.Any(x => (x.Tags ?? Array.Empty<MetaTag<object?>>()).Any(y => y.WasModified)))
                            {
                                var releaseDirectorySystemInfo = releaseDirInfo.ToDirectorySystemInfo();
                                // Set the value then change to NeedsAttention
                                foreach (var trackPlugin in _trackPlugins)
                                {
                                    foreach (var track in release.Tracks)
                                    {
                                        if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                                        {
                                            break;
                                        }

                                        await trackPlugin.UpdateTrackAsync(releaseDirectorySystemInfo, track, cancellationToken);
                                    }
                                }

                                release.Status = ReleaseStatus.NeedsAttention;
                            }
                        }

                        release.Directory = releaseDirInfo.ToDirectorySystemInfo();
                        release.Status = _releaseValidator.ValidateRelease(release).Data.ReleaseStatus;
                        var serialized = JsonSerializer.Serialize(release, _jsonSerializerOptions);
                        var jsonName = release.ToMelodeeJsonName(true);
                        if (jsonName != null)
                        {
                            await File.WriteAllTextAsync(Path.Combine(releaseDirInfo.FullName, jsonName), serialized, cancellationToken);
                            File.Delete(releaseKvp.Value);
                            if (_configuration.MagicOptions.IsMagicEnabled)
                            {
                                using (Operation.At(LogEventLevel.Debug).Time("ProcessDirectoryAsync \ud83e\ude84 DoMagic [{DirectoryInfo}]", releaseDirInfo.Name))
                                {
                                    await _releaseEditProcessor.DoMagic(release.UniqueId, cancellationToken);
                                }
                            }
                        }
                        else
                        {
                            processingMessages.Add($"Unable to determine JsonName for Release [{release}]");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogAndRaiseEvent(LogEventLevel.Error, "Processing Directory [{0}]", e, directoryInfoToProcess.ToString());
                processingErrors.Add(e);
                if (!_configuration.PluginProcessOptions.DoContinueOnDirectoryProcessingErrors)
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
        if (!_configuration.Scripting.Disabled && _postDiscoveryScript.IsEnabled)
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
        
        _configuration.StagingDirectoryInfo.DeleteAllEmptyDirectories();
        _configuration.InboundDirectoryInfo.DeleteAllEmptyDirectories();        

        LogAndRaiseEvent(LogEventLevel.Information, "Processing Complete!");

        processingMessages.Add($"Directory Plugin(s) process count [{directoryPluginProcessedFileCount}]");
        processingMessages.Add($"Conversion Plugin(s) process count [{conversionPluginsProcessedFileCount}]");
        processingMessages.Add($"Track Plugin(s) process count [{numberOfReleaseFilesProcessed}]");
        processingMessages.Add($"Release process count [{numberOfReleaseJsonFilesProcessed}]");
            
        return new OperationResult<DirectoryProcessorResult>(processingMessages)
        {
            Errors = processingErrors.ToArray(),
            Data = new DirectoryProcessorResult
                {
                    NumberOfConversionPluginsProcessed = numberOfReleaseFilesProcessed,
                    NumberOfDirectoryPluginProcessed = directoryPluginProcessedFileCount,
                    NumberOfReleaseFilesProcessed = numberOfReleaseJsonFilesProcessed
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
    ///     This is raised when a new Release is processed put into the Staging directory.
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

    private static async Task<IEnumerable<ImageInfo>> FindImagesForRelease(Release release, CancellationToken cancellationToken = default)
    {
        var imageInfos = new List<ImageInfo>();
        var imageFiles = ImageHelper.ImageFilesInDirectory(release.OriginalDirectory.Path, SearchOption.TopDirectoryOnly);
        var index = 0;
        foreach (var imageFile in imageFiles)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var fileInfo = new FileInfo(imageFile);
            if (release.IsFileForRelease(fileInfo) && !ReleaseValidator.IsImageAProofType(imageFile))
            {
                if (ImageHelper.IsReleaseImage(fileInfo) ||
                    ImageHelper.IsArtistImage(fileInfo) ||
                    ImageHelper.IsArtistSecondaryImage(fileInfo) ||
                    ImageHelper.IsReleaseSecondaryImage(fileInfo))
                {
                    var pictureIdentifier = PictureIdentifier.NotSet;
                    if (ImageHelper.IsReleaseImage(fileInfo))
                    {
                        pictureIdentifier = PictureIdentifier.Front;
                    }
                    else if (ImageHelper.IsReleaseSecondaryImage(fileInfo))
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
                        CrcHash = CRC32.Calculate(fileInfo),
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

    public async Task<OperationResult<(IEnumerable<Release>, int)>> AllReleasesForDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var releases = new List<Release>();
        var messages = new List<string>();
        var viaPlugins = new List<string>();
        var tracks = new List<Track>();

        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (dirInfo.Exists)
        {
            using (Operation.At(LogEventLevel.Debug).Time("AllReleasesForDirectoryAsync [{directoryInfo}]", fileSystemDirectoryInfo.Name))
            {
                foreach (var fileSystemInfo in dirInfo.EnumerateFileSystemInfos("*.*", SearchOption.TopDirectoryOnly))
                {
                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                    {
                        break;
                    }

                    var fsi = fileSystemInfo.ToFileSystemInfo();
                    foreach (var plugin in _trackPlugins.OrderBy(x => x.SortOrder))
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
                                    tracks.Add(pluginResult.Data);
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

                foreach (var track in tracks)
                {
                    if (cancellationToken.IsCancellationRequested || _stopProcessingTriggered)
                    {
                        break;
                    }

                    var foundRelease = releases.FirstOrDefault(x => x.UniqueId == track.ReleaseUniqueId);
                    if (foundRelease != null)
                    {
                        releases.Remove(foundRelease);
                        releases.Add(foundRelease.MergeTracks(tracks));
                    }
                    else
                    {
                        var trackTotal = track.TrackTotalNumber();
                        if (trackTotal < 1)
                        {
                            trackTotal = tracks.Count;
                        }

                        var newReleaseTags = new List<MetaTag<object?>>
                        {
                            new()
                            {
                                Identifier = MetaTagIdentifier.Album, Value = track.ReleaseTitle(), SortOrder = 1
                            },
                            new()
                            {
                                Identifier = MetaTagIdentifier.AlbumArtist, Value = track.ReleaseArtist(), SortOrder = 2
                            },
                            new()
                            {
                                Identifier = MetaTagIdentifier.DiscNumber, Value = track.MediaNumber(), SortOrder = 4
                            },
                            new()
                            {
                                Identifier = MetaTagIdentifier.DiscTotal, Value = track.MediaNumber(), SortOrder = 4
                            },                            
                            new()
                            {
                                Identifier = MetaTagIdentifier.OrigReleaseYear, Value = track.ReleaseYear(),
                                SortOrder = 100
                            },
                            new() { Identifier = MetaTagIdentifier.TrackTotal, Value = trackTotal, SortOrder = 101 }
                        };
                        var genres = tracks
                            .SelectMany(x => x.Tags ?? Array.Empty<MetaTag<object?>>())
                            .Where(x => x.Identifier == MetaTagIdentifier.Genre);
                        newReleaseTags.AddRange(genres
                            .GroupBy(x => x.Value)
                            .Select((genre, i) => new MetaTag<object?>
                            {
                                Identifier = MetaTagIdentifier.Genre,
                                Value = genre.Key,
                                SortOrder = 5 + i
                            }));
                        releases.Add(new Release
                        {
                            Images = tracks.Where(x => x.Images != null)
                                .SelectMany(x => x.Images!)
                                .DistinctBy(x => x.CrcHash).ToArray(),
                            OriginalDirectory = fileSystemDirectoryInfo,
                            Tags = newReleaseTags,
                            Tracks = tracks.OrderBy(x => x.SortOrder).ToArray(),
                            ViaPlugins = viaPlugins.Distinct().ToArray()
                        });
                        if (releases.Count > _configuration.PluginProcessOptions.MaximumProcessingCountValue)
                        {
                            _stopProcessingTriggered = true;
                            break;
                        }
                    }
                }
            }
        }
        return new OperationResult<(IEnumerable<Release>, int)>(messages)
        {
            Data = (releases, tracks.Count)
        };
    }
}
