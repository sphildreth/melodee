using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.Grids;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Directory;
using Melodee.Plugins.MetaData.Track;
using Melodee.Plugins.Processor;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Plugins.Discovery.Releases;

public sealed class ReleasesDiscoverer : IReleasesDiscoverer
{
    public const short MinimumDiscNumber = 1;

    public const int MaximumDiscNumber = 500;    
 
    private readonly IEnumerable<ITrackPlugin> _trackPlugins;

    private readonly IEnumerable<IDirectoryPlugin> _enabledReleasePlugins;
    
    public string DisplayName => nameof(ReleasesDiscoverer);

    public string Id => "3528BA3F-4130-4913-9C9F-C7F0F8FF2B4D";

    public bool IsEnabled { get; set; } = true;
    
    public int SortOrder => 0;

    public ReleasesDiscoverer(Configuration configuration)
    {
        var config = configuration;
        
        _trackPlugins = new ITrackPlugin[]
        {
            new MetaTag(new MetaTagsProcessor(config), config)
        };
        _enabledReleasePlugins = new IDirectoryPlugin[]
        {
            new CueSheet(_trackPlugins, config),
            new Nfo(config),
            new M3UPlaylist(_trackPlugins, config),
            new SimpleFileVerification(_trackPlugins, config)
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
                OriginalDirectory = fileSystemDirectoryInfo
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
        return new PagedResult<Release>()
        {
            Data = releases
                .OrderBy(x => x.SortValue)
                .Skip(pagedRequest.SkipValue)
                .Take(pagedRequest.TakeValue)
        };        
    }

    private async Task<OperationResult<IEnumerable<Release>>> AllReleasesForDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var releases = new List<Release>();
        var messages = new List<string>();
        
        var dirInfo = new System.IO.DirectoryInfo(fileSystemDirectoryInfo.Path);
        if (dirInfo.Exists)
        {
            using (Operation.At(LogEventLevel.Debug).Time("AllReleasesForDirectoryAsync [{directoryInfo}]", fileSystemDirectoryInfo.Name))
            {
                foreach (var jsonFile in dirInfo.EnumerateFiles(Release.JsonFileName, SearchOption.AllDirectories))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    try
                    {
                        releases.Add(System.Text.Json.JsonSerializer.Deserialize<Release>(await File.ReadAllBytesAsync(jsonFile.FullName, cancellationToken))!);
                    }
                    catch (Exception e)
                    {
                        Log.Warning("Unable to load release json file [{FileName}]", dirInfo.FullName);
                        messages.Add($"Unable to load release json file [{dirInfo.FullName}]");
                    }
                }
            }
        }
        return new OperationResult<IEnumerable<Release>>(messages)
        {
            Data = releases
        };
    }

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
                TrackCount = x.TrackTotalValue(),
                ReleaseStatus = ReleaseStatus.NotSet,
                ViaPlugins = x.ViaPlugins,
                UniqueId = x.UniqueId
            })
        };
    }
}