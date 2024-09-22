using System.Text.Json;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.Cards;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Plugins.MetaData.Directory;
using Melodee.Plugins.MetaData.Track;
using Melodee.Plugins.Processor;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Plugins.Discovery.Releases;

public sealed class ReleasesDiscoverer : IReleasesDiscoverer
{
    private readonly Configuration _configuration;

    private readonly IEnumerable<IDirectoryPlugin> _enabledReleasePlugins;
    private readonly IDictionary<FileSystemDirectoryInfo, IEnumerable<Release>> _releaseCache = new Dictionary<FileSystemDirectoryInfo, IEnumerable<Release>>();

    private readonly IEnumerable<ITrackPlugin> _trackPlugins;

    public ReleasesDiscoverer(Configuration configuration)
    {
        _configuration = configuration;
        var config = configuration;

        _trackPlugins = new ITrackPlugin[]
        {
            new AtlMetaTag(new MetaTagsProcessor(config), config)
        };
        _enabledReleasePlugins = new IDirectoryPlugin[]
        {
            new CueSheet(_trackPlugins, config),
            new Nfo(config),
            new M3UPlaylist(_trackPlugins, config),
            new SimpleFileVerification(_trackPlugins, config)
        };
    }

    public string DisplayName => nameof(ReleasesDiscoverer);

    public string Id => "3528BA3F-4130-4913-9C9F-C7F0F8FF2B4D";

    public bool IsEnabled { get; set; } = true;

    public int SortOrder => 0;

    public void ClearCache()
    {
        _releaseCache.Clear();
    }

    public async Task<Release> ReleaseByUniqueIdAsync(
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
        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);

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

        if (!string.IsNullOrWhiteSpace(pagedRequest.Search))
        {
            releases = releases.Where(x =>
                (x.ReleaseTitle() != null && x.ReleaseTitle()!.Contains(pagedRequest.Search, StringComparison.CurrentCultureIgnoreCase)) ||
                (x.Artist() != null && x.Artist()!.Contains(pagedRequest.Search, StringComparison.CurrentCultureIgnoreCase)))?.ToList();
        }

        if (pagedRequest.Filter != ReleaseResultFilter.All && releases != null && releases.Count != 0)
        {
            switch (pagedRequest.Filter)
            {
                case ReleaseResultFilter.Duplicates:
                    var duplicates = releases
                        .GroupBy(x => x.UniqueId)
                        .Where(x => x.Count() > 1)
                        .Select(x => x.Key);
                    releases = releases.Where(x => duplicates.Contains(x.UniqueId)).ToList();
                    break;

                case ReleaseResultFilter.Incomplete:
                    releases = releases.Where(x => x.Status == ReleaseStatus.Incomplete).ToList();
                    break;

                case ReleaseResultFilter.LessThanConfiguredTracks:
                    releases = releases.Where(x => x.Tracks?.Count() < _configuration.FilterLessThanTrackCount || x.TrackTotalValue() < _configuration.FilterLessThanTrackCount).ToList();
                    break;

                case ReleaseResultFilter.NeedsAttention:
                    releases = releases.Where(x => x.Status == ReleaseStatus.NeedsAttention).ToList();
                    break;

                case ReleaseResultFilter.New:
                    releases = releases.Where(x => x.Status == ReleaseStatus.New).ToList();
                    break;

                case ReleaseResultFilter.ReadyToMove:
                    releases = releases.Where(x => x.Status is ReleaseStatus.Ok or ReleaseStatus.Reviewed).ToList();
                    break;

                case ReleaseResultFilter.Selected:
                    if (pagedRequest.SelectedReleaseIds.Length > 0)
                    {
                        releases = releases.Where(x => pagedRequest.SelectedReleaseIds.Contains(x.UniqueId)).ToList();
                    }

                    break;

                case ReleaseResultFilter.LessThanConfiguredDuration:
                    releases = releases.Where(x => x.TotalDuration() < _configuration.FilterLessThanConfiguredDuration).ToList();
                    break;
            }
        }

        var releasesCount = releases?.Count ?? 0;
        return new PagedResult<Release>
        {
            TotalCount = releasesCount,
            TotalPages = (releasesCount + pagedRequest.TakeValue - 1) / pagedRequest.TakeValue,
            Data = (releases ?? [])
                .OrderBy(x => x.SortValue)
                .Skip(pagedRequest.SkipValue)
                .Take(pagedRequest.TakeValue)
        };
    }

    public async Task<PagedResult<ReleaseCard>> ReleasesGridsForDirectoryAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        var releasesForDirectoryInfo = await ReleasesForDirectoryAsync(fileSystemDirectoryInfo, pagedRequest, cancellationToken);
        var data = releasesForDirectoryInfo.Data.Select(async x => new ReleaseCard
        {
            Artist = x.Artist(),
            Created = x.Created,
            Duration = x.Duration(),
            Directory = x.Directory?.FullName() ?? fileSystemDirectoryInfo.FullName(),
            ImageBytes = await x.CoverImageBytesAsync(),
            IsValid = x.IsValid(_configuration),
            Title = x.ReleaseTitle(),
            Year = x.ReleaseYear(),
            TrackCount = x.TrackTotalValue(),
            ReleaseStatus = x.Status,
            ViaPlugins = x.ViaPlugins,
            UniqueId = x.UniqueId
        });
        var d = await Task.WhenAll(data);
        return new PagedResult<ReleaseCard>
        {
            TotalCount = releasesForDirectoryInfo.TotalCount,
            TotalPages = releasesForDirectoryInfo.TotalPages,
            Data = d.OrderByDescending(x => x.Created).ToArray()
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
                                    r.Created = File.GetCreationTimeUtc(jsonFile.FullName);
                                    releases.Add(r);
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Warning(e, "Unable to load release json file [{FileName}]", dirInfo.FullName);
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
}
