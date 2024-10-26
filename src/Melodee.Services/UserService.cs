using Ardalis.GuardClauses;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using SmartFormat;

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

    public async Task<PagedResult<User>> ListAsync(PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        int usersCount;
        User[] users = [];
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            // TODO Filter and Sort dynamically
            usersCount = await scopedContext
                .Users
                .AsNoTracking()
                .CountAsync(cancellationToken)
                .ConfigureAwait(false);
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                users = await scopedContext
                    .Users
                    .AsNoTracking()
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return new PagedResult<User>
        {
            TotalCount = usersCount,
            TotalPages = pagedRequest.TotalPages(usersCount),
            Data = users
        };
    }

    public async Task<OperationResult<bool>> DeleteAsync(User currentuser, Guid apiKey, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        if (!currentuser.IsAdmin)
        {
            return new OperationResult<bool>
            {
                Data = false,
                Type = OperationResponseType.Unauthorized
            };
        }

        var user = await GetByApiKeyAsync(currentuser, apiKey, cancellationToken).ConfigureAwait(false);
        if (user.Data == null || !user.IsSuccess)
        {
            return new OperationResult<bool>
            {
                Data = false,
                Type = OperationResponseType.NotFound
            };
        }

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var deletedResult = await scopedContext.Users
                .Where(x => x.Id == user.Data.Id)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);
            return new OperationResult<bool>
            {
                Data = deletedResult > 0
            };
        }
    }

    public async Task<OperationResult<User?>> GetByEmailAddressAsync(User currentUser, string emailAddress, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(emailAddress, nameof(emailAddress));

        return await CacheManager.GetAsync(CacheKeyDetailByEmailAddressKeyTemplate.FormatSmart(emailAddress), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var userId = await scopedContext
                    .Users
                    .AsNoTracking()
                    .Where(x => x.Email.ToUpper() == emailAddress.ToUpper())
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
                return userId < 1
                    ? new OperationResult<User?>
                    {
                        Data = null
                    }
                    : await GetAsync(currentUser, userId, cancellationToken).ConfigureAwait(false);
            }
        }, cancellationToken);
    }

    public Task<OperationResult<User?>> GetByApiKeyAsync(User currentUser, Guid apiKey, CancellationToken cancellationToken = default)
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
                return await GetAsync(currentUser, userId, cancellationToken).ConfigureAwait(false);
            }
        }, cancellationToken);
    }

    public async Task<OperationResult<User?>> GetAsync(User currentUser, int id, CancellationToken cancellationToken = default)
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
        return new OperationResult<User?>
        {
            Data = result
        };
    }

    public async Task<OperationResult<User?>> LoginUserAsync(string emailAddress, string? password, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(emailAddress, nameof(emailAddress));

        if (password.Nullify() == null)
        {
            return new OperationResult<User?>
            {
                Data = null,
                Type = OperationResponseType.Unauthorized
            };
        }

        var user = await GetByEmailAddressAsync(ServiceUser.Instance.Value, emailAddress, cancellationToken).ConfigureAwait(false);
        if (!user.IsSuccess)
        {
            return new OperationResult<User?>
            {
                Data = null,
                Type = OperationResponseType.NotFound
            };
        }

        if (user.Data?.PasswordHash != password.ToPasswordHash())
        {
            return new OperationResult<User?>
            {
                Data = null,
                Type = OperationResponseType.Unauthorized
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

        // Sets return object so consumer sees new value, actual update to DB happens in another non blocking thread.
        user.Data.LastActivityAt = now;
        user.Data.LastLoginAt = now;
        return user;
    }

    public async Task<OperationResult<User?>> RegisterAsync(string username, string emailAddress, string password, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(emailAddress, nameof(emailAddress));
        Guard.Against.NullOrWhiteSpace(password, nameof(password));


        // Ensure no user exists with given email address
        var dbUserByEmailAddress = await GetByEmailAddressAsync(ServiceUser.Instance.Value, emailAddress, cancellationToken).ConfigureAwait(false);
        if (dbUserByEmailAddress.IsSuccess)
        {
            return new OperationResult<User?>(["User exists with Email address."])
            {
                Data = dbUserByEmailAddress.Data,
                Type = OperationResponseType.ValidationFailure
            };
        }

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var newUser = new User
            {
                UserName = username,
                Email = emailAddress,
                PasswordHash = password.ToPasswordHash(),
                CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow)
            };
            scopedContext.Users.Add(newUser);
            if (await scopedContext
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false) < 1)
            {
                return new OperationResult<User?>
                {
                    Data = null,
                    Type = OperationResponseType.Error
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

            return GetByEmailAddressAsync(ServiceUser.Instance.Value, emailAddress, cancellationToken).Result;
        }
    }

    public async Task<OperationResult<bool>> UpdateAsync(User currentUser, User detailToUpdate, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, detailToUpdate?.Id ?? 0, nameof(detailToUpdate));

        var result = false;
        var validationResult = ValidateModel(detailToUpdate);
        if (!validationResult.IsSuccess)
        {
            return new OperationResult<bool>(validationResult.Data.Item2?.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage!).ToArray() ?? [])
            {
                Data = false,
                Type = OperationResponseType.ValidationFailure
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
                    return new OperationResult<bool>
                    {
                        Data = false,
                        Type = OperationResponseType.NotFound
                    };
                }

                // Update values and save to db
                dbDetail.Description = detailToUpdate.Description;
                dbDetail.Email = detailToUpdate.Email;
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
                dbDetail.PasswordHash = detailToUpdate.PasswordHash.ToPasswordHash();
                dbDetail.SortOrder = detailToUpdate.SortOrder;
                dbDetail.Tags = detailToUpdate.Tags;
                dbDetail.UserName = detailToUpdate.UserName;

                dbDetail.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);

                result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;

                if (result)
                {
                    ClearCache(dbDetail.Email, dbDetail.ApiKey, dbDetail.Id);
                }
            }
        }

        return new OperationResult<bool>
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
