using System.Globalization;
using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Constants;
using Melodee.Common.Data.Models.DTOs;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.MessageBus.Events;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Models.OpenSubsonic;
using Melodee.Common.Models.OpenSubsonic.DTO;
using Melodee.Common.Models.OpenSubsonic.Enums;
using Melodee.Common.Models.OpenSubsonic.Requests;
using Melodee.Common.Models.OpenSubsonic.Responses;
using Melodee.Common.Models.OpenSubsonic.Searching;
using Melodee.Common.Plugins.Conversion.Image;
using Melodee.Common.Services.Extensions;
using Melodee.Common.Services.Interfaces;
using Melodee.Common.Services.Scanning;
using Melodee.Common.Services.SearchEngines;
using Melodee.Common.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using NodaTime;
using Quartz;
using Rebus.Bus;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using SmartFormat;
using dbModels = Melodee.Common.Data.Models;
using Artist = Melodee.Common.Models.OpenSubsonic.Artist;
using Directory = Melodee.Common.Models.OpenSubsonic.Directory;
using ImageInfo = Melodee.Common.Models.ImageInfo;
using License = Melodee.Common.Models.OpenSubsonic.License;
using Playlist = Melodee.Common.Models.OpenSubsonic.Playlist;
using PlayQueue = Melodee.Common.Models.OpenSubsonic.PlayQueue;
using ScanStatus = Melodee.Common.Models.OpenSubsonic.ScanStatus;

namespace Melodee.Common.Services;

