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
using SerilogTimings;

namespace Melodee.Plugins.Processor;

/// <summary>
/// Take a given directory and process all the directories in it. 
/// </summary>
public sealed class DirectoryDirectoryProcessor : IDirectoryProcessorPlugin
{
    private readonly Configuration _configuration;
    private readonly IScriptPlugin _preDiscoveryScript;
    private readonly IScriptPlugin _postDiscoveryScript;
    private readonly IEnumerable<ITrackPlugin> _trackPlugins;
    private readonly IEnumerable<IConversionPlugin> _conversionPlugins;
    private readonly IEnumerable<IDirectoryPlugin> _directoryPlugins;

    public string Id => "9BF95E5A-2EB5-4E28-820A-6F3B857356BD";

    public string DisplayName => nameof(DirectoryDirectoryProcessor);

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; } = 0;

    public DirectoryDirectoryProcessor(
        IScriptPlugin preDiscoveryScript,
        IScriptPlugin postDiscoveryScript,
        Configuration configuration)
    {
        _configuration = configuration;

        _preDiscoveryScript = preDiscoveryScript;
        _postDiscoveryScript = postDiscoveryScript;

        _trackPlugins = new []
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

    public async Task<OperationResult<bool>> ProcessDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        // Ensure directory to process exists
        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (!dirInfo.Exists)
        {
            return new OperationResult<bool>
            {
                Errors = new[]
                {
                    new Exception($"Directory [{fileSystemDirectoryInfo}] not found.")
                },
                Data = false
            };
        }

        // Ensure that staging directory exists
        var stagingInfo = new DirectoryInfo(_configuration.StagingDirectory);
        if (!stagingInfo.Exists)
        {
            return new OperationResult<bool>
            {
                Errors = new[]
                {
                    new Exception($"Staging Directory [{_configuration.StagingDirectory}] not found.")
                },
                Data = false
            };
        }

        // Run PreDiscovery script
        if (_preDiscoveryScript.IsEnabled)
        {
            var preDiscoveryScriptResult = await _preDiscoveryScript.ProcessAsync(fileSystemDirectoryInfo, cancellationToken);
            if (!preDiscoveryScriptResult.IsSuccess)
            {
                return new OperationResult<bool>(preDiscoveryScriptResult.Messages)
                {
                    Errors = preDiscoveryScriptResult.Errors,
                    Data = false
                };
            }
        }

        var directoriesToProcess = fileSystemDirectoryInfo.GetFileSystemDirectoryInfosToProcess(SearchOption.AllDirectories).ToList();
        foreach (var directoryInfoToProcess in directoriesToProcess)
        {
            var allFilesInDirectory = directoryInfoToProcess.FileInfosForExtension("*").ToArray();

            // Run Enabled Conversion scripts on each file in directory
            // e.g. Convert FLAC to MP3, Convert non JPEG files into JPEGs, etc.
            foreach (var fileSystemInfo in allFilesInDirectory)
            {
                var fsi = fileSystemInfo.ToFileSystemInfo();
                foreach (var plugin in _conversionPlugins.OrderBy(x => x.SortOrder))
                {
                    if (plugin.DoesHandleFile(directoryInfoToProcess, fsi))
                    {
                        using (Operation.Time("Conversion: File [{File}] Plugin [{Plugin}]", fileSystemInfo.Name, plugin.DisplayName))
                        {
                            var pluginResult = await plugin.ProcessFileAsync(directoryInfoToProcess, fsi, cancellationToken);
                            if (!pluginResult.IsSuccess)
                            {
                                return new OperationResult<bool>(pluginResult.Messages)
                                {
                                    Errors = pluginResult.Errors,
                                    Data = false
                                };
                            }
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
            foreach (var plugin in _directoryPlugins.OrderBy(x => x.SortOrder))
            {
                using (Operation.Time("MetaData:Directory: Plugin [{Plugin}]", plugin.DisplayName))
                {
                    var pluginResult = await plugin.ProcessDirectoryAsync(directoryInfoToProcess, cancellationToken);
                    if (!pluginResult.IsSuccess)
                    {
                        return new OperationResult<bool>(pluginResult.Messages)
                        {
                            Errors = pluginResult.Errors,
                            Data = false
                        };
                    }
                }

                if (plugin.StopProcessing)
                {
                    break;
                }
            }

            // Check if any Release json files exist in given directory, if none then create from track files.
            var releaseJsonFiles = directoryInfoToProcess.FileInfosForExtension("melodee.json");
            if (!releaseJsonFiles.Any())
            {
                var releasesForDirectory = await AllReleasesForDirectoryAsync(directoryInfoToProcess, cancellationToken);
                if (!releasesForDirectory.IsSuccess)
                {
                    return new OperationResult<bool>(releasesForDirectory.Messages)
                    {
                        Errors = releasesForDirectory.Errors,
                        Data = false
                    };
                }

                foreach (var releaseForDirectory in releasesForDirectory.Data)
                {
                    var serialized = System.Text.Json.JsonSerializer.Serialize(releaseForDirectory);
                    var releaseDataName = Path.Combine(directoryInfoToProcess.Path, releaseForDirectory.ToMelodeeJsonName());
                    await File.WriteAllTextAsync(releaseDataName, serialized, cancellationToken);
                }
            }

            // Find all release json files in given directory, if none bail.
            releaseJsonFiles = directoryInfoToProcess.FileInfosForExtension("melodee.json").ToArray();
            if (!releaseJsonFiles.Any())
            {
                return new OperationResult<bool>($"No Releases found in given directory [{directoryInfoToProcess}]")
                {
                    Data = false
                };
            }

            // For each Release json find all image files and add to Release to be moved below to staging folder.
            var releaseAndJsonFile = new Dictionary<Release, string>();
            foreach (var releaseJsonFile in releaseJsonFiles)
            {
                var release = System.Text.Json.JsonSerializer.Deserialize<Release>(await File.ReadAllTextAsync(releaseJsonFile.FullName, cancellationToken));
                if (release == null || !release.IsValid())
                {
                    return new OperationResult<bool>($"Invalid Release json file [{releaseJsonFile.FullName}]")
                    {
                        Data = false
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
                releaseAndJsonFile.Add(release, releaseJsonFile.FullName);
            }

            // Create directory and move files for each found release in staging folder
            foreach (var releaseKvp in releaseAndJsonFile)
            {
                var release = releaseKvp.Key;
                var releaseDirInfo = new DirectoryInfo(Path.Combine(_configuration.StagingDirectory, $"{release.Artist()} - [{release.ReleaseYear()}] {release.ReleaseTitle()}".ToFileNameFriendly()));
                if (!releaseDirInfo.Exists)
                {
                    releaseDirInfo.Create();
                }

                release.Images?.Where(x => x.FileInfo != null).Each((image, index) =>
                {
                    var newImageFileName = Path.Combine(releaseDirInfo.FullName, $"{ (index+1).ToStringPadLeft(2) }-{image.PictureIdentifier}.jpg");
                    if (File.Exists(newImageFileName))
                    {
                        File.Delete(newImageFileName);
                    }
                    File.Copy(image.FileInfo!.FullName(directoryInfoToProcess), newImageFileName);
                    if (_configuration.PluginProcessOptions.DoDeleteOriginal)
                    {
                        File.Delete(image.FileInfo!.FullName(directoryInfoToProcess));
                    }
                });

                if (release.Tracks != null)
                {
                    foreach (var track in release.Tracks)
                    {
                        var newTrackFileName = Path.Combine(releaseDirInfo.FullName, track.TrackFileName(release, _configuration));
                        if (_configuration.PluginProcessOptions.DoDeleteOriginal)
                        {
                            File.Move(track.File.FullName(directoryInfoToProcess), newTrackFileName);
                        }
                        else
                        {
                            if (File.Exists(newTrackFileName))
                            {
                                File.Delete(newTrackFileName);
                            }

                            File.Copy(track.File.FullName(directoryInfoToProcess), newTrackFileName);
                        }
                    }
                }
                File.Copy(releaseAndJsonFile[release],  Path.Combine(releaseDirInfo.FullName, release.ToMelodeeJsonName()));
            }
        }

        // Run PostDiscovery script
        if (_postDiscoveryScript.IsEnabled)
        {
            var postDiscoveryScriptResult = await _postDiscoveryScript.ProcessAsync(fileSystemDirectoryInfo, cancellationToken);
            if (!postDiscoveryScriptResult.IsSuccess)
            {
                return new OperationResult<bool>(postDiscoveryScriptResult.Messages)
                {
                    Errors = postDiscoveryScriptResult.Errors,
                    Data = false
                };
            }
        }

        return new OperationResult<bool>
        {
            Data = true
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

    private async Task<OperationResult<IEnumerable<Release>>> AllReleasesForDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var releases = new List<Release>();
        var messages = new List<string>();
        var viaPlugins = new List<string>();
        
        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (dirInfo.Exists)
        {
            using (Operation.Time("AllReleasesForDirectoryAsync [{directoryInfo}]", fileSystemDirectoryInfo.Name))
            {
                var tracks = new List<Track>();
                foreach (var fileSystemInfo in dirInfo.EnumerateFileSystemInfos("*.*", SearchOption.TopDirectoryOnly))
                {
                    var fsi = fileSystemInfo.ToFileSystemInfo();
                    foreach (var plugin in _trackPlugins.OrderBy(x => x.SortOrder))
                    {
                        if (plugin.DoesHandleFile(fileSystemDirectoryInfo, fsi))
                        {
                            using (Operation.Time("File [{File}] Plugin [{Plugin}]", fileSystemInfo.Name, plugin.DisplayName))
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
                                new MetaTag<object?> { Identifier = MetaTagIdentifier.Album, Value = track.ReleaseTitle(), SortOrder = 1 },
                                new MetaTag<object?> { Identifier = MetaTagIdentifier.Artist, Value = track.Artist(), SortOrder = 2 },
                                new MetaTag<object?> { Identifier = MetaTagIdentifier.DiscNumber, Value = track.MediaNumber(), SortOrder = 3 },
                                new MetaTag<object?> { Identifier = MetaTagIdentifier.OrigReleaseYear, Value = track.ReleaseYear(), SortOrder = 100 },
                                new MetaTag<object?> { Identifier = MetaTagIdentifier.TrackTotal, Value = trackTotal, SortOrder = 101 }
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
                                OriginalDirectory = fileSystemDirectoryInfo,
                                Tags = newReleaseTags,
                                Tracks = tracks,
                                ViaPlugins = viaPlugins.Distinct().ToArray()
                            });
                        }
                    }
                }

                // Now all tracks have been found renumber SortOrder 
                Parallel.ForEach(releases, (release) =>
                {
                    if (release.Tracks != null)
                    {
                        foreach (var track in release.Tracks)
                        {
                            track.SortOrder = track.TrackNumber();
                        }
                    }
                });
            }
        }

        return new OperationResult<IEnumerable<Release>>(messages)
        {
            Data = releases
        };
    }
}