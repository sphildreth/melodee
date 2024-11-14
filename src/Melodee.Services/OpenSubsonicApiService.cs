using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.Enums;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Common.Utility;
using Melodee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Quartz;
using Serilog;
using License = Melodee.Common.Models.OpenSubsonic.License;
using Playlist = Melodee.Common.Models.OpenSubsonic.Playlist;

namespace Melodee.Services;

/// <summary>
///     Handles OpenSubsonic API calls.
/// </summary>
public class OpenSubsonicApiService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    SettingService settingService,
    UserService userService,
    ArtistService artistService,
    AlbumService albumService,
    IScheduler schedule)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    public const char ImageApiIdSeparator = '_';
    
    private Lazy<Task<IMelodeeConfiguration>> Configuration => new(() => settingService.GetMelodeeConfigurationAsync());

    /// <summary>
    /// Get details about the software license.
    /// </summary>
    public async Task<ResponseModel> GetLicense(ApiRequest apiApiRequest, CancellationToken cancellationToken = default)
    {
        return new ResponseModel
        {
            UserInfo = BlankUserInfo,
            IsSuccess = true,
            ResponseData = await DefaultApiResponse() with
            {
                DataPropertyName = "license",
                Data = new License(true,
                    (await Configuration.Value).GetValue<string>(SettingRegistry.OpenSubsonicServerLicenseEmail) ?? ServiceUser.Instance.Value.Email,
                    DateTimeOffset.UtcNow.AddYears(50).ToString("O"),
                    DateTimeOffset.UtcNow.AddYears(50).ToString("O")
                )
            }
        };
    }

    /// <summary>
    /// Returns all playlists a user is allowed to play.
    /// </summary>
    public async Task<ResponseModel> GetPlaylists(ApiRequest apiRequest, CancellationToken cancellationToken = default)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return new ResponseModel
            {
                UserInfo = BlankUserInfo,
                ResponseData = authResponse.ResponseData
            };
        }

        Playlist[] data = [];
        var sql = string.Empty;

        try
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                var sqlParameters = new Dictionary<string, object>
                {
                    { "UserId", authResponse.UserInfo.Id }
                };
                sql = """
                    SELECT *
                    FROM "Playlists"
                    where "UserId" = @userId
                    or "IsPublic" is true
                    ORDER BY "SortOrder";
                    """;
                data = (await dbConn
                    .QueryAsync<Playlist>(sql, sqlParameters)
                    .ConfigureAwait(false)).ToArray();
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to get Playlists SQL [{Sql}] Request [{ApiResult}]", sql, apiRequest);
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            IsSuccess = true,
            ResponseData = await DefaultApiResponse() with
            {
                Data = data,
                DataPropertyName = "playlists",
                DataDetailPropertyName = "playlist"
            }
        };        
    }
    
    /// <summary>
    /// Returns a list of random, newest, highest rated etc. albums.
    /// </summary>
    public async Task<ResponseModel> GetAlbumList2Async(GetAlbumListRequest albumListRequest, ApiRequest apiRequest, CancellationToken cancellationToken = default)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return new ResponseModel
            {
                UserInfo = BlankUserInfo,
                ResponseData = authResponse.ResponseData
            };
        }

        AlbumList2[] data = [];
        var sql = string.Empty;

        try
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                var sqlParameters = new Dictionary<string, object>();
                var selectSql = """
                                SELECT 
                                cast(a."ApiKey" as varchar(50)) as "Id",
                                a."Name" as "Album",
                                a."Name" as "Title",
                                a."Name" as "Name",
                                'album_' || cast(a."ApiKey" as varchar(50)) as "CoverArt",
                                a."SongCount",
                                a."CreatedAt" as "CreatedRaw",
                                a."Duration"/1000 as "Duration",
                                a."PlayedCount",
                                cast(aa."ApiKey" as varchar(50)) as "ArtistId",
                                aa."Name" as "Artist",
                                DATE_PART('year', a."ReleaseDate"::date) as "Year",
                                unnest(a."Genres") as "Genre"
                                FROM "Albums" a 
                                LEFT JOIN "Artists" aa on (a."ArtistId" = aa."Id")
                                """;
                var whereSQL = string.Empty;
                var limitSQL = $"OFFSET {albumListRequest.OffsetValue} ROWS FETCH NEXT {albumListRequest.SizeValue} ROWS ONLY;";
                switch (albumListRequest.Type)
                {
                    case ListType.Random:
                        whereSQL = "ORDER BY RANDOM()";
                        break;

                    case ListType.Newest:
                        break;
                    case ListType.Highest:
                        break;
                    case ListType.Frequent:
                        break;
                    case ListType.Recent:
                        break;
                    case ListType.AlphabeticalByName:
                        break;
                    case ListType.AlphabeticalByArtist:
                        break;
                    case ListType.Starred:
                        break;
                    case ListType.ByYear:
                        break;
                    case ListType.ByGenre:
                        break;
                }

                sql = $"{selectSql} {whereSQL} {limitSQL}";
                data = (await dbConn
                    .QueryAsync<AlbumList2>(sql, sqlParameters)
                    .ConfigureAwait(false)).ToArray();
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to get AlbumList2 SQL [{Sql}] Request [{ApiResult}]", sql, apiRequest);
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            IsSuccess = true,
            ResponseData = await DefaultApiResponse() with
            {
                Data = data,
                DataPropertyName = "albumList2",
                DataDetailPropertyName = "album"
            }
        };
    }

    /// <summary>
    /// Returns all genres.
    /// </summary>
    public async Task<ResponseModel> GetGenresAsync(ApiRequest apiRequest, CancellationToken cancellationToken = default)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return new ResponseModel
            {
                UserInfo = BlankUserInfo,
                ResponseData = authResponse.ResponseData
            };
        }

        var data = new List<Genre>();

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var allGenres = await dbConn.QueryAsync<string>("""
                                                            select genre
                                                            from
                                                            (
                                                                select genre
                                                                from "Albums", unnest("Genres") as genre 
                                                                union ALL
                                                                select genre
                                                                from "Songs", unnest("Genres") as genre 
                                                            ) t
                                                            group by genre
                                                            """, cancellationToken).ConfigureAwait(false);
            var songGenres = await dbConn.QueryAsync<Genre>("select genre as Value, count(1) as SongCount from \"Songs\", unnest(\"Genres\") as genre group by genre order by genre;", cancellationToken).ConfigureAwait(false);
            var albumGenres = await dbConn.QueryAsync<Genre>("select genre as Value, count(1) as AlbumCount from \"Albums\", unnest(\"Genres\") as genre group by genre order by genre;", cancellationToken).ConfigureAwait(false);

            foreach (var genre in allGenres)
            {
                var songGenre = songGenres.FirstOrDefault(x => x.Value == genre);
                var albumGenre = albumGenres.FirstOrDefault(x => x.Value == genre);
                data.Add(new Genre
                    {
                        Value = genre,
                        SongCount = songGenre?.SongCount ?? 0,
                        AlbumCount = albumGenre?.AlbumCount ?? 0
                    }
                );
            }
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,            
            ResponseData = authResponse.ResponseData with
            {
                Data = data,
                DataPropertyName = "genres",
                DataDetailPropertyName = "genre"
            }
        };
    }

    /// <summary>
    /// Returns a cover art image.
    /// </summary>
    public async Task<ResponseModel> GetCoverArt(string apiId, int? size, ApiRequest apiRequest, CancellationToken cancellationToken = default)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return new ResponseModel
            {
                UserInfo = BlankUserInfo,
                ResponseData = authResponse.ResponseData
            };
        }

        var apiIdParts = apiId.Split(ImageApiIdSeparator);
        if (apiIdParts.Length != 2)
        {
            return new ResponseModel
            {
                UserInfo = BlankUserInfo,
                ResponseData = authResponse.ResponseData with
                {
                    Error = Error.InvalidApiKeyError
                }
            };
        }
        var albumResponse = await albumService.GetByApiKeyAsync(SafeParser.ToGuid(apiIdParts[1])!.Value, cancellationToken);
        if (!albumResponse.IsSuccess)
        {
            return new ResponseModel
            {
                UserInfo = BlankUserInfo,
                ResponseData = authResponse.ResponseData with
                {
                    Error = Error.InvalidApiKeyError
                }
            };
        }
        byte[]? coverBytes = null;

        var sql = """
                  select l."Path" || a."Directory"
                  from "Albums" a 
                  left join "Libraries" l on (a."Id" = a."LibraryId")
                  where a."ApiKey" = @apiKey
                  limit 1;
                  """;
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var pathToAlbum = dbConn.QuerySingle<string>(sql, new { productID = 1 });
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,            
            ResponseData = authResponse.ResponseData with
            {
                Data = coverBytes,
                DataPropertyName = string.Empty,
                DataDetailPropertyName = string.Empty
            }
        };
        
    }

    /// <summary>
    /// List the OpenSubsonic extensions supported by this server.
    /// <remarks>Unlike all other APIs getOpenSubsonicExtensions must be publicly accessible.</remarks>
    /// </summary>
    public async Task<ResponseModel> GetOpenSubsonicExtensions(ApiRequest apiApiRequest, CancellationToken cancellationToken = default)
    {
        var authResponse = new ResponseModel
        {
            UserInfo = BlankUserInfo,
            IsSuccess = true,
            ResponseData = await NewApiResponse(true, string.Empty, string.Empty)
        };
        var data = new List<OpenSubsonicExtension>
        {
            new OpenSubsonicExtension("template", [1, 2]),
            new OpenSubsonicExtension("transcodeOffset", [1])
        };
        return new ResponseModel
        {
            UserInfo = BlankUserInfo,
            ResponseData = authResponse.ResponseData with
            {
                Data = data,
                DataPropertyName = "openSubsonicExtensions"
            }
        };
    }

    public async Task<ResponseModel> StartScanAsync(ApiRequest apiRequest, CancellationToken cancellationToken = default)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return new ResponseModel
            {
                UserInfo = BlankUserInfo,
                ResponseData = authResponse.ResponseData
            };
        }

        await schedule.TriggerJob(JobKeyRegistry.LibraryProcessJobJobKey, cancellationToken);
        
        return new ResponseModel
        {
            UserInfo = BlankUserInfo,
            ResponseData = authResponse.ResponseData with
            {
                Data = new ScanStatus(true, 0),
                DataPropertyName = "scanStatus"
            }
        };        
    }
    
    public async Task<object> GetScanStatusAsync(ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return new ResponseModel
            {
                UserInfo = BlankUserInfo,
                ResponseData = authResponse.ResponseData
            };
        }
        
        var executingJobs = await schedule.GetCurrentlyExecutingJobs(cancellationToken);
        var libraryProcessJob = executingJobs.FirstOrDefault(x => Equals(x.JobDetail.Key, JobKeyRegistry.LibraryProcessJobJobKey));

        var data = new ScanStatus(false, 0);
        if (libraryProcessJob != null)
        {
            var dataMap = libraryProcessJob.JobDetail.JobDataMap;
            data = new ScanStatus(dataMap.GetString(JobMapNameRegistry.ScanStatus) == Common.Enums.ScanStatus.InProcess.ToString(), dataMap.GetIntValue(JobMapNameRegistry.Count));
        }
        return new ResponseModel
        {
            UserInfo = BlankUserInfo,
            ResponseData = authResponse.ResponseData with
            {
                Data = data,
                DataPropertyName = "scanStatus"
            }
        };         
    }    

    public async Task<ResponseModel> AuthenticateSubsonicApiAsync(ApiRequest apiApiRequest, CancellationToken cancellationToken = default)
    {
        if (apiApiRequest.Username?.Nullify() == null ||
            (apiApiRequest.Password?.Nullify() == null &&
             apiApiRequest.Token?.Nullify() == null))
        {
            return new ResponseModel
            {
                UserInfo = new UserInfo(0, Guid.Empty, string.Empty, string.Empty),
                IsSuccess = false,
                ResponseData = await NewApiResponse(false, string.Empty, string.Empty, Error.AuthError)
            };
        }

        var result = false;
        var user = await userService.GetByUsernameAsync(apiApiRequest.Username, cancellationToken).ConfigureAwait(false);
        try
        {
            if (!user.IsSuccess || (user.Data?.IsLocked ?? false))
            {
                Logger.Warning("Locked user [{Username}] attempted to authenticate with [{Client}]", apiApiRequest.Username, apiApiRequest.ApiRequestPlayer);
            }
            else
            {
                bool isAuthenticated;
                var authUsingToken = apiApiRequest.Token?.Nullify() != null;
                var configuration = await settingService.GetMelodeeConfigurationAsync(cancellationToken);
                var usersPassword = user.Data.Decrypt(user.Data.PasswordEncrypted, configuration);
                var apiRequestPassword = apiApiRequest.Password;
                if (apiApiRequest.Password?.StartsWith("enc:", StringComparison.Ordinal) ?? false)
                {
                    apiRequestPassword = apiRequestPassword?[4..].FromHexString();
                }

                if (authUsingToken)
                {
                    var userMd5 = HashHelper.CreateMd5($"{usersPassword}{apiApiRequest.Salt}");
                    isAuthenticated = string.Equals(userMd5, apiApiRequest.Token, StringComparison.InvariantCultureIgnoreCase);
                }
                else
                {
                    isAuthenticated = usersPassword == apiRequestPassword;
                }

                if (isAuthenticated)
                {
                    var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
                    await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken))
                    {
                        await scopedContext.Users
                            .Where(x => x.Id == user.Data.Id)
                            .ExecuteUpdateAsync(setters =>
                                setters.SetProperty(x => x.LastActivityAt, now)
                                    .SetProperty(x => x.LastLoginAt, now), cancellationToken).ConfigureAwait(false);
                    }

                    result = true;
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error authenticating user, request [{ApiRequest}]", apiApiRequest);
        }

        return new ResponseModel
        {
            UserInfo = user.Data?.ToUserInfo() ?? BlankUserInfo,
            IsSuccess = result,
            ResponseData = await NewApiResponse(result, string.Empty, string.Empty, result ? null : Error.AuthError)
        };
    }

    private UserInfo BlankUserInfo => new UserInfo(0, Guid.Empty, string.Empty, string.Empty);
    
    private Task<ApiResponse> DefaultApiResponse()
    {
        return NewApiResponse(true, string.Empty, string.Empty);
    }

    private async Task<ApiResponse> NewApiResponse(bool isOk, string dataPropertyName, string dataDetailPropertyName, Error? error = null, object? data = null)
    {
        return new ApiResponse
        {
            IsSuccess = isOk,
            Status = isOk ? "ok" : "failed",
            Version = (await Configuration.Value).GetValue<string>(SettingRegistry.OpenSubsonicServerSupportedVersion) ?? throw new InvalidOperationException(),
            Type = (await Configuration.Value).GetValue<string>(SettingRegistry.OpenSubsonicServerType) ?? throw new InvalidOperationException(),
            ServerVersion = (await Configuration.Value).GetValue<string>(SettingRegistry.OpenSubsonicServerVersion) ?? throw new InvalidOperationException(),
            Error = error,
            Data = data,
            DataDetailPropertyName = dataDetailPropertyName,
            DataPropertyName = dataPropertyName
        };
    }


}
