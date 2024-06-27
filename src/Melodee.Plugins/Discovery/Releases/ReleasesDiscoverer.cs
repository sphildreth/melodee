using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.Grids;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Directory;
using Melodee.Plugins.MetaData.Track;
using Serilog;
using SerilogTimings;
using SimpleFileVerification = Melodee.Plugins.MetaData.Directory.SimpleFileVerification;

namespace Melodee.Plugins.Discovery.Releases;

public sealed class ReleasesDiscoverer : IReleasesDiscoverer
{
    public const short MinimumDiscNumber = 1;

    public const int MaximumDiscNumber = 500;    
 
    private readonly IEnumerable<ITrackPlugin> _enabledTrackPlugins;

    private readonly IEnumerable<IDirectoryPlugin> _enabledReleasePlugins;
    
    
    public string DisplayName => nameof(ReleasesDiscoverer);

    public string Id => "3528BA3F-4130-4913-9C9F-C7F0F8FF2B4D";

    public bool IsEnabled { get; set; } = true;
    
    public int SortOrder => 0;


    public ReleasesDiscoverer(Configuration configuration)
    {
        var config = configuration;
        
        _enabledTrackPlugins = new ITrackPlugin[]
        {
            new MetaTag(config)
        };
        _enabledReleasePlugins = new IDirectoryPlugin[]
        {
            //  new CueSheet(),
            new M3UPlaylist(config),
            new SimpleFileVerification(_enabledTrackPlugins, config)
        };
    }
    
