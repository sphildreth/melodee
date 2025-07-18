using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using Ardalis.GuardClauses;
using CsvHelper;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Filtering;
using Melodee.Common.MessageBus.Events;
using Melodee.Common.Models.Collection;
using Melodee.Common.Models.Importing;
using Melodee.Common.Plugins.Conversion.Image;
using Melodee.Common.Services.Caching;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Rebus.Bus;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Common.Services;

/// <summary>
///     User data domain service.
/// </summary>
public sealed class UserService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    IMelodeeConfigurationFactory configurationFactory,
    LibraryService libraryService,
    ArtistService artistService,
    AlbumService albumService,
    SongService songService,
    PlaylistService playlistService,
    IBus bus)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:user:apikey:{0}";
    private const string CacheKeyDetailByEmailAddressKeyTemplate = "urn:user:emailaddress:{0}";
    private const string CacheKeyDetailByUsernameTemplate = "urn:user:username:{0}";
    private const string CacheKeyDetailTemplate = "urn:user:{0}";

    public async Task<MelodeeModels.PagedResult<UserDataInfo>> ListAsync(
        MelodeeModels.PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        // Build the base query with performance optimizations
        var baseQuery = scopedContext.Users
            .AsNoTracking();

        // Apply filters using EF Core instead of raw SQL
        var filteredQuery = ApplyFilters(baseQuery, pagedRequest);

        // Get count efficiently
        var userCount = await filteredQuery.CountAsync(cancellationToken).ConfigureAwait(false);

        UserDataInfo[] users = [];
        if (!pagedRequest.IsTotalCountOnlyRequest)
        {
            // Apply ordering, skip, and take with projection to UserDataInfo
            var orderedQuery = ApplyOrdering(filteredQuery, pagedRequest);

            users = await orderedQuery
                .Skip(pagedRequest.SkipValue)
                .Take(pagedRequest.TakeValue)
                .Select(u => new UserDataInfo(
                    u.Id,
                    u.ApiKey,
                    u.IsLocked,
                    u.UserName,
                    u.Email,
                    u.IsAdmin,
                    u.LastActivityAt,
                    u.CreatedAt,
                    u.Tags,
                    u.LastUpdatedAt,
                    u.LastLoginAt))
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return new MelodeeModels.PagedResult<UserDataInfo>
        {
            TotalCount = userCount,
            TotalPages = pagedRequest.TotalPages(userCount),
            Data = users
        };
    }

    private static IQueryable<User> ApplyFilters(IQueryable<User> query, MelodeeModels.PagedRequest pagedRequest)
    {
        if (pagedRequest.FilterBy == null || pagedRequest.FilterBy.Length == 0)
        {
            return query;
        }

        // If there's only one filter, apply it directly
        if (pagedRequest.FilterBy.Length == 1)
        {
            var filter = pagedRequest.FilterBy[0];
            var filterValue = filter.Value.ToString().ToNormalizedString() ?? string.Empty;

            return filter.PropertyName.ToLowerInvariant() switch
            {
                "username" or "usernamenormalized" => filter.Operator switch
                {
                    FilterOperator.Contains => query.Where(u => u.UserNameNormalized.Contains(filterValue)),
                    FilterOperator.Equals => query.Where(u => u.UserNameNormalized == filterValue),
                    FilterOperator.StartsWith => query.Where(u => u.UserNameNormalized.StartsWith(filterValue)),
                    _ => query
                },
                "email" or "emailnormalized" => filter.Operator switch
                {
                    FilterOperator.Contains => query.Where(u => u.EmailNormalized.Contains(filterValue)),
                    FilterOperator.Equals => query.Where(u => u.EmailNormalized == filterValue),
                    FilterOperator.StartsWith => query.Where(u => u.EmailNormalized.StartsWith(filterValue)),
                    _ => query
                },
                "islocked" => filter.Operator switch
                {
                    FilterOperator.Equals when bool.TryParse(filterValue, out var boolValue) =>
                        query.Where(u => u.IsLocked == boolValue),
                    _ => query
                },
                "isadmin" => filter.Operator switch
                {
                    FilterOperator.Equals when bool.TryParse(filterValue, out var boolValue) =>
                        query.Where(u => u.IsAdmin == boolValue),
                    _ => query
                },
                _ => query
            };
        }

        // For multiple filters, combine them with OR logic
        var filterPredicates = new List<Expression<Func<User, bool>>>();

        foreach (var filter in pagedRequest.FilterBy)
        {
            var filterValue = filter.Value.ToString().ToNormalizedString() ?? string.Empty;

            var predicate = filter.PropertyName.ToLowerInvariant() switch
            {
                "username" or "usernamenormalized" => filter.Operator switch
                {
                    FilterOperator.Contains => (Expression<Func<User, bool>>)(u => u.UserNameNormalized.Contains(filterValue)),
                    FilterOperator.Equals => (Expression<Func<User, bool>>)(u => u.UserNameNormalized == filterValue),
                    FilterOperator.StartsWith => (Expression<Func<User, bool>>)(u => u.UserNameNormalized.StartsWith(filterValue)),
                    _ => null
                },
                "email" or "emailnormalized" => filter.Operator switch
                {
                    FilterOperator.Contains => (Expression<Func<User, bool>>)(u => u.EmailNormalized.Contains(filterValue)),
                    FilterOperator.Equals => (Expression<Func<User, bool>>)(u => u.EmailNormalized == filterValue),
                    FilterOperator.StartsWith => (Expression<Func<User, bool>>)(u => u.EmailNormalized.StartsWith(filterValue)),
                    _ => null
                },
                "islocked" => filter.Operator switch
                {
                    FilterOperator.Equals when bool.TryParse(filterValue, out var boolValue) =>
                        (Expression<Func<User, bool>>)(u => u.IsLocked == boolValue),
                    _ => null
                },
                "isadmin" => filter.Operator switch
                {
                    FilterOperator.Equals when bool.TryParse(filterValue, out var boolValue) =>
                        (Expression<Func<User, bool>>)(u => u.IsAdmin == boolValue),
                    _ => null
                },
                _ => null
            };

            if (predicate != null)
            {
                filterPredicates.Add(predicate);
            }
        }

        // If we have predicates, combine them with OR logic
        if (filterPredicates.Count > 0)
        {
            var combinedPredicate = filterPredicates.Aggregate((prev, next) =>
            {
                var parameter = Expression.Parameter(typeof(User), "u");
                var left = Expression.Invoke(prev, parameter);
                var right = Expression.Invoke(next, parameter);
                var or = Expression.OrElse(left, right);
                return Expression.Lambda<Func<User, bool>>(or, parameter);
            });

            query = query.Where(combinedPredicate);
        }

        return query;
    }

    private static IQueryable<User> ApplyOrdering(IQueryable<User> query, MelodeeModels.PagedRequest pagedRequest)
    {
        // Use the existing OrderByValue method from PagedRequest
        var orderByClause = pagedRequest.OrderByValue("UserName", MelodeeModels.PagedRequest.OrderAscDirection);

        // Parse the order by clause to determine field and direction
        var isDescending = orderByClause.Contains("DESC", StringComparison.OrdinalIgnoreCase);
        var fieldName = orderByClause.Split(' ')[0].Trim('"').ToLowerInvariant();

        return fieldName switch
        {
            "username" or "usernamenormalized" => isDescending ? query.OrderByDescending(u => u.UserNameNormalized) : query.OrderBy(u => u.UserNameNormalized),
            "email" or "emailnormalized" => isDescending ? query.OrderByDescending(u => u.EmailNormalized) : query.OrderBy(u => u.EmailNormalized),
            "createdat" => isDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            "lastupdatedat" => isDescending ? query.OrderByDescending(u => u.LastUpdatedAt) : query.OrderBy(u => u.LastUpdatedAt),
            "lastactivityat" => isDescending ? query.OrderByDescending(u => u.LastActivityAt) : query.OrderBy(u => u.LastActivityAt),
            "lastloginat" => isDescending ? query.OrderByDescending(u => u.LastLoginAt) : query.OrderBy(u => u.LastLoginAt),
            "isadmin" => isDescending ? query.OrderByDescending(u => u.IsAdmin) : query.OrderBy(u => u.IsAdmin),
            "islocked" => isDescending ? query.OrderByDescending(u => u.IsLocked) : query.OrderBy(u => u.IsLocked),
            _ => query.OrderBy(u => u.UserName)
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> DeleteAsync(
        int[] userIds,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(userIds, nameof(userIds));

        bool result;

        foreach (var userId in userIds)
        {
            var user = await GetAsync(userId, cancellationToken).ConfigureAwait(false);
            if (user.Data == null || !user.IsSuccess)
            {
                return new MelodeeModels.OperationResult<bool>
                {
                    Data = false,
                    Type = MelodeeModels.OperationResponseType.NotFound
                };
            }
        }

        var userImageLibrary = await libraryService.GetUserImagesLibraryAsync(cancellationToken).ConfigureAwait(false);

        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            foreach (var userId in userIds)
            {
                var user = scopedContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken).Result;
                if (user != null)
                {
                    var userAvatarFullname = user.ToAvatarFileName(userImageLibrary.Data.Path);
                    if (File.Exists(userAvatarFullname))
                    {
                        File.Delete(userAvatarFullname);
                    }

                    scopedContext.Users.Remove(user);
                }
            }

            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            result = true;
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<User?>> GetByEmailAddressAsync(
        string emailAddress,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(emailAddress, nameof(emailAddress));

        var emailAddressNormalized = emailAddress.ToNormalizedString() ?? emailAddress;
        var id = await CacheManager.GetAsync(
            CacheKeyDetailByEmailAddressKeyTemplate.FormatSmart(emailAddressNormalized), async () =>
            {
                using (Operation.At(LogEventLevel.Debug).Time("[{ServiceName}] GetByEmailAddressAsync [{EmailAddress}]",
                           nameof(UserService), emailAddress))
                {
                    await using (var scopedContext =
                                 await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
                    {
                        return await scopedContext.Users
                            .Where(u => u.EmailNormalized == emailAddressNormalized)
                            .Select(u => (int?)u.Id)
                            .FirstOrDefaultAsync(cancellationToken)
                            .ConfigureAwait(false);
                    }
                }
            }, cancellationToken).ConfigureAwait(false);
        return id == null
            ? new MelodeeModels.OperationResult<User?>
            {
                Data = null
            }
            : await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MelodeeModels.OperationResult<User?>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(username, nameof(username));
        var usernameNormalized = username.ToNormalizedString() ?? username;
        var id = await CacheManager.GetAsync(CacheKeyDetailByUsernameTemplate.FormatSmart(usernameNormalized),
            async () =>
            {
                using (Operation.At(LogEventLevel.Debug).Time("[{ServiceName}] GetByUsernameAsync [{Username}]",
                           nameof(UserService), username))
                {
                    await using (var scopedContext =
                                 await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
                    {
                        return await scopedContext.Users
                            .Where(u => u.UserNameNormalized == usernameNormalized)
                            .Select(u => (int?)u.Id)
                            .FirstOrDefaultAsync(cancellationToken)
                            .ConfigureAwait(false);
                    }
                }
            }, cancellationToken).ConfigureAwait(false);
        return id == null
            ? new MelodeeModels.OperationResult<User?>("User not found")
            {
                Data = null
            }
            : await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> IsUserAdminAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await GetByUsernameAsync(username, cancellationToken).ConfigureAwait(false);
        return user.Data?.IsAdmin ?? false;
    }

    public async Task<MelodeeModels.OperationResult<User?>> GetByApiKeyAsync(Guid apiKey, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(_ => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        var id = await CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using (var scopedContext =
                         await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext.Users
                    .Where(u => u.ApiKey == apiKey)
                    .Select(u => (int?)u.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }, cancellationToken).ConfigureAwait(false);
        return id == null
            ? new MelodeeModels.OperationResult<User?>
            {
                Data = null
            }
            : await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }

    private async Task<dynamic[]?> DatabaseSongInfosForAlbumApiKey(Guid albumApiKey, CancellationToken cancellationToken = default)
    {
        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await scopedContext.Songs
            .Include(s => s.Album)
            .Where(s => s.Album.ApiKey == albumApiKey)
            .Select(s => new { s.Id, Name = s.Title })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<UserArtist?> UserArtistAsync(int userId, Guid artistApiKey,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await scopedContext.UserArtists
            .Include(ua => ua.Artist)
            .Where(ua => ua.UserId == userId && ua.Artist.ApiKey == artistApiKey)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<UserAlbum?> UserAlbumAsync(int userId, Guid albumApiKey,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await scopedContext.UserAlbums
            .Include(ua => ua.Album)
            .Where(ua => ua.UserId == userId && ua.Album.ApiKey == albumApiKey)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<UserSong?> UserSongAsync(int userId, Guid songApiKey, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await scopedContext.UserSongs
            .Include(us => us.Song)
            .Where(us => us.UserId == userId && us.Song.ApiKey == songApiKey)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<MelodeeModels.OperationResult<UserSong?[]>> UserLastPlayedSongsAsync(int userId, int count, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var data = await scopedContext.UserSongs
            .Where(us => us.UserId == userId)
            .OrderByDescending(us => us.LastPlayedAt)
            .Take(count)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return new MelodeeModels.OperationResult<UserSong?[]>
        {
            Data = data.Cast<UserSong?>().ToArray()
        };
    }

    public async Task<UserSong[]?> UserSongsForAlbumAsync(int userId, Guid albumApiKey, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await scopedContext.UserSongs
            .Include(us => us.Song)
            .ThenInclude(s => s.Album)
            .Where(us => us.UserId == userId && us.Song.Album.ApiKey == albumApiKey)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<UserSong[]?> UserSongsForPlaylistAsync(int userId, Guid playlistApiKey,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        // Get all song IDs for the given playlist through Songs relationship
        var playlistSongIds = await scopedContext.Playlists
            .Where(p => p.ApiKey == playlistApiKey)
            .SelectMany(p => p.Songs)
            .Select(ps => ps.SongId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!playlistSongIds.Any())
        {
            return Array.Empty<UserSong>();
        }

        // Get user songs for songs in the playlist
        return await scopedContext.UserSongs
            .Include(us => us.Song)
            .Where(us => us.UserId == userId && playlistSongIds.Contains(us.SongId))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///     Return all shares that user created.
    /// </summary>
    public async Task<Share[]?> UserSharesAsync(int userId, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await scopedContext.Shares
            .Include(s => s.User)
            .Where(s => s.UserId == userId)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Generate a salt.</summary>
    /// <param name="saltLength">Length of the salt to generate</param>
    /// <param name="logRounds">
    ///     The log2 of the number of rounds of hashing to apply. The work factor therefore increases as (2
    ///     ** logRounds).
    /// </param>
    /// <returns>An encoded salt value.</returns>
    public static string GenerateSalt(int saltLength = 16, int logRounds = 10)
    {
        var randomBytes = new byte[saltLength];
        RandomNumberGenerator.Create().GetBytes(randomBytes);

        var rs = new StringBuilder(randomBytes.Length * 2 + 8);

        rs.Append("$2a$");
        if (logRounds < 10)
        {
            rs.Append('0');
        }

        rs.Append(logRounds);
        rs.Append('$');
        rs.Append(Encoding.UTF8.GetString(randomBytes).ToBase64());

        return rs.ToString();
    }

    public async Task<bool> IsPinned(int userId, UserPinType pinType, int pinId,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        await using var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await scopedContext.UserPins
            .Where(up => up.UserId == userId && up.PinId == pinId && up.PinType == (int)pinType)
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<MelodeeModels.OperationResult<bool>> SetAlbumRatingAsync(int userId, int albumId, int rating,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        var result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var album = await albumService.GetAsync(albumId, cancellationToken).ConfigureAwait(false);
            if (album.Data != null)
            {
                var userAlbum = await scopedContext.UserAlbums
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.AlbumId == albumId, cancellationToken)
                    .ConfigureAwait(false);
                if (userAlbum == null)
                {
                    userAlbum = new UserAlbum
                    {
                        UserId = userId,
                        AlbumId = albumId,
                        CreatedAt = now
                    };
                    scopedContext.UserAlbums.Add(userAlbum);
                }

                userAlbum.Rating = rating;
                userAlbum.LastUpdatedAt = now;
                result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;

                // Update album calculated rating using EF Core instead of raw SQL
                var avgRating = await scopedContext.UserAlbums
                    .Where(ua => ua.AlbumId == userAlbum.AlbumId)
                    .AverageAsync(ua => (decimal?)ua.Rating, cancellationToken)
                    .ConfigureAwait(false) ?? 0;

                await scopedContext.Albums
                    .Where(a => a.Id == userAlbum.AlbumId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(a => a.LastUpdatedAt, now)
                        .SetProperty(a => a.CalculatedRating, avgRating), cancellationToken)
                    .ConfigureAwait(false);

                await albumService.ClearCacheAsync(userAlbum.AlbumId, cancellationToken).ConfigureAwait(false);

                var user = await GetAsync(userId, cancellationToken).ConfigureAwait(false);
                ClearCache(user.Data!);
            }
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> SetSongRatingAsync(int userId, int songId, int rating,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        var result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var song = await songService.GetAsync(songId, cancellationToken).ConfigureAwait(false);
            if (song.Data != null)
            {
                var userSong = await scopedContext.UserSongs
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.SongId == songId, cancellationToken)
                    .ConfigureAwait(false);
                if (userSong == null)
                {
                    userSong = new UserSong
                    {
                        UserId = userId,
                        SongId = songId,
                        CreatedAt = now
                    };
                    scopedContext.UserSongs.Add(userSong);
                }

                userSong.Rating = rating;
                userSong.LastUpdatedAt = now;
                result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;

                // Update song calculated rating using EF Core instead of raw SQL
                var avgRating = await scopedContext.UserSongs
                    .Where(us => us.SongId == userSong.SongId)
                    .AverageAsync(us => (decimal?)us.Rating, cancellationToken)
                    .ConfigureAwait(false) ?? 0;

                await scopedContext.Songs
                    .Where(s => s.Id == userSong.SongId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.LastUpdatedAt, now)
                        .SetProperty(s => s.CalculatedRating, avgRating), cancellationToken)
                    .ConfigureAwait(false);

                await songService.ClearCacheAsync(userSong.SongId, cancellationToken).ConfigureAwait(false);

                var user = await GetAsync(userId, cancellationToken).ConfigureAwait(false);
                ClearCache(user.Data!);
            }
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> SetArtistRatingAsync(int userId, Guid artistApiKey,
        int rating, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        var result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var artist = await artistService.GetByApiKeyAsync(artistApiKey, cancellationToken).ConfigureAwait(false);
            if (artist.Data != null)
            {
                var userArtist = await scopedContext.UserArtists
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.ArtistId == artist.Data.Id, cancellationToken)
                    .ConfigureAwait(false);
                if (userArtist == null)
                {
                    userArtist = new UserArtist
                    {
                        UserId = userId,
                        ArtistId = artist.Data.Id,
                        CreatedAt = now
                    };
                    scopedContext.UserArtists.Add(userArtist);
                }

                userArtist.Rating = rating;
                userArtist.LastUpdatedAt = now;
                result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;

                // Update artist calculated rating using EF Core instead of raw SQL
                var avgRating = await scopedContext.UserArtists
                    .Where(ua => ua.ArtistId == userArtist.ArtistId)
                    .AverageAsync(ua => (decimal?)ua.Rating, cancellationToken)
                    .ConfigureAwait(false) ?? 0;

                await scopedContext.Artists
                    .Where(a => a.Id == userArtist.ArtistId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(a => a.LastUpdatedAt, now)
                        .SetProperty(a => a.CalculatedRating, avgRating), cancellationToken)
                    .ConfigureAwait(false);

                await artistService.ClearCacheAsync(userArtist.ArtistId, cancellationToken).ConfigureAwait(false);

                var user = await GetAsync(userId, cancellationToken).ConfigureAwait(false);
                ClearCache(user.Data!);
            }
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> SaveProfileImageAsync(int userId, byte[] imageBytes,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(imageBytes, nameof(imageBytes));
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        var userResult = await GetAsync(userId, cancellationToken).ConfigureAwait(false);
        if (!userResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<bool>(["Unknown user id"])
            {
                Data = false
            };
        }

        var user = userResult.Data!;
        var userImageLibrary = await libraryService.GetUserImagesLibraryAsync(cancellationToken).ConfigureAwait(false);
        var userAvatarFullname = user.ToAvatarFileName(userImageLibrary.Data.Path);
        if (File.Exists(userAvatarFullname))
        {
            File.Delete(userAvatarFullname);
        }

        imageBytes = await ImageConvertor.ConvertToGifFormat(imageBytes, cancellationToken).ConfigureAwait(false);

        await File.WriteAllBytesAsync(userAvatarFullname, imageBytes, cancellationToken).ConfigureAwait(false);

        return new MelodeeModels.OperationResult<bool>
        {
            Data = true
        };
    }

    public async Task<MelodeeModels.OperationResult<User?>> GetAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            using (Operation.At(LogEventLevel.Debug).Time("[{ServiceName}] GetAsync [{id}]", nameof(UserService), id))
            {
                await using (var scopedContext =
                             await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
                {
                    var user = await scopedContext
                        .Users
                        .Include(x => x.Pins)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                        .ConfigureAwait(false);

                    if (user?.Pins.Count > 0)
                    {
                        foreach (var pin in user.Pins)
                        {
                            switch (pin.PinTypeValue)
                            {
                                case UserPinType.Artist:
                                    var artistResult = await artistService.GetAsync(pin.PinId, cancellationToken)
                                        .ConfigureAwait(false);
                                    if (artistResult is { IsSuccess: true, Data: not null })
                                    {
                                        pin.Icon = "artist";
                                        pin.ImageUrl = $"/images/{artistResult.Data.ToApiKey()}{ImageSize.Thumbnail}";
                                        pin.LinkUrl = $"/data/artist/ {artistResult.Data.ApiKey}";
                                        pin.Text = artistResult.Data.Name;
                                    }

                                    break;
                                case UserPinType.Album:
                                    var albumResult = await albumService.GetAsync(pin.PinId, cancellationToken)
                                        .ConfigureAwait(false);
                                    if (albumResult is { IsSuccess: true, Data: not null })
                                    {
                                        pin.Icon = "album";
                                        pin.ImageUrl = $"/images/{albumResult.Data.ToApiKey()}/{ImageSize.Thumbnail}";
                                        pin.LinkUrl = $"/data/album/ {albumResult.Data.ApiKey}";
                                        pin.Text = albumResult.Data.Name;
                                    }

                                    break;
                                case UserPinType.Song:
                                    var songResult = await songService.GetAsync(pin.PinId, cancellationToken)
                                        .ConfigureAwait(false);
                                    if (songResult is { IsSuccess: true, Data: not null })
                                    {
                                        pin.Icon = "music_note";
                                        pin.ImageUrl = $"/images/{songResult.Data.ToApiKey()}/{ImageSize.Thumbnail}";
                                        pin.LinkUrl = $"/data/album/ {songResult.Data.Album.ApiKey}";
                                        pin.Text = songResult.Data.Title;
                                    }

                                    break;
                                case UserPinType.Playlist:
                                    var playlistResult = await playlistService.GetAsync(pin.PinId, cancellationToken)
                                        .ConfigureAwait(false);
                                    if (playlistResult is { IsSuccess: true, Data: not null })
                                    {
                                        pin.Icon = "playlist_play";
                                        pin.ImageUrl = $"/images/{playlistResult.Data.ToApiKey()}/{ImageSize.Thumbnail}";
                                        pin.LinkUrl = $"/data/playlist/ {playlistResult.Data.ApiKey}";
                                        pin.Text = playlistResult.Data.Name;
                                    }

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }

                    return user;
                }
            }
        }, cancellationToken).ConfigureAwait(false);
        return new MelodeeModels.OperationResult<User?>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<User?>> LoginUserByUsernameAsync(string userName, string? password,
        CancellationToken cancellationToken = default)
    {
        var user = await GetByUsernameAsync(userName, cancellationToken).ConfigureAwait(false);
        if (!user.IsSuccess || user.Data == null)
        {
            return new MelodeeModels.OperationResult<User?>
            {
                Data = null,
                Type = MelodeeModels.OperationResponseType.Unauthorized
            };
        }

        return await LoginUserAsync(user.Data.Email, password, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MelodeeModels.OperationResult<User?>> LoginUserAsync(string emailAddress, string? password,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(emailAddress, nameof(emailAddress));

        if (password.Nullify() == null)
        {
            return new MelodeeModels.OperationResult<User?>
            {
                Data = null,
                Type = MelodeeModels.OperationResponseType.Unauthorized
            };
        }

        var user = await GetByEmailAddressAsync(emailAddress, cancellationToken).ConfigureAwait(false);
        if (!user.IsSuccess || user.Data == null)
        {
            return new MelodeeModels.OperationResult<User?>("User not found")
            {
                Data = null,
                Type = MelodeeModels.OperationResponseType.NotFound
            };
        }

        bool authenticated;
        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);
        if (password?.StartsWith("enc:") ?? false)
        {
            authenticated = password[4..] == user.Data.PasswordEncrypted;
        }
        else
        {
            authenticated = user.Data.PasswordEncrypted == user.Data.Encrypt(password!, configuration);
        }

        if (!authenticated)
        {
            Log.Warning("[{ServiceName}] LoginUserAsync [{EmailAddress}] failed", nameof(UserService), emailAddress);
            return new MelodeeModels.OperationResult<User?>
            {
                Data = null,
                Type = MelodeeModels.OperationResponseType.Unauthorized
            };
        }

        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);

        await bus.SendLocal(new UserLoginEvent(user.Data!.Id, user.Data.UserName)).ConfigureAwait(false);

        // Sets return object so consumer sees new value, actual update to DB happens in another non-blocking thread.
        user.Data.LastActivityAt = now;
        user.Data.LastLoginAt = now;
        return user;
    }

    public async Task<MelodeeModels.OperationResult<int>> ImportUserFavoriteSongs(
        UserFavoriteSongConfiguration configuration, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(configuration, nameof(configuration));

        var user = await GetByApiKeyAsync(configuration.UserApiKey, cancellationToken).ConfigureAwait(false);
        if (!user.IsSuccess || user.Data == null)
        {
            return new MelodeeModels.OperationResult<int>("Unknown user")
            {
                Data = 0,
                Type = MelodeeModels.OperationResponseType.NotFound
            };
        }

        if (user.Data.IsLocked)
        {
            return new MelodeeModels.OperationResult<int>("User is locked.")
            {
                Data = 0,
                Type = MelodeeModels.OperationResponseType.Unauthorized
            };
        }

        var recordsCreated = 0;
        var recordsUpdated = 0;
        var recordsFound = 0;
        int songsFromCsv;

        var csvFilenfo = new FileInfo(configuration.CsvFileName);
        if (!csvFilenfo.Exists)
        {
            return new MelodeeModels.OperationResult<int>("CSV file does not exist.")
            {
                Data = 0,
                Type = MelodeeModels.OperationResponseType.NotFound
            };
        }

        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var userSongs = await scopedContext.UserSongs.Where(x => x.UserId == user.Data.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var newUserSongs = new List<UserSong>();

            var now = Instant.FromDateTimeUtc(DateTime.UtcNow);

            using (var reader = new StreamReader(csvFilenfo.OpenRead(), Encoding.UTF8))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    using (var dr = new CsvDataReader(csv))
                    {
                        var dt = new DataTable();
                        dt.Columns.Add(configuration.ArtistColumn, typeof(string));
                        dt.Columns.Add(configuration.AlbumColumn, typeof(string));
                        dt.Columns.Add(configuration.SongColumn, typeof(string));

                        dt.Load(dr);
                        songsFromCsv = dt.Rows.Count;
                        foreach (DataRow row in dt.Rows)
                        {
                            var artistName = row[configuration.ArtistColumn] as string;
                            var albumName = row[configuration.AlbumColumn] as string;
                            var songName = row[configuration.SongColumn] as string;

                            if (artistName.Nullify() != null && albumName.Nullify() != null &&
                                songName.Nullify() != null)
                            {
                                var artist = artistName.ToNormalizedString() ?? artistName!;
                                var album = albumName.ToNormalizedString() ?? albumName!;
                                var song = songName.ToNormalizedString() ?? songName!;
                                var artistResult = await artistService.GetByNameNormalized(artist, cancellationToken)
                                    .ConfigureAwait(false);
                                if (!artistResult.IsSuccess)
                                {
                                    Log.Warning(
                                        "[{ServiceName}] ImportUserFavoriteSongs failed : UNKNOWN ARTIST : [{ArtistName}] [{AlbumName}] [{SongName}]",
                                        nameof(UserService),
                                        artist,
                                        album,
                                        song);
                                    continue;
                                }

                                var artistAlbumListResult = await albumService
                                    .ListForArtistApiKeyAsync(new MelodeeModels.PagedRequest { PageSize = 1000 },
                                        artistResult.Data!.ApiKey, cancellationToken).ConfigureAwait(false);
                                var artistAlbum =
                                    artistAlbumListResult.Data.FirstOrDefault(x => x.NameNormalized == album);
                                if (artistAlbum == null)
                                {
                                    Log.Warning(
                                        "[{ServiceName}] ImportUserFavoriteSongs failed : UNKNOWN ALBUM : [{ArtistName}] [{AlbumName}] [{SongName}]",
                                        nameof(UserService),
                                        artist,
                                        album,
                                        song);
                                    continue;
                                }

                                var dbSongInfo =
                                    await DatabaseSongInfosForAlbumApiKey(artistAlbum.ApiKey, user.Data.Id,
                                        cancellationToken).ConfigureAwait(false);
                                var albumSong = dbSongInfo?.FirstOrDefault(x => x.Name.ToNormalizedString() == song);
                                if (albumSong == null)
                                {
                                    var dbSong = await scopedContext.Songs
                                        .Include(x => x.Album)
                                        .FirstOrDefaultAsync(
                                            x => x.TitleNormalized == song && x.Album.ArtistId == artistResult.Data.Id,
                                            cancellationToken)
                                        .ConfigureAwait(false);
                                    if (dbSong != null)
                                    {
                                        albumSong =
                                            (await DatabaseSongInfosForAlbumApiKey(dbSong.Album.ApiKey, user.Data.Id,
                                                cancellationToken).ConfigureAwait(false))
                                            ?.FirstOrDefault(x => x.Name.ToNormalizedString() == song);
                                    }

                                    if (albumSong == null)
                                    {
                                        Log.Warning(
                                            "[{ServiceName}] ImportUserFavoriteSongs failed : UNKNOWN SONG : [{ArtistName}] [{AlbumName}] [{SongName}]",
                                            nameof(UserService),
                                            artist,
                                            album,
                                            song);
                                        continue;
                                    }
                                }

                                var userSong = userSongs.FirstOrDefault(x => x.SongId == albumSong.Id);
                                if (userSong == null)
                                {
                                    userSong = new UserSong
                                    {
                                        UserId = user.Data.Id,
                                        SongId = albumSong.Id,
                                        CreatedAt = now
                                    };
                                    newUserSongs.Add(userSong);
                                    recordsCreated++;
                                }
                                else
                                {
                                    if (userSong is { IsStarred: true, Rating: > 0 })
                                    {
                                        recordsFound++;
                                        continue;
                                    }

                                    userSong.LastUpdatedAt = now;
                                    recordsUpdated++;
                                }

                                userSong.IsStarred = true;
                                userSong.Rating = userSong.Rating > 0 ? userSong.Rating : 1;
                                userSongs.Add(userSong);
                            }
                        }
                    }
                }
            }

            if (!configuration.IsPretend)
            {
                if (recordsCreated > 0 || recordsUpdated > 0)
                {
                    if (newUserSongs.Count > 0)
                    {
                        await scopedContext.UserSongs.AddRangeAsync(newUserSongs, cancellationToken)
                            .ConfigureAwait(false);
                    }

                    await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        Log.Information(
            "[{ServiceName}] ImportUserFavoriteSongs {Pretend} [{UserApiKey}] Songs From Csv [{CsvSongCount}] found {RecordsFound} created {RecordsCreated} records, updated {RecordsUpdated} records, missing [{MissingCount}]",
            nameof(UserService),
            configuration.IsPretend ? "[Pretend]" : string.Empty,
            songsFromCsv,
            user.Data.ApiKey,
            recordsFound,
            recordsCreated,
            recordsUpdated,
            songsFromCsv - (recordsFound + recordsCreated + recordsUpdated));

        return new MelodeeModels.OperationResult<int>
        {
            Data = recordsCreated + recordsUpdated
        };
    }

    public async Task<MelodeeModels.OperationResult<User?>> RegisterAsync(string username,
        string emailAddress,
        string plainTextPassword,
        string? registerPrivateCode,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(emailAddress, nameof(emailAddress));
        Guard.Against.NullOrWhiteSpace(plainTextPassword, nameof(plainTextPassword));

        // Ensure no user exists with given email address
        var dbUserByEmailAddress = await GetByEmailAddressAsync(emailAddress, cancellationToken).ConfigureAwait(false);
        if (dbUserByEmailAddress.IsSuccess)
        {
            return new MelodeeModels.OperationResult<User?>(["User exists with Email address."])
            {
                Data = null,
                Type = MelodeeModels.OperationResponseType.ValidationFailure
            };
        }

        // Ensure no user exists with given username
        var dbUserByUserName = await GetByUsernameAsync(username, cancellationToken).ConfigureAwait(false);
        if (dbUserByUserName.IsSuccess)
        {
            return new MelodeeModels.OperationResult<User?>(["User exists with Username."])
            {
                Data = null,
                Type = MelodeeModels.OperationResponseType.ValidationFailure
            };
        }

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);

            var configuredRegisterPrivateCode = configuration.GetValue<string>(SettingRegistry.RegisterPrivateCode);
            if (configuredRegisterPrivateCode != null && registerPrivateCode != configuredRegisterPrivateCode)
            {
                return new MelodeeModels.OperationResult<User?>("Invalid access code.")
                {
                    Data = null,
                    Type = MelodeeModels.OperationResponseType.Unauthorized
                };
            }

            var usersPublicKey = EncryptionHelper.GenerateRandomPublicKeyBase64();
            var emailNormalized = emailAddress.ToNormalizedString() ?? emailAddress.ToUpperInvariant();
            var newUser = new User
            {
                UserName = username,
                UserNameNormalized = username.ToNormalizedString() ?? username.ToUpperInvariant(),
                Email = emailAddress,
                EmailNormalized = emailNormalized,
                PublicKey = usersPublicKey,
                PasswordEncrypted =
                    EncryptionHelper.Encrypt(configuration.GetValue<string>(SettingRegistry.EncryptionPrivateKey)!,
                        plainTextPassword, usersPublicKey),
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            scopedContext.Users.Add(newUser);
            if (await scopedContext
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false) < 1)
            {
                return new MelodeeModels.OperationResult<User?>
                {
                    Data = null,
                    Type = MelodeeModels.OperationResponseType.Error
                };
            }

            // See if user is first user to register, is so then set to administrator
            var dbUserCount = await scopedContext
                .Users
                .CountAsync(cancellationToken)
                .ConfigureAwait(false);
            if (dbUserCount == 1)
            {
                await scopedContext
                    .Users
                    .Where(x => x.Email == emailAddress)
                    .ExecuteUpdateAsync(x => x.SetProperty(u => u.IsAdmin, true), cancellationToken)
                    .ConfigureAwait(false);
            }

            ClearCache(newUser.EmailNormalized, newUser.ApiKey, newUser.Id, newUser.UserNameNormalized);

            await LoginUserAsync(emailAddress, plainTextPassword, cancellationToken).ConfigureAwait(false);

            return GetByEmailAddressAsync(emailAddress, cancellationToken).Result;
        }
    }

    public async Task<MelodeeModels.OperationResult<bool>> UpdateAsync(User currentUser, User detailToUpdate,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, detailToUpdate.Id, nameof(detailToUpdate));

        bool result;
        var validationResult = ValidateModel(detailToUpdate);
        if (!validationResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<bool>(validationResult.Data.Item2
                ?.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage!).ToArray() ?? [])
            {
                Data = false,
                Type = MelodeeModels.OperationResponseType.ValidationFailure
            };
        }

        // Ensure no user exists with given email address
        var dbUserByEmailAddress =
            await GetByEmailAddressAsync(currentUser.Email, cancellationToken).ConfigureAwait(false);
        if (dbUserByEmailAddress.IsSuccess && dbUserByEmailAddress.Data!.Id != detailToUpdate.Id)
        {
            return new MelodeeModels.OperationResult<bool>(["User exists with Email address."])
            {
                Data = false,
                Type = MelodeeModels.OperationResponseType.ValidationFailure
            };
        }

        // Ensure no user exists with given username
        var dbUserByUserName = await GetByUsernameAsync(currentUser.UserName, cancellationToken).ConfigureAwait(false);
        if (dbUserByUserName.IsSuccess && dbUserByUserName.Data!.Id != detailToUpdate.Id)
        {
            return new MelodeeModels.OperationResult<bool>(["User exists with Username."])
            {
                Data = false,
                Type = MelodeeModels.OperationResponseType.ValidationFailure
            };
        }

        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            // Load the detail by DetailToUpdate.Id
            var dbDetail = await scopedContext
                .Users
                .FirstOrDefaultAsync(x => x.Id == detailToUpdate.Id, cancellationToken)
                .ConfigureAwait(false);

            if (dbDetail == null)
            {
                return new MelodeeModels.OperationResult<bool>
                {
                    Data = false,
                    Type = MelodeeModels.OperationResponseType.NotFound
                };
            }

            // Update values and save to db
            dbDetail.Description = detailToUpdate.Description;
            dbDetail.Email = detailToUpdate.Email;
            dbDetail.EmailNormalized =
                detailToUpdate.Email.ToNormalizedString() ?? detailToUpdate.Email.ToUpperInvariant();
            dbDetail.HasCommentRole = detailToUpdate.HasCommentRole;
            dbDetail.HasCoverArtRole = detailToUpdate.HasCoverArtRole;
            dbDetail.HasDownloadRole = detailToUpdate.HasDownloadRole;
            dbDetail.HasJukeboxRole = detailToUpdate.HasJukeboxRole;
            dbDetail.HasPlaylistRole = detailToUpdate.HasPlaylistRole;
            dbDetail.HasPodcastRole = detailToUpdate.HasPodcastRole;
            dbDetail.HasSettingsRole = detailToUpdate.HasSettingsRole;
            dbDetail.HasShareRole = detailToUpdate.HasShareRole;
            dbDetail.HasStreamRole = detailToUpdate.HasStreamRole;
            dbDetail.HasUploadRole = detailToUpdate.HasUploadRole;
            dbDetail.IsAdmin = detailToUpdate.IsAdmin;
            dbDetail.IsEditor = detailToUpdate.IsEditor;
            dbDetail.IsLocked = detailToUpdate.IsLocked;
            dbDetail.IsScrobblingEnabled = detailToUpdate.IsScrobblingEnabled;
            // Take whatever is newer
            dbDetail.LastActivityAt = dbDetail.LastActivityAt > detailToUpdate.LastActivityAt
                ? dbDetail.LastActivityAt
                : detailToUpdate.LastActivityAt;
            // Take whatever is newer
            dbDetail.LastLoginAt = dbDetail.LastLoginAt > detailToUpdate.LastLoginAt
                ? dbDetail.LastLoginAt
                : detailToUpdate.LastLoginAt;
            dbDetail.Notes = detailToUpdate.Notes;
            dbDetail.SortOrder = detailToUpdate.SortOrder;
            dbDetail.Tags = detailToUpdate.Tags;
            dbDetail.UserName = detailToUpdate.UserName;
            dbDetail.UserNameNormalized = detailToUpdate.UserName.ToUpperInvariant();

            dbDetail.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);

            result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;

            if (result)
            {
                ClearCache(dbDetail.EmailNormalized, dbDetail.ApiKey, dbDetail.Id, dbDetail.UserNameNormalized);
            }
        }


        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> UpdateLastLogin(UserLoginEvent eventData,
        CancellationToken cancellationToken = default)
    {
        using (Operation.At(LogEventLevel.Debug)
                   .Time("[{ServiceName}]: Data [{EventData}]", nameof(UserService), eventData.ToString()))
        {
            var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken))
            {
                var user = await GetAsync(eventData.UserId, cancellationToken).ConfigureAwait(false);
                if (user.Data != null)
                {
                    Trace.WriteLine($"[{nameof(UpdateLastLogin)}]: {eventData}");
                    await scopedContext.Users
                        .Where(x => x.Id == eventData.UserId)
                        .ExecuteUpdateAsync(setters =>
                            setters.SetProperty(x => x.LastActivityAt, now)
                                .SetProperty(x => x.LastLoginAt, now), cancellationToken).ConfigureAwait(false);
                    ClearCache(user.Data.Email, user.Data.ApiKey, user.Data.Id, user.Data.UserName);
                    // Prefetch as the user is clearly active
                    await GetAsync(eventData.UserId, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = true
        };
    }

    private void ClearCache(User user)
    {
        ClearCache(user.Email, user.ApiKey, user.Id, user.UserName);
    }

    private void ClearCache(string? emailAddress, Guid? apiKey, int? id, string? username)
    {
        if (emailAddress != null)
        {
            CacheManager.Remove(
                CacheKeyDetailByEmailAddressKeyTemplate.FormatSmart(emailAddress.ToNormalizedString() ?? emailAddress));
        }

        if (apiKey != null)
        {
            CacheManager.Remove(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey));
        }

        if (id != null)
        {
            CacheManager.Remove(CacheKeyDetailTemplate.FormatSmart(id));
        }

        if (username != null)
        {
            CacheManager.Remove(CacheKeyDetailByUsernameTemplate.FormatSmart(username));
        }
    }

    public async Task<MelodeeModels.OperationResult<bool>> ToggleGenreHatedAsync(int userId, string genre,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        var result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var user = await scopedContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
                .ConfigureAwait(false);
            if (user != null)
            {
                var normalizedGenre = genre.ToNormalizedString() ?? genre;
                var hatedGenres = user.HatedGenres.ToTags()?.ToList() ?? [];
                if (hatedGenres.Contains(normalizedGenre))
                {
                    hatedGenres.Remove(normalizedGenre);
                }
                else
                {
                    hatedGenres.Add(normalizedGenre);
                }

                user.HatedGenres = "".AddTags(hatedGenres);
                user.LastUpdatedAt = now;
                result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
                ClearCache(user);
            }
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> ToggleAristHatedAsync(int userId, Guid artistApiKey,
        bool isHated, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        var result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var artist = await artistService.GetByApiKeyAsync(artistApiKey, cancellationToken).ConfigureAwait(false);
            if (artist.Data != null)
            {
                var userArtist = await scopedContext.UserArtists
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.ArtistId == artist.Data.Id, cancellationToken)
                    .ConfigureAwait(false);
                if (userArtist == null)
                {
                    userArtist = new UserArtist
                    {
                        UserId = userId,
                        ArtistId = artist.Data.Id,
                        CreatedAt = now
                    };
                    scopedContext.UserArtists.Add(userArtist);
                }

                userArtist.IsHated = isHated;
                if (isHated)
                {
                    userArtist.IsStarred = false;
                    userArtist.StarredAt = null;
                }

                userArtist.LastUpdatedAt = now;
                result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
                var user = await GetAsync(userId, cancellationToken).ConfigureAwait(false);
                ClearCache(user.Data!);
            }
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> ToggleAristStarAsync(int userId, Guid artistApiKey,
        bool isStarred, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        var result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var artist = await artistService.GetByApiKeyAsync(artistApiKey, cancellationToken).ConfigureAwait(false);
            if (artist.Data != null)
            {
                var userArtist = await scopedContext.UserArtists
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.ArtistId == artist.Data.Id, cancellationToken)
                    .ConfigureAwait(false);
                if (userArtist == null)
                {
                    userArtist = new UserArtist
                    {
                        UserId = userId,
                        ArtistId = artist.Data.Id,
                        CreatedAt = now
                    };
                    scopedContext.UserArtists.Add(userArtist);
                }

                userArtist.StarredAt = isStarred ? now : null;
                userArtist.IsStarred = isStarred;
                if (isStarred)
                {
                    userArtist.IsHated = false;
                }

                userArtist.LastUpdatedAt = now;
                result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
                var user = await GetAsync(userId, cancellationToken).ConfigureAwait(false);
                ClearCache(user.Data!);
            }
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> ToggleArtistHatedAsync(int userId, Guid artistApiKey,
        bool isHated, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        var result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var artist = await artistService.GetByApiKeyAsync(artistApiKey, cancellationToken).ConfigureAwait(false);
            if (artist.Data != null)
            {
                var userArtist = await scopedContext.UserArtists
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.ArtistId == artist.Data.Id, cancellationToken)
                    .ConfigureAwait(false);
                if (userArtist == null)
                {
                    userArtist = new UserArtist
                    {
                        UserId = userId,
                        ArtistId = artist.Data.Id,
                        CreatedAt = now
                    };
                    scopedContext.UserArtists.Add(userArtist);
                }

                userArtist.IsHated = isHated;
                if (isHated)
                {
                    userArtist.IsStarred = false;
                    userArtist.StarredAt = null;
                }

                userArtist.LastUpdatedAt = now;
                result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
                var user = await GetAsync(userId, cancellationToken).ConfigureAwait(false);
                ClearCache(user.Data!);
            }
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> ToggleAlbumHatedAsync(int userId, Guid albumApiKey,
        bool isHated, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        var result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var album = await albumService.GetByApiKeyAsync(albumApiKey, cancellationToken).ConfigureAwait(false);
            if (album.Data != null)
            {
                var userAlbum = await scopedContext.UserAlbums
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.AlbumId == album.Data.Id, cancellationToken)
                    .ConfigureAwait(false);
                if (userAlbum == null)
                {
                    userAlbum = new UserAlbum
                    {
                        UserId = userId,
                        AlbumId = album.Data.Id,
                        CreatedAt = now,
                        LastPlayedAt = null
                    };
                    scopedContext.UserAlbums.Add(userAlbum);
                }

                userAlbum.IsHated = isHated;
                if (isHated)
                {
                    userAlbum.IsStarred = false;
                    userAlbum.StarredAt = null;
                }

                userAlbum.LastUpdatedAt = now;
                result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
                var user = await GetAsync(userId, cancellationToken).ConfigureAwait(false);
                ClearCache(user.Data!);
            }
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> ToggleArtistStarAsync(int userId, Guid albumApiKey,
        bool isStarred, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        var result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var artist = await artistService.GetByApiKeyAsync(albumApiKey, cancellationToken).ConfigureAwait(false);
            if (artist.Data != null)
            {
                var userArtist = await scopedContext.UserArtists
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.ArtistId == artist.Data.Id, cancellationToken)
                    .ConfigureAwait(false);
                if (userArtist == null)
                {
                    userArtist = new UserArtist
                    {
                        UserId = userId,
                        ArtistId = artist.Data.Id,
                        CreatedAt = now
                    };
                    scopedContext.UserArtists.Add(userArtist);
                }

                userArtist.StarredAt = isStarred ? now : null;
                userArtist.IsStarred = isStarred;
                if (isStarred)
                {
                    userArtist.IsHated = false;
                }

                userArtist.LastUpdatedAt = now;
                result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
                var user = await GetAsync(userId, cancellationToken).ConfigureAwait(false);
                ClearCache(user.Data!);
            }
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> ToggleAlbumStarAsync(int userId, Guid albumApiKey,
        bool isStarred, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        var result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var album = await albumService.GetByApiKeyAsync(albumApiKey, cancellationToken).ConfigureAwait(false);
            if (album.Data != null)
            {
                var userAlbum = await scopedContext.UserAlbums
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.AlbumId == album.Data.Id, cancellationToken)
                    .ConfigureAwait(false);
                if (userAlbum == null)
                {
                    userAlbum = new UserAlbum
                    {
                        UserId = userId,
                        AlbumId = album.Data.Id,
                        CreatedAt = now,
                        LastPlayedAt = null
                    };
                    scopedContext.UserAlbums.Add(userAlbum);
                }

                userAlbum.StarredAt = isStarred ? now : null;
                userAlbum.IsStarred = isStarred;
                if (isStarred)
                {
                    userAlbum.IsHated = false;
                }

                userAlbum.LastUpdatedAt = now;
                result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
                var user = await GetAsync(userId, cancellationToken).ConfigureAwait(false);
                ClearCache(user.Data!);
            }
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> ToggleSongHatedAsync(int userId, Guid songApiKey,
        bool isHated, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        var result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var song = await songService.GetByApiKeyAsync(songApiKey, cancellationToken).ConfigureAwait(false);
            if (song.Data != null)
            {
                var userSong = await scopedContext.UserSongs
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.SongId == song.Data.Id, cancellationToken)
                    .ConfigureAwait(false);
                if (userSong == null)
                {
                    userSong = new UserSong
                    {
                        UserId = userId,
                        SongId = song.Data.Id,
                        CreatedAt = now,
                        LastPlayedAt = null
                    };
                    scopedContext.UserSongs.Add(userSong);
                }

                userSong.IsHated = isHated;
                if (isHated)
                {
                    userSong.IsStarred = false;
                    userSong.StarredAt = null;
                }

                userSong.LastUpdatedAt = now;
                result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
                var user = await GetAsync(userId, cancellationToken).ConfigureAwait(false);
                ClearCache(user.Data!);
            }
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> TogglePinnedAsync(int userId, UserPinType pinType, int pinId,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        bool result;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var userPinTypeValue = (int)pinType;
            var userPin = await scopedContext
                .UserPins
                .Where(x => x.UserId == userId && x.PinId == pinId && x.PinType == userPinTypeValue)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (userPin == null)
            {
                userPin = new UserPin
                {
                    UserId = userId,
                    PinId = pinId,
                    PinType = userPinTypeValue,
                    CreatedAt = now
                };
                scopedContext.UserPins.Add(userPin);
            }
            else
            {
                scopedContext.UserPins.Remove(userPin);
            }

            result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
            var user = await GetAsync(userId, cancellationToken).ConfigureAwait(false);
            ClearCache(user.Data!);
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> ToggleSongStarAsync(int userId, Guid songApiKey, bool isStarred, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, userId, nameof(userId));

        var result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var song = await songService.GetByApiKeyAsync(songApiKey, cancellationToken).ConfigureAwait(false);
            if (song.Data != null)
            {
                var userSong = await scopedContext.UserSongs
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.SongId == song.Data.Id, cancellationToken)
                    .ConfigureAwait(false);
                if (userSong == null)
                {
                    userSong = new UserSong
                    {
                        UserId = userId,
                        SongId = song.Data.Id,
                        CreatedAt = now,
                        LastPlayedAt = null
                    };
                    scopedContext.UserSongs.Add(userSong);
                }

                userSong.StarredAt = isStarred ? now : null;
                userSong.IsStarred = isStarred;
                if (isStarred)
                {
                    userSong.IsHated = false;
                }

                userSong.LastUpdatedAt = now;
                result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
                var user = await GetAsync(userId, cancellationToken).ConfigureAwait(false);
                ClearCache(user.Data!);
            }
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }
}
