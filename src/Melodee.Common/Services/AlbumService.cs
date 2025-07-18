using System.Linq.Expressions;
using Ardalis.GuardClauses;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.MessageBus.Events;
using Melodee.Common.Models.Collection;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.Conversion.Image;
using Melodee.Common.Serialization;
using Melodee.Common.Services.Caching;
using Melodee.Common.Services.Extensions;
using Melodee.Common.Services.Scanning;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Rebus.Bus;
using Serilog;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Common.Services;

public class AlbumService(
    ILogger logger,
    ICacheManager cacheManager,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IBus bus,
    ISerializer serializer,
    IHttpClientFactory httpClientFactory,
    MediaEditService mediaEditService)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:album:apikey:{0}";
    private const string CacheKeyDetailByNameNormalizedTemplate = "urn:album:namenormalized:{0}";
    private const string CacheKeyDetailByMusicBrainzIdTemplate = "urn:album:musicbrainzid:{0}";
    private const string CacheKeyDetailTemplate = "urn:album:{0}";
    private const string CacheKeyAlbumImageBytesAndEtagTemplate = "urn:album:imagebytesandetag:{0}:{1}";

    public async Task ClearCacheForArtist(int artistId, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            // Get the artist from db context
            var dbArtist = await scopedContext
                .Artists
                .Include(x => x.Albums)
                .FirstOrDefaultAsync(x => x.Id == artistId, cancellationToken).ConfigureAwait(false);

            // For each album for artist clear the cache for the artist
            foreach (var album in dbArtist?.Albums ?? [])
            {
                await ClearCacheAsync(album.Id, cancellationToken).ConfigureAwait(false);
            }
        }
    }
    
    public async Task ClearCacheAsync(int albumId, CancellationToken cancellationToken = default)
    {
        var album = await GetAsync(albumId, cancellationToken).ConfigureAwait(false);
        ClearCache(album.Data!, cancellationToken);
    }    

    public void ClearCache(Album album, CancellationToken cancellationToken = default)
    {
        CacheManager.Remove(CacheKeyDetailByApiKeyTemplate.FormatSmart(album.ApiKey), Album.CacheRegion);
        CacheManager.Remove(CacheKeyDetailByNameNormalizedTemplate.FormatSmart(album.NameNormalized), Album.CacheRegion);

        CacheManager.Remove(CacheKeyDetailTemplate.FormatSmart(album.Id), Album.CacheRegion);

        CacheManager.Remove(CacheKeyAlbumImageBytesAndEtagTemplate.FormatSmart(album.Id, ImageSize.Thumbnail), Album.CacheRegion);
        CacheManager.Remove(CacheKeyAlbumImageBytesAndEtagTemplate.FormatSmart(album.Id, ImageSize.Small), Album.CacheRegion);
        CacheManager.Remove(CacheKeyAlbumImageBytesAndEtagTemplate.FormatSmart(album.Id, ImageSize.Medium), Album.CacheRegion);
        CacheManager.Remove(CacheKeyAlbumImageBytesAndEtagTemplate.FormatSmart(album.Id, ImageSize.Large), Album.CacheRegion);


        // This is needed because the OpenSubsonicApiService caches the image bytes after potentially resizing
        CacheManager.Remove(OpenSubsonicApiService.ImageCacheRegion);

        if (album.MusicBrainzId != null)
        {
            CacheManager.Remove(CacheKeyDetailByMusicBrainzIdTemplate.FormatSmart(album.MusicBrainzId.Value.ToString()), Album.CacheRegion);
        }
    }

    public async Task<MelodeeModels.PagedResult<AlbumDataInfo>> ListForContributorsAsync(
        MelodeeModels.PagedRequest pagedRequest,
        string contributorName,
        CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            // Create base query using EF Core with proper joins and filtering
            var baseQuery = scopedContext.Contributors
                .Where(c => c.ContributorName != null && EF.Functions.ILike(c.ContributorName, $"%{contributorName}%"))
                .Select(c => c.Album)
                .Distinct();

            // Get total count efficiently
            var albumCount = await baseQuery.CountAsync(cancellationToken).ConfigureAwait(false);

            AlbumDataInfo[] albums = [];

            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                // First, get the album data with artist information
                var albumsQueryWithIncludes = baseQuery
                    .Include(a => a.Artist);

                // Apply ordering on the entity properties before projection
                var orderByClause = pagedRequest.OrderByValue("Name", MelodeeModels.PagedRequest.OrderAscDirection);
                var isDescending = orderByClause.Contains("DESC", StringComparison.OrdinalIgnoreCase);
                var fieldName = orderByClause.Split(' ')[0].Trim('"').ToLowerInvariant();

                IQueryable<Album> albumsQuery = fieldName switch
                {
                    "name" => isDescending ? albumsQueryWithIncludes.OrderByDescending(a => a.Name) : albumsQueryWithIncludes.OrderBy(a => a.Name),
                    "createdat" => isDescending ? albumsQueryWithIncludes.OrderByDescending(a => a.CreatedAt) : albumsQueryWithIncludes.OrderBy(a => a.CreatedAt),
                    "releasedate" => isDescending ? albumsQueryWithIncludes.OrderByDescending(a => a.ReleaseDate) : albumsQueryWithIncludes.OrderBy(a => a.ReleaseDate),
                    _ => albumsQueryWithIncludes.OrderBy(a => a.Name)
                };

                // Apply paging and get the raw entities
                var albumEntities = await albumsQuery
                    .Skip(pagedRequest.SkipValue)
                    .Take(pagedRequest.TakeValue)
                    .AsNoTracking()
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);

                // Project to AlbumDataInfo in memory
                albums = albumEntities
                    .Select(a => new AlbumDataInfo(
                        a.Id,
                        a.ApiKey,
                        a.IsLocked,
                        a.Name,
                        a.NameNormalized,
                        a.AlternateNames,
                        a.Artist.ApiKey,
                        a.Artist.Name,
                        a.SongCount ?? 0,
                        a.Duration,
                        a.CreatedAt,
                        a.Tags,
                        a.ReleaseDate,
                        a.AlbumStatus
                    ))
                    .ToArray();
            }

            return new MelodeeModels.PagedResult<AlbumDataInfo>
            {
                TotalCount = albumCount,
                TotalPages = pagedRequest.TotalPages(albumCount),
                Data = albums
            };
        }
    }

    public async Task<MelodeeModels.PagedResult<AlbumDataInfo>> ListForArtistApiKeyAsync(
        MelodeeModels.PagedRequest pagedRequest,
        Guid filterToArtistApiKey,
        CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            // Create base query filtering by artist API key
            var baseQuery = scopedContext.Albums
                .Where(x => x.Artist.ApiKey == filterToArtistApiKey);

            // Apply name filter if provided
            var filterBy = pagedRequest.FilterBy?.FirstOrDefault(x => x.PropertyName == "Name")?.Value.ToString()
                .ToNormalizedString();
            if (!string.IsNullOrEmpty(filterBy))
            {
                baseQuery = baseQuery.Where(x => EF.Functions.ILike(x.NameNormalized, $"%{filterBy}%"));
            }

            // Get total count efficiently
            var albumCount = await baseQuery.CountAsync(cancellationToken).ConfigureAwait(false);

            AlbumDataInfo[] albums = [];

            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                // Apply ordering first on the base query, then project
                var orderByClause = pagedRequest.OrderByValue("Name", MelodeeModels.PagedRequest.OrderAscDirection);
                var isDescending = orderByClause.Contains("DESC", StringComparison.OrdinalIgnoreCase);
                var fieldName = orderByClause.Split(' ')[0].Trim('"').ToLowerInvariant();

                var orderedQuery = fieldName switch
                {
                    "name" or "namenormalized" => isDescending ? baseQuery.OrderByDescending(a => a.Name) : baseQuery.OrderBy(a => a.SortName).ThenBy(x => x.Name),
                    "createdat" => isDescending ? baseQuery.OrderByDescending(a => a.CreatedAt) : baseQuery.OrderBy(a => a.CreatedAt),
                    "directory" => isDescending ? baseQuery.OrderByDescending(a => a.Directory) : baseQuery.OrderBy(a => a.Directory),
                    "duration" => isDescending ? baseQuery.OrderByDescending(a => a.Duration) : baseQuery.OrderBy(a => a.Duration),
                    "releasedate" => isDescending ? baseQuery.OrderByDescending(a => a.ReleaseDate) : baseQuery.OrderBy(a => a.ReleaseDate),
                    "songcount" => isDescending ? baseQuery.OrderByDescending(a => a.SongCount) : baseQuery.OrderBy(a => a.SongCount),
                    _ => baseQuery.OrderBy(a => a.Name)
                };

                // Apply paging and include Artist for projection
                var pagedQuery = orderedQuery
                    .Include(a => a.Artist)
                    .Skip(pagedRequest.SkipValue)
                    .Take(pagedRequest.TakeValue)
                    .AsNoTracking();

                // Execute query and project to AlbumDataInfo
                var rawAlbums = await pagedQuery.ToArrayAsync(cancellationToken).ConfigureAwait(false);

                albums = rawAlbums.Select(a => new AlbumDataInfo(
                    a.Id,
                    a.ApiKey,
                    a.IsLocked,
                    a.Name,
                    a.NameNormalized,
                    a.AlternateNames,
                    a.Artist.ApiKey,
                    a.Artist.Name,
                    a.SongCount ?? 0,
                    a.Duration,
                    a.CreatedAt,
                    a.Tags,
                    a.ReleaseDate,
                    a.AlbumStatus
                )).ToArray();
            }

            return new MelodeeModels.PagedResult<AlbumDataInfo>
            {
                TotalCount = albumCount,
                TotalPages = pagedRequest.TotalPages(albumCount),
                Data = albums
            };
        }
    }

    public async Task<MelodeeModels.PagedResult<AlbumDataInfo>> ListAsync(
        MelodeeModels.PagedRequest pagedRequest,
        string? filteringColumnNamePrefix = null,
        CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            // Create base query
            var baseQuery = scopedContext.Albums.AsQueryable();

            // Apply filters
            baseQuery = ApplyFilters(baseQuery, pagedRequest);

            // Get total count efficiently
            var albumCount = await baseQuery.CountAsync(cancellationToken).ConfigureAwait(false);

            AlbumDataInfo[] albums = [];

            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                // Apply ordering first on the base query, then project
                var orderByClause = pagedRequest.OrderByValue("Name", MelodeeModels.PagedRequest.OrderAscDirection);
                var isDescending = orderByClause.Contains("DESC", StringComparison.OrdinalIgnoreCase);
                var fieldName = orderByClause.Split(' ')[0].Trim('"').ToLowerInvariant();

                var orderedQuery = fieldName switch
                {
                    "name" or "namenormalized" => isDescending ? baseQuery.OrderByDescending(a => a.SortName).ThenByDescending(x => x.Name) : baseQuery.OrderBy(a => a.SortName).ThenBy(x => x.Name),
                    "createdat" => isDescending ? baseQuery.OrderByDescending(a => a.CreatedAt) : baseQuery.OrderBy(a => a.CreatedAt),
                    "directory" => isDescending ? baseQuery.OrderByDescending(a => a.Directory) : baseQuery.OrderBy(a => a.Directory),
                    "duration" => isDescending ? baseQuery.OrderByDescending(a => a.Duration) : baseQuery.OrderBy(a => a.Duration),
                    "releasedate" => isDescending ? baseQuery.OrderByDescending(a => a.ReleaseDate) : baseQuery.OrderBy(a => a.ReleaseDate),
                    "songcount" => isDescending ? baseQuery.OrderByDescending(a => a.SongCount) : baseQuery.OrderBy(a => a.SongCount),
                    _ => baseQuery.OrderBy(a => a.Name)
                };

                // Apply paging and include Artist for projection
                var pagedQuery = orderedQuery
                    .Include(a => a.Artist)
                    .Skip(pagedRequest.SkipValue)
                    .Take(pagedRequest.TakeValue)
                    .AsNoTracking();

                // Execute query and project to AlbumDataInfo
                var rawAlbums = await pagedQuery.ToArrayAsync(cancellationToken).ConfigureAwait(false);

                albums = rawAlbums.Select(a => new AlbumDataInfo(
                    a.Id,
                    a.ApiKey,
                    a.IsLocked,
                    a.Name,
                    a.NameNormalized,
                    a.AlternateNames,
                    a.Artist.ApiKey,
                    a.Artist.Name,
                    a.SongCount ?? 0,
                    a.Duration,
                    a.CreatedAt,
                    a.Tags,
                    a.ReleaseDate,
                    a.AlbumStatus
                )).ToArray();
            }

            return new MelodeeModels.PagedResult<AlbumDataInfo>
            {
                TotalCount = albumCount,
                TotalPages = pagedRequest.TotalPages(albumCount),
                Data = albums
            };
        }
    }


    public async Task<MelodeeModels.OperationResult<bool>> DeleteAsync(
        int[] albumIds,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(albumIds, nameof(albumIds));

        bool result;

        var artistIds = new List<int>();
        var libraryIds = new List<int>();

        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            foreach (var albumId in albumIds)
            {
                var artist = await GetAsync(albumId, cancellationToken).ConfigureAwait(false);
                if (!artist.IsSuccess)
                {
                    return new MelodeeModels.OperationResult<bool>("Unknown album.")
                    {
                        Data = false
                    };
                }
            }

            foreach (var albuMid in albumIds)
            {
                var album = await scopedContext
                    .Albums.Include(x => x.Artist).ThenInclude(x => x.Library)
                    .FirstAsync(x => x.Id == albuMid, cancellationToken)
                    .ConfigureAwait(false);

                var albumDirectory = Path.Combine(album.Artist.Library.Path, album.Artist.Directory, album.Directory);
                if (Directory.Exists(albumDirectory))
                {
                    Directory.Delete(albumDirectory, true);
                }

                scopedContext.Albums.Remove(album);
                artistIds.Add(album.ArtistId);
                libraryIds.Add(album.Artist.LibraryId);
            }

            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            foreach (var artistId in artistIds.Distinct())
            {
                await UpdateArtistAggregateValuesByIdAsync(artistId, cancellationToken).ConfigureAwait(false);
            }

            foreach (var libraryId in libraryIds.Distinct())
            {
                await UpdateLibraryAggregateStatsByIdAsync(libraryId, cancellationToken).ConfigureAwait(false);
            }

            Logger.Information("Deleted albums [{AlbumIds}].", albumIds);
            result = true;
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<Album?>> GetAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            await using (var scopedContext =
                         await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext
                    .Albums
                    .Include(x => x.Artist).ThenInclude(x => x.Library)
                    .Include(x => x.Contributors).ThenInclude(x => x.Artist)
                    .Include(x => x.Songs).ThenInclude(x => x.Contributors).ThenInclude(x => x.Artist)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                    .ConfigureAwait(false);
            }
        }, cancellationToken, region: Album.CacheRegion);

        return new MelodeeModels.OperationResult<Album?>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<Album?>> GetByMusicBrainzIdAsync(
        Guid musicBrainzId,
        CancellationToken cancellationToken = default)
    {
        var id = await CacheManager.GetAsync<int?>(
            CacheKeyDetailByMusicBrainzIdTemplate.FormatSmart(musicBrainzId.ToString()), async () =>
            {
                await using (var scopedContext =
                             await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
                {
                    return await scopedContext.Albums
                        .Where(a => a.MusicBrainzId == musicBrainzId)
                        .Select(a => a.Id)
                        .FirstOrDefaultAsync(cancellationToken)
                        .ConfigureAwait(false);
                }
            }, cancellationToken, region: Album.CacheRegion);
        if (id is null or 0)
        {
            return new MelodeeModels.OperationResult<Album?>("Unknown album.")
            {
                Data = null
            };
        }

        return await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MelodeeModels.OperationResult<Album?>> GetByApiKeyAsync(
        Guid apiKey,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(_ => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        var id = await CacheManager.GetAsync<int?>(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using (var scopedContext =
                         await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext.Albums
                    .Where(a => a.ApiKey == apiKey)
                    .Select(a => a.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }, cancellationToken, region: Album.CacheRegion);
        if (id is null or 0)
        {
            return new MelodeeModels.OperationResult<Album?>("Unknown album.")
            {
                Data = null
            };
        }

        return await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MelodeeModels.OperationResult<bool>> UpdateAsync(
        Album album,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(album, nameof(album));

        var validationResult = ValidateModel(album);
        if (!validationResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<bool>(validationResult.Data.Item2
                ?.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage!).ToArray() ?? [])
            {
                Data = false,
                Type = MelodeeModels.OperationResponseType.ValidationFailure
            };
        }

        bool result;
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbDetail = await scopedContext
                .Albums
                .Include(x => x.Artist).ThenInclude(x => x.Library)
                .FirstOrDefaultAsync(x => x.Id == album.Id, cancellationToken)
                .ConfigureAwait(false);

            if (dbDetail == null)
            {
                return new MelodeeModels.OperationResult<bool>
                {
                    Data = false,
                    Type = MelodeeModels.OperationResponseType.NotFound
                };
            }

            var didChangeName = dbDetail.Name != album.Name;

            var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);

            var albumDirectory = album.ToMelodeeAlbumModel().AlbumDirectoryName(configuration.Configuration);
            if (!albumDirectory.ToFileSystemDirectoryInfo().ToDirectoryInfo().IsSameDirectory(dbDetail.Directory))
            {
                // Details that are used to build the albums directory has changed, rename directory to new name
                var existingAlbumDirectory = Path.Combine(dbDetail.Artist.Library.Path, dbDetail.Artist.Directory,
                    dbDetail.Directory);
                var newAlbumDirectory =
                    Path.Combine(dbDetail.Artist.Library.Path, dbDetail.Artist.Directory, albumDirectory);
                if (Directory.Exists(existingAlbumDirectory))
                {
                    Directory.Move(existingAlbumDirectory, newAlbumDirectory);
                }

                album.Directory = albumDirectory;
            }

            dbDetail.Directory = album.Directory;

            dbDetail.AlbumStatus = album.AlbumStatus;
            dbDetail.AlbumType = album.AlbumType;
            dbDetail.AlternateNames = album.AlternateNames;
            dbDetail.AmgId = album.AmgId;
            dbDetail.ArtistId = album.ArtistId;
            dbDetail.Comment = album.Comment;
            dbDetail.Description = album.Description;
            dbDetail.DeezerId = album.DeezerId;
            dbDetail.DiscogsId = album.DiscogsId;
            dbDetail.Genres = album.Genres;
            dbDetail.ImageCount = album.ImageCount;
            dbDetail.IsCompilation = album.IsCompilation;
            dbDetail.IsLocked = album.IsLocked;
            dbDetail.ItunesId = album.ItunesId;
            dbDetail.LastFmId = album.LastFmId;
            dbDetail.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);
            dbDetail.Moods = album.Moods;
            dbDetail.MusicBrainzId = album.MusicBrainzId;
            dbDetail.Name = album.Name;
            dbDetail.NameNormalized = album.Name.ToNormalizedString() ?? album.Name;
            dbDetail.Notes = album.Notes;
            dbDetail.OriginalReleaseDate = album.OriginalReleaseDate;
            dbDetail.ReleaseDate = album.ReleaseDate;
            dbDetail.SortName = album.SortName;
            dbDetail.SortOrder = album.SortOrder;
            dbDetail.SpotifyId = album.SpotifyId;
            dbDetail.Tags = album.Tags;
            dbDetail.WikiDataId = album.WikiDataId;

            result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;

            if (result)
            {
                ClearCache(dbDetail);

                if (didChangeName)
                {
                    try
                    {
                        await mediaEditService.InitializeAsync(token: cancellationToken);
                        var newAlbumPath = Path.Combine(dbDetail.Artist.Library.Path, dbDetail.Artist.Directory, dbDetail.Directory);

                        // Check if the melodee.json file exists before trying to read it
                        var melodeeJsonPath = Path.Combine(newAlbumPath, "melodee.json");
                        if (File.Exists(melodeeJsonPath))
                        {
                            var melodeeAlbum = await MelodeeModels.Album.DeserializeAndInitializeAlbumAsync(serializer, melodeeJsonPath, cancellationToken).ConfigureAwait(false);
                            if (melodeeAlbum != null)
                            {
                                melodeeAlbum.AlbumDbId = dbDetail.Id;
                                melodeeAlbum.Directory = newAlbumPath.ToFileSystemDirectoryInfo();
                                melodeeAlbum.MusicBrainzId = dbDetail.MusicBrainzId;
                                melodeeAlbum.SpotifyId = dbDetail.SpotifyId;
                                melodeeAlbum.SetTagValue(MetaTagIdentifier.Album, dbDetail.Name);
                                foreach (var song in melodeeAlbum.Songs ?? [])
                                {
                                    melodeeAlbum.SetSongTagValue(song.Id, MetaTagIdentifier.Album, dbDetail.Name);
                                }

                                await mediaEditService.SaveMelodeeAlbum(melodeeAlbum, true, cancellationToken).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            Logger.Warning("Melodee.json file not found at [{Path}] during album update.", melodeeJsonPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(ex, "Failed to update melodee.json file during album name change for album [{AlbumId}].", dbDetail.Id);
                        // Don't fail the entire operation if we can't update the melodee.json file
                    }
                }
            }
        }


        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }


    public async Task<MelodeeModels.OperationResult<Album?>> FindAlbumAsync(
        int artistId,
        MelodeeModels.Album melodeeAlbum,
        CancellationToken cancellationToken = default)
    {
        var albumTitle = melodeeAlbum.AlbumTitle()?.CleanStringAsIs() ??
                         throw new Exception("Album title is required.");
        var nameNormalized = albumTitle.ToNormalizedString() ?? albumTitle;

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            int? id = null;

            try
            {
                if (melodeeAlbum.AlbumDbId.HasValue)
                {
                    id = await scopedContext.Albums
                        .Where(a => a.Id == melodeeAlbum.AlbumDbId.Value)
                        .Select(a => (int?)a.Id)
                        .FirstOrDefaultAsync(cancellationToken)
                        .ConfigureAwait(false);
                }

                if (id == null && melodeeAlbum.Id != Guid.Empty)
                {
                    id = await scopedContext.Albums
                        .Where(a => a.ApiKey == melodeeAlbum.Id)
                        .Select(a => (int?)a.Id)
                        .FirstOrDefaultAsync(cancellationToken)
                        .ConfigureAwait(false);
                }

                id ??= await scopedContext.Albums
                    .Where(a => a.ArtistId == artistId)
                    .Where(a => a.NameNormalized == nameNormalized ||
                                (a.MusicBrainzId == melodeeAlbum.MusicBrainzId && a.MusicBrainzId != null) ||
                                (a.SpotifyId == melodeeAlbum.SpotifyId && a.SpotifyId != null))
                    .Select(a => (int?)a.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.Error(e,
                    "[{ServiceName}] attempting to Find Album id [{Id}], apiKey [{ApiKey}], name [{Name}] musicbrainzId [{MbId}] spotifyId [{SpotifyId}].",
                    nameof(AlbumService),
                    melodeeAlbum.AlbumDbId,
                    melodeeAlbum.Id,
                    nameNormalized,
                    melodeeAlbum.MusicBrainzId,
                    melodeeAlbum.SpotifyId);
            }

            if (id is null or 0)
            {
                return new MelodeeModels.OperationResult<Album?>("Unknown album.")
                {
                    Data = null
                };
            }

            return await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<MelodeeModels.OperationResult<bool>> RescanAsync(int[] albumIds,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(albumIds, nameof(albumIds));

        var successfulScans = 0;

        foreach (var albumId in albumIds)
        {
            var albumResult = await GetAsync(albumId, cancellationToken).ConfigureAwait(false);
            if (!albumResult.IsSuccess || albumResult.Data == null)
            {
                return new MelodeeModels.OperationResult<bool>("Unknown album.")
                {
                    Data = false
                };
            }

            var album = albumResult.Data;
            var albumDirectory = Path.Combine(album.Artist.Library.Path, album.Artist.Directory, album.Directory);
            if (!Directory.Exists(albumDirectory))
            {
                Logger.Warning("Album directory [{AlbumDirectory}] does not exist for rescan.", albumDirectory);
                // Continue with other albums but don't count this as successful
                continue;
            }

            await bus.SendLocal(new AlbumRescanEvent(album.Id, albumDirectory, false)).ConfigureAwait(false);
            successfulScans++;
        }

        // Return false if no albums were successfully scanned
        return new MelodeeModels.OperationResult<bool>
        {
            Data = successfulScans > 0
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> LockUnlockAlbumAsync(int albumId, bool doLock,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, albumId, nameof(albumId));

        var artistResult = await GetAsync(albumId, cancellationToken).ConfigureAwait(false);
        if (!artistResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<bool>($"Unknown album to lock [{albumId}].")
            {
                Data = false
            };
        }

        artistResult.Data!.IsLocked = doLock;
        var result = (await UpdateAsync(artistResult.Data, cancellationToken).ConfigureAwait(false)).Data;
        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<Album?>> AddAlbumAsync(
        Album album,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(album, nameof(album));
        Guard.Against.NullOrEmpty(album.Name, nameof(album.Name));
        Guard.Against.Null(album.Artist, nameof(album), "Artist is required for album.");
        Guard.Against.Expression(x => x < 1, album.ArtistId, nameof(album.ArtistId), "ArtistId is required for album.");

        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);

        album.ApiKey = Guid.NewGuid();
        album.Directory = album.ToMelodeeAlbumModel().AlbumDirectoryName(configuration.Configuration);
        album.CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);
        album.MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess;
        album.NameNormalized = album.NameNormalized.Nullify() ?? album.Name.ToNormalizedString() ?? album.Name;

        var validationResult = ValidateModel(album);
        if (!validationResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<Album?>(validationResult.Data.Item2
                ?.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage!).ToArray() ?? [])
            {
                Data = null,
                Type = MelodeeModels.OperationResponseType.ValidationFailure
            };
        }

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            scopedContext.Albums.Add(album);
            var result = 0;
            try
            {
                result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (result > 0)
            {
                await UpdateLibraryAggregateStatsByIdAsync(album.Artist.LibraryId, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        // After saving, the album.Id should be populated by EF Core
        if (album.Id > 0)
        {
            return await GetAsync(album.Id, cancellationToken);
        }

        // If for some reason the ID wasn't set, return the album as-is
        return new MelodeeModels.OperationResult<Album?>
        {
            Data = album
        };
    }

    public async Task<MelodeeModels.ImageBytesAndEtag> GetAlbumImageBytesAndEtagAsync(Guid? apiKey, string? size = null, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(apiKey, nameof(apiKey));
        Guard.Against.Expression(x => x == Guid.Empty, apiKey.Value, nameof(apiKey));

        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var album = await GetByApiKeyAsync(apiKey.Value, cancellationToken).ConfigureAwait(false);
        if (!album.IsSuccess || album.Data == null)
        {
            return new MelodeeModels.ImageBytesAndEtag(null, null);
        }

        var albumId = album.Data.Id;
        var cacheKey = CacheKeyAlbumImageBytesAndEtagTemplate.FormatSmart(albumId, size ?? nameof(ImageSize.Large));
        return await CacheManager.GetAsync(cacheKey, async () =>
        {
            var badEtag = Instant.MinValue.ToEtag();
            var sizeValue = size ?? nameof(ImageSize.Large);

            var albumDirectory = album.Data.ToFileSystemDirectoryInfo();
            if (!albumDirectory.Exists())
            {
                Logger.Warning("Album directory [{Directory}] does not exist for album [{AlbumId}].", albumDirectory.FullName(), albumId);
                return new MelodeeModels.ImageBytesAndEtag(null, badEtag);
            }

            // The size parameter allows for the admin to pre-create sized images so on the fly resize doesn't happen
            var albumImages = albumDirectory.AllFileImageTypeFileInfos().ToArray();
            var imageFile = albumImages
                .FirstOrDefault(x => x.Name.Contains($"-{sizeValue}", StringComparison.OrdinalIgnoreCase) ||
                                     x.Name.Contains($"-{sizeValue}", StringComparison.OrdinalIgnoreCase)) ?? albumImages.OrderBy(x => x.Name).FirstOrDefault();

            if (imageFile is not { Exists: true })
            {
                Logger.Warning("No image found for album [{ArtistId}] with size [{Size}].", albumId, sizeValue);
                return new MelodeeModels.ImageBytesAndEtag(null, badEtag);
            }

            var imageBytes = await File.ReadAllBytesAsync(imageFile.FullName, cancellationToken).ConfigureAwait(false);
            Logger.Information("Image found for album [{AlbumId}] with size [{Size}] CacheKey [{CacheKey}].", album, sizeValue, cacheKey);
            return new MelodeeModels.ImageBytesAndEtag(imageBytes, (album.Data.LastUpdatedAt ?? album.Data.CreatedAt).ToEtag());
        }, cancellationToken, configuration.CacheDuration(), Album.CacheRegion);
    }

    public async Task<MelodeeModels.OperationResult<bool>> SaveImageAsAlbumImageAsync(
        int albumId,
        bool deleteAllImages,
        byte[] imageBytes,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, albumId, nameof(albumId));
        Guard.Against.NullOrEmpty(imageBytes, nameof(imageBytes));

        var album = await GetAsync(albumId, cancellationToken);
        if (!album.IsSuccess || album.Data == null)
        {
            return new MelodeeModels.OperationResult<bool>("Unknown album.")
            {
                Data = false
            };
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = await SaveImageBytesAsAlbumImageAsync(
                    album.Data,
                    deleteAllImages,
                    imageBytes,
                    cancellationToken)
                .ConfigureAwait(false)
        };
    }

    private async Task<bool> SaveImageBytesAsAlbumImageAsync(
        Album album,
        bool deleteAllImages,
        byte[] imageBytes,
        CancellationToken cancellationToken = default)
    {
        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);
        var imageConvertor = new ImageConvertor(configuration);

        var albumPath = album.ToFileSystemDirectoryInfo();
        var albumImages = albumPath.FileInfosForExtension("jpg", false).ToArray();        
        if (deleteAllImages)
        {
            albumPath.DeleteAllFilesForExtension("*.jpg");
        }
        var totalAlbumImageCount = albumImages.Length == 1 ? 1 : albumImages.Length + 1;
        var newAlbumCoverFilename = Path.Combine(albumPath.FullName(), $"i-01-{Album.FrontImageType}.jpg");
        if (File.Exists(newAlbumCoverFilename))
        {
            File.Delete(newAlbumCoverFilename);
        }

        await File.WriteAllBytesAsync(newAlbumCoverFilename, imageBytes, cancellationToken).ConfigureAwait(false);
        await imageConvertor.ProcessFileAsync(
            albumPath,
            new MelodeeModels.FileSystemFileInfo
            {
                Name = newAlbumCoverFilename,
                Size = imageBytes.Length
            },
            cancellationToken).ConfigureAwait(false);
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
            await scopedContext.Albums
                .Where(x => x.Id == album.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.LastUpdatedAt, now)
                    .SetProperty(x => x.ImageCount, albumPath.ImageFilesFound), cancellationToken)
                .ConfigureAwait(false);
        }

        await ClearCacheAsync(album.Id, cancellationToken).ConfigureAwait(false);
        Logger.Information("Saved image for album [{ArtistId}] with {ImageCount} images.",
            album.Id, totalAlbumImageCount);        
        return true;
    }

    public async Task<MelodeeModels.OperationResult<bool>> SaveImageUrlAsAlbumImageAsync(
        int albumId,
        string imageUrl,
        bool deleteAllImages,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, albumId, nameof(albumId));
        Guard.Against.NullOrEmpty(imageUrl, nameof(imageUrl));

        var album = await GetAsync(albumId, cancellationToken);
        if (!album.IsSuccess || album.Data == null)
        {
            return new MelodeeModels.OperationResult<bool>("Unknown album.")
            {
                Data = false
            };
        }

        var result = false;
        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);
        try
        {
            var imageBytes = await httpClientFactory.BytesForImageUrlAsync(
                configuration.GetValue<string?>(SettingRegistry.SearchEngineUserAgent) ?? string.Empty,
                imageUrl,
                cancellationToken);
            if (imageBytes != null)
            {
                result = await SaveImageBytesAsAlbumImageAsync(
                    album.Data,
                    deleteAllImages,
                    imageBytes,
                    cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error attempting to download mage Url [{Url}] for album [{Album}]", imageUrl,
                album.Data.ToString());
        }

        return new MelodeeModels.OperationResult<bool>("An error has occured. OH NOES!")
        {
            Data = result
        };
    }

    private static IQueryable<Album> ApplyFilters(IQueryable<Album> query, MelodeeModels.PagedRequest pagedRequest)
    {
        if (pagedRequest.FilterBy == null || pagedRequest.FilterBy.Length == 0)
        {
            return query;
        }

        // If there's only one filter, apply it directly
        if (pagedRequest.FilterBy.Length == 1)
        {
            var filter = pagedRequest.FilterBy[0];
            var value = filter.Value.ToString();
            if (!string.IsNullOrEmpty(value))
            {
                var normalizedValue = value.ToNormalizedString();
                return filter.PropertyName.ToLowerInvariant() switch
                {
                    "name" or "namenormalized" => query.Where(a => EF.Functions.ILike(a.NameNormalized, $"%{normalizedValue}%")),
                    "alternatenames" => query.Where(a => a.AlternateNames != null && EF.Functions.ILike(a.AlternateNames, $"%{normalizedValue}%")),
                    "artistname" => query.Where(a => EF.Functions.ILike(a.Artist.NameNormalized, $"%{normalizedValue}%")),
                    "tags" => query.Where(a => a.Tags != null && EF.Functions.ILike(a.Tags, $"%{normalizedValue}%")),
                    "albumstatus" => int.TryParse(value, out var statusValue)
                        ? query.Where(a => a.AlbumStatus == statusValue)
                        : query,
                    "islocked" => bool.TryParse(value, out var lockedValue)
                        ? query.Where(a => a.IsLocked == lockedValue)
                        : query,
                    "releasedate" => DateTime.TryParse(value, out var dateValue)
                        ? query.Where(a => a.ReleaseDate.Year == dateValue.Year)
                        : query,
                    _ => query
                };
            }

            return query;
        }

        // For multiple filters, combine them with OR logic
        var filterPredicates = new List<Expression<Func<Album, bool>>>();

        foreach (var filter in pagedRequest.FilterBy)
        {
            var value = filter.Value.ToString();
            if (!string.IsNullOrEmpty(value))
            {
                var normalizedValue = value.ToNormalizedString();

                var predicate = filter.PropertyName switch
                {
                    "Name" or "NameNormalized" => (Expression<Func<Album, bool>>)(a => EF.Functions.ILike(a.NameNormalized, $"%{normalizedValue}%")),
                    "ArtistName" => (Expression<Func<Album, bool>>)(a => EF.Functions.ILike(a.Artist.NameNormalized, $"%{normalizedValue}%")),
                    "Tags" => (Expression<Func<Album, bool>>)(a => a.Tags != null && EF.Functions.ILike(a.Tags, $"%{normalizedValue}%")),
                    "AlbumStatus" => int.TryParse(value, out var statusValue)
                        ? (Expression<Func<Album, bool>>)(a => a.AlbumStatus == statusValue)
                        : null,
                    "IsLocked" => bool.TryParse(value, out var lockedValue)
                        ? (Expression<Func<Album, bool>>)(a => a.IsLocked == lockedValue)
                        : null,
                    "ReleaseDate" => DateTime.TryParse(value, out var dateValue)
                        ? (Expression<Func<Album, bool>>)(a => a.ReleaseDate.Year == dateValue.Year)
                        : null,
                    _ => null
                };

                if (predicate != null)
                {
                    filterPredicates.Add(predicate);
                }
            }
        }

        // If we have predicates, combine them with OR logic
        if (filterPredicates.Count > 0)
        {
            var combinedPredicate = filterPredicates.Aggregate((prev, next) =>
            {
                var parameter = Expression.Parameter(typeof(Album), "a");
                var left = Expression.Invoke(prev, parameter);
                var right = Expression.Invoke(next, parameter);
                var or = Expression.OrElse(left, right);
                return Expression.Lambda<Func<Album, bool>>(or, parameter);
            });

            query = query.Where(combinedPredicate);
        }

        return query;
    }
}