    public async Task<Common.Models.Release> ReleaseByUniqueIdAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo,
        long uniqueId,
        CancellationToken cancellationToken = default)
    {
        var result = (await AllReleasesForDirectoryAsync(fileSystemDirectoryInfo, cancellationToken)).Data.FirstOrDefault(x => x.UniqueId == uniqueId);
        if (result == null)
        {
            Log.Error("Unable to find Release by id[{UniqueId}]", uniqueId);
            return new Release
            {
                ViaPlugins = [],
                Directory = fileSystemDirectoryInfo
            };
        }
        return result;
    }

    public async Task<PagedResult<Release>> ReleasesForDirectoryAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo,
        PagedRequest pagedRequest, 
        CancellationToken cancellationToken = default)
    {
        var releases = new List<Release>();
        var dirInfo = new System.IO.DirectoryInfo(fileSystemDirectoryInfo.Path);
        
        var dataForDirectoryInfoResult = await AllReleasesForDirectoryAsync(fileSystemDirectoryInfo, cancellationToken);
        if (dataForDirectoryInfoResult.IsSuccess)
        {
            releases.AddRange(dataForDirectoryInfoResult.Data);
        }
        
        foreach (var childDir in dirInfo.EnumerateDirectories("*.*", SearchOption.AllDirectories))
        {
            var dataForChildDirResult = await AllReleasesForDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = childDir.FullName,
                Name = childDir.Name
            }, cancellationToken);

            if (dataForChildDirResult.IsSuccess)
            {
                foreach (var r in dataForChildDirResult.Data)
                {
                    if (releases.All(x => x.UniqueId != r.UniqueId))
                    {
                        releases.Add(r);
                    }
                }
            }
        }
        var resultReleases = releases
            .Skip(pagedRequest.SkipValue)
            .Take(pagedRequest.TakeValue);
     
        var processedReleasesViaPlugins = new List<Release>();
        await Parallel.ForEachAsync(resultReleases, cancellationToken, async (release, ct) =>
        {
            processedReleasesViaPlugins.Add(await FindImagesForRelease(release, ct));
        });
        
        return new PagedResult<Release>()
        {
            Data = processedReleasesViaPlugins
        };        
    }

    private async Task<OperationResult<IEnumerable<Release>>> AllReleasesForDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var releases = new List<Release>();
        var messages = new List<string>();
        var viaPlugins = new List<string>();
        var releaseFiles = new List<ReleaseFile>();
        
        var dirInfo = new System.IO.DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (dirInfo.Exists)
        {
            using (Operation.Time("AllReleasesForDirectoryAsync [{directoryInfo}]", fileSystemDirectoryInfo.Name))
            {
                var tracks = new List<Track>();
                foreach (var fileSystemInfo in dirInfo.EnumerateFileSystemInfos("*.*", SearchOption.TopDirectoryOnly))
                {
                    var fsi = fileSystemInfo.ToFileSystemInfo();
                    foreach (var plugin in _enabledTrackPlugins.OrderBy(x => x.SortOrder))
                    {
                        if (plugin.DoesHandleFile(fsi))
                        {
                            using (Operation.Time("File [{File}] Plugin [{Plugin}]", fileSystemInfo.Name, plugin.DisplayName))
                            {
                                var pluginResult = await plugin.ProcessFileAsync(fsi, cancellationToken);
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
                            foundRelease = foundRelease.MergeTracks(tracks);
                        }
                        else
                        {
                            var newReleaseTags = new List<MetaTag<object?>>
                            {
                                new MetaTag<object?> { Identifier = MetaTagIdentifier.Album, Value = track.ReleaseTitle(), SortOrder = 1},
                                new MetaTag<object?> { Identifier = MetaTagIdentifier.Artist, Value = track.Artist(), SortOrder = 2 },
                                new MetaTag<object?> { Identifier = MetaTagIdentifier.DiscNumber, Value = track.MediaNumber(), SortOrder = 3 },
                                new MetaTag<object?> { Identifier = MetaTagIdentifier.OrigReleaseYear, Value = track.ReleaseYear(), SortOrder = 100 },
                                new MetaTag<object?> { Identifier = MetaTagIdentifier.TrackTotal, Value = track.TrackTotalNumber(), SortOrder = 101 }
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
                                Directory = fileSystemDirectoryInfo,
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

    private async Task<Release> FindImagesForRelease(Release release, CancellationToken cancellationToken = default)
    {
        var imageInfos = new List<ImageInfo>();
        var imageFiles = ImageHelper.ImageFilesInDirectory(release.Directory.Path, SearchOption.TopDirectoryOnly);
        foreach (var imageFile in imageFiles)
        {
            var fileInfo = new System.IO.FileInfo(imageFile);
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
                        PictureIdentifier = pictureIdentifier,
                        Bytes = await File.ReadAllBytesAsync(fileInfo.FullName, cancellationToken),
                        Width = imageInfo.Width,
                        Height = imageInfo.Height,
                        SortOrder = 0
                    });
                }
            }
        }

        if (imageInfos.Count == 0 && (release.Tracks ?? Array.Empty<Track>()).Any())
        {
            var allTrackImages = release.Tracks?
                .Where(x => x.Images != null)?
                .SelectMany(x => x.Images!)?
                .ToArray() ?? [];
            var firstTrackImageOfEachGroup = allTrackImages.GroupBy(x => x.PictureIdentifier).FirstOrDefault();
            if (firstTrackImageOfEachGroup != null && firstTrackImageOfEachGroup.Any())
            {
                imageInfos.AddRange(firstTrackImageOfEachGroup);
            }
        }
        release.Images = imageInfos;
        return release;
    }

    // private async Task<Release> ProcessReleasePluginsOnRelease(Release release, CancellationToken cancellationToken = default)
    // {
    //     // Process the given release in each enabled Release plugin. 
    //     //  A release plugin example is sfv which parses the Sfv and checks the files CRC and then marks the Release as complete if all sfv track files are found and valid.
    //     
    //     foreach (var plugin in _enabledReleasePlugins.OrderBy(x => x.SortOrder))
    //     {
    //         using (Operation.Time("ProcessReleasePluginsOnRelease [{Release}] Plugin [{Plugin}]", release.ToString(), plugin.DisplayName))
    //         {
    //             var pluginResult = await plugin.ProcessReleaseAsync(release, cancellationToken);
    //             if (pluginResult.IsSuccess)
    //             {
    //                 release = pluginResult.Data;
    //             }
    //         }
    //     }
    //     return release;
    // }
    

    public async Task<PagedResult<ReleaseGrid>> ReleasesGridsForDirectoryAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo, 
        PagedRequest pagedRequest, 
        CancellationToken cancellationToken = default) {

        var releasesForDirectoryInfo = await ReleasesForDirectoryAsync(fileSystemDirectoryInfo, pagedRequest, cancellationToken);
        
        return new PagedResult<ReleaseGrid>
        {
            Data = releasesForDirectoryInfo.Data.Select(x => new ReleaseGrid
            {
                Artist = x.Artist(),
                Title = x.ReleaseTitle(),
                Year = x.ReleaseYear(),
                TrackCount = x.TrackCountValue(),
                ReleaseStatus = ReleaseStatus.NotSet,
                ViaPlugins = x.ViaPlugins,
                UniqueId = x.UniqueId
            })
        };
    }
}