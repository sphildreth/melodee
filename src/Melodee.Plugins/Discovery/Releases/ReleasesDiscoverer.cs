using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.Grids;
using Melodee.Plugins.MetaData.Release;
using Melodee.Plugins.MetaData.Track;
using Serilog;
using SerilogTimings;
using DirectoryInfo = Melodee.Common.Models.DirectoryInfo;
using SimpleFileVerification = Melodee.Plugins.MetaData.Release.SimpleFileVerification;

namespace Melodee.Plugins.Discovery.Releases;

public sealed class ReleasesDiscoverer : IReleasesDiscoverer
{
    public const short MinimumDiscNumber = 1;

    public const int MaximumDiscNumber = 500;    
    
    public string DisplayName => nameof(ReleasesDiscoverer);

    public string Id => "3528BA3F-4130-4913-9C9F-C7F0F8FF2B4D";

    public bool IsEnabled { get; set; } = true;
    
    public int SortOrder => 0;

    private IEnumerable<ITrackPlugin> EnabledTrackPlugins { get; } 

    private IEnumerable<IReleasePlugin> EnabledReleasePlugins { get; }

    public ReleasesDiscoverer(Configuration configuration)
    {
        var config = configuration;
        
        EnabledTrackPlugins = new ITrackPlugin[]
        {
            new MetaTag(config)
        };
        EnabledReleasePlugins = new IReleasePlugin[]
        {
            //  new CueSheet(),
            new M3UPlaylist(config),
            new SimpleFileVerification(config)
        };
    }
    
    public async Task<Common.Models.Release> ReleaseByUniqueIdAsync(
        DirectoryInfo directoryInfo,
        long uniqueId,
        CancellationToken cancellationToken = default)
    {
        var result = (await AllReleasesForDirectoryAsync(directoryInfo, cancellationToken)).Data.FirstOrDefault(x => x.UniqueId == uniqueId);
        if (result == null)
        {
            Log.Error("Unable to find Release by id[{UniqueId}]", uniqueId);
            return new Release
            {
                ViaPlugins = [],
                DirectoryInfo = directoryInfo
            };
        }
        return result;
    }

    public async Task<PagedResult<Release>> ReleasesForDirectoryAsync(
        DirectoryInfo directoryInfo,
        PagedRequest pagedRequest, 
        CancellationToken cancellationToken = default)
    {
        var releases = new List<Release>();
        var dirInfo = new System.IO.DirectoryInfo(directoryInfo.Path);
        
        var dataForDirectoryInfoResult = await AllReleasesForDirectoryAsync(directoryInfo, cancellationToken);
        if (dataForDirectoryInfoResult.IsSuccess)
        {
            releases.AddRange(dataForDirectoryInfoResult.Data);
        }
        
        foreach (var childDir in dirInfo.EnumerateDirectories("*.*", SearchOption.AllDirectories))
        {
            var dataForChildDirResult = await AllReleasesForDirectoryAsync(new DirectoryInfo
            {
                Path = childDir.FullName,
                ShortName = childDir.Name
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
            processedReleasesViaPlugins.Add(await ProcessReleasePluginsOnRelease(release, ct));
        });
        
        return new PagedResult<Release>()
        {
            Data = processedReleasesViaPlugins
        };        
    }

    private async Task<OperationResult<IEnumerable<Release>>> AllReleasesForDirectoryAsync(DirectoryInfo directoryInfo, CancellationToken cancellationToken = default)
    {
        var releases = new List<Release>();
        var messages = new List<string>();
        var viaPlugins = new List<string>();
        
        var dirInfo = new System.IO.DirectoryInfo(directoryInfo.Path);
        if (dirInfo.Exists)
        {
            using (Operation.Time("AllReleasesForDirectoryAsync [{directoryInfo}]", directoryInfo.ShortName))
            {
                var tracks = new List<Track>();
                foreach (var fileSystemInfo in dirInfo.EnumerateFileSystemInfos("*.*", SearchOption.TopDirectoryOnly))
                {
                    foreach (var plugin in EnabledTrackPlugins.OrderBy(x => x.SortOrder))
                    {
                        if (plugin.DoesHandleFile(fileSystemInfo))
                        {
                            using (Operation.Time("File [{File}] Plugin [{Plugin}]", fileSystemInfo.Name, plugin.DisplayName))
                            {
                                var pluginResult = await plugin.ProcessFileAsync(fileSystemInfo, cancellationToken);
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
                                DirectoryInfo = directoryInfo,
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

    private async Task<Release> ProcessReleasePluginsOnRelease(Release release, CancellationToken cancellationToken = default)
    {
        // Process the given release in each enabled Release plugin. 
        //  A release plugin example is sfv which parses the Sfv and checks the files CRC and then marks the Release as complete if all sfv track files are found and valid.
        
        foreach (var plugin in EnabledReleasePlugins.OrderBy(x => x.SortOrder))
        {
            using (Operation.Time("ProcessReleasePluginsOnRelease [{Release}] Plugin [{Plugin}]", release.ToString(), plugin.DisplayName))
            {
                var pluginResult = await plugin.ProcessReleaseAsync(release, cancellationToken);
                if (pluginResult.IsSuccess)
                {
                    release = pluginResult.Data;
                }
            }
        }
        return release;
    }
    

    public async Task<PagedResult<ReleaseGrid>> ReleasesGridsForDirectoryAsync(
        DirectoryInfo directoryInfo, 
        PagedRequest pagedRequest, 
        CancellationToken cancellationToken = default) {

        var releasesForDirectoryInfo = await ReleasesForDirectoryAsync(directoryInfo, pagedRequest, cancellationToken);
        
        return new PagedResult<ReleaseGrid>()
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