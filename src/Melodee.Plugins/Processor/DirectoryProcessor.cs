using System.Text.Json;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.Conversion;
using Melodee.Plugins.Conversion.Image;
using Melodee.Plugins.Conversion.Media;
using Melodee.Plugins.MetaData.Directory;
using Melodee.Plugins.MetaData.Track;
using Melodee.Plugins.Scripting;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Plugins.Processor;

/// <summary>
/// Take a given directory and process all the directories in it. 
/// </summary>
public sealed class DirectoryProcessor : IDirectoryProcessorPlugin
{
    private readonly Configuration _configuration;
    private readonly IScriptPlugin _preDiscoveryScript;
    private readonly IScriptPlugin _postDiscoveryScript;
    private readonly IEnumerable<ITrackPlugin> _trackPlugins;
    private readonly IEnumerable<IConversionPlugin> _conversionPlugins;
    private readonly IEnumerable<IDirectoryPlugin> _directoryPlugins;

    public string Id => "9BF95E5A-2EB5-4E28-820A-6F3B857356BD";

    public string DisplayName => nameof(DirectoryProcessor);

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; } = 0;

    public DirectoryProcessor(
        IScriptPlugin preDiscoveryScript,
        IScriptPlugin postDiscoveryScript,
        Configuration configuration)
    {
        _configuration = configuration;

        _preDiscoveryScript = preDiscoveryScript;
        _postDiscoveryScript = postDiscoveryScript;

        _trackPlugins = new[]
        {
            new MetaTag(new MetaTagsProcessor(_configuration), _configuration)
        };

        _conversionPlugins = new IConversionPlugin[]
        {
            new ImageConvertor(_configuration),
            new MediaConvertor(_configuration)
        };

        _directoryPlugins = new IDirectoryPlugin[]
        {
            new CueSheet(_trackPlugins, _configuration),
            new SimpleFileVerification(_trackPlugins, _configuration),
            new M3UPlaylist(_trackPlugins, _configuration),
            new Nfo(_configuration),
        };
    }

    public async Task<OperationResult<int>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var processingErrors = new List<Exception>();
        var numberOfReleaseJsonFilesProcessed = 0;
        var conversionPluginsProcessedFileCount = 0;
        var directoryPluginProcessedFileCount = 0;
        var numberOfReleaseFilesProcessed = 0;
        
        // Ensure directory to process exists
        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (!dirInfo.Exists)
        {
            return new OperationResult<int>
            {
                Errors = new[]
                {
                    new Exception($"Directory [{fileSystemDirectoryInfo}] not found.")
                },
                Data = 0
            };
        }

        // Ensure that staging directory exists
        var stagingInfo = new DirectoryInfo(_configuration.StagingDirectory);
        if (!stagingInfo.Exists)
        {
            return new OperationResult<int>
            {
                Errors = new[]
                {
                    new Exception($"Staging Directory [{_configuration.StagingDirectory}] not found.")
                },
                Data = 0
            };
        }

        // Run PreDiscovery script
        if (_preDiscoveryScript.IsEnabled)
        {        
            Log.Debug("Executing PreDiscoveryScript [{script}] directories to process", _preDiscoveryScript.DisplayName);
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
                Log.Error(e, "PreDiscoveryScript [{$ScriptName}]", _preDiscoveryScript.DisplayName);
                preDiscoveryScriptResult.AddError(e);
            }
            if (!preDiscoveryScriptResult.IsSuccess)
            {
                return new OperationResult<int>(preDiscoveryScriptResult.Messages)
                {
                    Errors = preDiscoveryScriptResult.Errors,
                    Data = 0
                };
            }
        }

        var directoriesToProcess = fileSystemDirectoryInfo.GetFileSystemDirectoryInfosToProcess(SearchOption.AllDirectories).ToList();
        if (directoriesToProcess.Count > 1)
        {
            Log.Debug("\u251c Found [{count}] directories to process", directoriesToProcess.Count);
        }
        foreach (var directoryInfoToProcess in directoriesToProcess)
        {
            try
            {
                var allFilesInDirectory = directoryInfoToProcess.FileInfosForExtension("*").ToArray();

                Log.Debug("\u251c Processing [{DirectoryName}] Number of files to process [{FileCount}]", directoryInfoToProcess.Name, allFilesInDirectory.Length);
                
                // Run Enabled Conversion scripts on each file in directory
                // e.g. Convert FLAC to MP3, Convert non JPEG files into JPEGs, etc.
                foreach (var fileSystemInfo in allFilesInDirectory)
                {
                    var fsi = fileSystemInfo.ToFileSystemInfo();
                    foreach (var plugin in _conversionPlugins.Where(x => x.IsEnabled).OrderBy(x => x.SortOrder))
                    {
                        if (plugin.DoesHandleFile(directoryInfoToProcess, fsi))
                        {
                            using (Operation.At(LogEventLevel.Debug).Time("[{Plugin}] Processing [{File}] ", plugin.DisplayName, fileSystemInfo.Name))
                            {
                                var pluginResult = await plugin.ProcessFileAsync(directoryInfoToProcess, fsi, cancellationToken);
                                if (!pluginResult.IsSuccess)
                                {
                                    return new OperationResult<int>(pluginResult.Messages)
                                    {
                                        Errors = pluginResult.Errors,
                                        Data = 0
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
                    var pluginResult = await plugin.ProcessDirectoryAsync(directoryInfoToProcess, cancellationToken);
                    if (!pluginResult.IsSuccess)
                    {
                        return new OperationResult<int>(pluginResult.Messages)
                        {
                            Errors = pluginResult.Errors,
                            Data = 0
                        };
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
                        return new OperationResult<int>(releasesForDirectory.Messages)
                        {
                            Errors = releasesForDirectory.Errors,
                            Data = 0
                        };
                    }

                    numberOfReleaseFilesProcessed = releasesForDirectory.Data.Item2;

                    foreach (var releaseForDirectory in releasesForDirectory.Data.Item1.ToArray())
                    {
                        Release? mergedRelease = releaseForDirectory;
                        
                        string serialized = string.Empty;
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
                            Log.Error(e, "Error processing [{FileSystemDirectoryInfo}]", fileSystemDirectoryInfo);
                        }                        
                        await File.WriteAllTextAsync(releaseDataName, serialized, cancellationToken);
                        numberOfReleaseJsonFilesProcessed++;
                    }
                }

                // Find all release json files in given directory, if none bail.
                releaseJsonFiles = directoryInfoToProcess.FileInfosForExtension(Release.JsonFileName).ToArray();
                if (!releaseJsonFiles.Any())
                {
                    return new OperationResult<int>($"No Releases found in given directory [{directoryInfoToProcess}]")
                    {
                        Data = 0
                    };
                }

                // For each Release json find all image files and add to Release to be moved below to staging directory.
                var releaseAndJsonFile = new Dictionary<Release, string>();
                foreach (var releaseJsonFile in releaseJsonFiles)
                {
                    try
                    {
                        var release = JsonSerializer.Deserialize<Release>(await File.ReadAllTextAsync(releaseJsonFile.FullName, cancellationToken));
                        if (release == null || !release.IsValid())
                        {
                            return new OperationResult<int>($"Invalid Release json file [{releaseJsonFile.FullName}]")
                            {
                                Data = 0
                            };
                        }

                        var releaseImages = release.Images?.ToList() ?? [];
                        var foundReleaseImages = (await FindImagesForRelease(release, cancellationToken)).ToArray();
                        if (foundReleaseImages.Length != 0)
                        {
                            foreach (var foundReleaseImage in foundReleaseImages)
                            {
                                if (!releaseImages.Any(x => x.IsCrcHashMatch(foundReleaseImage.CrcHash)))
                                {
                                    releaseImages.Add(foundReleaseImage);
                                }
                            }
                        }

                        release.Images = releaseImages;
                        if (release.Tracks != null)
                        {
                            foreach (var track in release.Tracks)
                            {
                                track.File.Name = track.ToTrackFileName();
                            }
                        }
                        releaseAndJsonFile.Add(release, releaseJsonFile.FullName);
                    }
                    catch (Exception e)
                    {
                        processingErrors.Add(e);
                        Log.Error(e, "Unable to load release json [{FullName}]", releaseJsonFile.FullName);
                    }
                }

                // Create directory and move files for each found release in staging directory.
                foreach (var releaseKvp in releaseAndJsonFile)
                {
                    var release = releaseKvp.Key;
                    var releaseDirInfo = new DirectoryInfo(Path.Combine(_configuration.StagingDirectory, release.ToDirectoryName()));
                    if (!releaseDirInfo.Exists)
                    {
                        releaseDirInfo.Create();
                    }

                    release.Images?.Where(x => x.FileInfo != null).Each((image, index) =>
                    {
                        var newImageFileName = Path.Combine(releaseDirInfo.FullName, $"{(index + 1).ToStringPadLeft(2)}-{image.PictureIdentifier}.jpg");
                        File.Copy(image.FileInfo!.FullName(directoryInfoToProcess), newImageFileName, true);
                        if (_configuration.PluginProcessOptions.DoDeleteOriginal)
                        {
                            File.Delete(image.FileInfo!.FullName(directoryInfoToProcess));
                        }
                    });

                    if (release.Tracks != null)
                    {
                        foreach (var track in release.Tracks)
                        {
                            var newTrackFileName = Path.Combine(releaseDirInfo.FullName, track.File.Name);
                            if (_configuration.PluginProcessOptions.DoDeleteOriginal)
                            {
                                File.Move(track.File.FullOriginalName(directoryInfoToProcess), newTrackFileName);
                            }
                            else
                            {
                                File.Copy(track.File.FullOriginalName(directoryInfoToProcess), newTrackFileName, true);
                            }
                        }
                    }

                  var serialized = System.Text.Json.JsonSerializer.Serialize(release);
                  await File.WriteAllTextAsync(Path.Combine(releaseDirInfo.FullName, release.ToMelodeeJsonName(true)), serialized, cancellationToken);
                  File.Delete(releaseKvp.Value);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Processing Directory [{$DirectoryName}]", directoryInfoToProcess.ToString());
                processingErrors.Add(e);
                if (!_configuration.PluginProcessOptions.DoContinueOnDirectoryProcessingErrors)
                {
                    return new OperationResult<int>()
                    {
                        Errors = processingErrors,
                        Data = 0
                    };
                }
            }
        }

        // Run PostDiscovery script
        if (_postDiscoveryScript.IsEnabled)
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
                Log.Error(e, "PostDiscoveryScript [{$ScriptName}]", _postDiscoveryScript.DisplayName);
                postDiscoveryScriptResult.AddError(e);
            }
            if (!postDiscoveryScriptResult.IsSuccess)
            {
                return new OperationResult<int>(postDiscoveryScriptResult.Messages)
                {
                    Errors = postDiscoveryScriptResult.Errors,
                    Data = 0
                };
            }
        }

        return new OperationResult<int>(new []
        {
            $"Directory Plugin(s) process count [{ directoryPluginProcessedFileCount }]",
            $"Conversion Plugin(s) process count [{ conversionPluginsProcessedFileCount }]",
            $"Track Plugin(s) process count [{ numberOfReleaseFilesProcessed }]",
            $"Release process count [{ numberOfReleaseJsonFilesProcessed }]"
        })
        {
            Errors = processingErrors.ToArray(),
            Data = numberOfReleaseJsonFilesProcessed + conversionPluginsProcessedFileCount + directoryPluginProcessedFileCount + numberOfReleaseFilesProcessed
        };
    }

    private static async Task<IEnumerable<ImageInfo>> FindImagesForRelease(Release release, CancellationToken cancellationToken = default)
    {
        var imageInfos = new List<ImageInfo>();
        var imageFiles = ImageHelper.ImageFilesInDirectory(release.OriginalDirectory.Path, SearchOption.TopDirectoryOnly);
        foreach (var imageFile in imageFiles)
        {
            var fileInfo = new FileInfo(imageFile);
            if (release.IsFileForRelease(fileInfo))
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

                    var imageInfo = await SixLabors.ImageSharp.Image.LoadAsync(fileInfo.FullName, cancellationToken);
                    imageInfos.Add(new ImageInfo
                    {
                        CrcHash = CRC32.Calculate(fileInfo),
                        FileInfo = fileInfo.ToFileSystemInfo(),
                        PictureIdentifier = pictureIdentifier,
                        Width = imageInfo.Width,
                        Height = imageInfo.Height,
                        SortOrder = 0
                    });
                }
            }
        }

        return imageInfos;
    }

    private async Task<OperationResult<(IEnumerable<Release>, int)>> AllReleasesForDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
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
                    var fsi = fileSystemInfo.ToFileSystemInfo();
                    foreach (var plugin in _trackPlugins.OrderBy(x => x.SortOrder))
                    {
                        if (plugin.DoesHandleFile(fileSystemDirectoryInfo, fsi))
                        {
                            using (Operation.At(LogEventLevel.Debug).Time("File [{File}] Plugin [{Plugin}]", fileSystemInfo.Name, plugin.DisplayName))
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
                            new() { Identifier = MetaTagIdentifier.Album, Value = track.ReleaseTitle(), SortOrder = 1 },
                            new() { Identifier = MetaTagIdentifier.AlbumArtist, Value = track.ReleaseArtist(), SortOrder = 2 },
                            new() { Identifier = MetaTagIdentifier.DiscNumber, Value = track.MediaNumber(), SortOrder = 4 },
                            new() { Identifier = MetaTagIdentifier.OrigReleaseYear, Value = track.ReleaseYear(), SortOrder = 100 },
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
                            Images = tracks.Where(x => x.Images != null).SelectMany(x => x.Images!).DistinctBy(x => x.CrcHash).ToArray(),
                            OriginalDirectory = fileSystemDirectoryInfo,
                            Tags = newReleaseTags,
                            Tracks = tracks.OrderBy(x => x.SortOrder).ToArray(),
                            ViaPlugins = viaPlugins.Distinct().ToArray()
                        });
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