/// <summary>
///     Handles OpenSubsonic API calls.
/// </summary>
public class OpenSubsonicApiService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    DefaultImages defaultImages,
    IMelodeeConfigurationFactory configurationFactory,
    UserService userService,
    ArtistService artistService,
    AlbumService albumService,
    SongService songService,
    AlbumDiscoveryService albumDiscoveryService,
    IScheduler schedule,
    ScrobbleService scrobbleService,
    LibraryService libraryService,
    ArtistSearchEngineService artistSearchEngineService,
    IBus bus
)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string ImageCacheRegion = "urn:openSubsonic:artist-and-album-images";

    private Lazy<Task<IMelodeeConfiguration>> Configuration => new(() => configurationFactory.GetConfigurationAsync());

    public static UserInfo BlankUserInfo => new(0, Guid.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

    private static bool IsApiIdForArtist(string? id)
    {
        return id.Nullify() != null && (id?.StartsWith($"artist{OpenSubsonicServer.ApiIdSeparator}") ?? false);
    }

    private static bool IsApiIdForAlbum(string? id)
    {
        return id.Nullify() != null && (id?.StartsWith($"album{OpenSubsonicServer.ApiIdSeparator}") ?? false);
    }

    private static bool IsApiIdForUser(string? id)
    {
        return id.Nullify() != null && (id?.StartsWith($"user{OpenSubsonicServer.ApiIdSeparator}") ?? false);
    }

    private static bool IsApiIdForSong(string? id)
    {
        return id.Nullify() != null && (id?.StartsWith($"song{OpenSubsonicServer.ApiIdSeparator}") ?? false);
    }

    private static Guid? ApiKeyFromId(string? id)
    {
        if (id.Nullify() == null)
        {
            return null;
        }

        var apiIdParts = id!.Split(OpenSubsonicServer.ApiIdSeparator);
        var toParse = id;
        if (apiIdParts.Length < 2)
        {
            Log.Warning("ApiKeyFromId: Invalid ApiKey [{Key}]", id);
        }
        else
        {
            toParse = apiIdParts[1];
        }

        return SafeParser.ToGuid(toParse);
    }

    /// <summary>
    ///     Get details about the software license.
    /// </summary>
    public async Task<ResponseModel> GetLicenseAsync(ApiRequest apiApiRequest, CancellationToken cancellationToken = default)
    {
        return new ResponseModel
        {
            UserInfo = BlankUserInfo,

            ResponseData = await DefaultApiResponse() with
            {
                DataPropertyName = "license",
                Data = new License(true,
                    (await Configuration.Value).GetValue<string>(SettingRegistry.OpenSubsonicServerLicenseEmail) ?? ServiceUser.Instance.Value.Email,
                    DateTimeOffset.UtcNow.AddYears(50).ToXmlSchemaDateTimeFormat(),
                    DateTimeOffset.UtcNow.AddYears(50).ToXmlSchemaDateTimeFormat()
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
            return authResponse with { UserInfo = BlankUserInfo };
        }

        Playlist[] data = [];
        var sql = string.Empty;

        try
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var playLists = await scopedContext
                    .Playlists
                    .Include(x => x.User)
                    .Include(x => x.Songs).ThenInclude(x => x.Song).ThenInclude(x => x.AlbumDisc).ThenInclude(x => x.Album).ThenInclude(x => x.Artist)
                    .Include(x => x.Songs).ThenInclude(x => x.Song).ThenInclude(x => x.UserSongs.Where(ua => ua.UserId == authResponse.UserInfo.Id))
                    .Where(x => x.UserId == authResponse.UserInfo.Id)
                    .AsSplitQuery()
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);
                data = playLists.Select(x => x.ToApiPlaylist()).ToArray();
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to get Playlists SQL [{Sql}] Request [{ApiResult}]", sql, apiRequest);
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            ResponseData = await DefaultApiResponse() with
            {
                Data = data,
                DataPropertyName = "playlists",
                DataDetailPropertyName = "playlist"
            }
        };
    }

    public async Task<ResponseModel> UpdatePlaylistAsync(UpdatePlayListRequest updateRequest, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        Error? notAuthorizedError = null;
        var result = false;

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var apiKey = ApiKeyFromId(updateRequest.PlaylistId);
            var playlist = await scopedContext
                .Playlists
                .Include(x => x.User)
                .Include(x => x.Songs).ThenInclude(x => x.Song).ThenInclude(x => x.AlbumDisc).ThenInclude(x => x.Album).ThenInclude(x => x.Artist)
                .Include(x => x.Songs).ThenInclude(x => x.Song).ThenInclude(x => x.UserSongs.Where(ua => ua.UserId == authResponse.UserInfo.Id))
                .FirstOrDefaultAsync(x => x.ApiKey == apiKey, cancellationToken)
                .ConfigureAwait(false);
            if (playlist != null)
            {
                if (playlist.UserId != authResponse.UserInfo.Id)
                {
                    notAuthorizedError = Error.UserNotAuthorizedError;
                }
                else
                {
                    foreach (var songToRemove in updateRequest.SongIdToRemove ?? [])
                    {
                        var songApiKey = ApiKeyFromId(songToRemove);
                        var song = await scopedContext.Songs.FirstOrDefaultAsync(x => x.ApiKey == songApiKey, cancellationToken);
                        if (song != null)
                        {
                            var playListSong = playlist.Songs.FirstOrDefault(x => x.SongId == song.Id);
                            if (playListSong != null)
                            {
                                playlist.Songs.Remove(playListSong);
                            }
                        }
                    }

                    foreach (var songToAdd in updateRequest.SongIdToAdd ?? [])
                    {
                        var songApiKey = ApiKeyFromId(songToAdd);
                        var song = await scopedContext.Songs.FirstOrDefaultAsync(x => x.ApiKey == songApiKey, cancellationToken);
                        if (song != null)
                        {
                            var playListSong = playlist.Songs.FirstOrDefault(x => x.SongId == song.Id);
                            if (playListSong == null)
                            {
                                playlist.Songs.Add(new dbModels.PlaylistSong
                                {
                                    PlaylistOrder = playlist.Songs.Count + 1,
                                    Song = song
                                });
                            }
                        }
                    }

                    playlist.Comment = updateRequest.Comment;
                    playlist.Duration = playlist.Songs.Sum(x => x.Song.Duration);
                    playlist.IsPublic = updateRequest.Public ?? playlist.IsPublic;
                    playlist.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);
                    playlist.Name = updateRequest.Name ?? playlist.Name;
                    playlist.SongCount = SafeParser.ToNumber<short>(playlist.Songs.Count);
                    await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    result = true;
                }
            }
        }

        return new ResponseModel
        {
            UserInfo = BlankUserInfo,
            IsSuccess = result,
            ResponseData = await NewApiResponse(result, string.Empty, string.Empty, notAuthorizedError ?? (result ? null : Error.InvalidApiKeyError))
        };
    }

    /// <summary>
    ///     Deletes a saved playlist.
    /// </summary>
    public async Task<ResponseModel> DeletePlaylistAsync(string id, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        Error? notAuthorizedError = null;
        var result = false;

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var apiKey = ApiKeyFromId(id);
            var playlist = await scopedContext
                .Playlists
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.ApiKey == apiKey, cancellationToken)
                .ConfigureAwait(false);
            if (playlist != null)
            {
                if (playlist.UserId != authResponse.UserInfo.Id)
                {
                    notAuthorizedError = Error.UserNotAuthorizedError;
                }
                else
                {
                    scopedContext.Playlists.Remove(playlist);
                    await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    result = true;
                }
            }
        }

        return new ResponseModel
        {
            UserInfo = BlankUserInfo,
            IsSuccess = result,
            ResponseData = await NewApiResponse(result, string.Empty, string.Empty, notAuthorizedError ?? (result ? null : Error.InvalidApiKeyError))
        };
    }

    public async Task<ResponseModel> CreatePlaylistAsync(string? id, string? name, string[]? songId, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        var playListId = string.Empty;
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
            var isCreatingPlaylist = id.Nullify() == null && name.Nullify() != null;
            if (isCreatingPlaylist)
            {
                // creating new with name and songs 
                var songApiKeysForPlaylist = songId?.Where(x => x.Nullify() != null).Select(ApiKeyFromId).ToArray() ?? [];
                var songsForPlaylist = await scopedContext.Songs.Where(x => songApiKeysForPlaylist.Contains(x.ApiKey)).ToArrayAsync(cancellationToken).ConfigureAwait(false);
                var newPlaylist = new dbModels.Playlist
                {
                    CreatedAt = now,
                    Name = name!,
                    UserId = authResponse.UserInfo.Id,
                    SongCount = SafeParser.ToNumber<short>(songsForPlaylist.Length),
                    Duration = songsForPlaylist.Sum(x => x.Duration),
                    Songs = songsForPlaylist.Select((x, i) => new dbModels.PlaylistSong
                    {
                        SongId = x.Id,
                        SongApiKey = x.ApiKey,
                        PlaylistOrder = i
                    }).ToArray()
                };
                await scopedContext.Playlists.AddAsync(newPlaylist, cancellationToken).ConfigureAwait(false);
                await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                playListId = newPlaylist.ToApiKey();
                Logger.Information("User [{UserInfo}] created playlist [{Name}] with [{SongCount}] songs.", authResponse.UserInfo, name, songsForPlaylist.Length);
            }
            // updating either new name or songs on playlist
        }

        return await GetPlaylistAsync(playListId, apiRequest, cancellationToken);
    }

    /// <summary>
    ///     Returns a listing of files in a saved playlist.
    /// </summary>
    public async Task<ResponseModel> GetPlaylistAsync(string id, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        Playlist? data;

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var apiKey = ApiKeyFromId(id);
            var playlist = await scopedContext
                .Playlists
                .Include(x => x.User)
                .Include(x => x.Songs).ThenInclude(x => x.Song).ThenInclude(x => x.AlbumDisc).ThenInclude(x => x.Album).ThenInclude(x => x.Artist)
                .Include(x => x.Songs).ThenInclude(x => x.Song).ThenInclude(x => x.UserSongs.Where(ua => ua.UserId == authResponse.UserInfo.Id))
                .FirstOrDefaultAsync(x => x.ApiKey == apiKey, cancellationToken)
                .ConfigureAwait(false);

            data = playlist?.ToApiPlaylist();
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,

            ResponseData = await DefaultApiResponse() with
            {
                IsSuccess = data != null,
                Data = data,
                DataPropertyName = "playlist"
            }
        };
    }

    public async Task<ResponseModel> GetAlbumListAsync(GetAlbumListRequest albumListRequest, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        long totalCount = 0;
        AlbumList[] data = [];
        var sql = string.Empty;

        try
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                var sqlParameters = new Dictionary<string, object?>
                {
                    { "genre", albumListRequest.Genre },
                    { "fromYear", albumListRequest.FromYear },
                    { "toYear", albumListRequest.ToYear },
                    { "userId", authResponse.UserInfo.Id }
                };
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
                                a."Genres",
                                (SELECT "IsStarred" FROM "UserAlbums" WHERE "UserId" = @userId AND "AlbumId" = a."Id") as "Starred", 
                                (SELECT COUNT(*) FROM "UserAlbums" WHERE "IsStarred" AND "AlbumId" = a."Id") as "UserStarredCount",
                                a."CalculatedRating" as "AverageRating",
                                ''
                                FROM "Albums" a 
                                JOIN "Artists" aa on (a."ArtistId" = aa."Id")
                                """;
                var whereSql = string.Empty;
                var orderSql = string.Empty;
                var limitSql = $"OFFSET {albumListRequest.OffsetValue} ROWS FETCH NEXT {albumListRequest.SizeValue} ROWS ONLY;";
                switch (albumListRequest.Type)
                {
                    case ListType.Random:
                        orderSql = "ORDER BY RANDOM()";
                        break;

                    case ListType.Newest:
                        orderSql = "ORDER BY a.\"CreatedAt\" DESC";
                        break;

                    case ListType.Highest:
                        orderSql = "ORDER BY a.\"CalculatedRating\" DESC";
                        break;

                    case ListType.Frequent:
                        orderSql = "ORDER BY a.\"PlayedCount\" DESC";
                        break;

                    case ListType.Recent:
                        orderSql = "ORDER BY a.\"LastPlayedAt\" DESC";
                        break;

                    case ListType.AlphabeticalByName:
                        orderSql = "ORDER BY a.\"SortName\"";
                        break;

                    case ListType.AlphabeticalByArtist:
                        orderSql = "ORDER BY aa.\"SortName\"";
                        break;

                    case ListType.Starred:
                        orderSql = "ORDER BY \"UserStarredCount\" DESC";
                        break;

                    case ListType.ByYear:
                        whereSql = "where DATE_PART('year', a.\"ReleaseDate\"::date) between @fromYear AND @toYear";
                        orderSql = "ORDER BY \"Year\" DESC";
                        break;

                    case ListType.ByGenre:
                        whereSql = "where @genre = any(a.\"Genres\")";
                        orderSql = "ORDER BY \"Genre\" DESC";
                        break;
                }

                sql = $"{selectSql} {whereSql} {orderSql} {limitSql}";
                data = (await dbConn
                    .QueryAsync<AlbumList>(sql, sqlParameters)
                    .ConfigureAwait(false)).ToArray();

                totalCount = await dbConn
                    .ExecuteScalarAsync<long>($"SELECT COUNT(*) FROM \"Albums\" {whereSql};")
                    .ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to get AlbumList SQL [{Sql}] Request [{ApiResult}]", sql, apiRequest);
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            TotalCount = totalCount,
            ResponseData = await DefaultApiResponse() with
            {
                Data = data,
                DataPropertyName = "albumList",
                DataDetailPropertyName = "album"
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
            return authResponse with { UserInfo = BlankUserInfo };
        }

        long totalCount = 0;
        AlbumList2[] data = [];
        var sql = string.Empty;

        try
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                var sqlParameters = new Dictionary<string, object?>
                {
                    { "genre", albumListRequest.Genre },
                    { "fromYear", albumListRequest.FromYear },
                    { "toYear", albumListRequest.ToYear },
                    { "userId", authResponse.UserInfo.Id }
                };

                var selectSql = """
                                SELECT 
                                'album_' || cast(a."ApiKey" as varchar(50)) as "Id",
                                'library_' || cast(aa."ApiKey" as varchar(50)) as "Parent",
                                a."Name" as "Album",
                                a."Name" as "Title",
                                a."Name" as "Name",
                                a."SortName",
                                a."Comment",
                                a."MusicBrainzId",
                                'album_' || cast(a."ApiKey" as varchar(50)) as "CoverArt",
                                a."SongCount",
                                a."CreatedAt" as "CreatedRaw",
                                a."Duration"/1000 as "Duration",
                                a."PlayedCount",
                                'artist_' || cast(aa."ApiKey" as varchar(50)) as "ArtistId",
                                aa."Name" as "Artist",
                                DATE_PART('year', a."ReleaseDate"::date) as "Year",
                                a."Genres",
                                (SELECT "StarredAt" FROM "UserAlbums" WHERE "UserId" = @userId AND "AlbumId" = a."Id") as "Starred", 
                                (SELECT COUNT(*) FROM "UserAlbums" WHERE "IsStarred" AND "AlbumId" = a."Id") as "UserStarredCount"
                                FROM "Albums" a 
                                JOIN "Artists" aa on (a."ArtistId" = aa."Id")
                                JOIN "Libraries" l on (aa."LibraryId" = l."Id")
                                """;
                var whereSql = string.Empty;
                string orderSql;
                var limitSql = $"OFFSET {albumListRequest.OffsetValue} ROWS FETCH NEXT {albumListRequest.SizeValue} ROWS ONLY;";
                switch (albumListRequest.Type)
                {
                    case ListType.Newest:
                        orderSql = "ORDER BY a.\"CreatedAt\" DESC nulls last";
                        break;

                    case ListType.Highest:
                        orderSql = "ORDER BY a.\"CalculatedRating\" DESC nulls last";
                        break;

                    case ListType.Frequent:
                        orderSql = "ORDER BY a.\"PlayedCount\" DESC nulls last";
                        break;

                    case ListType.Recent:
                        orderSql = "ORDER BY a.\"LastPlayedAt\" DESC nulls last ";
                        break;

                    case ListType.AlphabeticalByName:
                        orderSql = "ORDER BY a.\"SortName\"";
                        break;

                    case ListType.AlphabeticalByArtist:
                        orderSql = "ORDER BY aa.\"SortName\"";
                        break;

                    case ListType.Starred:
                        orderSql = "ORDER BY \"Starred\" DESC nulls last";
                        break;

                    case ListType.ByYear:
                        if (albumListRequest.FromYear < albumListRequest.ToYear)
                        {
                            whereSql = "where DATE_PART('year', a.\"ReleaseDate\"::date) between @fromYear AND @toYear";
                            orderSql = "ORDER BY \"Year\" DESC nulls last";
                        }
                        else
                        {
                            orderSql = "ORDER BY \"Year\" ASC nulls last";
                        }

                        break;

                    case ListType.ByGenre:
                        whereSql = "where @genre = any(a.\"Genres\")";
                        orderSql = "ORDER BY \"Genres\" DESC nulls last";
                        break;

                    default:
                        orderSql = "ORDER BY RANDOM()";
                        break;
                }

                sql = $"{selectSql} {whereSql} {orderSql} {limitSql}";
                data = (await dbConn
                    .QueryAsync<AlbumList2>(sql, sqlParameters)
                    .ConfigureAwait(false)).ToArray();

                sql = """
                      SELECT COUNT(a.*)
                      FROM "Albums" a 
                      JOIN "Artists" aa on (a."ArtistId" = aa."Id")
                      JOIN "Libraries" l on (aa."LibraryId" = l."Id")
                      """;
                totalCount = await dbConn
                    .ExecuteScalarAsync<long>($"{sql} {whereSql}", sqlParameters)
                    .ConfigureAwait(false);
            }

            Logger.Debug("[{MethodName}] Total Count [{TotalCount}] Result Count [{Count}]",
                nameof(GetAlbumList2Async), totalCount, data.Length);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to get AlbumList2 SQL [{Sql}] Request [{ApiResult}]", sql, apiRequest);
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            TotalCount = totalCount,
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
            return authResponse with { UserInfo = BlankUserInfo };
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
            IsSuccess = songResponse.IsSuccess,
            UserInfo = authResponse.UserInfo,
            ResponseData = authResponse.ResponseData with
            {
                Data = songResponse.Data?.ToApiChild(songResponse.Data.AlbumDisc.Album, userSong),
                DataPropertyName = "song"
            }
        };
    }

    public async Task<ResponseModel> GetAlbumAsync(string apiId, ApiRequest apiRequest, CancellationToken cancellationToken = default)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        AlbumId3WithSongs? data = null;

        var apiKey = ApiKeyFromId(apiId);
        if (apiKey != null)
        {
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


            var album = albumResponse.Data!;
            var userAlbum = await userService.UserAlbumAsync(apiRequest.Username, apiKey.Value, cancellationToken);
            var userSongsForAlbum = await userService.UserSongsForAlbumAsync(apiRequest.Username, apiKey.Value, cancellationToken) ?? [];
            data = new AlbumId3WithSongs
            {
                AlbumDate = album.ReleaseDate.ToItemDate(),
                Artist = album.Artist.Name,
                ArtistId = album.Artist.ToApiKey(),
                Artists = album.ContributingArtists(),
                CoverArt = album.ToApiKey(),
                Created = album.CreatedAt.ToString(),
                DiscTitles = album.Discs.Select(x => new DiscTitle(x.DiscNumber, x.Title ?? string.Empty)).ToArray(),
                DisplayArtist = album.Artist.Name,
                Duration = album.Duration.ToSeconds(),
                Genre = album.Genres?.ToCsv(),
                Genres = album.Genres?.Select(x => new ItemGenre(x)).ToArray() ?? [],
                Id = album.ToApiKey(),
                IsCompilation = album.IsCompilation,
                Moods = album.Moods ?? [],
                MusicBrainzId = album.MusicBrainzId?.ToString(),
                Name = album.Name,
                OriginalAlbumDate = album.OriginalReleaseDate?.ToItemDate() ?? album.ReleaseDate.ToItemDate(),
                Parent = album.ToApiKey(),
                PlayCount = album.PlayedCount,
                Played = album.LastPlayedAt.ToString(),
                RecordLabels = album.RecordLabels(),
                Song = album.Discs.SelectMany(x => x.Songs).OrderBy(x => x.AlbumDisc.DiscNumber).ThenBy(x => x.SongNumber)
                    .Select(x => x.ToApiChild(album, userSongsForAlbum.FirstOrDefault(us => us.SongId == x.Id)))
                    .ToArray(),
                SongCount = album.SongCount ?? 0,
                SortName = album.SortName,
                Starred = userAlbum?.LastUpdatedAt?.ToString(),
                Title = album.Name,
                UserRating = userAlbum?.Rating,
                Year = album.ReleaseDate.Year
            };
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            ResponseData = authResponse.ResponseData with
            {
                Data = data,
                DataPropertyName = apiRequest.IsXmlRequest ? string.Empty : "album"
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
            return authResponse with { UserInfo = BlankUserInfo };
        }

        var data = new List<Genre>();

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var allGenres = await dbConn.QueryAsync<string>("""
                                                            select distinct "Genres" 
                                                            from 
                                                            (
                                                            	select unnest("Genres") as "Genres"
                                                            	from "Albums"
                                                            	union ALL
                                                            	select  unnest("Genres") as "Genres"
                                                            	from "Songs"
                                                            ) t
                                                            group by "Genres"
                                                            order by "Genres";
                                                            """, cancellationToken).ConfigureAwait(false);
            var songGenres = (await dbConn.QueryAsync<Genre>("select genre as Value, count(1) as \"SongCount\" from \"Songs\", unnest(\"Genres\") as genre group by genre order by genre;", cancellationToken).ConfigureAwait(false)).ToArray();
            var albumGenres = (await dbConn.QueryAsync<Genre>("select genre as Value, count(1) as \"AlbumCount\" from \"Albums\", unnest(\"Genres\") as genre group by genre order by genre;", cancellationToken).ConfigureAwait(false)).ToArray();

            foreach (var genre in allGenres)
            {
                var genreNormalized = genre.ToNormalizedString() ?? genre;
                if (data.All(x => x.ValueNormalized != genreNormalized))
                {
                    var songCount = songGenres.Where(x => x.ValueNormalized == genreNormalized).Sum(x => x.SongCount);
                    var albumCount = albumGenres.Where(x => x.ValueNormalized == genreNormalized).Sum(x => x.AlbumCount);
                    data.Add(new Genre
                        {
                            Value = genre.CleanString() ?? genre,
                            SongCount = songCount,
                            AlbumCount = albumCount
                        }
                    );
                }
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
    public async Task<ResponseModel> GetAvatarAsync(string username, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        byte[]? avatarBytes = null;

        try
        {
            avatarBytes = await CacheManager.GetAsync($"urn:openSubsonic:avatar:{username}", async () =>
            {
                using (Operation.At(LogEventLevel.Debug).Time("GetAvatarAsync: [{Username}]", username))
                {
                    var userLibraryResult = await libraryService.GetUserImagesLibraryAsync(cancellationToken).ConfigureAwait(false);
                    if (userLibraryResult.IsSuccess)
                    {
                        var userAvatarFilename = authResponse.UserInfo.ToAvatarFileName(userLibraryResult.Data.Path);
                        if (File.Exists(userAvatarFilename))
                        {
                            return await File.ReadAllBytesAsync(userAvatarFilename, cancellationToken).ConfigureAwait(false);
                        }
                    }

                    return null;
                }
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to get avatar for user [{Username}]", username);
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            ResponseData = authResponse.ResponseData with
            {
                Data = avatarBytes ?? defaultImages.UserAvatarBytes,
                DataPropertyName = string.Empty,
                DataDetailPropertyName = string.Empty
            }
        };
    }

    public static string GenerateImageCacheKeyForApiId(string apiId, string size)
    {
        return $"urn:openSubsonic:imageForApikey:{apiId}:{size}";
    }

    public static void ClearImageCacheForApiId(string apiId, ICacheManager cacheManager)
    {
        // TODO this seems really inefficient as this clears all images in the region
        cacheManager.ClearRegion(ImageCacheRegion);
    }

    /// <summary>
    ///     Returns an artist, album, or song art image.
    /// </summary>
    public async Task<ResponseModel> GetImageForApiKeyId(string apiId, string? size, ApiRequest apiRequest, CancellationToken cancellationToken = default)
    {
        var isUserImageRequest = IsApiIdForUser(apiId);
        // If a user image request don't auth as it's used in the UI in the header (before auth'ed).
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess && !isUserImageRequest)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        var badEtag = Instant.MinValue.ToEtag();
        var sizeValue = size ?? ImageSizeRegistry.Large;
        var imageBytesAndEtag = await CacheManager.GetAsync(GenerateImageCacheKeyForApiId(apiId, sizeValue), async () =>
        {
            using (Operation.At(LogEventLevel.Debug).Time("GetImageForApiKeyId: [{Username}] Size [{Size}]", apiId, sizeValue))
            {
                byte[]? result = null;
                var eTag = string.Empty;
                try
                {
                    var apiKey = ApiKeyFromId(apiId);
                    if (apiKey == null)
                    {
                        return new ImageBytesAndEtag(null, null);
                    }

                    if (IsApiIdForArtist(apiId))
                    {
                        var artistInfo = await DatabaseArtistInfoForArtistApiKey(apiKey.Value, authResponse.UserInfo.Id, cancellationToken).ConfigureAwait(false);
                        if (artistInfo?.Directory != null)
                        {
                            var artistDirectoryInfo = new FileSystemDirectoryInfo
                            {
                                Path = artistInfo.Directory,
                                Name = artistInfo.Directory
                            };
                            var firstArtistImage = artistDirectoryInfo.AllFileImageTypeFileInfos().OrderBy(x => x.Name).FirstOrDefault();
                            if (firstArtistImage != null)
                            {
                                result = await File.ReadAllBytesAsync(firstArtistImage.FullName, cancellationToken).ConfigureAwait(false);
                                eTag = (artistInfo.LastUpdatedAt ?? artistInfo.CreatedAt).ToEtag();
                            }
                        }

                        if (result == null)
                        {
                            result = defaultImages.ArtistBytes;
                            eTag = badEtag;
                        }
                    }
                    else if (isUserImageRequest)
                    {
                        var userResult = await userService.GetByApiKeyAsync(apiKey.Value, cancellationToken).ConfigureAwait(false);
                        var userImageLibrary = await libraryService.GetUserImagesLibraryAsync(cancellationToken).ConfigureAwait(false);
                        var userImageFileName = userResult.Data.ToAvatarFileName(userImageLibrary.Data.Path);
                        var userImageFileInfo = new FileInfo(userImageFileName);
                        if (userImageFileInfo.Exists)
                        {
                            result = await File.ReadAllBytesAsync(userImageFileInfo.FullName, cancellationToken).ConfigureAwait(false);
                            eTag = userImageFileInfo.LastWriteTimeUtc.ToEtag();
                        }
                        else
                        {
                            result = defaultImages.UserAvatarBytes;
                            eTag = badEtag;
                        }
                    }
                    else if (IsApiIdForSong(apiId) || IsApiIdForAlbum(apiId))
                    {
                        if (IsApiIdForSong(apiId))
                        {
                            // If it's a song get the album ApiKey and proceed to get Album cover
                            var songInfo = await DatabaseSongIdsInfoForSongApiKey(apiKey.Value, cancellationToken).ConfigureAwait(false);
                            if (songInfo != null)
                            {
                                apiKey = songInfo.AlbumApiKey;
                            }
                        }

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
                                var pathToAlbum = dbConn.ExecuteScalar<string>(sql.FormatSmart(apiKey.Value.ToString())) ?? string.Empty;
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
                                    result = await File.ReadAllBytesAsync(image.FullName, cancellationToken).ConfigureAwait(false);
                                    eTag = melodeeFile!.Created.ToUnixTimeMilliseconds().ToString();
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
                                    var firstFrontImage = imagesForFolder.FirstOrDefault(x => string.Equals(x.Name, $" {ImageInfo.ImageFilePrefix}01-front.jpg", StringComparison.OrdinalIgnoreCase));
                                    if (firstFrontImage != null)
                                    {
                                        result = await File.ReadAllBytesAsync(firstFrontImage.FullName, cancellationToken).ConfigureAwait(false);
                                        eTag = firstFrontImage.LastWriteTimeUtc.Ticks.ToString();
                                    }
                                }
                            }
                        }

                        if (result == null)
                        {
                            result = defaultImages.AlbumCoverBytes;
                            eTag = badEtag;
                        }
                    }

                    if (result != null)
                    {
                        if (!string.Equals(sizeValue, ImageSizeRegistry.Large, StringComparison.OrdinalIgnoreCase))
                        {
                            var sizeValueParsed = SafeParser.ToNumber<int>(sizeValue);
                            if (sizeValueParsed > 0)
                            {
                                result = ImageConvertor.ResizeImageIfNeeded(result,
                                    sizeValueParsed,
                                    sizeValueParsed, isUserImageRequest);
                                eTag = HashHelper.CreateMd5(eTag + sizeValueParsed);
                            }
                            else
                            {
                                switch (sizeValue.ToLowerInvariant())
                                {
                                    case ImageSizeRegistry.Small:
                                        var smallSize = (await Configuration.Value).GetValue<int?>(SettingRegistry.ImagingSmallSize) ?? throw new Exception($"Invalid configuration [{SettingRegistry.ImagingSmallSize}] not found.");
                                        result = ImageConvertor.ResizeImageIfNeeded(result,
                                            smallSize,
                                            smallSize,
                                            isUserImageRequest);
                                        eTag = HashHelper.CreateMd5(eTag + ImageSizeRegistry.Small);
                                        break;

                                    case ImageSizeRegistry.Medium:
                                        var mediumSize = (await Configuration.Value).GetValue<int?>(SettingRegistry.ImagingMediumSize) ?? throw new Exception($"Invalid configuration [{SettingRegistry.ImagingMediumSize}] not found.");
                                        result = ImageConvertor.ResizeImageIfNeeded(result,
                                            mediumSize,
                                            mediumSize,
                                            isUserImageRequest);
                                        eTag = HashHelper.CreateMd5(eTag + ImageSizeRegistry.Medium);
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Failed to get cover image for [{ApiId}]", apiId);
                }

                return new ImageBytesAndEtag(result, eTag);
            }
        }, cancellationToken, new TimeSpan(0, 0, 120, 0), ImageCacheRegion);

        return new ResponseModel
        {
            ApiKeyId = apiId,
            IsSuccess = imageBytesAndEtag.Bytes != null,
            UserInfo = authResponse.UserInfo,
            ResponseData = authResponse.ResponseData with
            {
                Data = imageBytesAndEtag.Bytes,
                DataPropertyName = string.Empty,
                DataDetailPropertyName = string.Empty,
                Etag = imageBytesAndEtag.Etag,
                ContentType = "image/jpeg"
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
            ResponseData = await NewApiResponse(true, string.Empty, string.Empty)
        };
        var data = new List<OpenSubsonicExtension>
        {
            // Custom extensions added for Melodee
            new("melodeeExtensions", [1]),
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
            return authResponse with { UserInfo = BlankUserInfo };
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

    public async Task<ResponseModel> GetScanStatusAsync(ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
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
                    data = new ScanStatus(dataMap.GetString(JobMapNameRegistry.ScanStatus) == Enums.ScanStatus.InProcess.ToString(), dataMap.GetIntValue(JobMapNameRegistry.Count));
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

    /// <summary>
    ///     Test connectivity with the server.
    ///     <remarks>
    ///         This method does NOT do authentication as its only purpose is for a 'test' for the consumer to get a result
    ///         from the server.
    ///     </remarks>
    /// </summary>
    public async Task<ResponseModel> PingAsync(ApiRequest apiRequest, CancellationToken cancellationToken = default)
    {
        return new ResponseModel
        {
            UserInfo = BlankUserInfo,
            ResponseData = await NewApiResponse(true, string.Empty, string.Empty)
        };
    }

    public async Task<ResponseModel> AuthenticateSubsonicApiAsync(ApiRequest apiRequest, CancellationToken cancellationToken = default)
    {
        if (!apiRequest.RequiresAuthentication)
        {
            return new ResponseModel
            {
                UserInfo = BlankUserInfo,
                ResponseData = await NewApiResponse(true, string.Empty, string.Empty)
            };
        }

        if (apiRequest.Username?.Nullify() == null ||
            (apiRequest.Password?.Nullify() == null &&
             apiRequest.Token?.Nullify() == null))
        {
            Logger.Warning("[{MethodName}] [{ApiRequest}] is invalid",
                nameof(AuthenticateSubsonicApiAsync),
                apiRequest.ToString());
            return new ResponseModel
            {
                UserInfo = new UserInfo(0, Guid.Empty, string.Empty, string.Empty, string.Empty, string.Empty),
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
                    var unknownOrLocked = user.Data is { IsLocked: true } ? "Locked" : "Unknown";
                    Logger.Warning("{LockedOrUnknown} user [{Username}] attempted to authenticate with [{Client}]",
                        unknownOrLocked, apiRequest.Username, apiRequest.ApiRequestPlayer);
                }
                else
                {
                    bool isAuthenticated;
                    var authUsingToken = apiRequest.Token?.Nullify() != null;
                    var usersPassword = user.Data?.Decrypt(user.Data.PasswordEncrypted, await Configuration.Value);
                    var apiRequestPassword = apiRequest.Password;
                    if (apiRequest.Password?.StartsWith("enc:", StringComparison.Ordinal) ?? false)
                    {
                        apiRequestPassword = apiRequestPassword?.FromHexString();
                    }

                    if (apiRequest.Jwt.Nullify() != null)
                    {
                        // TODO is this something used by systems other than Navidrome?
                        // see https://github.com/navidrome/navidrome/blob/acce3c97d5dcf22a005a46d855bb1763a8bb8b66/server/subsonic/middlewares.go#L132
                        throw new NotImplementedException();
                    }

                    if (authUsingToken)
                    {
                        var userMd5 = HashHelper.CreateMd5($"{usersPassword}{apiRequest.Salt}");
                        isAuthenticated = string.Equals(userMd5, apiRequest.Token, StringComparison.OrdinalIgnoreCase);

                        if (!isAuthenticated)
                        {
                            Logger.Warning("[{MethodName}] user client [{Client}] attempted token auth, provided salt [{Salt}] token [{Token}] did not match generated md5 [{Md5}]",
                                nameof(AuthenticateSubsonicApiAsync),
                                apiRequest.ApiRequestPlayer.Client,
                                apiRequest.Salt,
                                apiRequest.Token,
                                userMd5);
                        }
                    }
                    else
                    {
                        isAuthenticated = usersPassword == apiRequestPassword;
                    }

                    if (isAuthenticated)
                    {
                        await bus.SendLocal(new UserLoginEvent(user.Data!.Id, user.Data.UserName)).ConfigureAwait(false);
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
            return authResponse with { UserInfo = BlankUserInfo };
        }

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var user = await userService.GetByUsernameAsync(apiRequest.Username!, cancellationToken).ConfigureAwait(false);
            var usersPlayQues = await scopedContext
                .PlayQues.Include(x => x.Song).ThenInclude(x => x.AlbumDisc).ThenInclude(x => x.Album).ThenInclude(x => x.Artist)
                .Where(x => x.UserId == user.Data!.Id)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            var current = usersPlayQues.FirstOrDefault(x => x.IsCurrentSong);
            var data = new PlayQueue
            {
                Current = current?.PlayQueId ?? 0,
                Position = current?.Position ?? 0,
                ChangedBy = current?.ChangedBy ?? user.Data!.UserName,
                Changed = current?.LastUpdatedAt.ToString() ?? string.Empty,
                Username = user.Data!.UserName,
                Entry = usersPlayQues.Select(x => x.Song.ToApiChild(x.Song.AlbumDisc.Album, null)).ToArray()
            };

            return new ResponseModel
            {
                UserInfo = BlankUserInfo,
                ResponseData = authResponse.ResponseData with
                {
                    Data = current == null ? null : data,
                    DataPropertyName = current == null ? string.Empty : "playQueue"
                }
            };
        }
    }

    public async Task<ResponseModel> SavePlayQueueAsync(string[]? apiIds, string? currentApiId, double? position, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        bool result;
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
                var user = await userService.GetByUsernameAsync(apiRequest.Username!, cancellationToken).ConfigureAwait(false);
                var usersPlayQues = await scopedContext
                    .PlayQues.Include(x => x.Song)
                    .Where(x => x.UserId == user.Data!.Id)
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);
                var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
                var changedByValue = apiRequest.ApiRequestPlayer.Client ?? user.Data!.UserName;
                if (usersPlayQues.Length > 0)
                {
                    foreach (var userPlay in usersPlayQues)
                    {
                        if (!apiKeys.Contains(userPlay.Song.ApiKey))
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

                var addedPlayQues = new List<dbModels.PlayQueue>();
                foreach (var apiKeyToAdd in apiKeys.Except(foundQuesSongApiKeys))
                {
                    var song = await scopedContext.Songs.FirstOrDefaultAsync(x => x.ApiKey == apiKeyToAdd, cancellationToken).ConfigureAwait(false);
                    if (song != null)
                    {
                        addedPlayQues.Add(new dbModels.PlayQueue
                        {
                            PlayQueId = addedPlayQues.Count + 1,
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
            return authResponse with { UserInfo = BlankUserInfo };
        }

        if (times?.Length > 0 && times.Length != ids.Length)
        {
            return new ResponseModel
            {
                UserInfo = BlankUserInfo,
                ResponseData = await NewApiResponse(false, string.Empty, string.Empty, Error.GenericError("Wrong number of timestamps."))
            };
        }

        await scrobbleService.InitializeAsync(await Configuration.Value, cancellationToken).ConfigureAwait(false);

        // If not provided then default to this is a "submission" versus a "now playing" notification.
        var isSubmission = submission ?? true;

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
                    await scrobbleService.Scrobble(authResponse.UserInfo, id, time, false, apiRequest.ApiRequestPlayer.Client ?? string.Empty, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    Logger.Debug("Scrobble: Ignoring duplicate scrobble submission for [{UniqueId}]", uniqueId);
                }
            }
        }

        var result = true;
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
        var range = apiRequest.RequestHeaders.FirstOrDefault(x => x.Key == "Range")?.Value ?? string.Empty;
        if (!request.IsDownloadingRequest && range.Nullify() != null)
        {
            if (string.Equals(range, "bytes=0-", StringComparison.OrdinalIgnoreCase))
            {
                long.TryParse(range, out rangeBegin);
            }
            else
            {
                var rangeParts = range.Split('-');
                long.TryParse(rangeParts[0], out rangeBegin);
                if (rangeParts.Length > 1)
                {
                    long.TryParse(rangeParts[1], out rangeEnd);
                }
            }
        }

        if (request.IsDownloadingRequest)
        {
            var isDownloadingEnabled = (await Configuration.Value).GetValue<bool?>(SettingRegistry.SystemIsDownloadingEnabled) ?? false;
            if (!isDownloadingEnabled)
            {
                Logger.Warning("[{ServiceName}] Downloading is disabled [{SettingName}]. Request [{Request}",
                    nameof(OpenSubsonicApiService),
                    SettingRegistry.SystemIsDownloadingEnabled,
                    request);
                return new StreamResponse(new HeaderDictionary(), false, []);
            }
        }

        if (request is { IsDownloadingRequest: false, TimeOffset: not null })
        {
            // TODO If specified, start streaming at the given offset (in seconds) into the media. 
            Logger.Warning("[{ServiceName}] Stream request has TimeOffset. Request [{Request}", nameof(OpenSubsonicApiService), request);
        }

        var sql = """
                  select l."Path" || aa."Directory" || a."Directory" || s."FileName" as Path, s."FileSize", s."Duration"/1000 as "Duration",s."BitRate", s."ContentType"
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
                Logger.Warning("[{ServiceName}] Stream request for song that was not found. User [{ApiRequest}] Request [{Request}]", nameof(OpenSubsonicApiService), apiRequest.ToString(), request.ToString());
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

            if (request.IsTranscodingRequest)
            {
                if (request.MaxBitRate != songStreamInfo.BitRate)
                {
                    //TODO transcoding for the format and maxBitRate as needed
                    Logger.Warning("[{ServiceName}] Stream request has MaxBitRate [{MaxBitRate}] different than song BitRate [{SongRate}] has TimeOffset. Request [{Request}",
                        nameof(OpenSubsonicApiService),
                        request.MaxBitRate,
                        songStreamInfo.BitRate,
                        request);
                }
            }

            var numberOfBytesRead = 0;
            await using (var fs = songStreamInfo.TrackFileInfo.OpenRead())
            {
                try
                {
                    fs.Seek(rangeBegin, SeekOrigin.Begin);
                    numberOfBytesRead = await fs.ReadAsync(trackBytes.AsMemory(0, bytesToRead), cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Reading song [{SongInfo}]", songStreamInfo);
                }
            }

            if (request.IsDownloadingRequest)
            {
                Logger.Information("[{ServiceName}] User downloaded song [{SongId}]", nameof(OpenSubsonicApiService), request.Id);
            }

            return new StreamResponse
            (
                new Dictionary<string, StringValues>
                {
                    { "Accept-Ranges", "bytes" },
                    { "Cache-Control", "no-store, must-revalidate, no-cache, max-age=0" },
                    { "Content-Duration", songStreamInfo.Duration.ToString(CultureInfo.InvariantCulture) },
                    { "Content-Length", numberOfBytesRead.ToString() },
                    { "Content-Range", $"bytes {rangeBegin}-{rangeEnd}/{numberOfBytesRead}" },
                    { "Content-Type", songStreamInfo.ContentType },
                    { "Expires", "Mon, 01 Jan 1990 00:00:00 GMT" }
                },
                numberOfBytesRead > 0,
                trackBytes,
                request.IsDownloadingRequest ? songStreamInfo.TrackFileInfo.Name : null,
                request.IsDownloadingRequest ? songStreamInfo.ContentType : null
            );
        }
    }

    public async Task<ResponseModel> GetNowPlayingAsync(ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
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
                data.Add(nowPlayingSong.ToApiChild(album, userSong, nowPlaying.Data.FirstOrDefault(x => x.UniqueId == nowPlayingSongUniqueId)));
            }
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,

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
            return authResponse with { UserInfo = BlankUserInfo };
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
                    { "artistOffset", request.ArtistOffset ?? 0 },
                    { "artistCount", request.ArtistCount ?? defaultPageSize },
                    { "albumOffset", request.AlbumOffset ?? 0 },
                    { "albumCount", request.AlbumCount ?? defaultPageSize },
                    { "songOffset", request.SongOffset ?? 0 },
                    { "songCount", request.SongCount ?? defaultPageSize }
                };
                var sql = """
                          select 'artist_' || "ApiKey"::varchar as "Id", "Name", 'artist_' || "ApiKey" as "CoverArt", "AlbumCount"
                          from "Artists" a
                          where a."NameNormalized" like @normalizedQuery
                          or a."AlternateNames" like @normalizedQuery
                          ORDER BY a."SortName" OFFSET @artistOffset ROWS FETCH NEXT @artistCount ROWS ONLY;
                          """;
                artists = (await dbConn
                    .QueryAsync<ArtistSearchResult>(sql, sqlParameters)
                    .ConfigureAwait(false)).ToArray();

                sql = """
                      select 'album_' || a."ApiKey"::varchar as "Id", a."Name", 'album_' || a."ApiKey"::varchar as "CoverArt", a."SongCount", a."CreatedAt", 
                             a."Duration" as "DurationMs", 'artist_' || aa."ApiKey"::varchar as "ArtistId", aa."Name" as "Artist",a."Genres"  
                      from "Albums" a
                      left join "Artists" aa on (a."ArtistId" = aa."Id")
                      where a."NameNormalized"  like @normalizedQuery
                      or a."AlternateNames" like @normalizedQuery
                      ORDER BY a."SortName" OFFSET @albumOffset ROWS FETCH NEXT @albumCount ROWS ONLY;
                      """;
                albums = (await dbConn
                    .QueryAsync<AlbumSearchResult>(sql, sqlParameters)
                    .ConfigureAwait(false)).ToArray();

                sql = """
                      select 'song_' || s."ApiKey"::varchar as "Id", a."ApiKey"::varchar as Parent, s."Title", a."Name" as Album, aa."Name" as "Artist", 'song_' || s."ApiKey"::varchar as "CoverArt", 
                             a."SongCount", s."CreatedAt", s."Duration" as "DurationMs", s."BitRate", s."SongNumber" as "Track", 
                             DATE_PART('year', a."ReleaseDate"::date) as "Year", a."Genres", s."FileSize" as "Size", 
                             s."ContentType", l."Path" || aa."Directory" || a."Directory" || s."FileName" as "Path", RIGHT(s."FileName", 3) as "Suffix", 'album_' ||a."ApiKey"::varchar as "AlbumId", 
                             'artist_' || aa."ApiKey"::varchar as "ArtistId", aa."Name" as "Artist"    
                      from "Songs" s
                      join "AlbumDiscs" ad on (s."AlbumDiscId" = ad."Id")
                      join "Albums" a on (ad."AlbumId" = a."Id")
                      join "Artists" aa on (a."ArtistId" = aa."Id")
                      join "Libraries" l on (aa."LibraryId" = l."Id")
                      where s."TitleNormalized"  like @normalizedQuery
                      or s."AlternateNames" like @normalizedQuery
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
            ResponseData = await DefaultApiResponse() with
            {
                Data = new Search3Result(artists, albums, songs),
                DataPropertyName = apiRequest.IsXmlRequest ? string.Empty : "searchResult3"
            }
        };
    }


    public async Task<ResponseModel> GetMusicDirectoryAsync(string apiId, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
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
                        artistInfo.CalculatedRating,
                        artistInfo.PlayCount,
                        artistInfo.Played.ToString(),
                        artistAlbums.Select(x => x.ToApiChild(x.UserAlbums.FirstOrDefault())).ToArray());
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
                            albumSongs.Select(x => x.ToApiChild(x.AlbumDisc.Album, x.UserSongs.FirstOrDefault())).ToArray());
                    }
                }
            }
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,

            ResponseData = await DefaultApiResponse() with
            {
                Data = data,
                DataPropertyName = "directory"
            }
        };
    }


    public async Task<ResponseModel> GetIndexesAsync(bool isArtistIndex, string dataPropertyName, Guid? musicFolderId, long? ifModifiedSince, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        var indexLimit = (await Configuration.Value).GetValue<short>(SettingRegistry.OpenSubsonicIndexesArtistLimit);
        if (indexLimit == 0)
        {
            indexLimit = short.MaxValue;
        }

        object? data;
        var libraryId = 0;
        var lastModified = string.Empty;
        if (musicFolderId.HasValue)
        {
            var libraryResult = await libraryService.ListAsync(new PagedRequest(), cancellationToken).ConfigureAwait(false);
            var library = libraryResult.Data.FirstOrDefault(x => x.ApiKey == musicFolderId.Value);
            libraryId = library?.Id ?? 0;
            lastModified = library?.LastUpdatedAt.ToString() ?? string.Empty;
        }

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var sql = """
                      select a."Id", a."ApiKey", LEFT(a."SortName", 1) as "Index", a."Name", 'artist_' || a."ApiKey" as "CoverArt", 
                             a."CalculatedRating", a."AlbumCount", a."PlayedCount" as "PlayCount", a."CreatedAt" as "CreatedAt", a."LastUpdatedAt" as "LastUpdatedAt", a."LastPlayedAt" as "Played", a."Directory",
                             (SELECT ua."StarredAt" FROM "UserArtists" ua WHERE a."Id" = ua."ArtistId" and ua."UserId" = @userId and ua."IsStarred") as "UserStarred", 
                             (SELECT ua."Rating" FROM "UserArtists" ua WHERE a."Id" = ua."ArtistId" and ua."UserId" = @userId) as "UserRating"
                      from "Artists" a
                      where ((@libraryId = 0) or (@libraryId > 0 and a."LibraryId" = @libraryId))
                      and (EXTRACT(EPOCH from a."LastUpdatedAt") >= 0)
                      order by a."SortOrder", a."SortName"
                      """;
            var indexes = await dbConn.QueryAsync<DatabaseDirectoryInfo>(sql, new { libraryId, modifiedSince = ifModifiedSince ?? 0, userId = authResponse.UserInfo.Id }).ConfigureAwait(false);

            var configuration = await Configuration.Value;

            var artists = new List<ArtistIndex>();
            foreach (var grouped in indexes.GroupBy(x => x.Index))
            {
                var aa = new List<Artist>();
                foreach (var info in grouped)
                {
                    aa.Add(new Artist(info.CoverArt,
                        info.Name,
                        info.AlbumCount,
                        info.UserRatingValue,
                        info.CalculatedRating,
                        info.CoverArt,
                        configuration.GetBuildImageUrl(info.CoverArt, ImageSize.Large),
                        info.UserStarred?.ToString()));
                }

                artists.Add(new ArtistIndex(grouped.Key, aa.Take(indexLimit).ToArray()));
            }

            if (!isArtistIndex)
            {
                data = new Indexes(
                    (await Configuration.Value).GetValue<string>(SettingRegistry.ProcessingIgnoredArticles) ?? string.Empty, lastModified,
                    [],
                    artists.ToArray(),
                    []);
            }
            else
            {
                data = new Artists(
                    (await Configuration.Value).GetValue<string>(SettingRegistry.ProcessingIgnoredArticles) ?? string.Empty, lastModified,
                    artists.ToArray());
            }
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            ResponseData = await DefaultApiResponse() with
            {
                Data = data,
                DataPropertyName = apiRequest.IsXmlRequest ? string.Empty : dataPropertyName
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
            return authResponse with { UserInfo = BlankUserInfo };
        }

        NamedInfo[] data = [];

        var libraryResult = await libraryService.ListAsync(new PagedRequest(), cancellationToken).ConfigureAwait(false);
        if (libraryResult.IsSuccess)
        {
            data = libraryResult.Data.Where(x => x.TypeValue == LibraryType.Storage).Select(x => new NamedInfo(x.ToApiKey(), x.Name)).ToArray();
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,

            ResponseData = await DefaultApiResponse() with
            {
                Data = data,
                DataPropertyName = "musicFolders",
                DataDetailPropertyName = "musicFolder"
            }
        };
    }

    public async Task<ResponseModel> GetArtistAsync(string id, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        Artist? data = null;

        var apiKey = ApiKeyFromId(id);
        if (apiKey != null)
        {
            var artistInfo = await DatabaseArtistInfoForArtistApiKey(apiKey.Value, authResponse.UserInfo.Id, cancellationToken).ConfigureAwait(false);
            if (artistInfo != null)
            {
                var configuration = await Configuration.Value;
                data = new Artist(
                    id,
                    artistInfo.Name,
                    artistInfo.AlbumCount,
                    artistInfo.UserRating,
                    artistInfo.CalculatedRating,
                    artistInfo.CoverArt,
                    configuration.GetBuildImageUrl(id, ImageSize.Large),
                    artistInfo.UserStarred?.ToString(),
                    await AlbumListForArtistApiKey(apiKey.Value, authResponse.UserInfo.Id, cancellationToken).ConfigureAwait(false));
            }
            else
            {
                Logger.Warning("[{MethodName}] invalid artist id [{Id}] ApiRequest [{ApiRequest}]", nameof(GetArtistAsync), id, apiRequest.ToString());
            }
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            TotalCount = data?.AlbumCount ?? 0,
            ResponseData = await DefaultApiResponse() with
            {
                Data = data,
                DataPropertyName = apiRequest.IsXmlRequest ? string.Empty : "artist"
            }
        };
    }

    private async Task<bool> ToggleStar(int userId, bool isStarred, string? id, string? albumId, string? artistId, CancellationToken cancellationToken)
    {
        var result = false;
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var idValue = id ?? albumId ?? artistId;
            var apiKey = ApiKeyFromId(idValue);
            if (apiKey != null)
            {
                var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
                if (IsApiIdForArtist(idValue))
                {
                    var artist = await artistService.GetByApiKeyAsync(apiKey.Value, cancellationToken).ConfigureAwait(false);
                    if (artist.Data != null)
                    {
                        var userArtist = await scopedContext.UserArtists.FirstOrDefaultAsync(x => x.UserId == userId && x.ArtistId == artist.Data.Id, cancellationToken).ConfigureAwait(false);
                        if (userArtist == null)
                        {
                            userArtist = new dbModels.UserArtist
                            {
                                UserId = userId,
                                ArtistId = artist.Data.Id,
                                CreatedAt = now
                            };
                            scopedContext.UserArtists.Add(userArtist);
                        }

                        userArtist.StarredAt = isStarred ? now : null;
                        userArtist.IsStarred = isStarred;
                        userArtist.LastUpdatedAt = now;
                        result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
                    }
                }

                if (IsApiIdForAlbum(idValue))
                {
                    var album = await albumService.GetByApiKeyAsync(apiKey.Value, cancellationToken).ConfigureAwait(false);
                    if (album.Data != null)
                    {
                        var userAlbum = await scopedContext.UserAlbums.FirstOrDefaultAsync(x => x.UserId == userId && x.AlbumId == album.Data.Id, cancellationToken).ConfigureAwait(false);
                        if (userAlbum == null)
                        {
                            userAlbum = new dbModels.UserAlbum
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
                        userAlbum.LastUpdatedAt = now;
                        result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
                    }
                }

                if (IsApiIdForSong(idValue))
                {
                    var song = await songService.GetByApiKeyAsync(apiKey.Value, cancellationToken).ConfigureAwait(false);
                    if (song.Data != null)
                    {
                        var userSong = await scopedContext.UserSongs.FirstOrDefaultAsync(x => x.UserId == userId && x.SongId == song.Data.Id, cancellationToken).ConfigureAwait(false);
                        if (userSong == null)
                        {
                            userSong = new dbModels.UserSong
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
                        userSong.LastUpdatedAt = now;
                        result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    ///     Toggles a star to a song, album or artist.
    /// </summary>
    public async Task<ResponseModel> ToggleStarAsync(bool isStarred, string? id, string? albumId, string? artistId, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        var result = await ToggleStar(authResponse.UserInfo.Id, isStarred, id, albumId, artistId, cancellationToken).ConfigureAwait(false);

        return new ResponseModel
        {
            UserInfo = BlankUserInfo,
            IsSuccess = result,
            ResponseData = await NewApiResponse(result, string.Empty, string.Empty, result ? null : Error.InvalidApiKeyError)
        };
    }

    /// <summary>
    ///     Sets the rating for a music file.
    /// </summary>
    public async Task<ResponseModel> SetRatingAsync(string id, int rating, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        var result = false;
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var apiKey = ApiKeyFromId(id);
            if (apiKey != null)
            {
                var now = Instant.FromDateTimeUtc(DateTime.UtcNow);

                if (IsApiIdForSong(id))
                {
                    var song = await songService.GetByApiKeyAsync(apiKey.Value, cancellationToken).ConfigureAwait(false);
                    if (song.Data != null)
                    {
                        var userSong = await scopedContext.UserSongs.FirstOrDefaultAsync(x => x.UserId == authResponse.UserInfo.Id && x.SongId == song.Data.Id, cancellationToken).ConfigureAwait(false);
                        if (userSong == null)
                        {
                            userSong = new dbModels.UserSong
                            {
                                UserId = authResponse.UserInfo.Id,
                                SongId = song.Data.Id,
                                CreatedAt = now
                            };
                            scopedContext.UserSongs.Add(userSong);
                        }

                        userSong.Rating = rating;
                        userSong.LastUpdatedAt = now;
                        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        var sql = """
                                  update "Songs" s set 
                                    "LastUpdatedAt" = now(), 
                                    "CalculatedRating" = (select coalesce(avg("Rating"),0) from "UserSongs" where "SongId" = s."Id")
                                  where s."Id" = @dbId;
                                  """;
                        await dbConn.ExecuteAsync(sql, new { dbId = userSong.SongId }).ConfigureAwait(false);
                        await songService.ClearCacheAsync(userSong.SongId, cancellationToken).ConfigureAwait(false);
                        result = true;
                    }
                }
                else if (IsApiIdForAlbum(id))
                {
                    var album = await albumService.GetByApiKeyAsync(apiKey.Value, cancellationToken).ConfigureAwait(false);
                    if (album.Data != null)
                    {
                        var userAlbum = await scopedContext.UserAlbums.FirstOrDefaultAsync(x => x.UserId == authResponse.UserInfo.Id && x.AlbumId == album.Data.Id, cancellationToken).ConfigureAwait(false);
                        if (userAlbum == null)
                        {
                            userAlbum = new dbModels.UserAlbum
                            {
                                UserId = authResponse.UserInfo.Id,
                                AlbumId = album.Data.Id,
                                CreatedAt = now
                            };
                            scopedContext.UserAlbums.Add(userAlbum);
                        }

                        userAlbum.Rating = rating;
                        userAlbum.LastUpdatedAt = now;
                        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        var sql = """
                                  update "Albums" a set 
                                    "LastUpdatedAt" = now(), 
                                    "CalculatedRating" = (select coalesce(avg("Rating"),0) from "UserAlbums" where "AlbumId" = a."Id")
                                  where a."Id" = @dbId;
                                  """;
                        await dbConn.ExecuteAsync(sql, new { dbId = userAlbum.AlbumId }).ConfigureAwait(false);
                        await albumService.ClearCacheAsync(userAlbum.AlbumId, cancellationToken).ConfigureAwait(false);
                        result = true;
                    }
                }
                else if (IsApiIdForArtist(id))
                {
                    var artist = await artistService.GetByApiKeyAsync(apiKey.Value, cancellationToken).ConfigureAwait(false);
                    if (artist.Data != null)
                    {
                        var userArtist = await scopedContext.UserArtists.FirstOrDefaultAsync(x => x.UserId == authResponse.UserInfo.Id && x.ArtistId == artist.Data.Id, cancellationToken).ConfigureAwait(false);
                        if (userArtist == null)
                        {
                            userArtist = new dbModels.UserArtist
                            {
                                UserId = authResponse.UserInfo.Id,
                                ArtistId = artist.Data.Id,
                                CreatedAt = now
                            };
                            scopedContext.UserArtists.Add(userArtist);
                        }

                        userArtist.Rating = rating;
                        userArtist.LastUpdatedAt = now;
                        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                        var sql = """
                                  update "Artists" a set 
                                    "LastUpdatedAt" = now(), 
                                    "CalculatedRating" = (select coalesce(avg("Rating"),0) from "UserArtists" where "ArtistId" = a."Id")
                                  where a."Id" = @dbId;
                                  """;
                        await dbConn.ExecuteAsync(sql, new { dbId = userArtist.ArtistId }).ConfigureAwait(false);
                        await artistService.ClearCacheAsync(userArtist.ArtistId, cancellationToken).ConfigureAwait(false);
                        result = true;
                    }
                }
            }
        }

        return new ResponseModel
        {
            UserInfo = BlankUserInfo,
            IsSuccess = result,
            ResponseData = await NewApiResponse(result, string.Empty, string.Empty, result ? null : Error.InvalidApiKeyError)
        };
    }

    public async Task<ResponseModel> GetTopSongsAsync(string artist, int? count, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        Child[]? data;

        await artistSearchEngineService.InitializeAsync(await Configuration.Value, cancellationToken).ConfigureAwait(false);

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var artistId = await scopedContext.Artists.Where(x => x.Name == artist).Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);
            var topSongsResult = await artistSearchEngineService.DoArtistTopSongsSearchAsync(artist, artistId, count, cancellationToken).ConfigureAwait(false);
            var songIds = topSongsResult.Data.Where(x => x.Id != null).Select(x => x.Id).ToArray();
            var songs = await scopedContext
                .Songs.Include(x => x.AlbumDisc).ThenInclude(x => x.Album).ThenInclude(x => x.Artist)
                .Include(x => x.UserSongs.Where(us => us.UserId == authResponse.UserInfo.Id))
                .Where(x => songIds.Contains(x.Id)).ToArrayAsync(cancellationToken).ConfigureAwait(false);
            data = (from s in songs
                    join tsr in topSongsResult.Data on s.Id equals tsr.Id
                    orderby tsr.SortOrder
                    select s
                ).Select(x => x.ToApiChild(x.AlbumDisc.Album, x.UserSongs.FirstOrDefault())).ToArray();
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,

            ResponseData = await DefaultApiResponse() with
            {
                Data = data,
                DataPropertyName = "topSongs",
                DataDetailPropertyName = "song"
            }
        };
    }

    public async Task<ResponseModel> GetStarred2Async(string? musicFolderId, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        ArtistID3[] artists;
        AlbumID3[] albums;
        Child[] songs;

        var indexLimit = (await Configuration.Value).GetValue<short>(SettingRegistry.OpenSubsonicIndexesArtistLimit);
        if (indexLimit == 0)
        {
            indexLimit = short.MaxValue;
        }

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var userStarredArtists = await scopedContext
                .UserArtists.Include(x => x.Artist)
                .Where(x => x.UserId == authResponse.UserInfo.Id && x.IsStarred)
                .OrderBy(x => x.Id)
                .Take(indexLimit)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            artists = userStarredArtists.Select(x => x.Artist.ToApiArtistID3(x)).ToArray();

            var userStarredAlbums = await scopedContext
                .UserAlbums.Include(x => x.Album).ThenInclude(x => x.Artist)
                .Where(x => x.UserId == authResponse.UserInfo.Id && x.IsStarred)
                .OrderBy(x => x.Id)
                .Take(indexLimit)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            albums = userStarredAlbums.Select(x => x.Album.ToArtistID3(x, null)).ToArray();

            var userStarredSongs = await scopedContext
                .UserSongs.Include(x => x.Song).ThenInclude(x => x.AlbumDisc).ThenInclude(x => x.Album).ThenInclude(x => x.Artist)
                .Where(x => x.UserId == authResponse.UserInfo.Id && x.IsStarred)
                .OrderBy(x => x.Id)
                .Take(indexLimit)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            songs = userStarredSongs.Select(x => x.Song.ToApiChild(x.Song.AlbumDisc.Album, x)).ToArray();
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            ResponseData = await DefaultApiResponse() with
            {
                Data = new StarredInfo2(artists, albums, songs),
                DataPropertyName = "starred2"
            }
        };
    }

    public async Task<ResponseModel> GetStarredAsync(string? musicFolderId, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        Artist[] artists;
        Child[] albums;
        Child[] songs;

        var indexLimit = (await Configuration.Value).GetValue<short>(SettingRegistry.OpenSubsonicIndexesArtistLimit);
        if (indexLimit == 0)
        {
            indexLimit = short.MaxValue;
        }

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var userStarredArtists = await scopedContext
                .UserArtists.Include(x => x.Artist)
                .Where(x => x.UserId == authResponse.UserInfo.Id && x.IsStarred)
                .OrderBy(x => x.Id)
                .Take(indexLimit)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            artists = userStarredArtists.Select(x => x.Artist.ToApiArtist(x)).ToArray();

            var userStarredAlbums = await scopedContext
                .UserAlbums.Include(x => x.Album).ThenInclude(x => x.Artist)
                .Where(x => x.UserId == authResponse.UserInfo.Id && x.IsStarred)
                .OrderBy(x => x.Id)
                .Take(indexLimit)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            albums = userStarredAlbums.Select(x => x.Album.ToApiChild(x)).ToArray();

            var userStarredSongs = await scopedContext
                .UserSongs.Include(x => x.Song).ThenInclude(x => x.AlbumDisc).ThenInclude(x => x.Album).ThenInclude(x => x.Artist)
                .Where(x => x.UserId == authResponse.UserInfo.Id && x.IsStarred)
                .OrderBy(x => x.Id)
                .Take(indexLimit)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            songs = userStarredSongs.Select(x => x.Song.ToApiChild(x.Song.AlbumDisc.Album, x)).ToArray();
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            ResponseData = await DefaultApiResponse() with
            {
                Data = new StarredInfo(artists, albums, songs),
                DataPropertyName = "starred"
            }
        };
    }

    public async Task<ResponseModel> GetSongsByGenreAsync(string genre, int? count, int? offset, string? musicFolderId, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        var indexLimit = (await Configuration.Value).GetValue<short>(SettingRegistry.OpenSubsonicIndexesArtistLimit);
        if (indexLimit == 0)
        {
            indexLimit = short.MaxValue;
        }

        long totalCount;
        Child[] songs;

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var totalCountSql = """
                                select COUNT(s."Id")
                                from "Songs" s
                                join "AlbumDiscs" ad on (s."AlbumDiscId" = ad."Id")
                                join "Albums" a on (ad."AlbumId" = a."Id")
                                join "Artists" aa on (a."ArtistId" = aa."Id")
                                where @genre = any(a."Genres") or @genre = any(s."Genres")
                                """;
            var sql = """
                      select s."Id"
                      from "Songs" s
                      join "AlbumDiscs" ad on (s."AlbumDiscId" = ad."Id")
                      join "Albums" a on (ad."AlbumId" = a."Id")
                      join "Artists" aa on (a."ArtistId" = aa."Id")
                      where @genre = any(a."Genres") or @genre = any(s."Genres")
                      offset @offset rows fetch next @takeSize rows only;
                      """;

            totalCount = await dbConn
                .ExecuteScalarAsync<long>(totalCountSql, new { genre })
                .ConfigureAwait(false);

            var dbSongIds = (await dbConn.QueryAsync<int>(sql, new { genre, offset, takeSize = count < indexLimit ? count : indexLimit }).ConfigureAwait(false)).ToArray();
            var dbSongs = await (from s in scopedContext.Songs
                    .Include(x => x.AlbumDisc).ThenInclude(x => x.Album).ThenInclude(x => x.Artist)
                    .Include(x => x.UserSongs.Where(ua => ua.UserId == authResponse.UserInfo.Id))
                join ss in dbSongIds on s.Id equals ss
                select s).ToArrayAsync(cancellationToken).ConfigureAwait(false);
            songs = dbSongs.Select(x => x.ToApiChild(x.AlbumDisc.Album, x.UserSongs.FirstOrDefault())).ToArray();
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            TotalCount = totalCount,
            ResponseData = await DefaultApiResponse() with
            {
                Data = songs,
                DataPropertyName = "songsByGenre",
                DataDetailPropertyName = "song"
            }
        };
    }

    public async Task<ResponseModel> GetBookmarksAsync(ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        Bookmark[] data = [];

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var userBookmarks = await scopedContext.Bookmarks
                .Include(x => x.Song).ThenInclude(x => x.AlbumDisc).ThenInclude(x => x.Album).ThenInclude(x => x.Artist)
                .Include(x => x.Song).ThenInclude(x => x.UserSongs.Where(ua => ua.UserId == authResponse.UserInfo.Id))
                .Where(x => x.UserId == authResponse.UserInfo.Id)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            if (userBookmarks.Any())
            {
                data = userBookmarks.Select(x => x.ToApiBookmark()).ToArray();
            }
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,
            ResponseData = await DefaultApiResponse() with
            {
                Data = data,
                DataPropertyName = "bookmarks",
                DataDetailPropertyName = "bookmark"
            }
        };
    }

    public async Task<ResponseModel> CreateBookmarkAsync(string id, int position, string? comment, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        var result = false;

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var apiKey = ApiKeyFromId(id);
            if (apiKey != null)
            {
                var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
                var songInfo = await DatabaseSongIdsInfoForSongApiKey(apiKey.Value, cancellationToken).ConfigureAwait(false);
                if (songInfo != null)
                {
                    var existingBookmark = await scopedContext
                        .Bookmarks
                        .FirstOrDefaultAsync(x => x.UserId == authResponse.UserInfo.Id && x.SongId == songInfo.SongId, cancellationToken)
                        .ConfigureAwait(false);
                    if (existingBookmark != null)
                    {
                        existingBookmark.LastUpdatedAt = now;
                        existingBookmark.Comment = comment;
                        existingBookmark.Position = position;
                    }
                    else
                    {
                        var newBookmark = new dbModels.Bookmark
                        {
                            CreatedAt = now,
                            UserId = authResponse.UserInfo.Id,
                            SongId = songInfo.SongId,
                            Comment = comment,
                            Position = position
                        };
                        scopedContext.Bookmarks.Add(newBookmark);
                    }

                    await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    result = true;
                }
            }
        }

        return new ResponseModel
        {
            UserInfo = BlankUserInfo,
            IsSuccess = result,
            ResponseData = await NewApiResponse(result, string.Empty, string.Empty, result ? null : Error.InvalidApiKeyError)
        };
    }

    public async Task<ResponseModel> DeleteBookmarkAsync(string id, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        var result = false;

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var apiKey = ApiKeyFromId(id);
            if (apiKey != null)
            {
                var songInfo = await DatabaseSongIdsInfoForSongApiKey(apiKey.Value, cancellationToken).ConfigureAwait(false);
                if (songInfo != null)
                {
                    var existingBookmark = await scopedContext
                        .Bookmarks
                        .FirstOrDefaultAsync(x => x.UserId == authResponse.UserInfo.Id && x.SongId == songInfo.SongId, cancellationToken)
                        .ConfigureAwait(false);
                    if (existingBookmark != null)
                    {
                        scopedContext.Bookmarks.Remove(existingBookmark);
                        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        result = true;
                    }
                }
            }
        }

        return new ResponseModel
        {
            UserInfo = BlankUserInfo,
            IsSuccess = result,
            ResponseData = await NewApiResponse(result, string.Empty, string.Empty, result ? null : Error.InvalidApiKeyError)
        };
    }

    public async Task<ResponseModel> GetArtistInfoAsync(string id, int? count, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        ArtistInfo? data = null;

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var apiKey = ApiKeyFromId(id);
            var artist = await scopedContext.Artists.FirstOrDefaultAsync(x => x.ApiKey == apiKey, cancellationToken).ConfigureAwait(false);
            if (artist != null)
            {
                var configuration = await Configuration.Value;

                // TODO How to get similar artists?
                Artist[]? similarArtists = null;

                data = new ArtistInfo(artist.ToApiKey(),
                    artist.Name,
                    configuration.GetBuildImageUrl(id, ImageSize.Thumbnail),
                    configuration.GetBuildImageUrl(id, ImageSize.Medium),
                    configuration.GetBuildImageUrl(id, ImageSize.Large),
                    artist.SongCount,
                    artist.AlbumCount,
                    artist.Biography,
                    artist.MusicBrainzId,
                    artist.LastFmId,
                    similarArtists);
            }
        }

        return new ResponseModel
        {
            UserInfo = authResponse.UserInfo,

            ResponseData = await DefaultApiResponse() with
            {
                Data = data,
                DataPropertyName = apiRequest.IsXmlRequest ? string.Empty : "artistInfo"
            }
        };
    }

    public async Task<ResponseModel> GetAlbumInfoAsync(string id, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        AlbumInfo? data = null;

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var apiKey = ApiKeyFromId(id);
            var album = await scopedContext.Albums.FirstOrDefaultAsync(x => x.ApiKey == apiKey, cancellationToken).ConfigureAwait(false);
            if (album != null)
            {
                var configuration = await Configuration.Value;

                data = new AlbumInfo(album.ToApiKey(),
                    album.Name,
                    configuration.GetBuildImageUrl(id, ImageSize.Thumbnail),
                    configuration.GetBuildImageUrl(id, ImageSize.Medium),
                    configuration.GetBuildImageUrl(id, ImageSize.Large),
                    album.SongCount,
                    1,
                    album.Notes,
                    album.MusicBrainzId);
            }
        }

        return new ResponseModel
        {
            IsSuccess = data != null,
            UserInfo = authResponse.UserInfo,
            ResponseData = await DefaultApiResponse() with
            {
                Data = data,
                DataPropertyName = apiRequest.IsXmlRequest ? string.Empty : "albumInfo"
            }
        };
    }

    public async Task<ResponseModel> GetUserAsync(string username, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        // Only users with admin privileges are allowed to call this method.
        var isUserAdmin = await userService.IsUserAdminAsync(authResponse.UserInfo.UserName, cancellationToken).ConfigureAwait(false);
        if (!isUserAdmin)
        {
            return new ResponseModel
            {
                UserInfo = BlankUserInfo,
                IsSuccess = false,
                ResponseData = await NewApiResponse(false, string.Empty, string.Empty, Error.UserNotAuthorizedError)
            };
        }

        User? data = null;
        var user = await userService.GetByUsernameAsync(username, cancellationToken).ConfigureAwait(false);
        if (user.IsSuccess)
        {
            data = user.Data!.ToApiUser();
        }

        return new ResponseModel
        {
            IsSuccess = data != null,
            UserInfo = authResponse.UserInfo,
            ResponseData = await DefaultApiResponse() with
            {
                Data = data,
                DataPropertyName = "user"
            }
        };
    }

    public async Task<ResponseModel> GetRandomSongsAsync(int size, string? genre, int? fromYear, int? toYear, string? musicFolderId, ApiRequest apiRequest, CancellationToken cancellationToken)
    {
        var authResponse = await AuthenticateSubsonicApiAsync(apiRequest, cancellationToken);
        if (!authResponse.IsSuccess)
        {
            return authResponse with { UserInfo = BlankUserInfo };
        }

        Child[]? songs;

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var indexLimit = (await Configuration.Value).GetValue<short>(SettingRegistry.OpenSubsonicIndexesArtistLimit);
            if (indexLimit == 0)
            {
                indexLimit = short.MaxValue;
            }

            var dbConn = scopedContext.Database.GetDbConnection();
            var sql = """
                      select s."Id"
                      from "Songs" s
                      join "AlbumDiscs" ad on (s."AlbumDiscId" = ad."Id")
                      join "Albums" a on (ad."AlbumId" = a."Id")
                      join "Artists" aa on (a."ArtistId" = aa."Id")
                      where (@genre = any(a."Genres") or @genre = any(s."Genres") or @genre = '')
                      and (DATE_PART('year', a."ReleaseDate"::date) between @fromYear and @toYear)
                      order by Random()
                      offset 0 rows fetch next @takeSize rows only;
                      """;

            var dbSongIds = (await dbConn.QueryAsync<int>(sql,
                    new
                    {
                        genre = genre ?? string.Empty,
                        takeSize = size < indexLimit ? size : indexLimit,
                        fromYear = fromYear ?? 0,
                        toYear = toYear ?? 9999
                    })
                .ConfigureAwait(false)).ToArray();
            var dbSongs = await (from s in scopedContext.Songs
                    .Include(x => x.AlbumDisc).ThenInclude(x => x.Album).ThenInclude(x => x.Artist)
                    .Include(x => x.UserSongs.Where(ua => ua.UserId == authResponse.UserInfo.Id))
                join ss in dbSongIds on s.Id equals ss
                select s).ToArrayAsync(cancellationToken).ConfigureAwait(false);
            songs = dbSongs.Select(x => x.ToApiChild(x.AlbumDisc.Album, x.UserSongs.FirstOrDefault())).ToArray();
        }

        return new ResponseModel
        {
            IsSuccess = songs.Length > 0,
            UserInfo = authResponse.UserInfo,
            ResponseData = await DefaultApiResponse() with
            {
                Data = songs,
                DataPropertyName = "randomSongs",
                DataDetailPropertyName = "song"
            }
        };
    }

    private record ImageBytesAndEtag(byte[]? Bytes, string? Etag);
}
