using System.Security.Cryptography;
using System.Text;
using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Extensions;
using Melodee.Common.MessageBus.Events;
using Melodee.Common.Services.Interfaces;
using Melodee.Common.Utility;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NodaTime;
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
    IMelodeeConfigurationFactory configurationFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:user:apikey:{0}";
    private const string CacheKeyDetailByEmailAddressKeyTemplate = "urn:user:emailaddress:{0}";
    private const string CacheKeyDetailByUsernameTemplate = "urn:user:username:{0}";
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
                    listSql = $"{listSqlParts.Item1} ORDER BY {orderBy} LIMIT {pagedRequest.TakeValue} OFFSET {pagedRequest.SkipValue};";
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

    public async Task<MelodeeModels.OperationResult<bool>> DeleteUsersAsync(int[] ids, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<MelodeeModels.OperationResult<bool>> DeleteAsync(User currentUser, Guid apiKey, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        if (!currentUser.IsAdmin)
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

        var emailAddressNormalized = emailAddress.ToNormalizedString() ?? emailAddress;
        var id = await CacheManager.GetAsync(CacheKeyDetailByEmailAddressKeyTemplate.FormatSmart(emailAddressNormalized), async () =>
        {
            using (Operation.At(LogEventLevel.Debug).Time("[{ServiceName}] GetByEmailAddressAsync [{EmailAddress}]", nameof(UserService), emailAddress))
            {
                await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
                {
                    var dbConn = scopedContext.Database.GetDbConnection();
                    return await dbConn
                        .ExecuteScalarAsync<int?>("SELECT \"Id\" FROM \"Users\" WHERE \"EmailNormalized\" = @Email;", new { Email = emailAddressNormalized })
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
        var id = await CacheManager.GetAsync(CacheKeyDetailByUsernameTemplate.FormatSmart(usernameNormalized), async () =>
        {
            using (Operation.At(LogEventLevel.Debug).Time("[{ServiceName}] GetByUsernameAsync [{Username}]", nameof(UserService), username))
            {
                await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
                {
                    var dbConn = scopedContext.Database.GetDbConnection();
                    return await dbConn
                        .ExecuteScalarAsync<int?>("SELECT \"Id\" FROM \"Users\" WHERE \"UserNameNormalized\" = @Username;", new { Username = usernameNormalized })
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

    public async Task<bool> IsUserAdminAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await GetByUsernameAsync(username, cancellationToken).ConfigureAwait(false);
        return user?.Data?.IsAdmin ?? false;
    }

    public async Task<MelodeeModels.OperationResult<User?>> GetByApiKeyAsync(Guid apiKey, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        var id = await CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                return await dbConn
                    .ExecuteScalarAsync<int?>("SELECT \"Id\" FROM \"Users\" WHERE \"ApiKey\" = @ApiKey;", new { ApiKey = apiKey })
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

    public async Task<MelodeeModels.OperationResult<User?>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            using (Operation.At(LogEventLevel.Debug).Time("[{ServiceName}] GetAsync [{id}]", nameof(UserService), id))
            {
                await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
                {
                    return await scopedContext
                        .Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                        .ConfigureAwait(false);
                }
            }
        }, cancellationToken).ConfigureAwait(false);
        return new MelodeeModels.OperationResult<User?>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<User?>> LoginUserByUsernameAsync(string userName, string? password, CancellationToken cancellationToken = default)
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
        if (!user.IsSuccess || user.Data == null)
        {
            return new MelodeeModels.OperationResult<User?>
            {
                Data = null,
                Type = MelodeeModels.OperationResponseType.NotFound
            };
        }

        var authenticated = false;
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
            return new MelodeeModels.OperationResult<User?>
            {
                Data = null,
                Type = MelodeeModels.OperationResponseType.Unauthorized
            };
        }

        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            await scopedContext.Users
                .Where(x => x.Id == user.Data.Id)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(x => x.LastActivityAt, now)
                        .SetProperty(x => x.LastLoginAt, now), cancellationToken)
                .ConfigureAwait(false);
            ClearCache(user.Data.EmailNormalized, user.Data.ApiKey, user.Data.Id, user.Data.UserNameNormalized);
        }

        // Sets return object so consumer sees new value, actual update to DB happens in another non-blocking thread.
        user.Data.LastActivityAt = now;
        user.Data.LastLoginAt = now;
        return user;
    }

    public async Task<MelodeeModels.OperationResult<User?>> RegisterAsync(string username, string emailAddress, string plainTextPassword, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(emailAddress, nameof(emailAddress));
        Guard.Against.NullOrWhiteSpace(plainTextPassword, nameof(plainTextPassword));

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
            var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);
            var usersPublicKey = EncryptionHelper.GenerateRandomPublicKeyBase64();
            var emailNormalized = emailAddress.ToNormalizedString() ?? emailAddress.ToUpperInvariant();
            var newUser = new User
            {
                UserName = username,
                UserNameNormalized = username.ToNormalizedString() ?? username.ToUpperInvariant(),
                Email = emailAddress,
                EmailNormalized = emailNormalized,
                PublicKey = usersPublicKey,
                PasswordEncrypted = EncryptionHelper.Encrypt(configuration.GetValue<string>(SettingRegistry.EncryptionPrivateKey)!, plainTextPassword, usersPublicKey),
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

            ClearCache(newUser.EmailNormalized, newUser.ApiKey, newUser.Id, newUser.UserNameNormalized);

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
                dbDetail.EmailNormalized = detailToUpdate.Email.ToNormalizedString() ?? detailToUpdate.Email.ToUpperInvariant();
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
                // Take whatever is newer
                dbDetail.LastActivityAt = dbDetail.LastActivityAt > detailToUpdate.LastActivityAt ? dbDetail.LastActivityAt : detailToUpdate.LastActivityAt;
                // Take whatever is newer
                dbDetail.LastLoginAt = dbDetail.LastLoginAt > detailToUpdate.LastLoginAt ? dbDetail.LastLoginAt : detailToUpdate.LastLoginAt;
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

    public async Task<MelodeeModels.OperationResult<bool>> UpdateLastLogin(UserLoginEvent eventData, CancellationToken cancellationToken = default)
    {
        using (Operation.At(LogEventLevel.Debug).Time("[{ServiceName}]: Data [{EventData}]", nameof(UserService), eventData.ToString()))
        {
            var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken))
            {
                var user = await GetAsync(eventData.UserId, cancellationToken).ConfigureAwait(false);
                if (user.Data != null)
                {
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

    private void ClearCache(string? emailAddress, Guid? apiKey, int? id, string? username)
    {
        if (emailAddress != null)
        {
            CacheManager.Remove(CacheKeyDetailByEmailAddressKeyTemplate.FormatSmart(emailAddress.ToNormalizedString() ?? emailAddress));
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

    public async Task<UserAlbum?> UserAlbumAsync(string? userName, Guid albumApiKey, CancellationToken cancellationToken)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var sql = """
                      select ua.*
                      from "UserAlbums" ua 
                      left join "Users" u on (ua."UserId" = u."Id")
                      left join "Albums" a on (ua."AlbumId" = a."Id")
                      where u."UserNameNormalized" = @userName
                      and a."ApiKey" = @albumApiKey;
                      """;
            var dbConn = scopedContext.Database.GetDbConnection();
            return await dbConn.QuerySingleOrDefaultAsync<UserAlbum?>(sql, new { userName = userName.ToNormalizedString(), albumApiKey })
                .ConfigureAwait(false);
        }
    }

    public async Task<UserSong?> UserSongAsync(string? userName, Guid songApiKey, CancellationToken cancellationToken)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var sql = """
                      select us.*
                      from "UserSongs" us 
                      left join "Users" u on (us."UserId" = u."Id")
                      left join "Songs" s on (us."SongId" = s."Id")
                      where u."UserNameNormalized" = @userName
                      and s."ApiKey" = @songApiKey;
                      """;
            var dbConn = scopedContext.Database.GetDbConnection();
            return await dbConn.QuerySingleOrDefaultAsync<UserSong?>(sql, new { userName = userName.ToNormalizedString(), songApiKey })
                .ConfigureAwait(false);
        }
    }

    public async Task<UserSong[]?> UserSongsForAlbumAsync(string? userName, Guid albumApiKey, CancellationToken cancellationToken)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var sql = """
                      select us.*
                      from "UserSongs" us 
                      left join "Users" u on (us."UserId" = u."Id")
                      left join "Songs" s on (us."SongId" = s."Id")
                      left join "AlbumDiscs" ad on (s."AlbumDiscId" = ad."Id")
                      left join "Albums" a on (ad."AlbumId" = a."Id")
                      where u."UserNameNormalized" = @userName
                      and a."ApiKey" = @albumApiKey;
                      """;
            var dbConn = scopedContext.Database.GetDbConnection();
            return (await dbConn.QueryAsync<UserSong>(sql, new { userName = userName.ToNormalizedString(), albumApiKey })
                    .ConfigureAwait(false))
                .ToArray();
        }
    }
    
    /// <summary>Generate a salt.</summary>
    /// <param name="saltLength">Length of the salt to generate</param>
    /// <param name="logRounds">The log2 of the number of rounds of hashing to apply. The work factor therefore increases as (2 ** logRounds).</param>
    /// <returns>An encoded salt value.</returns>
    public static string GenerateSalt(int saltLength = 16, int logRounds = 10)
    {
        var randomBytes = new byte[saltLength];
        RandomNumberGenerator.Create().GetBytes(randomBytes);
    
        var rs = new StringBuilder((randomBytes.Length * 2) + 8);
    
        rs.Append("$2a$");
        if (logRounds < 10) rs.Append('0');
        rs.Append(logRounds);
        rs.Append('$');
        rs.Append(Encoding.UTF8.GetString(randomBytes).ToBase64());
    
        return rs.ToString();
    }    
}
