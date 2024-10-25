using System.Text.Json;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.Cards;

using Melodee.Common.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Directory;
using Melodee.Plugins.MetaData.Song;
using Melodee.Plugins.Processor;
using Melodee.Plugins.Validation;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Plugins.Discovery.Albums;

public sealed class AlbumsDiscoverer : IAlbumsDiscoverer
{
    private readonly IAlbumValidator _albumValidator;
    private readonly Dictionary<string, object?> _configuration;

    private readonly IEnumerable<IDirectoryPlugin> _enabledAlbumPlugins;
    private readonly IDictionary<FileSystemDirectoryInfo, IEnumerable<Album>> _albumCache = new Dictionary<FileSystemDirectoryInfo, IEnumerable<Album>>();

    private readonly IEnumerable<ISongPlugin> _songPlugins;

    public AlbumsDiscoverer(IAlbumValidator albumValidator, IPluginsConfiguration configuration, ISerializer serializer)
    {
        _albumValidator = albumValidator;
        _configuration = configuration.Configuration;
        var config = configuration;

        _songPlugins = new ISongPlugin[]
        {
            new AtlMetaTag(new MetaTagsProcessor(config, serializer), config)
        };
        _enabledAlbumPlugins = new IDirectoryPlugin[]
        {
            new CueSheet(_songPlugins, config),
            new Nfo(config),
            new M3UPlaylist(_songPlugins, _albumValidator, config),
            new SimpleFileVerification(_songPlugins, _albumValidator, config)
        };
    }

    public string DisplayName => nameof(AlbumsDiscoverer);

    public string Id => "3528BA3F-4130-4913-9C9F-C7F0F8FF2B4D";

    public bool IsEnabled { get; set; } = true;

    public int SortOrder => 0;

    public void ClearCache()
    {
        _albumCache.Clear();
    }

    public async Task<Album> AlbumByUniqueIdAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo,
        long uniqueId,
        CancellationToken cancellationToken = default)
    {
        var result = (await AllMelodeeAlbumDataFilesForDirectoryAsync(fileSystemDirectoryInfo, cancellationToken)).Data.FirstOrDefault(x => x.UniqueId == uniqueId);
        if (result == null)
        {
            Log.Error("Unable to find Album by id[{UniqueId}]", uniqueId);
            return new Album
            {
                ViaPlugins = [],
                OriginalDirectory = fileSystemDirectoryInfo
            };
        }

        return result;
    }

    public async Task<PagedResult<Album>> AlbumsForDirectoryAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        var albums = new List<Album>();
        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);

        var dataForDirectoryInfoResult = await AllMelodeeAlbumDataFilesForDirectoryAsync(fileSystemDirectoryInfo, cancellationToken);
        if (dataForDirectoryInfoResult.IsSuccess)
        {
            albums.AddRange(dataForDirectoryInfoResult.Data);
        }

        foreach (var childDir in dirInfo.EnumerateDirectories("*.*", SearchOption.AllDirectories))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var dataForChildDirResult = await AllMelodeeAlbumDataFilesForDirectoryAsync(new FileSystemDirectoryInfo
            {
                Path = childDir.FullName,
                Name = childDir.Name
            }, cancellationToken);

            if (dataForChildDirResult.IsSuccess)
            {
                foreach (var r in dataForChildDirResult.Data)
                {
                    if (albums.All(x => x.UniqueId != r.UniqueId))
                    {
                        albums.Add(r);
                    }
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(pagedRequest.Search))
        {
            albums = albums.Where(x =>
                (x.AlbumTitle() != null && x.AlbumTitle()!.Contains(pagedRequest.Search, StringComparison.CurrentCultureIgnoreCase)) ||
                (x.Artist() != null && x.Artist()!.Contains(pagedRequest.Search, StringComparison.CurrentCultureIgnoreCase)))?.ToList();
        }

        if (pagedRequest.AlbumResultFilter != AlbumResultFilter.All && albums != null && albums.Count != 0)
        {
            switch (pagedRequest.AlbumResultFilter)
            {
                case AlbumResultFilter.Duplicates:
                    var duplicates = albums
                        .GroupBy(x => x.UniqueId)
                        .Where(x => x.Count() > 1)
                        .Select(x => x.Key);
                    albums = albums.Where(x => duplicates.Contains(x.UniqueId)).ToList();
                    break;

                case AlbumResultFilter.Incomplete:
                    albums = albums.Where(x => x.Status == AlbumStatus.Incomplete).ToList();
                    break;

                case AlbumResultFilter.LessThanConfiguredSongs:
                    var filterLessThanSongs = SafeParser.ToNumber<int>(_configuration[SettingRegistry.FilteringLessThanSongCount]);
                    albums = albums.Where(x => x.Songs?.Count() < filterLessThanSongs || x.SongTotalValue() < filterLessThanSongs).ToList();
                    break;

                case AlbumResultFilter.NeedsAttention:
                    albums = albums.Where(x => x.Status == AlbumStatus.NeedsAttention).ToList();
                    break;

                case AlbumResultFilter.New:
                    albums = albums.Where(x => x.Status == AlbumStatus.New).ToList();
                    break;

                case AlbumResultFilter.ReadyToMove:
                    albums = albums.Where(x => x.Status is AlbumStatus.Ok or AlbumStatus.Reviewed).ToList();
                    break;

                case AlbumResultFilter.Selected:
                    if (pagedRequest.SelectedAlbumIds.Length > 0)
                    {
                        albums = albums.Where(x => pagedRequest.SelectedAlbumIds.Contains(x.UniqueId)).ToList();
                    }

                    break;

                case AlbumResultFilter.LessThanConfiguredDuration:
                    var filterLessDuration = SafeParser.ToNumber<int>(_configuration[SettingRegistry.FilteringLessThanDuration]);
                    albums = albums.Where(x => x.TotalDuration() < filterLessDuration).ToList();
                    break;
            }
        }

        var albumsCount = albums?.Count ?? 0;
        return new PagedResult<Album>
        {
            TotalCount = albumsCount,
            TotalPages = (albumsCount + pagedRequest.PageSizeValue - 1) / pagedRequest.PageSizeValue,
            Data = (albums ?? [])
                .OrderBy(x => x.SortValue)
                .Skip(pagedRequest.SkipValue)
                .Take(pagedRequest.PageSizeValue)
        };
    }

    public async Task<PagedResult<AlbumCard>> AlbumsGridsForDirectoryAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        var albumsForDirectoryInfo = await AlbumsForDirectoryAsync(fileSystemDirectoryInfo, pagedRequest, cancellationToken);
        var data = albumsForDirectoryInfo.Data.Select(async x => new AlbumCard
        {
            Artist = x.Artist(),
            Created = x.Created,
            Duration = x.Duration(),
            Directory = x.Directory?.FullName() ?? fileSystemDirectoryInfo.FullName(),
            ImageBytes = await x.CoverImageBytesAsync(),
            IsValid = x.IsValid(_configuration),
            Title = x.AlbumTitle(),
            Year = x.AlbumYear(),
            SongCount = x.SongTotalValue(),
            AlbumStatus = x.Status,
            ViaPlugins = x.ViaPlugins,
            UniqueId = x.UniqueId
        });
        var d = await Task.WhenAll(data);
        return new PagedResult<AlbumCard>
        {
            TotalCount = albumsForDirectoryInfo.TotalCount,
            TotalPages = albumsForDirectoryInfo.TotalPages,
            Data = d.OrderByDescending(x => x.Created).ToArray()
        };
    }

    private async Task<OperationResult<IEnumerable<Album>>> AllMelodeeAlbumDataFilesForDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        var albums = new List<Album>();
        var errors = new List<Exception>();
        var messages = new List<string>();

        try
        {
            if (!_albumCache.ContainsKey(fileSystemDirectoryInfo))
            {
                var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
                if (dirInfo.Exists)
                {
                    using (Operation.At(LogEventLevel.Debug).Time("AllMelodeeAlbumDataFilesForDirectoryAsync [{directoryInfo}]", fileSystemDirectoryInfo.Name))
                    {
                        foreach (var jsonFile in dirInfo.EnumerateFiles(Album.JsonFileName, SearchOption.AllDirectories))
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            try
                            {
                                var r = JsonSerializer.Deserialize<Album>(await File.ReadAllBytesAsync(jsonFile.FullName, cancellationToken));
                                if (r != null)
                                {
                                    r.Directory = jsonFile.Directory?.ToDirectorySystemInfo();
                                    r.Created = File.GetCreationTimeUtc(jsonFile.FullName);
                                    albums.Add(r);
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Warning(e, "Deleting invalid Melodee Data file [{FileName}]", jsonFile.FullName);
                                messages.Add($"Deleting invalid Melodee Data file [{dirInfo.FullName}]");
                                jsonFile.Delete();
                            }
                        }
                    }
                }

                _albumCache.Add(fileSystemDirectoryInfo, albums);
            }
        }
        catch (Exception e)
        {
            Log.Warning("Unable to load Albums for [{DirInfo}]", fileSystemDirectoryInfo.FullName);
            errors.Add(e);
        }

        return new OperationResult<IEnumerable<Album>>(messages)
        {
            Errors = errors,
            Data = _albumCache[fileSystemDirectoryInfo]
        };
    }
}
