using System.Collections.Concurrent;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Collection;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Serialization;
using Melodee.Common.Services.Interfaces;
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
    ISerializer serializer)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private IAlbumValidator _albumValidator = null!;
    private IMelodeeConfiguration _configuration = new MelodeeConfiguration([]);
    private bool _initialized;

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
        var result = (await AllMelodeeAlbumDataFilesForDirectoryAsync(fileSystemDirectoryInfo, cancellationToken)).Data?.FirstOrDefault(x => x.Id == id);
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
        var albums = new List<Album>();
        var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);

        var dataForDirectoryInfoResult = await AllMelodeeAlbumDataFilesForDirectoryAsync(fileSystemDirectoryInfo, cancellationToken);
        if (dataForDirectoryInfoResult.IsSuccess)
        {
            albums.AddRange(dataForDirectoryInfoResult.Data!);
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
                foreach (var r in dataForChildDirResult.Data!)
                {
                    if (albums.All(x => x.Id != r.Id))
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
                x.Artist.Name.Contains(pagedRequest.Search, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

        if (pagedRequest.AlbumResultFilter != AlbumResultFilter.All && albums.Count != 0)
        {
            switch (pagedRequest.AlbumResultFilter)
            {
                case AlbumResultFilter.Duplicates:
                    var duplicates = albums
                        .GroupBy(x => x.Id)
                        .Where(x => x.Count() > 1)
                        .Select(x => x.Key);
                    albums = albums.Where(x => duplicates.Contains(x.Id)).ToList();
                    break;

                case AlbumResultFilter.Incomplete:
                    albums = albums.Where(x => x.Status == AlbumStatus.Invalid).ToList();
                    break;

                case AlbumResultFilter.LessThanConfiguredSongs:
                    var filterLessThanSongs = SafeParser.ToNumber<int>(_configuration.Configuration[SettingRegistry.FilteringLessThanSongCount]);
                    albums = albums.Where(x => x.Songs?.Count() < filterLessThanSongs || x.SongTotalValue() < filterLessThanSongs).ToList();
                    break;

                case AlbumResultFilter.NeedsAttention:
                    albums = albums.Where(x => x.Status == AlbumStatus.Invalid).ToList();
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
                        albums = albums.Where(x => pagedRequest.SelectedAlbumIds.Contains(x.Id)).ToList();
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
                    case "ArtistName":
                        albums = albums.Where(x => x.Artist.NameNormalized.Contains(filterBy.Value.ToString()?.ToNormalizedString() ?? string.Empty)).ToList();
                        break;

                    case "AlbumStatus":
                        var filterStatusValue = SafeParser.ToEnum<AlbumStatus>(filterBy.Value);
                        albums = albums.Where(x => x.Status == filterStatusValue).ToList();
                        break;

                    case "NameNormalized":
                        albums = albums.Where(x =>
                            (x.AlbumTitle() != null && x.AlbumTitle()!.Contains(filterBy.Value.ToString() ?? string.Empty, StringComparison.CurrentCultureIgnoreCase)) ||
                            x.Artist.Name.Contains(filterBy.Value.ToString() ?? string.Empty, StringComparison.CurrentCultureIgnoreCase)).ToList();
                        break;

                    case "ReleaseDate":
                        var filterYearValue = SafeParser.ToNumber<int>(filterBy.Value);
                        albums = albums.Where(x => x.AlbumYear() == filterYearValue).ToList();
                        break;
                }
            }
        }

        switch (pagedRequest.OrderByValue("SortOrder"))
        {
            case "\"Artist\" ASC":
                albums = albums.OrderBy(x => x.Artist.SortName).ToList();
                break;

            case "\"Artist\" DESC":
                albums = albums.OrderByDescending(x => x.Artist.SortName).ToList();
                break;

            case "\"CreatedAt\" ASC":
                albums = albums.OrderBy(x => x.Created).ToList();
                break;

            case "\"CreatedAt\" DESC":
                albums = albums.OrderByDescending(x => x.Created).ToList();
                break;

            case "\"Duration\" ASC":
                albums = albums.OrderBy(x => x.Duration()).ToList();
                break;

            case "\"Duration\" DESC":
                albums = albums.OrderByDescending(x => x.Duration()).ToList();
                break;

            case "\"NeedsAttentionReasonsValue\" ASC":
                albums = albums.OrderBy(x => x.StatusReasons).ToList();
                break;

            case "\"NeedsAttentionReasonsValue\" DESC":
                albums = albums.OrderByDescending(x => x.StatusReasons).ToList();
                break;

            case "\"Title\" ASC":
                albums = albums.OrderBy(x => x.AlbumTitle()).ToList();
                break;

            case "\"Title\" DESC":
                albums = albums.OrderByDescending(x => x.AlbumTitle()).ToList();
                break;

            case "\"Year\" ASC":
                albums = albums.OrderBy(x => x.AlbumYear()).ToList();
                break;

            case "\"Year\" DESC":
                albums = albums.OrderByDescending(x => x.AlbumYear()).ToList();
                break;

            case "\"Status\" ASC":
                albums = albums.OrderBy(x => x.Status).ToList();
                break;

            case "\"Status\" DESC":
                albums = albums.OrderByDescending(x => x.Status).ToList();
                break;

            case "\"SongCount\" ASC":
                albums = albums.OrderBy(x => x.SongTotalValue()).ToList();
                break;

            case "\"SongCount\" DESC":
                albums = albums.OrderByDescending(x => x.SongTotalValue()).ToList();
                break;
        }


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

    public async Task<bool> DeleteAlbumsAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, Func<Album, bool> condition, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var result = false;
        var albumsForDirectoryInfo = await AlbumsForDirectoryAsync(fileSystemDirectoryInfo, new PagedRequest() { PageSize = short.MaxValue }, cancellationToken);
        if (albumsForDirectoryInfo.Data.Any())
        {
            foreach (var album in albumsForDirectoryInfo.Data)
            {
                if (!condition(album))
                {
                    continue;
                }

                var fileInfo = new FileInfo(album.MelodeeDataFileName ?? string.Empty);
                if (fileInfo.DirectoryName == null)
                {
                    continue;
                }

                Directory.Delete(fileInfo.DirectoryName, true);
                result = true;
            }
        }

        return result;
    }

    public async Task<OperationResult<Dictionary<AlbumNeedsAttentionReasons, int>>> AlbumsCountByStatusAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        CheckInitialized();
        var albumsForDirectoryInfo = await AlbumsForDirectoryAsync(fileSystemDirectoryInfo, new PagedRequest { PageSize = short.MaxValue }, cancellationToken);

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
        var albumsForDirectoryInfo = await AlbumsForDirectoryAsync(fileSystemDirectoryInfo, pagedRequest, cancellationToken);
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
            MelodeeDataFileName = Path.Combine(x.Directory.FullName(), Album.JsonFileName),
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

    public async Task<OperationResult<IEnumerable<Album>?>> AllMelodeeAlbumDataFilesForDirectoryAsync(FileSystemDirectoryInfo fileSystemDirectoryInfo, CancellationToken cancellationToken = default)
    {
        CheckInitialized();

        var albums = new ConcurrentBag<Album>();
        var errors = new ConcurrentBag<Exception>();
        var messages = new ConcurrentBag<string>();

        try
        {
            var dirInfo = new DirectoryInfo(fileSystemDirectoryInfo.Path);
            if (dirInfo.Exists)
            {
                //   foreach (var jsonFile in dirInfo.EnumerateFiles($"*{Album.JsonFileName}", SearchOption.AllDirectories))
                await Parallel.ForEachAsync(dirInfo.EnumerateFiles($"*{Album.JsonFileName}", SearchOption.AllDirectories), cancellationToken, async (jsonFile, token) =>
                {
                    try
                    {
                        var r = await Album.DeserializeAndInitializeAlbumAsync(serializer, jsonFile.FullName, token).ConfigureAwait(false);
                        if (r != null)
                        {
                            r.Directory = jsonFile.Directory!.ToDirectorySystemInfo();
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
                });
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
}
