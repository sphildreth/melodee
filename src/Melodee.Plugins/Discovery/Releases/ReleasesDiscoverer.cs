using System.Net.Security;
using System.Text.Json;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Cards;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Directory;
using Melodee.Plugins.MetaData.Track;
using Melodee.Plugins.Processor;
using Microsoft.VisualBasic;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Plugins.Discovery.Releases;

public sealed class ReleasesDiscoverer : IReleasesDiscoverer
{
    private readonly IDictionary<FileSystemDirectoryInfo, IEnumerable<Release>> _releaseCache = new Dictionary<FileSystemDirectoryInfo, IEnumerable<Release>>();
    
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
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
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
            TotalCount = releases.Count,
            TotalPages = (releases.Count + pagedRequest.TakeValue - 1) / pagedRequest.TakeValue,
            Data = releases
                .OrderBy(x => x.SortValue)
                .Skip(pagedRequest.SkipValue)
                .Take(pagedRequest.TakeValue)
        };        
    }

    private async Task<OperationResult<IEnumerable<Release>>> AllReleasesForDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var releases = new List<Release>();
        var errors = new List<Exception>();
        var messages = new List<string>();

        try
        {
            if (!_releaseCache.ContainsKey(fileSystemDirectoryInfo))
            {
                var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
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
                                var r = JsonSerializer.Deserialize<Release>(await File.ReadAllBytesAsync(jsonFile.FullName, cancellationToken));
                                if (r != null)
                                {
                                    r.Directory = jsonFile.Directory?.ToDirectorySystemInfo();
                                    releases.Add(r);
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Warning("Unable to load release json file [{FileName}]", dirInfo.FullName);
                                messages.Add($"Unable to load release json file [{dirInfo.FullName}]");
                            }
                        }
                    }
                }
                _releaseCache.Add(fileSystemDirectoryInfo, releases);
            }
        }
        catch (Exception e)
        {
            Log.Warning("Unable to load releases for [{DirInfo}]", fileSystemDirectoryInfo.FullName);
            errors.Add(e);
        }
        
        return new OperationResult<IEnumerable<Release>>(messages)
        {
            Errors = errors,
            Data = _releaseCache[fileSystemDirectoryInfo]
        };
    }

    public async Task<PagedResult<ReleaseCard>> ReleasesGridsForDirectoryAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo, 
        PagedRequest pagedRequest, 
        CancellationToken cancellationToken = default) {
        var releasesForDirectoryInfo = await ReleasesForDirectoryAsync(fileSystemDirectoryInfo, pagedRequest, cancellationToken);
        var data = releasesForDirectoryInfo.Data.Select(async x => new ReleaseCard
        {
            Artist = x.Artist(),
            Duration = x.Duration(),
            Directory = x.Directory?.FullName() ?? fileSystemDirectoryInfo.FullName(),
            ImageBytes = await x.CoverImageBytesAsync(),
            Title = x.ReleaseTitle(),
            Year = x.ReleaseYear(),
            TrackCount = x.TrackTotalValue(),
            ReleaseStatus = ReleaseStatus.NotSet,
            ViaPlugins = x.ViaPlugins,
            UniqueId = x.UniqueId
        });
        var d = await Task.WhenAll(data);
        return new PagedResult<ReleaseCard>
        {
            TotalCount = releasesForDirectoryInfo.TotalCount,
            TotalPages = releasesForDirectoryInfo.TotalPages,
            Data = d
        };
    }
}
