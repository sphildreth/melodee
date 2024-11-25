using System.Globalization;
using System.Net;
using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models.DTOs;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.DTO;
using Melodee.Common.Models.OpenSubsonic.Enums;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Common.Models.OpenSubsonic.Searching;
using Melodee.Common.Utility;
using Melodee.Services.Extensions;
using Melodee.Services.Interfaces;
using Melodee.Services.Scanning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using NodaTime;
using Quartz;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using SmartFormat;
using Artist = Melodee.Common.Models.OpenSubsonic.Artist;
using Directory = Melodee.Common.Models.OpenSubsonic.Directory;
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
    IScheduler schedule,
    ScrobbleService scrobbleService,
    ILibraryService libraryService)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    public static bool IsApiIdForArtist(string? id) => id.Nullify() != null && (id?.StartsWith($"artist{Common.Data.Contants.OpenSubsonicServer.ApiIdSeparator}") ?? false);
    
    public static bool IsApiIdForAlbum(string? id) => id.Nullify() != null && (id?.StartsWith($"album{Common.Data.Contants.OpenSubsonicServer.ApiIdSeparator}") ?? false);
    
    public static bool IsApiIdForSong(string? id) => id.Nullify() != null && (id?.StartsWith($"song{Common.Data.Contants.OpenSubsonicServer.ApiIdSeparator}") ?? false);

    private Lazy<Task<IMelodeeConfiguration>> Configuration => new(() => settingService.GetMelodeeConfigurationAsync());

    public static UserInfo BlankUserInfo => new(0, Guid.Empty, string.Empty, string.Empty);

    private static Guid? ApiKeyFromId(string? id)
    {
        if (id == null)
        {
            return null;
        }
        var apiIdParts = id.Nullify() == null ? [] : id.Split(Common.Data.Contants.OpenSubsonicServer.ApiIdSeparator);
        return SafeParser.ToGuid(SafeParser.ToGuid(apiIdParts[1])!.Value);
    }

    /// <summary>
    ///     Get details about the software license.
    /// </summary>
    public async Task<ResponseModel> GetLicenseAsync(ApiRequest apiApiRequest, CancellationToken cancellationToken = default)
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
    public async Task<ResponseModel> GetPlaylistsAsync(ApiRequest apiRequest, CancellationToken cancellationToken = default)
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
                                'album_' || cast(a."ApiKey" as varchar(50)) as "Id",
                                a."Name" as "Album",
                                a."Name" as "Title",
                                a."Name" as "Name",
                                'album_' || cast(a."ApiKey" as varchar(50)) as "CoverArt",
                                a."SongCount",
                                a."CreatedAt" as "CreatedRaw",
                                a."Duration"/1000 as "Duration",
                                a."PlayedCount",
                                'artist_' || cast(aa."ApiKey" as varchar(50)) as "ArtistId",
                                aa."Name" as "Artist",
                                DATE_PART('year', a."ReleaseDate"::date) as "Year",
                                unnest(a."Genres") as "Genre",
                                (SELECT COUNT(*) FROM "UserAlbums" WHERE "IsStarred" AND "AlbumId" = a."Id") as "UserStarredCount"
                                FROM "Albums" a 
                                LEFT JOIN "Artists" aa on (a."ArtistId" = aa."Id")
                                """;
                var whereSql = string.Empty;
                var limitSql = $"OFFSET {albumListRequest.OffsetValue} ROWS FETCH NEXT {albumListRequest.SizeValue} ROWS ONLY;";
                switch (albumListRequest.Type)
                {
                    case ListType.Random:
                        whereSql = "ORDER BY RANDOM()";
                        break;

                    case ListType.Newest:
                        whereSql = "ORDER BY a.\"CreatedAt\" DESC";
                        break;

                    case ListType.Highest:
                        whereSql = "ORDER BY a.\"CalculatedRating\" DESC";
                        break;

                    case ListType.Frequent:
                        whereSql = "ORDER BY a.\"PlayedCount\" DESC";
                        break;

                    case ListType.Recent:
                        whereSql = "ORDER BY a.\"LastPlayedAt\" DESC";
                        break;

                    case ListType.AlphabeticalByName:
                        whereSql = "ORDER BY a.\"SortName\"";
                        break;

                    case ListType.AlphabeticalByArtist:
                        whereSql = "ORDER BY aa.\"SortName\"";
                        break;

                    case ListType.Starred:
                        whereSql = "ORDER BY aa.\"UserStarredCount\" DESC";
                        break;

                    case ListType.ByYear:
                        //TODO fromYear and ToYear optional filtering
                        whereSql = "ORDER BY \"Year\" DESC";
                        break;

                    case ListType.ByGenre:
                        //TODO filter by given Genre
                        whereSql = "ORDER BY \"Genre\" DESC";
                        break;
                }

                sql = $"{selectSql} {whereSql} {limitSql}";
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

    public async Task<ResponseModel> GetSongAsync(string apiKey, ApiRequest apiRequest, CancellationToken cancellationToken)
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

        var songId = ApiKeyFromId(apiKey) ?? Guid.Empty;
        var songResponse = await songService.GetByApiKeyAsync(songId, cancellationToken);
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

        var userSong = await userService.UserSongAsync(apiRequest.Username, songId, cancellationToken);
        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            ResponseData = authResponse.ResponseData with
            {
                Data = songResponse.Data.ToChild(songResponse.Data.AlbumDisc.Album, userSong),
                DataPropertyName = "song"
            }
        };
    }

    public async Task<ResponseModel> GetAlbumAsync(string apiId, ApiRequest apiRequest, CancellationToken cancellationToken = default)
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

        var apiKey = ApiKeyFromId(apiId).Value;
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
            AlbumTypes = [], //TODO
            Artist = album.Artist.Name,
            ArtistId = album.Artist.ToApiKey(),
            Artists = [], //TODO
            CoverArt = $"album_{album.ApiKey}",
            Created = album.CreatedAt.ToString(),
            DiscTitles = album.Discs.Select(x => new DiscTitle(x.DiscNumber, x.Title ?? string.Empty)).ToArray(),
            DisplayArtist = album.Artist.Name,
            Duration = album.Duration.ToSeconds(),
            Genre = album.Genres?.ToCsv(),
            Genres = [], //TODO
            Id = album.ToApiKey(),
            IsCompilation = album.IsCompilation,
            Moods = [], //TODO
            MusicBrainzId = null,
            Name = album.Name,
            OriginalAlbumDate = album.OriginalReleaseDate?.ToItemDate() ?? album.ReleaseDate.ToItemDate(),
            Parent = album.ToApiKey(),
            PlayCount = album.PlayedCount,
            Played = album.LastPlayedAt.ToString(),
            RecordLabels = [], //TODO
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
    ///     Returns the avatar (personal image) for a user.
    /// </summary>
    public async Task<ResponseModel> GetAvatarAsync(string username, object o, ApiRequest apiRequest, CancellationToken cancellationToken)
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

        var avatarBytes = defaultImages.UserAvatarBytes;

        //TODO cache images?

        try
        {
            var userLibraryResult = await libraryService.GetUserImagesLibraryAsync(cancellationToken).ConfigureAwait(false);
            if (userLibraryResult.IsSuccess)
            {
                var userAvatarFilename = authResponse.UserInfo.ToAvatarFilename(userLibraryResult.Data.Path);
                if (File.Exists(userAvatarFilename))
                {
                    avatarBytes = await File.ReadAllBytesAsync(userAvatarFilename, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to get avatar for user [{Username}]", username);
        }

        return new ResponseModel
        {
            IsSuccess = true,
            UserInfo = authResponse.UserInfo,
            ResponseData = authResponse.ResponseData with
            {
                Data = avatarBytes,
                DataPropertyName = string.Empty,
                DataDetailPropertyName = string.Empty
            }
        };
    }

    /// <summary>
    ///     Returns a cover art image.
    /// </summary>
    public async Task<ResponseModel> GetCoverArtAsync(string apiId, int? size, ApiRequest apiRequest, CancellationToken cancellationToken = default)
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

        var coverBytes = defaultImages.AlbumCoverBytes;

        //TODO cache images?

        try
        {
            var apiKey = ApiKeyFromId(apiId);
            if (IsApiIdForArtist(apiId))
            {
                var artistInfo = await DatabaseArtistInfoForArtistApiKey(apiKey ?? Guid.Empty, authResponse.UserInfo.Id, cancellationToken).ConfigureAwait(false);
                if (artistInfo.Directory != null)
                {
                    var artistDirectoryInfo =new FileSystemDirectoryInfo
                    {
                        Path = artistInfo.Directory, 
                        Name = artistInfo.Directory
                    };
                    var firstArtistImage = artistDirectoryInfo.AllFileImageTypeFileInfos().OrderBy(x => x.Name).FirstOrDefault();
                    if (firstArtistImage != null)
                    {
                        coverBytes = await File.ReadAllBytesAsync(firstArtistImage.FullName, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            else if (IsApiIdForSong(apiId) || IsApiIdForAlbum(apiId))
            {
                if (IsApiIdForSong(apiId))
                {
                    // If it's a song get the album ApiKey and proceed to get Album cover
                    var songInfo = await DatabaseSongIdsInfoForSongApiKey(apiKey ?? Guid.Empty, cancellationToken).ConfigureAwait(false);
                    if (songInfo != null)
                    {
                        apiKey = songInfo.AlbumApiKey;
                    }
                }
                if (apiKey != null)
                {
                    var albumResponse = await albumService.GetByApiKeyAsync(apiKey.Value, cancellationToken);
                    if (albumResponse.IsSuccess)
                    {
                        var sql = """
                                  select l."Path" || aa."Directory" || a."Directory"
                                  from "Albums" a
                                  join "Artists" aa on (aa."Id" = a."ArtistId")
                                  join "Libraries" l on (l."Id" = aa."LibraryId")
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
                            else
                            {
                                Logger.Warning("[{ServiceName}] Album directory is missing Melodee data file [{AlbumPath}]", nameof(OpenSubsonicApiService), pathToAlbum);
                                var albumDirInfo = new FileSystemDirectoryInfo
                                {
                                    Name = pathToAlbum,
                                    Path = pathToAlbum
                                };
                                var imagesForFolder = albumDirInfo.AllFileImageTypeFileInfos();
                                var firstFrontImage = imagesForFolder.FirstOrDefault(x => string.Equals(x.Name, $" {ImageInfo.ImageFilePrefix }01-front.jpg", StringComparison.OrdinalIgnoreCase));
                                if (firstFrontImage != null)
                                {
                                    coverBytes = await File.ReadAllBytesAsync(firstFrontImage.FullName, cancellationToken).ConfigureAwait(false);
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to get cover image for [{ApiId}]", apiId);
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
    public async Task<ResponseModel> GetOpenSubsonicExtensionsAsync(ApiRequest apiApiRequest, CancellationToken cancellationToken = default)
    {
        var authResponse = new ResponseModel
        {
            UserInfo = BlankUserInfo,
            IsSuccess = true,
            ResponseData = await NewApiResponse(true, string.Empty, string.Empty)
        };
        var data = new List<OpenSubsonicExtension>
        {
            // Add support for POST request to the API (application/x-www-form-urlencoded).
            new("apiKeyAuthentication", [1]),   
            // Add support for POST request to the API (application/x-www-form-urlencoded).
            new("formPost", [1]),            
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

        using (Operation.At(LogEventLevel.Debug).Time("AuthenticateSubsonicApiAsync: username [{Username}]", apiRequest.Username))
        {
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
                    var usersPassword = user.Data.Decrypt(user.Data.PasswordEncrypted, await Configuration.Value);
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
                        _ = Task.Run(async () =>
                        {
                            using (Operation.At(LogEventLevel.Debug).Time("AuthenticateSubsonicApiAsync: username [{Username}] : update timestamps", apiRequest.Username))
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
                            }
                        }, cancellationToken);
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
    }

    private Task<ApiResponse> DefaultApiResponse()
    {
        return NewApiResponse(true, string.Empty, string.Empty);
    }

    public async Task<ApiResponse> NewApiResponse(bool isOk, string dataPropertyName, string dataDetailPropertyName, Error? error = null, object? data = null)
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

    public async Task<ResponseModel> SavePlayQueueAsync(string[]? apiIds, string? currentApiId, double? position, ApiRequest apiRequest, CancellationToken cancellationToken)
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
        var apiKeys = apiIds?.Select(x => ApiKeyFromId(x)!.Value).ToArray();
        var current = ApiKeyFromId(currentApiId);
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
        var registerResult = await userService.RegisterAsync(request.Username, request.Email, request.Password, cancellationToken).ConfigureAwait(false);
        var result = registerResult.IsSuccess;        
        if (!result)
        {
            return new ResponseModel
            {
                UserInfo = BlankUserInfo,
                IsSuccess = result,
                ResponseData = await NewApiResponse(result, string.Empty, string.Empty, new Error(10, "User creation failed."))
            };
        }

        return new ResponseModel
        {
            UserInfo = BlankUserInfo,
            IsSuccess = result,
            ResponseData = await NewApiResponse(result, string.Empty, string.Empty, result ? null : Error.AuthError)
        };
    }

    public async Task<ResponseModel> ScrobbleAsync(string[] ids, double[]? times, bool? submission, ApiRequest apiRequest, CancellationToken cancellationToken)
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

        if (times?.Length > 0 && times.Length != ids.Length)
        {
            return new ResponseModel
            {
                UserInfo = BlankUserInfo,
                ResponseData = await NewApiResponse(false, string.Empty, string.Empty, Error.GenericError("Wrong number of timestamps."))
            };
        }
        
        var result = false;
        await scrobbleService.InitializeAsync(await Configuration.Value, cancellationToken).ConfigureAwait(false);

        // If not provided then default to this is a "submission" versus a "now playing" notification.
        var isSubmission = submission ?? true;
        
        Console.WriteLine($"** Scrobble: isSubmission [{isSubmission}] ids [{string.Join(",", ids)}] times [{string.Join(",", times ?? [])}] Request [{apiRequest}]");

        if (!isSubmission)
        {
            foreach (var idAndIndex in ids.Select((id, index) => new { id, index }))
            {
                await scrobbleService.NowPlaying(authResponse.UserInfo, ApiKeyFromId(idAndIndex.id) ?? Guid.Empty, times?.Length > idAndIndex.index ? times[idAndIndex.index] : null, apiRequest.ApiRequestPlayer?.Client ?? string.Empty, cancellationToken).ConfigureAwait(false);
            }
        } 
        else 
        {
            foreach (var idAndIndex in ids.Select((id, index) => new { id, index }))
            {
                var id = ApiKeyFromId(idAndIndex.id) ?? Guid.Empty;
                double? time = times?.Length > idAndIndex.index ? times[idAndIndex.index] : null;
                var uniqueId = SafeParser.Hash(authResponse.UserInfo.ApiKey.ToString(), id.ToString());
                var nowPlayingInfo = (await scrobbleService.GetNowPlaying(cancellationToken).ConfigureAwait(false)).Data.FirstOrDefault(x => x.UniqueId == uniqueId);
                if (nowPlayingInfo != null)
                {
                    await scrobbleService.Scrobble(authResponse.UserInfo, id, time, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    Logger.Debug("Scrobble: Ignoring duplicate scrobble submission for [{UniqueId}]", uniqueId);
                }
            }
        }
        result = true;
        return new ResponseModel
        {
            UserInfo = BlankUserInfo,
            IsSuccess = result,
            ResponseData = await NewApiResponse(result, string.Empty, string.Empty, result ? null : Error.AuthError)
        };
    }

    /// <summary>
    ///     Get bytes for song with support for chunking from request header values.
    /// </summary>
    public async Task<StreamResponse> StreamAsync(StreamRequest request, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        long rangeBegin = 0;
        long rangeEnd = 0;
        var range = apiRequest.RequestHeaders.FirstOrDefault(x => x.Key == "Range")?.Value;
        if (range.Nullify() != null)
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
                  select l."Path" || aa."Directory" || a."Directory" || s."FileName" as Path, s."FileSize", s."Duration"/1000 as "Duration", s."ContentType"
                  from "Songs" s 
                  join "AlbumDiscs" ad on (ad."Id" = s."AlbumDiscId")
                  join "Albums" a on (a."Id" = ad."AlbumId")
                  join "Artists" aa on (a."ArtistId" = aa."Id")    
                  join "Libraries" l on (l."Id" = aa."LibraryId")
                  where s."ApiKey" = @apiKey;
                  """;
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var songStreamInfo = dbConn.QuerySingleOrDefault<SongStreamInfo>(sql, new { apiKey = ApiKeyFromId(request.Id) });
            if (!(songStreamInfo?.TrackFileInfo.Exists ?? false))
            {
                Logger.Warning("[{ServiceName}] Stream request for song that was not found. User [{ApiRequest}] Request [{Request}]", nameof(OpenSubsonicApiService), apiRequest.ToString(), request.ToString() );
                return new StreamResponse
                (
                    new Dictionary<string, StringValues>([]),
                    false,
                    []
                );
            }

            rangeEnd = rangeEnd == 0 ? songStreamInfo.FileSize : rangeEnd;

            var bytesToRead = (int)(rangeEnd - rangeBegin) + 1;
            if (bytesToRead > songStreamInfo.FileSize)
            {
                bytesToRead = (int)songStreamInfo.FileSize;
            }

            var trackBytes = new byte[bytesToRead];


            await using (var fs = songStreamInfo.TrackFileInfo.OpenRead())
            {
                try
                {
                    fs.Seek(rangeBegin, SeekOrigin.Begin);
                    await fs.ReadAsync(trackBytes.AsMemory(0, bytesToRead), cancellationToken).ConfigureAwait(false);
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

    public async Task<ResponseModel> GetNowPlayingAsync(ApiRequest apiRequest, CancellationToken cancellationToken)
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

        var nowPlaying = await scrobbleService.GetNowPlaying(cancellationToken).ConfigureAwait(false);
        var data = new List<Child>();
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var nowPlayingSongApiKeys = nowPlaying.Data.Select(x => x.Scrobble.SongApiKey).ToList();
            var nowPlayingSongs = await (from s in scopedContext
                        .Songs.Include(x => x.AlbumDisc)
                    where nowPlayingSongApiKeys.Contains(s.ApiKey)
                    select s)
                .AsNoTrackingWithIdentityResolution()
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            var nowPlayingSongIds = nowPlayingSongs.Select(x => x.Id).ToArray();
            var nowPlayingAlbumIds = nowPlayingSongs.Select(x => x.AlbumDisc).Select(x => x.AlbumId).Distinct().ToArray();
            var nowPlayingSongsAlbums = await (from a in scopedContext.Albums.Include(x => x.Artist)
                    where nowPlayingAlbumIds.Contains(a.Id)
                    select a)
                .AsNoTrackingWithIdentityResolution()
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            var nowPlayingUserSongs = await (from us in scopedContext.UserSongs
                    where us.UserId == authResponse.UserInfo.Id
                    where nowPlayingSongIds.Contains(us.Id)
                    select us)
                .AsNoTrackingWithIdentityResolution()
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var nowPlayingSong in nowPlayingSongs)
            {
                var album = nowPlayingSongsAlbums.First(x => x.Id == nowPlayingSong.AlbumDisc.AlbumId);
                var userSong = nowPlayingUserSongs.FirstOrDefault(x => x.SongId == nowPlayingSong.Id);
                var nowPlayingSongUniqueId = SafeParser.Hash(authResponse.UserInfo.ApiKey.ToString(), nowPlayingSong.ApiKey.ToString());
                data.Add(nowPlayingSong.ToChild(album, userSong, nowPlaying.Data.FirstOrDefault(x => x.UniqueId == nowPlayingSongUniqueId)));
            }
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            IsSuccess = true,
            ResponseData = await DefaultApiResponse() with
            {
                Data = data,
                DataPropertyName = "nowPlaying",
                DataDetailPropertyName = "entry"
            }
        };
    }

    public async Task<ResponseModel> SearchAsync(SearchRequest request, ApiRequest apiRequest, CancellationToken cancellationToken)
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

        ArtistSearchResult[] artists = [];
        AlbumSearchResult[] albums = [];
        SongSearchResult[] songs = [];

        if (request.QueryValue.Nullify() != null)
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                var defaultPageSize = (await Configuration.Value).GetValue<short>(SettingRegistry.DefaultsPageSize);
                var sqlParameters = new Dictionary<string, object>
                {
                    { "userId", authResponse.UserInfo.Id },
                    { "normalizedQuery", request.QueryNormalizedValue },
                    { "query", request.QueryValue },
                    { "artistOffset", request.ArtistOffset ?? 0 },
                    { "artistCount", request.ArtistCount ?? defaultPageSize },
                    { "albumOffset", request.AlbumOffset ?? 0 },
                    { "albumCount", request.AlbumCount ?? defaultPageSize },
                    { "songOffset", request.SongOffset ?? 0 },
                    { "songCount", request.SongCount ?? defaultPageSize }
                };
                var sql = """
                          select "ApiKey"::varchar as "Id", "Name", 'artist_' || "ApiKey" as "CoverArt", "AlbumCount"
                          from "Artists" a
                          where a."NameNormalized" like @normalizedQuery
                          or a."AlternateNames" like @query
                          ORDER BY a."SortName" OFFSET @artistOffset ROWS FETCH NEXT @artistCount ROWS ONLY;
                          """;
                artists = (await dbConn
                    .QueryAsync<ArtistSearchResult>(sql, sqlParameters)
                    .ConfigureAwait(false)).ToArray();

                sql = """
                      select a."ApiKey"::varchar as "Id", a."Name", 'album_' || a."ApiKey"::varchar as "CoverArt", a."SongCount", a."CreatedAt", 
                             a."Duration" as "DurationMs", aa."ApiKey"::varchar as "ArtistId", aa."Name" as "Artist"    
                      from "Albums" a
                      left join "Artists" aa on (a."ArtistId" = aa."Id")
                      where a."NameNormalized"  like @normalizedQuery
                      or a."AlternateNames" like @query
                      ORDER BY a."SortName" OFFSET @albumOffset ROWS FETCH NEXT @albumCount ROWS ONLY;
                      """;
                albums = (await dbConn
                    .QueryAsync<AlbumSearchResult>(sql, sqlParameters)
                    .ConfigureAwait(false)).ToArray();

                sql = """
                      select s."ApiKey"::varchar as "Id", a."ApiKey"::varchar as Parent, s."Title", a."Name" as Album, aa."Name" as "Artist", 'song_' || s."ApiKey"::varchar as "CoverArt", 
                             a."SongCount", s."CreatedAt", s."Duration" as "DurationMs", s."BitRate", s."SongNumber" as "Track", 
                             DATE_PART('year', a."ReleaseDate"::date) as "Year", unnest(a."Genres") as "Genre", s."FileSize" as "Size", 
                             s."ContentType", l."Path" || aa."Directory" || a."Directory" || s."FileName" as "Path", RIGHT(s."FileName", 3) as "Suffix", a."ApiKey"::varchar as "AlbumId", 
                             aa."ApiKey"::varchar as "ArtistId", aa."Name" as "Artist"    
                      from "Songs" s
                      join "AlbumDiscs" ad on (s."AlbumDiscId" = ad."Id")
                      join "Albums" a on (ad."AlbumId" = a."Id")
                      join "Artists" aa on (a."ArtistId" = aa."Id")
                      join "Libraries" l on (aa."LibraryId" = l."Id")
                      where s."TitleNormalized"  like @normalizedQuery
                      or s."AlternateNames" like @query
                      ORDER BY a."SortName" OFFSET @songOffset ROWS FETCH NEXT @songCount ROWS ONLY;
                      """;
                songs = (await dbConn
                    .QueryAsync<SongSearchResult>(sql, sqlParameters)
                    .ConfigureAwait(false)).ToArray();

                if (albums.Length == 0 && songs.Length == 0 && artists.Length == 0)
                {
                    Logger.Information("! No result for query [{Query}] Normalized [{QueryNormalized}]", request.QueryValue, request.QueryNormalizedValue);
                }
            }
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            IsSuccess = true,
            ResponseData = await DefaultApiResponse() with
            {
                Data = new
                {
                    Artist = artists,
                    Album = albums,
                    Song = songs
                },
                DataPropertyName = "searchResult3"
            }
        };
    }


    public async Task<ResponseModel> GetMusicDirectoryAsync(string apiId, ApiRequest apiRequest, CancellationToken cancellationToken)
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

        Directory? data = null;
        
        var apiKey = ApiKeyFromId(apiId);
        if (IsApiIdForArtist(apiId) && apiKey != null)
        {
            var artistInfo = await DatabaseArtistInfoForArtistApiKey(apiKey.Value, authResponse.UserInfo.Id, cancellationToken);
            if (artistInfo != null)
            {
                await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
                {
                    var artistAlbums = await scopedContext
                        .Albums
                        .Include(x => x.Artist)
                        .Include(x => x.UserAlbums.Where(ua => ua.UserId == authResponse.UserInfo.Id))
                        .Where(x => x.ArtistId == artistInfo.Id)
                        .ToArrayAsync(cancellationToken)
                        .ConfigureAwait(false);
                    data = new Directory(artistInfo.CoverArt,
                        null,
                        artistInfo.Name,
                        artistInfo.UserStarred.ToString(),
                        artistInfo.UserRating,
                        artistInfo.CalculatedRating ,
                        artistInfo.PlayCount,
                        artistInfo.Played.ToString(),
                        artistAlbums?.Select(x => x.ToChild(x.UserAlbums.FirstOrDefault(),null)).ToArray() ?? []);
                }
            }
        }
        else if (IsApiIdForAlbum(apiId) && apiKey != null)
        {
            var albumInfo = await DatabaseAlbumInfoForAlbumApiKey(apiKey.Value, authResponse.UserInfo.Id, cancellationToken);
            if (albumInfo != null)
            {
                var albumSongInfos = await DatabaseSongInfosForAlbumApiKey(apiKey.Value, authResponse.UserInfo.Id, cancellationToken);
                if (albumSongInfos != null)
                {
                    await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
                    {
                        var songIds = albumSongInfos.Select(x => x.Id).ToArray();
                        var albumSongs = await scopedContext
                            .Songs
                            .Include(x => x.AlbumDisc).ThenInclude(x => x.Album).ThenInclude(x => x.Artist)
                            .Include(x => x.UserSongs.Where(ua => ua.UserId == authResponse.UserInfo.Id))
                            .Where(x => songIds.Contains(x.Id))
                            .ToArrayAsync(cancellationToken)
                            .ConfigureAwait(false);
                        data = new Directory(albumInfo.CoverArt,
                            albumInfo.CoverArt,
                            albumInfo.Name,
                            albumInfo.UserStarred.ToString(),
                            albumInfo.UserRating,
                            albumInfo.CalculatedRating,
                            albumInfo.PlayCount,
                            albumInfo.Played.ToString(),
                            albumSongs?.Select(x => x.ToChild(x.AlbumDisc.Album, x.UserSongs.FirstOrDefault(), null)).ToArray() ?? []);
                    }
                }
            }
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            IsSuccess = true,
            ResponseData = await DefaultApiResponse() with
            {
                Data = data,
                DataPropertyName = "directory"
            }
        };        
    }

    public async Task<ResponseModel> GetIndexesAsync(string dataPropertyName, Guid? musicFolderId, long? ifModifiedSince, ApiRequest apiRequest, CancellationToken cancellationToken)
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

        var indexLimit = (await Configuration.Value).GetValue<short>(SettingRegistry.OpenSubsonicIndexesArtistLimit);
        if (indexLimit == 0)
        {
            indexLimit = short.MaxValue;
        }

        Indexes? data = null;
        var libraryId = 0;
        var lastModifed = string.Empty;
        if (musicFolderId.HasValue)
        {
            var libraryResult = await libraryService.ListAsync(new PagedRequest(), cancellationToken).ConfigureAwait(false);
            var library = libraryResult.Data?.Where(x => x.ApiKey == musicFolderId.Value).FirstOrDefault();
            libraryId = library?.Id ?? 0;
            lastModifed = library?.LastUpdatedAt.ToString() ?? string.Empty;
        }

        //TODO looks like these are values removed from sortname when sorting
        // see https://github.com/navidrome/navidrome/blob/9ae898d071e32cf56261f3b13a639fd01092c201/utils/str/sanitize_strings.go#L52
        var ignoredArticles = (await Configuration.Value).GetValue<string>(SettingRegistry.ProcessingIgnoredArticles) ?? string.Empty;

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var sql = """
                      select a."Id", a."ApiKey", LEFT(a."SortName", 1) as "Index", a."Name", 'artist_' || a."ApiKey" as "CoverArt", 
                             a."CalculatedRating", a."AlbumCount", a."PlayedCount" as "PlayCount", a."LastPlayedAt" as "Played", a."Directory",
                             ua."StarredAt" as "UserStarred", ua."Rating" as "UserRating"
                      from "Artists" a
                      join "Albums" a2 on (a."Id" = a2."ArtistId")
                      left join "UserArtists" ua on (a."Id" = ua."ArtistId" and ua."UserId" = @userId)
                      where ((@libraryId = 0) or (@libraryId > 0 and a."LibraryId" = @libraryId))
                      and (EXTRACT(EPOCH from a."LastUpdatedAt") >= 0)
                      order by a."SortOrder", a."SortName"
                      """;
            var indexes = await dbConn.QueryAsync<DatabaseDirectoryInfo>(sql, new { libraryId, modifiedSince = ifModifiedSince ?? 0, userId = authResponse.UserInfo.Id }).ConfigureAwait(false);

            var artists = new List<ArtistIndex>();
            foreach (var grouped in indexes.GroupBy(x => x.Index))
            {
                var aa = new List<Artist>();
                foreach (var info in grouped)
                {
                    aa.Add(new Artist(info.CoverArt, info.Name, info.AlbumCount, info.UserRatingValue, info.CalculatedRating, info.CoverArt, "Url"));
                }

                artists.Add(new ArtistIndex(grouped.Key, aa.Take(indexLimit).ToArray()));
            }

            data = new Indexes(ignoredArticles, lastModifed, [], artists.ToArray(), []);
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            IsSuccess = true,
            ResponseData = await DefaultApiResponse() with
            {
                Data = data,
                DataPropertyName = dataPropertyName
            }
        };
    }

    /// <summary>
    ///     Returns all configured top-level music folders.
    /// </summary>
    public async Task<ResponseModel> GetMusicFolders(ApiRequest apiRequest, CancellationToken cancellationToken)
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

        NamedInfo[] data = [];

        var libraryResult = await libraryService.ListAsync(new PagedRequest(), cancellationToken).ConfigureAwait(false);
        if (libraryResult.IsSuccess)
        {
            data = libraryResult.Data.Where(x => x.TypeValue == LibraryType.Library).Select(x => new NamedInfo(x.ApiKey, x.Name)).ToArray();
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            IsSuccess = true,
            ResponseData = await DefaultApiResponse() with
            {
                Data = data,
                DataPropertyName = "musicFolders",
                DataDetailPropertyName = "musicFolder"
            }
        };
    }

    public async Task<object> GetArtistAsync(string id, ApiRequest apiRequest, CancellationToken cancellationToken)
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

        Artist? data = null;

        var apiKey = ApiKeyFromId(id);
        if (apiKey != null)
        {
            var artistInfo = await DatabaseArtistInfoForArtistApiKey(apiKey.Value, authResponse.UserInfo.Id, cancellationToken).ConfigureAwait(false);
            if (artistInfo != null)
            {
                data = new Artist(
                    id,
                    artistInfo.Name,
                    artistInfo.AlbumCount,
                    artistInfo.UserRating,
                    artistInfo.CalculatedRating,
                    artistInfo.CoverArt,
                    "Url",
                    await AlbumListForArtistApiKey(apiKey.Value, authResponse.UserInfo.Id, cancellationToken).ConfigureAwait(false));
            }
        }
        
        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            IsSuccess = true,
            ResponseData = await DefaultApiResponse() with
            {
                Data = data,
                DataPropertyName = "artist"
            }
        };        
        
    }
}
