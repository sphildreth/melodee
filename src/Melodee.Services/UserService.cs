using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Services.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Services;

/// <summary>
///     User data domain service.
/// </summary>
public sealed class UserService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:user:apikey:{0}";
    private const string CacheKeyDetailByEmailAddressKeyTemplate = "urn:user:emailaddress:{0}";
    private const string CacheKeyDetailTemplate = "urn:user:{0}";

    public async Task<MelodeeModels.PagedResult<User>> ListAsync(MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        int usersCount;
        User[] users = [];
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var orderBy = pagedRequest.OrderByValue();
            var dbConn = scopedContext.Database.GetDbConnection();            
            var countSqlParts = pagedRequest.FilterByParts("SELECT COUNT(*) FROM \"Users\"");
            usersCount = await dbConn
                .QuerySingleAsync<int>(countSqlParts.Item1, countSqlParts.Item2)
                .ConfigureAwait(false);            
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                var listSqlParts = pagedRequest.FilterByParts("SELECT * FROM \"Users\"");
                var listSql = $"{listSqlParts.Item1} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
                if (dbConn is SqliteConnection)
                {
                    listSql = $"{listSqlParts.Item1 } ORDER BY {orderBy} LIMIT {pagedRequest.TakeValue} OFFSET {pagedRequest.SkipValue};";
                }
                users = (await dbConn
                    .QueryAsync<User>(listSql, listSqlParts.Item2)
                    .ConfigureAwait(false)).ToArray();
            }
        }

        return new MelodeeModels.PagedResult<User>
        {
            TotalCount = usersCount,
            TotalPages = pagedRequest.TotalPages(usersCount),
            Data = users
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> DeleteAsync(User currentuser, Guid apiKey, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        if (!currentuser.IsAdmin)
        {
            return new MelodeeModels.OperationResult<bool>
            {
                Data = false,
                Type = MelodeeModels.OperationResponseType.Unauthorized
            };
        }

        var user = await GetByApiKeyAsync(apiKey, cancellationToken).ConfigureAwait(false);
        if (user.Data == null || !user.IsSuccess)
        {
            return new MelodeeModels.OperationResult<bool>
            {
                Data = false,
                Type = MelodeeModels.OperationResponseType.NotFound
            };
        }

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var deletedResult = await scopedContext.Users
                .Where(x => x.Id == user.Data.Id)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);
            return new MelodeeModels.OperationResult<bool>
            {
                Data = deletedResult > 0
            };
        }
    }

    public async Task<MelodeeModels.OperationResult<User?>> GetByEmailAddressAsync(string emailAddress, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(emailAddress, nameof(emailAddress));

        return await CacheManager.GetAsync(CacheKeyDetailByEmailAddressKeyTemplate.FormatSmart(emailAddress), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    var dbConn = scopedContext.Database.GetDbConnection();
                    var userId = await dbConn
                        .ExecuteScalarAsync<int>("SELECT \"Id\" FROM \"Users\" WHERE \"EmailNormalized\" = @Email;", new { Email = emailAddress.ToUpperInvariant() })
                        .ConfigureAwait(false);
                    return userId < 1
                        ? new MelodeeModels.OperationResult<User?>
                        {
                            Data = null
                        }
                        : await GetAsync(userId, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Failed to get user by email address.");
                }

                return new MelodeeModels.OperationResult<User?>
                {
                    Data = null
                };
            }
        }, cancellationToken);
    }

    public Task<MelodeeModels.OperationResult<User?>> GetByApiKeyAsync(Guid apiKey, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        return CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var userId = await scopedContext
                    .Users
                    .AsNoTracking()
                    .Where(x => x.ApiKey == apiKey)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
                return await GetAsync(userId, cancellationToken).ConfigureAwait(false);
            }
        }, cancellationToken);
    }

    public async Task<MelodeeModels.OperationResult<User?>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext
                    .Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        return new MelodeeModels.OperationResult<User?>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<User?>> LoginUserAsync(string emailAddress, string? password, CancellationToken cancellationToken = default)
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
        if (!user.IsSuccess)
        {
            return new MelodeeModels.OperationResult<User?>
            {
                Data = null,
                Type = MelodeeModels.OperationResponseType.NotFound
            };
        }

        if (user.Data?.PasswordHash != (emailAddress + password).ToPasswordHash())
        {
            return new MelodeeModels.OperationResult<User?>
            {
                Data = null,
                Type = MelodeeModels.OperationResponseType.Unauthorized
            };
        }

        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);

        _ = Task.Run(async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbUser = await scopedContext
                    .Users
                    .SingleAsync(x => x.Email == emailAddress, cancellationToken)
                    .ConfigureAwait(false);
                dbUser.LastActivityAt = now;
                dbUser.LastLoginAt = now;
                await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                ClearCache(dbUser.Email, dbUser.ApiKey, dbUser.Id);
            }
        }, cancellationToken);

        // Sets return object so consumer sees new value, actual update to DB happens in another non-blocking thread.
        user.Data.LastActivityAt = now;
        user.Data.LastLoginAt = now;
        return user;
    }

    public async Task<MelodeeModels.OperationResult<User?>> RegisterAsync(string username, string emailAddress, string password, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(emailAddress, nameof(emailAddress));
        Guard.Against.NullOrWhiteSpace(password, nameof(password));


        // Ensure no user exists with given email address
        var dbUserByEmailAddress = await GetByEmailAddressAsync(emailAddress, cancellationToken).ConfigureAwait(false);
        if (dbUserByEmailAddress.IsSuccess)
        {
            return new MelodeeModels.OperationResult<User?>(["User exists with Email address."])
            {
                Data = dbUserByEmailAddress.Data,
                Type = MelodeeModels.OperationResponseType.ValidationFailure
            };
        }

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var newUser = new User
            {
                UserName = username,
                UserNameNormalized = username.ToUpperInvariant(),
                Email = emailAddress,
                EmailNormalized = emailAddress.ToUpperInvariant(),
                PasswordHash = (emailAddress + password).ToPasswordHash(),
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
                .CountAsync(x => x.Email == emailAddress, cancellationToken)
                .ConfigureAwait(false);
            if (dbUserCount == 1)
            {
                await scopedContext
                    .Users
                    .Where(x => x.Email == emailAddress)
                    .ExecuteUpdateAsync(x => x.SetProperty(u => u.IsAdmin, true), cancellationToken)
                    .ConfigureAwait(false);
            }
            
            ClearCache(emailAddress, null, null);
            
            return GetByEmailAddressAsync(emailAddress, cancellationToken).Result;
        }
    }

    public async Task<MelodeeModels.OperationResult<bool>> UpdateAsync(User currentUser, User detailToUpdate, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, detailToUpdate.Id, nameof(detailToUpdate));

        var result = false;
        var validationResult = ValidateModel(detailToUpdate);
        if (!validationResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<bool>(validationResult.Data.Item2?.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage!).ToArray() ?? [])
            {
                Data = false,
                Type = MelodeeModels.OperationResponseType.ValidationFailure
            };
        }

        if (detailToUpdate != null)
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
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
                dbDetail.EmailNormalized = detailToUpdate.Email.ToUpperInvariant();
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
                dbDetail.IsLocked = detailToUpdate.IsLocked;
                dbDetail.IsScrobblingEnabled = detailToUpdate.IsScrobblingEnabled;
                dbDetail.Notes = detailToUpdate.Notes;
                dbDetail.SortOrder = detailToUpdate.SortOrder;
                dbDetail.Tags = detailToUpdate.Tags;
                dbDetail.UserName = detailToUpdate.UserName;
                dbDetail.UserNameNormalized = detailToUpdate.UserName.ToUpperInvariant();

                dbDetail.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);

                result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;

                if (result)
                {
                    ClearCache(dbDetail.Email, dbDetail.ApiKey, dbDetail.Id);
                }
            }
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    private void ClearCache(string? emailAddress, Guid? apiKey, int? userId)
    {
        if (emailAddress != null)
        {
            CacheManager.Remove(CacheKeyDetailByEmailAddressKeyTemplate.FormatSmart(emailAddress));
        }

        if (apiKey != null)
        {
            CacheManager.Remove(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey));
        }

        if (userId != null)
        {
            CacheManager.Remove(CacheKeyDetailTemplate.FormatSmart(userId));
        }
    }
}
