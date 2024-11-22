using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.DTO;
using Melodee.Common.Models.OpenSubsonic.Enums;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Common.Utility;
using Melodee.Services.Extensions;
using Melodee.Services.Interfaces;
using Melodee.Services.Scanning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.Win32.SafeHandles;
using NodaTime;
using Quartz;
using Serilog;
using SmartFormat;
using License = Melodee.Common.Models.OpenSubsonic.License;
using Playlist = Melodee.Common.Models.OpenSubsonic.Playlist;
using PlayQueue = Melodee.Common.Models.OpenSubsonic.PlayQueue;
using ScanStatus = Melodee.Common.Models.OpenSubsonic.ScanStatus;

namespace Melodee.Services;

/// <summary>
///     Handles OpenSubsonic API calls.
/// </summary>
public class OpenSubsonicApiService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    DefaultImages defaultImages,
    ISettingService settingService,
    UserService userService,
    ArtistService artistService,
    AlbumService albumService,
    SongService songService,
    AlbumDiscoveryService albumDiscoveryService,
    IScheduler schedule)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    public const char ImageApiIdSeparator = '_';

    private Lazy<Task<IMelodeeConfiguration>> Configuration => new(() => settingService.GetMelodeeConfigurationAsync());

    private UserInfo BlankUserInfo => new(0, Guid.Empty, string.Empty, string.Empty);

    private Guid? ApiKeyFromId(string id)
    {
        var apiIdParts = id.Nullify() == null ? [] : id.Split(ImageApiIdSeparator);
        return SafeParser.ToGuid(SafeParser.ToGuid(apiIdParts[1])!.Value);
    }

    /// <summary>
    ///     Get details about the software license.
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
    ///     Returns all playlists a user is allowed to play.
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
    ///     Returns a list of random, newest, highest rated etc. albums.
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
    
    public async Task<ResponseModel> GetSongAsync(Guid apiKey, ApiRequest apiRequest, CancellationToken cancellationToken)
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
        
        var songResponse = await songService.GetByApiKeyAsync(apiKey, cancellationToken);
        if (!songResponse.IsSuccess)
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
        var userSong = await userService.UserSongAsync(apiRequest.Username, apiKey, cancellationToken);
        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            ResponseData = authResponse.ResponseData with
            {
                Data = songResponse.Data.ToChild(songResponse.Data.AlbumDisc.Album, userSong ),
                DataPropertyName = "song"
            }
        };
    }

    public async Task<ResponseModel> GetAlbumAsync(Guid apiKey, ApiRequest apiRequest, CancellationToken cancellationToken = default)
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

        var albumResponse = await albumService.GetByApiKeyAsync(apiKey, cancellationToken);
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

        var album = albumResponse.Data!;
        var userAlbum = await userService.UserAlbumAsync(apiRequest.Username, apiKey, cancellationToken);
        var userSongsForAlbum = await userService.UserSongsForAlbumAsync(apiRequest.Username, apiKey, cancellationToken) ?? [];
        var data = new AlbumId3WithSongs
        {
            AlbumDate = album.ReleaseDate.ToItemDate(),
            AlbumTypes = [], // TODO
            Artist = album.Artist.Name,
            ArtistId = album.Artist.ApiKey.ToString(),
            Artists = [], // TODO
            CoverArt = $"album_{album.ApiKey}",
            Created = album.CreatedAt.ToString(),
            DiscTitles = album.Discs.Select(x => new DiscTitle(x.DiscNumber, x.Title ?? string.Empty)).ToArray(),
            DisplayArtist = album.Artist.Name,
            Duration = SafeParser.ToNumber<int>(album.Duration / 1000),
            Genre = album.Genres?.ToCsv(),
            Genres = [], // TODO
            Id = album.ApiKey.ToString(),
            IsCompilation = album.IsCompilation,
            Moods = [], // TODO
            MusicBrainzId = null,
            Name = album.Name,
            OriginalAlbumDate = album.OriginalReleaseDate?.ToItemDate() ?? album.ReleaseDate.ToItemDate(),
            Parent = album.ApiKey.ToString(),
            PlayCount = album.PlayedCount,
            Played = album.LastPlayedAt.ToString(),
            RecordLabels = [], // TODO
            Song = album.Discs.SelectMany(x => x.Songs)
                .Select(x => x.ToChild(album, userSongsForAlbum.FirstOrDefault(us => us.SongId == x.Id)))
                .ToArray(),
            SongCount = album.SongCount ?? 0,
            SortName = album.SortName,
            Starred = userAlbum?.LastUpdatedAt?.ToString(),
            Title = album.Name,
            UserRating = userAlbum?.Rating,
            Year = album.ReleaseDate.Year
        };

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            ResponseData = authResponse.ResponseData with
            {
                Data = data,
                DataPropertyName = "album"
            }
        };
    }

    /// <summary>
    ///     Returns all genres.
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
    ///     Returns a cover art image.
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

        byte[] coverBytes = defaultImages.AlbumCoverBytes;

        // TODO cache images?
        
        try
        {
            var apiKey = ApiKeyFromId(apiId);
            if (apiKey == null)
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

            var albumResponse = await albumService.GetByApiKeyAsync(apiKey.Value, cancellationToken);
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

            var sql = """
                      select l."Path" || a."Directory"
                      from "Albums" a 
                      left join "Libraries" l on (l."Id" = a."LibraryId")
                      where a."ApiKey" = '{0}'
                      limit 1;
                      """;
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                var pathToAlbum = dbConn.ExecuteScalar<string>(sql.FormatSmart(apiKey.ToString())) ?? string.Empty;
                await albumDiscoveryService.InitializeAsync(await Configuration.Value, cancellationToken);
                var melodeeFile = (await albumDiscoveryService
                        .AllMelodeeAlbumDataFilesForDirectoryAsync(new FileSystemDirectoryInfo
                        {
                            Path = pathToAlbum,
                            Name = pathToAlbum
                        }, cancellationToken)
                        .ConfigureAwait(false))
                    .Data?
                    .FirstOrDefault();
                var image = melodeeFile?.CoverImage();
                if (image != null)
                {
                    coverBytes = await File.ReadAllBytesAsync(image.FullName, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to get cover image for album [{AlbumId}]", apiId);
        }

        return new ResponseModel
        {
            IsSuccess = true,
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
    ///     List the OpenSubsonic extensions supported by this server.
    ///     <remarks>Unlike all other APIs getOpenSubsonicExtensions must be publicly accessible.</remarks>
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
            // This is a template extension that allows servers to do marvelous stuff and clients to use that stuff.
            new("template", [1, 2]),
            // add support for synchronized lyrics, multiple languages, and retrieval by song ID
            new("songLyrics", [1]),
            // Add support for start offset for transcoding.
            new("transcodeOffset", [1])
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
        try
        {
            if (libraryProcessJob != null)
            {
                var dataMap = libraryProcessJob.JobDetail.JobDataMap;
                if (dataMap.ContainsKey(JobMapNameRegistry.ScanStatus) && dataMap.ContainsKey(JobMapNameRegistry.Count))
                {
                    data = new ScanStatus(dataMap.GetString(JobMapNameRegistry.ScanStatus) == Common.Enums.ScanStatus.InProcess.ToString(), dataMap.GetIntValue(JobMapNameRegistry.Count));
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Attempting to get Scan Status");
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

    public async Task<ResponseModel> AuthenticateSubsonicApiAsync(ApiRequest apiRequest, CancellationToken cancellationToken = default)
    {
        if (apiRequest.Username?.Nullify() == null ||
            (apiRequest.Password?.Nullify() == null &&
             apiRequest.Token?.Nullify() == null))
        {
            return new ResponseModel
            {
                UserInfo = new UserInfo(0, Guid.Empty, string.Empty, string.Empty),
                IsSuccess = false,
                ResponseData = await NewApiResponse(false, string.Empty, string.Empty, Error.AuthError)
            };
        }

        var result = false;
        var user = await userService.GetByUsernameAsync(apiRequest.Username, cancellationToken).ConfigureAwait(false);
        try
        {
            if (!user.IsSuccess || (user.Data?.IsLocked ?? false))
            {
                Logger.Warning("Locked user [{Username}] attempted to authenticate with [{Client}]", apiRequest.Username, apiRequest.ApiRequestPlayer);
            }
            else
            {
                bool isAuthenticated;
                var authUsingToken = apiRequest.Token?.Nullify() != null;
                var configuration = await settingService.GetMelodeeConfigurationAsync(cancellationToken);
                var usersPassword = user.Data.Decrypt(user.Data.PasswordEncrypted, configuration);
                var apiRequestPassword = apiRequest.Password;
                if (apiRequest.Password?.StartsWith("enc:", StringComparison.Ordinal) ?? false)
                {
                    apiRequestPassword = apiRequestPassword?[4..].FromHexString();
                }

                if (authUsingToken)
                {
                    var userMd5 = HashHelper.CreateMd5($"{usersPassword}{apiRequest.Salt}");
                    isAuthenticated = string.Equals(userMd5, apiRequest.Token, StringComparison.InvariantCultureIgnoreCase);
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
            Logger.Error(e, "Error authenticating user, request [{ApiRequest}]", apiRequest);
        }

        return new ResponseModel
        {
            UserInfo = user.Data?.ToUserInfo() ?? BlankUserInfo,
            IsSuccess = result,
            ResponseData = await NewApiResponse(result, string.Empty, string.Empty, result ? null : Error.AuthError)
        };
    }

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

    public async Task<ResponseModel> GetPlayQueueAsync(ApiRequest apiRequest, CancellationToken cancellationToken = default)
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

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var user = await userService.GetByUsernameAsync(apiRequest.Username, cancellationToken).ConfigureAwait(false);
            var usersPlayQues = await scopedContext
                .PlayQues.Include(x => x.Song).ThenInclude(x => x.AlbumDisc).ThenInclude(x => x.Album)
                .Where(x => x.UserId == user.Data!.Id)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            var current = usersPlayQues.FirstOrDefault(x => x.IsCurrentSong);
            var data = new PlayQueue
            {
                Current = current?.SongApiKey.ToString() ?? string.Empty,
                Position = current?.Position ?? 0,
                ChangedBy = current?.ChangedBy ?? user.Data!.UserName,
                Changed = current?.LastUpdatedAt.ToString() ?? string.Empty,
                Username = user.Data!.UserName,
                Entry = usersPlayQues.Select(x => x.Song.ToChild(x.Song.AlbumDisc.Album, null)).ToArray()
            };

            return new ResponseModel
            {
                UserInfo = BlankUserInfo,
                ResponseData = authResponse.ResponseData with
                {
                    Data = data,
                    DataPropertyName = "playQueue"
                }
            };
        }
    }

    public async Task<ResponseModel> SavePlayQueue(Guid[]? apiKeys, Guid? current, double? position, ApiRequest apiRequest, CancellationToken cancellationToken)
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

        var result = false;
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            // If the apikey is blank then remove any current saved que
            if (apiKeys == null)
            {
                var sql = """
                          delete from "PlayQues" pq
                          using "Users" u, "Songs" s
                          where pq."UserId" = u."Id"
                          and pq."SongId" = s."Id"
                          and u."UserNameNormalized" = @userNameNormalized
                          and s."ApiKey" = @apiKey
                          """;
                var dbConn = scopedContext.Database.GetDbConnection();
                await dbConn.ExecuteAsync(sql, new { apiKey = apiKeys, userNameNormalized = apiRequest.Username.ToNormalizedString() }).ConfigureAwait(false);
                result = true;
            }
            else
            {
                var foundQuesSongApiKeys = new List<Guid>();
                var user = await userService.GetByUsernameAsync(apiRequest.Username, cancellationToken).ConfigureAwait(false);
                var usersPlayQues = await scopedContext
                    .PlayQues.Include(x => x.Song)
                    .Where(x => x.UserId == user.Data!.Id)
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);
                var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
                var changedByValue = apiRequest.ApiRequestPlayer?.Client ?? user.Data!.UserName;
                if (usersPlayQues.Length > 0)
                {
                    foreach (var userPlay in usersPlayQues)
                    {
                        if (!apiKeys.Contains(userPlay.Song!.ApiKey))
                        {
                            scopedContext.PlayQues.Remove(userPlay);
                            continue;
                        }

                        if (userPlay.Song.ApiKey == current)
                        {
                            userPlay.Position = position ?? 0;
                        }

                        userPlay.IsCurrentSong = userPlay.Song.ApiKey == current;
                        userPlay.LastUpdatedAt = now;
                        userPlay.ChangedBy = changedByValue;
                        foundQuesSongApiKeys.Add(userPlay.Song.ApiKey);
                    }
                }

                var addedPlayQues = new List<Common.Data.Models.PlayQueue>();
                foreach (var apiKeyToAdd in apiKeys.Except(foundQuesSongApiKeys))
                {
                    var song = await scopedContext.Songs.FirstOrDefaultAsync(x => x.ApiKey == apiKeyToAdd, cancellationToken).ConfigureAwait(false);
                    if (song != null)
                    {
                        addedPlayQues.Add(new Common.Data.Models.PlayQueue
                        {
                            CreatedAt = now,
                            IsCurrentSong = song.ApiKey == current,
                            UserId = user.Data!.Id,
                            SongId = song.Id,
                            SongApiKey = song.ApiKey,
                            ChangedBy = changedByValue,
                            Position = apiKeyToAdd == current && position.HasValue ? position.Value : 0
                        });
                    }
                }

                if (addedPlayQues.Count > 0)
                {
                    await scopedContext.PlayQues.AddRangeAsync(addedPlayQues, cancellationToken).ConfigureAwait(false);
                }

                await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                result = true;
            }
        }

        return new ResponseModel
        {
            UserInfo = BlankUserInfo,
            IsSuccess = result,
            ResponseData = await NewApiResponse(result, string.Empty, string.Empty, result ? null : Error.AuthError)
        };
    }

    public async Task<ResponseModel> CreateUserAsync(CreateUserRequest request, ApiRequest apiRequest, CancellationToken cancellationToken)
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

        var result = false;

        // TODO

        return new ResponseModel
        {
            UserInfo = BlankUserInfo,
            IsSuccess = result,
            ResponseData = await NewApiResponse(result, string.Empty, string.Empty, result ? null : Error.AuthError)
        };
    }

    public async Task<ResponseModel> ScrobbleAsync(string id, double? time, bool? submission, ApiRequest apiRequest, CancellationToken cancellationToken)
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

        var result = false;

        // TODO scrobble

        return new ResponseModel
        {
            UserInfo = BlankUserInfo,
            IsSuccess = result,
            ResponseData = await NewApiResponse(result, string.Empty, string.Empty, result ? null : Error.AuthError)
        };
    }

    /// <summary>
    /// Get bytes for song with support for chunking from request header values.
    /// </summary>
    public async Task<StreamResponse> StreamAsync(StreamRequest request, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        long rangeBegin = 0;
        long rangeEnd = 0;
        if (apiRequest.RequestHeaders?.TryGetValue("Range", out var range) is true)
        {
            if (string.Equals(range, "bytes=0-", StringComparison.OrdinalIgnoreCase))
            {
               long.TryParse(range, out rangeBegin);
            }
            else
            {
                var rangeParts = range.ToString().Split('-');
                long.TryParse(rangeParts[0], out rangeBegin);
                if (rangeParts.Length > 1)
                {
                    long.TryParse(rangeParts[1], out rangeEnd);
                }
            }
        }
        
        var sql = """
                  select l."Path" || a."Directory" || '/' || s."FileName" as Path, s."FileSize", s."Duration"/1000 as "Duration", s."ContentType"
                  from "Songs" s 
                  left join "AlbumDiscs" ad on (ad."Id" = s."AlbumDiscId")
                  left join "Albums" a on (a."Id" = ad."AlbumId")
                  left join "Libraries" l on (l."Id" = a."LibraryId")
                  where s."ApiKey" = @apiKey;
                  """;
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var songStreamInfo = dbConn.QuerySingleOrDefault<SongStreamInfo>(sql, new { apiKey = request.Id});
            if (!(songStreamInfo?.TrackFileInfo.Exists ?? false))
            {
                return new StreamResponse
                (
                    new Dictionary<string, StringValues>([]),
                    false,
                    []
                );
            }
            rangeEnd = rangeEnd == 0 ? songStreamInfo.FileSize : rangeEnd;

            var bytesToRead = (int)(rangeEnd - rangeBegin) + 1;
            var trackBytes = new byte[bytesToRead];
            
            using (var fs = songStreamInfo.TrackFileInfo.OpenRead())
            {
                try
                {
                    fs.Seek(rangeBegin, SeekOrigin.Begin);
                    var r = await fs.ReadAsync(trackBytes.AsMemory(0, bytesToRead), cancellationToken);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Reading song [{SongInfo}]", songStreamInfo);
                }
            }
            
            return new StreamResponse
            (
                new Dictionary<string, StringValues>
                {
                    { "Accept-Ranges", "bytes" },
                    { "Cache-Control", "no-store, must-revalidate, no-cache, max-age=0" },
                    { "Content-Duration", songStreamInfo.Duration.ToString(CultureInfo.InvariantCulture) },
                    { "Content-Length", trackBytes.Length.ToString() },
                    { "Content-Range", $"bytes {rangeBegin}-{rangeEnd}/{trackBytes.Length}" },
                    { "Content-Type", songStreamInfo.ContentType },
                    { "Expires", "Mon, 01 Jan 1990 00:00:00 GMT" }
                },
                trackBytes.Length > 0,
                trackBytes
            );
        }

    }

}
