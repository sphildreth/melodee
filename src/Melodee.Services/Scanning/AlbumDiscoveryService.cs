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

    //data:image/jpeg;base64,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
    
    public async Task InitializeAsync(CancellationToken token = default)
    {
        _configuration = await settingService.GetMelodeeConfigurationAsync(token).ConfigureAwait(false);
        _initialized = true;
    }

    private void CheckInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Albums discovery service is not initialized.");
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
                    albums = albums.Where(x => x.Status == AlbumStatus.Incomplete).ToList();
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
                    albums = albums.Where(x => x.Status is AlbumStatus.Ok or AlbumStatus.Reviewed).ToList();
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
                    foreach (var jsonFile in dirInfo.EnumerateFiles($"*.{ Album.JsonFileName}", SearchOption.AllDirectories))
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
