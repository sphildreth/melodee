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
    IBus bus)
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


    public async Task<MelodeeModels.OperationResult<bool>> DeleteAsync(int[] userIds, CancellationToken cancellationToken = default)
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

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
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
        return user.Data?.IsAdmin ?? false;
    }

    public async Task<MelodeeModels.OperationResult<User?>> GetByApiKeyAsync(Guid apiKey, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(_ => apiKey == Guid.Empty, apiKey, nameof(apiKey));

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

    public async Task<MelodeeModels.OperationResult<bool>> UpdateAsync(User currentUser, User detailToUpdate, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, detailToUpdate.Id, nameof(detailToUpdate));

        bool result;
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
                    Console.WriteLine($"[{nameof(UpdateLastLogin)}]: {eventData}");
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
        => ClearCache(user.Email, user.ApiKey, user.Id, user.UserName);

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
    
    public async Task< MelodeeModels.OperationResult<bool>> ToggleGenreHatedAsync(int userId, string genre, CancellationToken cancellationToken = default)
    {
        bool result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var user = await scopedContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken).ConfigureAwait(false);
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

    public async Task< MelodeeModels.OperationResult<bool>> ToggleAristHatedAsync(int userId, Guid artistApiKey, bool isHated, CancellationToken cancellationToken = default)
    {
        bool result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var artist = await artistService.GetByApiKeyAsync(artistApiKey, cancellationToken).ConfigureAwait(false);
            if (artist.Data != null)
            {
                var userArtist = await scopedContext.UserArtists.FirstOrDefaultAsync(x => x.UserId == userId && x.ArtistId == artist.Data.Id, cancellationToken).ConfigureAwait(false);
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
    
    public async Task< MelodeeModels.OperationResult<bool>> ToggleAristStarAsync(int userId, Guid artistApiKey, bool isStarred, CancellationToken cancellationToken = default)
    {
        bool result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var artist = await artistService.GetByApiKeyAsync(artistApiKey, cancellationToken).ConfigureAwait(false);
            if (artist.Data != null)
            {
                var userArtist = await scopedContext.UserArtists.FirstOrDefaultAsync(x => x.UserId == userId && x.ArtistId == artist.Data.Id, cancellationToken).ConfigureAwait(false);
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
    
    public async Task<MelodeeModels.OperationResult<bool>> ToggleArtistHatedAsync(int userId, Guid artistApiKey, bool isHated, CancellationToken cancellationToken = default)
    {
        bool result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var artist = await artistService.GetByApiKeyAsync(artistApiKey, cancellationToken).ConfigureAwait(false);
            if (artist.Data != null)
            {
                var userArtist = await scopedContext.UserArtists.FirstOrDefaultAsync(x => x.UserId == userId && x.ArtistId == artist.Data.Id, cancellationToken).ConfigureAwait(false);
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

    public async Task<MelodeeModels.OperationResult<bool>> ToggleAlbumHatedAsync(int userId, Guid albumApiKey, bool isHated, CancellationToken cancellationToken = default)
    {
        bool result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var album = await albumService.GetByApiKeyAsync(albumApiKey, cancellationToken).ConfigureAwait(false);
            if (album.Data != null)
            {
                var userAlbum = await scopedContext.UserAlbums.FirstOrDefaultAsync(x => x.UserId == userId && x.AlbumId == album.Data.Id, cancellationToken).ConfigureAwait(false);
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

    public async Task<MelodeeModels.OperationResult<bool>> ToggleArtistStarAsync(int userId, Guid albumApiKey, bool isStarred, CancellationToken cancellationToken = default)
    {
        bool result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var artist = await artistService.GetByApiKeyAsync(albumApiKey, cancellationToken).ConfigureAwait(false);
            if (artist.Data != null)
            {
                var userArtist = await scopedContext.UserArtists.FirstOrDefaultAsync(x => x.UserId == userId && x.ArtistId == artist.Data.Id, cancellationToken).ConfigureAwait(false);
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
    
    
    public async Task< MelodeeModels.OperationResult<bool>> ToggleAlbumStarAsync(int userId, Guid albumApiKey, bool isStarred, CancellationToken cancellationToken = default)
    {
        bool result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var album = await albumService.GetByApiKeyAsync(albumApiKey, cancellationToken).ConfigureAwait(false);
            if (album.Data != null)
            {
                var userAlbum = await scopedContext.UserAlbums.FirstOrDefaultAsync(x => x.UserId == userId && x.AlbumId == album.Data.Id, cancellationToken).ConfigureAwait(false);
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
    
    public async Task< MelodeeModels.OperationResult<bool>> ToggleSongHatedAsync(int userId, Guid songApiKey, bool isHated, CancellationToken cancellationToken = default)
    {
        bool result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var song = await songService.GetByApiKeyAsync(songApiKey, cancellationToken).ConfigureAwait(false);
            if (song.Data != null)
            {
                var userSong = await scopedContext.UserSongs.FirstOrDefaultAsync(x => x.UserId == userId && x.SongId == song.Data.Id, cancellationToken).ConfigureAwait(false);
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

    public async Task< MelodeeModels.OperationResult<bool>> ToggleSongStarAsync(int userId, Guid songApiKey, bool isStarred, CancellationToken cancellationToken = default)
    {
        bool result = false;
        var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var song = await songService.GetByApiKeyAsync(songApiKey, cancellationToken).ConfigureAwait(false);
            if (song.Data != null)
            {
                var userSong = await scopedContext.UserSongs.FirstOrDefaultAsync(x => x.UserId == userId && x.SongId == song.Data.Id, cancellationToken).ConfigureAwait(false);
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
    
    public async Task<UserArtist?> UserArtistAsync(int userId, Guid artistApiKey, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var sql = """
                      select ua.*
                      from "UserArtists" ua 
                      left join "Artists" a on (ua."ArtistId" = a."Id")
                      where ua."UserId" = @userId
                      and a."ApiKey" = @artistApiKey;
                      """;
            var dbConn = scopedContext.Database.GetDbConnection();
            return await dbConn.QuerySingleOrDefaultAsync<UserArtist?>(sql, new { userId, artistApiKey })
                .ConfigureAwait(false);
        }
    }    

    public async Task<UserAlbum?> UserAlbumAsync(int userId, Guid albumApiKey, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var sql = """
                      select ua.*
                      from "UserAlbums" ua 
                      left join "Albums" a on (ua."AlbumId" = a."Id")
                      where ua."UserId" = @userId
                      and a."ApiKey" = @albumApiKey;
                      """;
            var dbConn = scopedContext.Database.GetDbConnection();
            return await dbConn.QuerySingleOrDefaultAsync<UserAlbum?>(sql, new { userId, albumApiKey })
                .ConfigureAwait(false);
        }
    }

    public async Task<UserSong?> UserSongAsync(int userId, Guid songApiKey, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var sql = """
                      select us.*
                      from "UserSongs" us 
                      left join "Songs" s on (us."SongId" = s."Id")
                      where us."UserId" = @userId
                      and s."ApiKey" = @songApiKey;
                      """;
            var dbConn = scopedContext.Database.GetDbConnection();
            return await dbConn.QuerySingleOrDefaultAsync<UserSong?>(sql, new { userId, songApiKey })
                .ConfigureAwait(false);
        }
    }

    public async Task<UserSong[]?> UserSongsForAlbumAsync(int userId, Guid albumApiKey, CancellationToken cancellationToken = default)
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
                      where u."Id" = @userId
                      and a."ApiKey" = @albumApiKey;
                      """;
            var dbConn = scopedContext.Database.GetDbConnection();
            return (await dbConn.QueryAsync<UserSong>(sql, new { userId, albumApiKey })
                    .ConfigureAwait(false))
                .ToArray();
        }
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
}
