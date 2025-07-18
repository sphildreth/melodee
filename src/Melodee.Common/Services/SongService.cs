using System.Globalization;
using Ardalis.GuardClauses;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Collection;
using Melodee.Common.Models.OpenSubsonic.DTO;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Common.Plugins.Scrobbling;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Serilog;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Common.Services;

public class SongService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    INowPlayingRepository nowPlayingRepository)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:song:apikey:{0}";
    private const string CacheKeyDetailByTitleNormalizedTemplate = "urn:song:titlenormalized:{0}";
    private const string CacheKeyDetailTemplate = "urn:song:{0}";

    public async Task<MelodeeModels.PagedResult<SongDataInfo>> ListNowPlayingAsync(MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var songCount = 0;
        SongDataInfo[] songs = [];
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var nowPlaying = await nowPlayingRepository.GetNowPlayingAsync(cancellationToken).ConfigureAwait(false);
            if (nowPlaying.Data.Length > 0)
            {
                var nowPlayingSongIds = nowPlaying.Data.Select(x => x.Scrobble.SongId).ToArray();
                songCount = nowPlayingSongIds.Length;

                if (!pagedRequest.IsTotalCountOnlyRequest)
                {
                    // Create base query using EF Core
                    var baseQuery = scopedContext.Songs
                        .Where(s => nowPlayingSongIds.Contains(s.Id))
                        .Include(s => s.Album)
                            .ThenInclude(a => a.Artist)
                        .AsNoTracking();

                    // Apply ordering
                    var orderedQuery = ApplyOrdering(baseQuery, pagedRequest);

                    // Execute query with paging and project to SongDataInfo
                    var rawSongs = await orderedQuery
                        .Skip(pagedRequest.SkipValue)
                        .Take(pagedRequest.TakeValue)
                        .ToArrayAsync(cancellationToken)
                        .ConfigureAwait(false);

                    songs = rawSongs.Select(s => new SongDataInfo(
                        s.Id,
                        s.ApiKey,
                        s.IsLocked,
                        s.Title,
                        s.TitleNormalized,
                        s.SongNumber,
                        s.Album.ReleaseDate,
                        s.Album.Name,
                        s.Album.ApiKey,
                        s.Album.Artist.Name,
                        s.Album.Artist.ApiKey,
                        s.FileSize,
                        s.Duration,
                        s.CreatedAt,
                        s.Tags ?? string.Empty,
                        false, // UserStarred - would need user context
                        0 // UserRating - would need user context
                    )).ToArray();
                }
            }
        }

        return new MelodeeModels.PagedResult<SongDataInfo>
        {
            TotalCount = songCount,
            TotalPages = pagedRequest.TotalPages(songCount),
            Data = songs
        };
    }

    public async Task<MelodeeModels.PagedResult<SongDataInfo>> ListForContributorsAsync(MelodeeModels.PagedRequest pagedRequest, string contributorName, CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        
        // Create base query using EF Core with proper joins and filtering
        // Use normalized string comparison for better performance
        var normalizedContributorName = contributorName.ToNormalizedString();
        
        var contributorsQuery = scopedContext.Contributors
            .Where(c => c.ContributorName != null && c.ContributorName.Contains(normalizedContributorName))
            .Where(c => c.Song != null)
            .Include(c => c.Song!)
                .ThenInclude(s => s.Album)
                    .ThenInclude(a => a.Artist)
            .AsNoTracking();

        // Get the songs from contributors and project to distinct song IDs first
        var songIds = await contributorsQuery
            .Select(c => c.Song!.Id)
            .Distinct()
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var songCount = songIds.Length;
        SongDataInfo[] songs = [];
        
        if (!pagedRequest.IsTotalCountOnlyRequest && songIds.Length > 0)
        {
            // Now query songs directly with proper includes
            var baseQuery = scopedContext.Songs
                .Where(s => songIds.Contains(s.Id))
                .Include(s => s.Album)
                    .ThenInclude(a => a.Artist)
                .AsNoTracking();

            // Apply ordering and paging
            var orderedQuery = ApplyOrdering(baseQuery, pagedRequest);
            
            var rawSongs = await orderedQuery
                .Skip(pagedRequest.SkipValue)
                .Take(pagedRequest.TakeValue)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            songs = rawSongs.Select(s => new SongDataInfo(
                s.Id,
                s.ApiKey,
                s.IsLocked,
                s.Title,
                s.TitleNormalized,
                s.SongNumber,
                s.Album.ReleaseDate,
                s.Album.Name,
                s.Album.ApiKey,
                s.Album.Artist.Name,
                s.Album.Artist.ApiKey,
                s.FileSize,
                s.Duration,
                s.CreatedAt,
                s.Tags ?? string.Empty,
                false, // UserStarred - would need user context
                0 // UserRating - would need user context
            )).ToArray();
        }

        return new MelodeeModels.PagedResult<SongDataInfo>
        {
            TotalCount = songCount,
            TotalPages = pagedRequest.TotalPages(songCount),
            Data = songs
        };
    }

    public async Task<MelodeeModels.PagedResult<SongDataInfo>> ListAsync(MelodeeModels.PagedRequest pagedRequest, int userId, CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        
        // Create base query with user-specific data
        var baseQuery = scopedContext.Songs
            .Include(s => s.Album)
                .ThenInclude(a => a.Artist)
            .Include(s => s.UserSongs.Where(us => us.UserId == userId))
            .AsNoTracking();

        // Apply filters
        var filteredQuery = ApplyFilters(baseQuery, pagedRequest, userId);

        // Get total count efficiently
        var songCount = await filteredQuery.CountAsync(cancellationToken).ConfigureAwait(false);

        SongDataInfo[] songs = [];
        
        if (!pagedRequest.IsTotalCountOnlyRequest)
        {
            // Apply ordering and paging
            var orderedQuery = ApplyOrdering(filteredQuery, pagedRequest);
            
            var rawSongs = await orderedQuery
                .Skip(pagedRequest.SkipValue)
                .Take(pagedRequest.TakeValue)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            songs = rawSongs.Select(s => new SongDataInfo(
                s.Id,
                s.ApiKey,
                s.IsLocked,
                s.Title,
                s.TitleNormalized,
                s.SongNumber,
                s.Album.ReleaseDate,
                s.Album.Name,
                s.Album.ApiKey,
                s.Album.Artist.Name,
                s.Album.Artist.ApiKey,
                s.FileSize,
                s.Duration,
                s.CreatedAt,
                s.Tags ?? string.Empty,
                s.UserSongs.FirstOrDefault()?.IsStarred ?? false,
                s.UserSongs.FirstOrDefault()?.Rating ?? 0
            )).ToArray();
        }

        return new MelodeeModels.PagedResult<SongDataInfo>
        {
            TotalCount = songCount,
            TotalPages = pagedRequest.TotalPages(songCount),
            Data = songs
        };
    }

    private static IQueryable<Song> ApplyFilters(IQueryable<Song> query, MelodeeModels.PagedRequest pagedRequest, int? userId = null)
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
                    "title" or "titlenormalized" => query.Where(s => s.TitleNormalized.Contains(normalizedValue)),
                    "albumname" => query.Where(s => s.Album.NameNormalized.Contains(normalizedValue)),
                    "artistname" => query.Where(s => s.Album.Artist.NameNormalized.Contains(normalizedValue)),
                    "tags" => query.Where(s => s.Tags != null && s.Tags.Contains(normalizedValue)),
                    "islocked" => bool.TryParse(value, out var lockedValue) 
                        ? query.Where(s => s.IsLocked == lockedValue) 
                        : query,
                    "userstarred" when userId.HasValue => bool.TryParse(value, out var starredValue) 
                        ? query.Where(s => s.UserSongs.Any(us => us.UserId == userId.Value && us.IsStarred == starredValue))
                        : query,
                    "userrating" when userId.HasValue => int.TryParse(value, out var ratingValue) 
                        ? query.Where(s => s.UserSongs.Any(us => us.UserId == userId.Value && us.Rating == ratingValue))
                        : query,
                    _ => query
                };
            }
            return query;
        }

        // For multiple filters, combine them with OR logic
        var filterPredicates = new List<System.Linq.Expressions.Expression<Func<Song, bool>>>();

        foreach (var filter in pagedRequest.FilterBy)
        {
            var value = filter.Value.ToString();
            if (!string.IsNullOrEmpty(value))
            {
                var normalizedValue = value.ToNormalizedString();
                
                var predicate = filter.PropertyName.ToLowerInvariant() switch
                {
                    "title" or "titlenormalized" => (System.Linq.Expressions.Expression<Func<Song, bool>>)(s => s.TitleNormalized.Contains(normalizedValue)),
                    "albumname" => (System.Linq.Expressions.Expression<Func<Song, bool>>)(s => s.Album.NameNormalized.Contains(normalizedValue)),
                    "artistname" => (System.Linq.Expressions.Expression<Func<Song, bool>>)(s => s.Album.Artist.NameNormalized.Contains(normalizedValue)),
                    "tags" => (System.Linq.Expressions.Expression<Func<Song, bool>>)(s => s.Tags != null && s.Tags.Contains(normalizedValue)),
                    "islocked" => bool.TryParse(value, out var lockedValue) 
                        ? (System.Linq.Expressions.Expression<Func<Song, bool>>)(s => s.IsLocked == lockedValue)
                        : null,
                    "userstarred" when userId.HasValue => bool.TryParse(value, out var starredValue) 
                        ? (System.Linq.Expressions.Expression<Func<Song, bool>>)(s => s.UserSongs.Any(us => us.UserId == userId.Value && us.IsStarred == starredValue))
                        : null,
                    "userrating" when userId.HasValue => int.TryParse(value, out var ratingValue) 
                        ? (System.Linq.Expressions.Expression<Func<Song, bool>>)(s => s.UserSongs.Any(us => us.UserId == userId.Value && us.Rating == ratingValue))
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
                var parameter = System.Linq.Expressions.Expression.Parameter(typeof(Song), "s");
                var left = System.Linq.Expressions.Expression.Invoke(prev, parameter);
                var right = System.Linq.Expressions.Expression.Invoke(next, parameter);
                var or = System.Linq.Expressions.Expression.OrElse(left, right);
                return System.Linq.Expressions.Expression.Lambda<Func<Song, bool>>(or, parameter);
            });

            query = query.Where(combinedPredicate);
        }

        return query;
    }

    private static IQueryable<Song> ApplyOrdering(IQueryable<Song> query, MelodeeModels.PagedRequest pagedRequest)
    {
        // Use the existing OrderByValue method from PagedRequest
        var orderByClause = pagedRequest.OrderByValue("Title", MelodeeModels.PagedRequest.OrderAscDirection);
        
        // Parse the order by clause to determine field and direction
        var isDescending = orderByClause.Contains("DESC", StringComparison.OrdinalIgnoreCase);
        var fieldName = orderByClause.Split(' ')[0].Trim('"').ToLowerInvariant();

        return fieldName switch
        {
            "title" or "titlenormalized" => isDescending ? query.OrderByDescending(s => s.Title) : query.OrderBy(s => s.Title),
            "songnumber" => isDescending ? query.OrderByDescending(s => s.SongNumber) : query.OrderBy(s => s.SongNumber),
            "albumname" => isDescending ? query.OrderByDescending(s => s.Album.Name) : query.OrderBy(s => s.Album.Name),
            "artistname" => isDescending ? query.OrderByDescending(s => s.Album.Artist.Name) : query.OrderBy(s => s.Album.Artist.Name),
            "duration" => isDescending ? query.OrderByDescending(s => s.Duration) : query.OrderBy(s => s.Duration),
            "filesize" => isDescending ? query.OrderByDescending(s => s.FileSize) : query.OrderBy(s => s.FileSize),
            "createdat" => isDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
            "releasedate" => isDescending ? query.OrderByDescending(s => s.Album.ReleaseDate) : query.OrderBy(s => s.Album.ReleaseDate),
            _ => query.OrderBy(s => s.Title)
        };
    }

    public async Task<MelodeeModels.OperationResult<Song?>> GetAsync(int id,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            await using (var scopedContext =
                         await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext
                    .Songs
                    .Include(x => x.Contributors).ThenInclude(x => x.Artist)
                    .Include(x => x.Album).ThenInclude(x => x.Artist)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        return new MelodeeModels.OperationResult<Song?>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<Song?>> GetByApiKeyAsync(Guid apiKey,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(_ => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        var id = await CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            
            return await scopedContext.Songs
                .AsNoTracking()
                .Where(s => s.ApiKey == apiKey)
                .Select(s => (int?)s.Id)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }, cancellationToken);
        
        if (id == null)
        {
            return new MelodeeModels.OperationResult<Song?>("Unknown song")
            {
                Data = null
            };
        }

        return await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task ClearCacheAsync(int songId, CancellationToken cancellationToken)
    {
        var song = await GetAsync(songId, cancellationToken).ConfigureAwait(false);
        if (song.Data != null)
        {
            CacheManager.Remove(CacheKeyDetailByApiKeyTemplate.FormatSmart(song.Data.ApiKey));
            CacheManager.Remove(CacheKeyDetailByTitleNormalizedTemplate.FormatSmart(song.Data.TitleNormalized));
            CacheManager.Remove(CacheKeyDetailTemplate.FormatSmart(song.Data.Id));
        }
    }

    public Task<MelodeeModels.OperationResult<bool>> DeleteAsync(int[] toArray, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<MelodeeModels.OperationResult<StreamResponse>> GetStreamForSongAsync(MelodeeModels.UserInfo user, Guid apiKey, CancellationToken cancellationToken = default)
    {
        var song = await GetByApiKeyAsync(apiKey, cancellationToken).ConfigureAwait(false);
        if (song.Data == null)
        {
            return new MelodeeModels.OperationResult<StreamResponse>("Unknown song")
            {
                Type = MelodeeModels.OperationResponseType.NotFound,
                Data = new StreamResponse
                (
                    new Dictionary<string, StringValues>([]),
                    false,
                    []
                )
            };
        }

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        
        // Use EF Core query instead of raw SQL
        var songStreamInfo = await scopedContext.Songs
            .Where(s => s.ApiKey == apiKey)
            .Include(s => s.Album)
                .ThenInclude(a => a.Artist)
                    .ThenInclude(ar => ar.Library)
            .AsNoTracking()
            .Select(s => new SongStreamInfo(
                s.Album.Artist.Library.Path + s.Album.Artist.Directory + s.Album.Directory + s.FileName,
                s.FileSize,
                s.Duration / 1000.0,
                s.BitRate,
                s.ContentType
            ))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!(songStreamInfo?.TrackFileInfo.Exists ?? false))
        {
            Logger.Warning("[{ServiceName}] Stream request for song that was not found. User [{ApiRequest}] Song ApiKey [{ApiKey}]",
                nameof(SongService), user.ToString(), song.Data.ApiKey);
            return new MelodeeModels.OperationResult<StreamResponse>
            {
                Data = new StreamResponse
                (
                    new Dictionary<string, StringValues>([]),
                    false,
                    []
                )
            };
        }

        var bytesToRead = (int)songStreamInfo.FileSize;
        var trackBytes = new byte[bytesToRead];
        var numberOfBytesRead = 0;
        await using (var fs = songStreamInfo.TrackFileInfo.OpenRead())
        {
            try
            {
                fs.Seek(0, SeekOrigin.Begin);
                numberOfBytesRead = await fs.ReadAsync(trackBytes.AsMemory(0, bytesToRead), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Reading song [{SongInfo}]", songStreamInfo);
            }
        }

        return new MelodeeModels.OperationResult<StreamResponse>
        {
            Data = new StreamResponse
            (
                new Dictionary<string, StringValues>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Accept-Ranges", "bytes" },
                    { "Cache-Control", "no-store, must-revalidate, no-cache, max-age=0" },
                    { "Content-Duration", songStreamInfo.Duration.ToString(CultureInfo.InvariantCulture) },
                    { "Content-Length", numberOfBytesRead.ToString() },
                    { "Content-Range", $"bytes 0-{songStreamInfo.FileSize}/{numberOfBytesRead}" },
                    { "Content-Type", songStreamInfo.ContentType },
                    { "Expires", "Mon, 01 Jan 1990 00:00:00 GMT" }
                },
                numberOfBytesRead > 0,
                trackBytes
            )
        };
    }
}
