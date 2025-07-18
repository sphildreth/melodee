using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Filtering;
using Melodee.Common.Models;
using Melodee.Common.Models.Collection;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Services.Caching;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;

namespace Melodee.Common.Services.Scanning;

/// <summary>
///     Service that returns Albums found from scanning media.
/// </summary>
public sealed class AlbumDiscoveryService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory,
    IFileSystemService fileSystemService)
    : ServiceBase(logger, cacheManager, contextFactory), IDisposable
{
    private static readonly ParallelOptions DefaultParallelOptions = new()
    {
        MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2)
    };

    private readonly SemaphoreSlim _cacheUpdateSemaphore = new(1, 1);

    // Performance optimizations
    private readonly ConcurrentDictionary<string, (DateTime LastWriteTime, Album[] Albums)> _directoryCache = new();
    private IAlbumValidator _albumValidator = null!;
    private IMelodeeConfiguration _configuration = new MelodeeConfiguration([]);
    private bool _initialized;

    public void Dispose()
    {
        _cacheUpdateSemaphore.Dispose();
        _directoryCache.Clear();
    }

    public async Task InitializeAsync(IMelodeeConfiguration? configuration = null, CancellationToken token = default)
    {
        _configuration = configuration ?? await configurationFactory.GetConfigurationAsync(token).ConfigureAwait(false);
        _albumValidator = new AlbumValidator(_configuration);
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
        Guid id,
        CancellationToken cancellationToken = default)
    {
        CheckInitialized();
        var result =
            (await AllMelodeeAlbumDataFilesForDirectoryAsync(fileSystemDirectoryInfo, cancellationToken)).Data
            ?.FirstOrDefault(x => x.Id == id);
        if (result == null)
        {
            Log.Error("Unable to find Album by id [{Id}] in [{DirectoryName}]", id, fileSystemDirectoryInfo.FullName());
            return new Album
            {
                Artist = new Artist(string.Empty, string.Empty, null),
                Directory = fileSystemDirectoryInfo,
                ViaPlugins = [],
                OriginalDirectory = fileSystemDirectoryInfo
            };
        }

        return result;
    }

    private async Task<PagedResult<Album>> AlbumsForDirectoryAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return new PagedResult<Album>
            {
                TotalCount = 0,
                TotalPages = 0,
                Data = []
            };
        }

        // Use HashSet for O(1) duplicate detection instead of O(n) All() checks
        var albumIds = new HashSet<Guid>();
        var albums = new List<Album>();

        var dataForDirectoryInfoResult =
            await AllMelodeeAlbumDataFilesForDirectoryAsync(fileSystemDirectoryInfo, cancellationToken);
        if (dataForDirectoryInfoResult is { IsSuccess: true, Data: not null })
        {
            foreach (var album in dataForDirectoryInfoResult.Data)
            {
                if (albumIds.Add(album.Id))
                {
                    albums.Add(album);
                }
            }
        }

        // Check cancellation before starting parallel operations
        if (cancellationToken.IsCancellationRequested)
        {
            return new PagedResult<Album>
            {
                TotalCount = albums.Count,
                TotalPages = 1,
                Data = albums.Skip(pagedRequest.SkipValue).Take(pagedRequest.PageSizeValue)
            };
        }

        // Use parallel processing for directory enumeration with controlled concurrency
        var childDirectories = fileSystemService.EnumerateDirectories(fileSystemDirectoryInfo.Path, "*.*", SearchOption.AllDirectories);
        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 4) // Conservative for I/O operations
        };

        var additionalAlbums = new ConcurrentBag<Album>();

        try
        {
            await Parallel.ForEachAsync(childDirectories, parallelOptions, async (childDir, token) =>
            {
                // Check cancellation at start of each iteration
                if (token.IsCancellationRequested)
                {
                    return;
                }

                var dataForChildDirResult = await AllMelodeeAlbumDataFilesForDirectoryAsync(new FileSystemDirectoryInfo
                {
                    Path = childDir.FullName,
                    Name = childDir.Name
                }, token);

                if (dataForChildDirResult is { IsSuccess: true, Data: not null })
                {
                    foreach (var album in dataForChildDirResult.Data)
                    {
                        additionalAlbums.Add(album);
                    }
                }
            });
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation gracefully - just continue with what we have
        }

        // Merge results with efficient duplicate detection
        foreach (var album in additionalAlbums)
        {
            if (albumIds.Add(album.Id))
            {
                albums.Add(album);
            }
        }

        // Apply filters early to reduce memory pressure
        albums = ApplyFilters(albums, pagedRequest);

        // Apply sorting with optimized comparisons
        albums = ApplySorting(albums, pagedRequest);

        var albumsCount = albums.Count;
        return new PagedResult<Album>
        {
            TotalCount = albumsCount,
            TotalPages = (albumsCount + pagedRequest.PageSizeValue - 1) / pagedRequest.PageSizeValue,
            Data = albums
                .Skip(pagedRequest.SkipValue)
                .Take(pagedRequest.PageSizeValue)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private List<Album> ApplyFilters(List<Album> albums, PagedRequest pagedRequest)
    {
        if (pagedRequest.AlbumResultFilter != AlbumResultFilter.All && albums.Count != 0)
        {
            albums = pagedRequest.AlbumResultFilter switch
            {
                AlbumResultFilter.Duplicates => albums
                    .GroupBy(x => x.Id)
                    .Where(x => x.Count() > 1)
                    .SelectMany(x => x)
                    .ToList(),

                AlbumResultFilter.Incomplete or AlbumResultFilter.NeedsAttention =>
                    albums.Where(x => x.Status == AlbumStatus.Invalid).ToList(),

                AlbumResultFilter.LessThanConfiguredSongs => FilterByMinSongs(albums),

                AlbumResultFilter.New => albums.Where(x => x.Status == AlbumStatus.New).ToList(),

                AlbumResultFilter.ReadyToMove => albums.Where(x => x.Status is AlbumStatus.Ok).ToList(),

                AlbumResultFilter.Selected when pagedRequest.SelectedAlbumIds.Length > 0 =>
                    FilterBySelectedIds(albums, pagedRequest.SelectedAlbumIds),

                AlbumResultFilter.LessThanConfiguredDuration => FilterByMinDuration(albums),

                _ => albums
            };
        }

        if (pagedRequest.FilterBy != null)
        {
            foreach (var filterBy in pagedRequest.FilterBy)
            {
                albums = ApplyPropertyFilter(albums, filterBy);
            }
        }

        return albums;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private List<Album> FilterByMinSongs(List<Album> albums)
    {
        var filterLessThanSongs = SafeParser.ToNumber<int>(
            _configuration.Configuration[SettingRegistry.FilteringLessThanSongCount]);
        return albums.Where(x => x.Songs?.Count() < filterLessThanSongs || x.SongTotalValue() < filterLessThanSongs).ToList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private List<Album> FilterBySelectedIds(List<Album> albums, Guid[] selectedIds)
    {
        var selectedSet = new HashSet<Guid>(selectedIds);
        return albums.Where(x => selectedSet.Contains(x.Id)).ToList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private List<Album> FilterByMinDuration(List<Album> albums)
    {
        var filterLessDuration = SafeParser.ToNumber<int>(
            _configuration.Configuration[SettingRegistry.FilteringLessThanDuration]);
        return albums.Where(x => x.TotalDuration() < filterLessDuration).ToList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private List<Album> ApplyPropertyFilter(List<Album> albums, FilterOperatorInfo filterBy)
    {
        return filterBy.PropertyName switch
        {
            "ArtistName" => albums.Where(x =>
                x.Artist.NameNormalized.Contains(filterBy.Value.ToString()?.ToNormalizedString() ?? string.Empty)).ToList(),

            "AlbumStatus" => albums.Where(x =>
                x.Status == SafeParser.ToEnum<AlbumStatus>(filterBy.Value)).ToList(),

            "NameNormalized" => FilterByNameNormalized(albums, filterBy.Value.ToString() ?? string.Empty),

            "ReleaseDate" => albums.Where(x =>
                x.AlbumYear() == SafeParser.ToNumber<int>(filterBy.Value)).ToList(),

            _ => albums
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private List<Album> FilterByNameNormalized(List<Album> albums, string filterValue)
    {
        return albums.Where(x =>
            x.AlbumTitle()?.Contains(filterValue, StringComparison.CurrentCultureIgnoreCase) == true ||
            x.Artist.Name.Contains(filterValue, StringComparison.CurrentCultureIgnoreCase)).ToList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private List<Album> ApplySorting(List<Album> albums, PagedRequest pagedRequest)
    {
        return pagedRequest.OrderByValue("SortOrder") switch
        {
            "\"Artist\" ASC" => albums.OrderBy(x => x.Artist.SortName).ToList(),
            "\"Artist\" DESC" => albums.OrderByDescending(x => x.Artist.SortName).ToList(),
            "\"CreatedAt\" ASC" => albums.OrderBy(x => x.Created).ToList(),
            "\"CreatedAt\" DESC" => albums.OrderByDescending(x => x.Created).ToList(),
            "\"Duration\" ASC" => albums.OrderBy(x => x.Duration()).ToList(),
            "\"Duration\" DESC" => albums.OrderByDescending(x => x.Duration()).ToList(),
            "\"NeedsAttentionReasonsValue\" ASC" => albums.OrderBy(x => x.StatusReasons).ToList(),
            "\"NeedsAttentionReasonsValue\" DESC" => albums.OrderByDescending(x => x.StatusReasons).ToList(),
            "\"Title\" ASC" => albums.OrderBy(x => x.AlbumTitle()).ToList(),
            "\"Title\" DESC" => albums.OrderByDescending(x => x.AlbumTitle()).ToList(),
            "\"Year\" ASC" => albums.OrderBy(x => x.AlbumYear()).ToList(),
            "\"Year\" DESC" => albums.OrderByDescending(x => x.AlbumYear()).ToList(),
            "\"Status\" ASC" => albums.OrderBy(x => x.Status).ToList(),
            "\"Status\" DESC" => albums.OrderByDescending(x => x.Status).ToList(),
            "\"SongCount\" ASC" => albums.OrderBy(x => x.SongTotalValue()).ToList(),
            "\"SongCount\" DESC" => albums.OrderByDescending(x => x.SongTotalValue()).ToList(),
            _ => albums
        };
    }

    public async Task<bool> DeleteAlbumsAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo,
        Func<Album, bool> condition, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var result = false;
        var albumsForDirectoryInfo = await AlbumsForDirectoryAsync(fileSystemDirectoryInfo,
            new PagedRequest { PageSize = short.MaxValue }, cancellationToken);
        if (albumsForDirectoryInfo.Data.Any())
        {
            foreach (var album in albumsForDirectoryInfo.Data)
            {
                if (!condition(album))
                {
                    continue;
                }

                var directoryName = fileSystemService.GetDirectoryName(album.MelodeeDataFileName ?? string.Empty);
                if (string.IsNullOrEmpty(directoryName))
                {
                    continue;
                }

                fileSystemService.DeleteDirectory(directoryName, true);
                result = true;
            }
        }

        return result;
    }

    public async Task<OperationResult<Dictionary<AlbumNeedsAttentionReasons, int>>> AlbumsCountByStatusAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        CheckInitialized();
        var albumsForDirectoryInfo = await AlbumsForDirectoryAsync(fileSystemDirectoryInfo,
            new PagedRequest { PageSize = short.MaxValue }, cancellationToken);

        return new OperationResult<Dictionary<AlbumNeedsAttentionReasons, int>>
        {
            Data = albumsForDirectoryInfo.Data.GroupBy(x => x.StatusReasons).ToDictionary(x => x.Key, x => x.Count())
        };
    }

    public async Task<PagedResult<AlbumDataInfo>> AlbumsDataInfosForDirectoryAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo,
        PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        CheckInitialized();
        var albumsForDirectoryInfo =
            await AlbumsForDirectoryAsync(fileSystemDirectoryInfo, pagedRequest, cancellationToken);
        var data = albumsForDirectoryInfo.Data.ToArray().Select(async x => new AlbumDataInfo(
            0,
            x.Id,
            false,
            x.AlbumTitle() ?? string.Empty,
            x.AlbumTitle().ToNormalizedString() ?? x.AlbumTitle() ?? string.Empty,
            null,
            Guid.Empty,
            x.Artist.Name,
            x.SongTotalValue(),
            x.TotalDuration(),
            Instant.FromDateTimeOffset(x.Created),
            null,
            SafeParser.ToLocalDate(x.AlbumYear() ?? 0),
            SafeParser.ToNumber<short>(_albumValidator.ValidateAlbum(x).Data.AlbumStatus)
        )
        {
            ImageBytes = await x.CoverImageBytesAsync(cancellationToken),
            MelodeeDataFileName = fileSystemService.CombinePath(x.Directory.FullName(), Album.JsonFileName),
            NeedsAttentionReasons = (int)x.StatusReasons
        });

        var d = await Task.WhenAll(data);

        return new PagedResult<AlbumDataInfo>
        {
            TotalCount = albumsForDirectoryInfo.TotalCount,
            TotalPages = albumsForDirectoryInfo.TotalPages,
            Data = d
        };
    }

    public async Task<int> NumberOfOkAlbumsAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        if (cancellationToken.IsCancellationRequested)
        {
            return 0;
        }

        var albumsForDirectoryInfo = await AlbumsForDirectoryAsync(
            fileSystemDirectoryInfo,
            new PagedRequest
            {
                PageSize = short.MaxValue
            },
            cancellationToken);

        return albumsForDirectoryInfo.Data.Count(x => x.Status == AlbumStatus.Ok);
    }

    public async Task<OperationResult<IEnumerable<Album>?>> AllMelodeeAlbumDataFilesForDirectoryAsync(
        FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
        {
            return new OperationResult<IEnumerable<Album>?>
            {
                Data = []
            };
        }

        // Check cache first with simple directory existence validation
        var cacheKey = fileSystemDirectoryInfo.Path;
        if (_directoryCache.TryGetValue(cacheKey, out var cached))
        {
            try
            {
                // Cache for 30 seconds to balance performance vs. freshness
                if (fileSystemService.DirectoryExists(fileSystemDirectoryInfo.Path) &&
                    DateTime.UtcNow - cached.LastWriteTime < TimeSpan.FromSeconds(30))
                {
                    return new OperationResult<IEnumerable<Album>?>
                    {
                        Data = cached.Albums
                    };
                }
            }
            catch
            {
                // If we can't check timestamps, proceed with fresh scan
            }
        }

        var albums = new ConcurrentBag<Album>();
        var errors = new ConcurrentBag<Exception>();
        var messages = new ConcurrentBag<string>();

        try
        {
            if (fileSystemService.DirectoryExists(fileSystemDirectoryInfo.Path))
            {
                var jsonFiles = fileSystemService.EnumerateFiles(fileSystemDirectoryInfo.Path, $"*{Album.JsonFileName}", SearchOption.AllDirectories);

                // Use optimized parallel processing with bounded concurrency
                var parallelOptions = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = DefaultParallelOptions.MaxDegreeOfParallelism
                };

                try
                {
                    await Parallel.ForEachAsync(jsonFiles, parallelOptions,
                        async (jsonFilePath, token) =>
                        {
                            // Check cancellation at start of each iteration
                            if (token.IsCancellationRequested)
                            {
                                return;
                            }

                            try
                            {
                                var album = await fileSystemService.DeserializeAlbumAsync(jsonFilePath, token).ConfigureAwait(false);
                                if (album != null)
                                {
                                    album.Directory = new FileSystemDirectoryInfo
                                    {
                                        Path = fileSystemService.GetDirectoryName(jsonFilePath),
                                        Name = fileSystemService.GetFileName(fileSystemService.GetDirectoryName(jsonFilePath))
                                    };
                                    album.Created = fileSystemService.GetFileCreationTimeUtc(jsonFilePath);
                                    albums.Add(album);
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Warning(e, "Error processing Melodee Data file [{FileName}]", jsonFilePath);
                                messages.Add($"Error processing Melodee Data file [{fileSystemDirectoryInfo.FullName()}]");
                                errors.Add(e);
                            }
                        });
                }
                catch (OperationCanceledException)
                {
                    // Handle cancellation gracefully - just continue with what we have
                }

                // Update cache with results if not canceled
                if (!cancellationToken.IsCancellationRequested)
                {
                    await UpdateCacheAsync(cacheKey, albums.ToArray(), cancellationToken);
                }
            }
        }
        catch (Exception e)
        {
            Log.Warning("Unable to load Albums for [{DirInfo}]", fileSystemDirectoryInfo.FullName);
            errors.Add(e);
        }

        return new OperationResult<IEnumerable<Album>?>(messages)
        {
            Errors = errors,
            Data = albums.IsEmpty ? null : albums.ToArray()
        };
    }

    private async Task UpdateCacheAsync(string cacheKey, Album[] albums, CancellationToken cancellationToken)
    {
        try
        {
            await _cacheUpdateSemaphore.WaitAsync(cancellationToken);
            try
            {
                var currentWriteTime = DateTime.UtcNow;

                _directoryCache.AddOrUpdate(cacheKey,
                    (currentWriteTime, albums),
                    (_, _) => (currentWriteTime, albums));

                // Implement cache size management to prevent memory bloat
                if (_directoryCache.Count > 1000) // Configurable threshold
                {
                    var oldestEntries = _directoryCache
                        .OrderBy(kvp => kvp.Value.LastWriteTime)
                        .Take(_directoryCache.Count - 800) // Keep 800 most recent
                        .Select(kvp => kvp.Key)
                        .ToArray();

                    foreach (var oldKey in oldestEntries)
                    {
                        _directoryCache.TryRemove(oldKey, out _);
                    }
                }
            }
            finally
            {
                _cacheUpdateSemaphore.Release();
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation during cache update
        }
    }

    public void ClearCache()
    {
        _directoryCache.Clear();
    }
}
