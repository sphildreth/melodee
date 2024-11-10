using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Common.Models.Cards;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Utility;
using Melodee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using SerilogTimings;


namespace Melodee.Services.Scanning;

/// <summary>
/// Service that returns Albums found from scanning media.
/// </summary>
public sealed class AlbumDiscoveryService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    SettingService settingService,
    ISerializer serializer)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private bool _initialized;
    private IMelodeeConfiguration _configuration = new MelodeeConfiguration([]);

    public async Task InitializeAsync(IMelodeeConfiguration? configuration = null, CancellationToken token = default)
    {
        _configuration = configuration ?? await settingService.GetMelodeeConfigurationAsync(token).ConfigureAwait(false);
        _initialized = true;
    }

    private void CheckInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Album discovery service is not initialized.");
        }
    }

    public async Task<Album> AlbumByUniqueIdAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo,
        long uniqueId,
        CancellationToken cancellationToken = default)
    {
        CheckInitialized();
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
        CheckInitialized();
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
                (x.Artist() != null && x.Artist()!.Contains(pagedRequest.Search, StringComparison.CurrentCultureIgnoreCase))).ToList();
        }

        if (pagedRequest.AlbumResultFilter != AlbumResultFilter.All && albums.Count != 0)
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
                    albums = albums.Where(x => x.Status == AlbumStatus.NeedsAttention).ToList();
                    break;

                case AlbumResultFilter.LessThanConfiguredSongs:
                    var filterLessThanSongs = SafeParser.ToNumber<int>(_configuration.Configuration[SettingRegistry.FilteringLessThanSongCount]);
                    albums = albums.Where(x => x.Songs?.Count() < filterLessThanSongs || x.SongTotalValue() < filterLessThanSongs).ToList();
                    break;

                case AlbumResultFilter.NeedsAttention:
                    albums = albums.Where(x => x.Status == AlbumStatus.NeedsAttention).ToList();
                    break;

                case AlbumResultFilter.New:
                    albums = albums.Where(x => x.Status == AlbumStatus.New).ToList();
                    break;

                case AlbumResultFilter.ReadyToMove:
                    albums = albums.Where(x => x.Status is AlbumStatus.Ok).ToList();
                    break;

                case AlbumResultFilter.Selected:
                    if (pagedRequest.SelectedAlbumIds.Length > 0)
                    {
                        albums = albums.Where(x => pagedRequest.SelectedAlbumIds.Contains(x.UniqueId)).ToList();
                    }

                    break;

                case AlbumResultFilter.LessThanConfiguredDuration:
                    var filterLessDuration = SafeParser.ToNumber<int>(_configuration.Configuration[SettingRegistry.FilteringLessThanDuration]);
                    albums = albums.Where(x => x.TotalDuration() < filterLessDuration).ToList();
                    break;
            }
        }

        if (pagedRequest.FilterBy != null)
        {
            foreach (var filterBy in pagedRequest.FilterBy)
            {
                switch (filterBy.PropertyName)
                {
                    case "Artist":
                        albums = albums.Where(x => x.Artist() != null && x.Artist()!.ToUpperInvariant().Contains(filterBy.Value?.ToString()?.ToUpperInvariant() ?? string.Empty)).ToList();
                        break;

                    case "AlbumStatus":
                        var filterStatusValue = SafeParser.ToEnum<AlbumStatus>(filterBy.Value);
                        albums = albums.Where(x => x.Status == filterStatusValue).ToList();
                        break;

                    case "Year":
                        var filterYearValue = SafeParser.ToNumber<int>(filterBy.Value);
                        albums = albums.Where(x => x.AlbumYear() == filterYearValue).ToList();
                        break;
                }
            }
        }

        var albumsCount = albums.Count;
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
        CheckInitialized();
        var albumsForDirectoryInfo = await AlbumsForDirectoryAsync(fileSystemDirectoryInfo, pagedRequest, cancellationToken);
        var data = albumsForDirectoryInfo.Data.Select(async x => new AlbumCard
        {
            Artist = x.Artist(),
            Created = x.Created,
            Duration = x.Duration(),
            MelodeeDataFileName = Path.Combine(x.Directory?.FullName() ?? fileSystemDirectoryInfo.FullName(), x.ToMelodeeJsonName()),
            ImageBytes = await x.CoverImageBytesAsync(),
            IsValid = x.IsValid(_configuration.Configuration),
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
        CheckInitialized();

        var albums = new List<Album>();
        var errors = new List<Exception>();
        var messages = new List<string>();

        try
        {
            var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
            if (dirInfo.Exists)
            {
                using (Operation.At(LogEventLevel.Debug).Time("AllMelodeeAlbumDataFilesForDirectoryAsync [{directoryInfo}]", fileSystemDirectoryInfo.Name))
                {
                    foreach (var jsonFile in dirInfo.EnumerateFiles($"*{Album.JsonFileName}", SearchOption.AllDirectories))
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        try
                        {
                            var r = serializer.Deserialize<Album>(await File.ReadAllBytesAsync(jsonFile.FullName, cancellationToken));
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
        }
        catch (Exception e)
        {
            Log.Warning("Unable to load Albums for [{DirInfo}]", fileSystemDirectoryInfo.FullName);
            errors.Add(e);
        }

        return new OperationResult<IEnumerable<Album>>(messages)
        {
            Errors = errors,
            Data = albums.ToArray()
        };
    }
}
