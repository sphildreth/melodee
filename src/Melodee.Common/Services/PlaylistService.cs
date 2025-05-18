using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Collection;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Serialization;
using Melodee.Common.Services.Interfaces;
using Melodee.Common.Utility;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using SmartFormat;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Common.Services;

public class PlaylistService(
    ILogger logger,
    ICacheManager cacheManager,
    ISerializer serializer,
    IMelodeeConfigurationFactory configurationFactory,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    LibraryService libraryService)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:playlist:apikey:{0}";
    private const string CacheKeyDetailTemplate = "urn:playlist:{0}";

    private async Task ClearCacheAsync(int playlistId, CancellationToken cancellationToken = default)
    {
        var playlist = await GetAsync(playlistId, cancellationToken).ConfigureAwait(false);
        if (playlist.Data != null)
        {
            CacheManager.Remove(CacheKeyDetailByApiKeyTemplate.FormatSmart(playlist.Data.ApiKey));
            CacheManager.Remove(CacheKeyDetailTemplate.FormatSmart(playlist.Data.Id));
        }
    }

    /// <summary>
    /// Return a paginated list of all playlists in the database.
    /// </summary>
    public async Task<MelodeeModels.PagedResult<Playlist>> ListAsync(MelodeeModels.UserInfo userInfo, MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        int playlistCount;
        //Playlist[] playlists = [];
        var playlists = new List<Playlist>();
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var orderBy = pagedRequest.OrderByValue();
            var dbConn = scopedContext.Database.GetDbConnection();
            var countSqlParts = pagedRequest.FilterByParts("SELECT COUNT(*) FROM \"Playlists\"");
            playlistCount = await dbConn
                .QuerySingleAsync<int>(countSqlParts.Item1, countSqlParts.Item2)
                .ConfigureAwait(false);
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                var listSqlParts = pagedRequest.FilterByParts("SELECT * FROM \"Playlists\"");
                var listSql = $"{listSqlParts.Item1} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
                playlists = (await dbConn
                    .QueryAsync<Playlist>(listSql, listSqlParts.Item2)
                    .ConfigureAwait(false))
                    .ToList();
            }
        }
        var dynamicPlaylists = await DynamicListAsync(userInfo, pagedRequest, cancellationToken);
        playlists.AddRange(dynamicPlaylists.Data);
        
        return new MelodeeModels.PagedResult<Playlist>
        {
            TotalCount = playlistCount,
            TotalPages = pagedRequest.TotalPages(playlistCount),
            Data = playlists
        };
    }

    /// <summary>
    /// Returns a paginated list of dynamic (those which are file defined) Playlists.
    /// </summary>
    public async Task<MelodeeModels.PagedResult<Playlist>> DynamicListAsync(MelodeeModels.UserInfo userInfo, MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var playlistCount = 0;
        var playlists = new List<Playlist>();

        var configuration = await configurationFactory.GetConfigurationAsync(cancellationToken);
        var isDynamicPlaylistsDisabled = configuration.GetValue<bool>(SettingRegistry.PlaylistDynamicPlaylistsDisabled);
        if (!isDynamicPlaylistsDisabled)
        {

            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                var playlistLibrary = await libraryService.GetPlaylistLibraryAsync(cancellationToken).ConfigureAwait(false);
                var dynamicPlaylistsJsonFiles = Path.Combine(playlistLibrary.Data.Path, "dynamic")
                    .ToFileSystemDirectoryInfo()
                    .AllFileInfos("*.json").ToArray();
                if (dynamicPlaylistsJsonFiles.Any())
                {
                    var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
                    var dynamicPlaylists = new List<MelodeeModels.DynamicPlaylist>();
                    foreach (var dynamicPlaylistJsonFile in dynamicPlaylistsJsonFiles)
                    {
                        dynamicPlaylists.Add(serializer.Deserialize<MelodeeModels.DynamicPlaylist>(
                            await File.ReadAllTextAsync(dynamicPlaylistJsonFile.FullName, cancellationToken)
                                .ConfigureAwait(false))!);
                    }

                    foreach (var dp in dynamicPlaylists.Where(x => x.IsEnabled))
                    {
                        try
                        {
                            if (dp.IsPublic || (dp.ForUserId != null && dp.ForUserId == userInfo.ApiKey))
                            {
                                var dpWhere = dp.PrepareSongSelectionWhere(userInfo);
                                var sql = $"""
                                           SELECT s."Id", s."ApiKey", s."IsLocked", s."Title", s."TitleNormalized", s."SongNumber", a."ReleaseDate",
                                                  a."Name" as "AlbumName", a."ApiKey" as "AlbumApiKey", ar."Name" as "ArtistName", ar."ApiKey" as "ArtistApiKey",
                                                  s."FileSize", s."Duration", s."CreatedAt", s."Tags", us."IsStarred" as "UserStarred", us."Rating" as "UserRating"
                                           FROM "Songs" s
                                           join "Albums" a on (s."AlbumId" = a."Id")
                                           join "Artists" ar on (a."ArtistId" = ar."Id")
                                           left join "UserSongs" us on (s."Id" = us."SongId")
                                           where {dpWhere}
                                           """;
                                var songDataInfosForDp = (await dbConn
                                    .QueryAsync<SongDataInfo>(sql)
                                    .ConfigureAwait(false)).ToArray();
                                playlists.Add(new Playlist
                                {
                                    Id = 1,
                                    IsLocked = false,
                                    SortOrder = 0,
                                    ApiKey = dp.Id,
                                    CreatedAt = now,
                                    Description = dp.Comment,
                                    Name = dp.Name,
                                    Comment = dp.Comment,
                                    User = ServiceUser.Instance.Value,
                                    IsDynamic = true,
                                    IsPublic = true,
                                    SongCount = SafeParser.ToNumber<short>(songDataInfosForDp.Count()),
                                    Duration = songDataInfosForDp.Sum(x => x.Duration),
                                    AllowedUserIds = userInfo.UserName,
                                    Songs = []
                                });
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Warning(e, "[{Name}] error loading dynamic playlist [{Playlist}]",
                                nameof(OpenSubsonicApiService), dp.Name);
                            throw;
                        }
                    }
                }
            }
        }        
        
        playlists  = playlists.Skip(pagedRequest.SkipValue).Take(pagedRequest.TakeValue).ToList();

        return new MelodeeModels.PagedResult<Playlist>
        {
            TotalCount = playlistCount,
            TotalPages = pagedRequest.TotalPages(playlistCount),
            Data = playlists
        };
    }

    public async Task<MelodeeModels.PagedResult<SongDataInfo>> SongsForPlaylistAsync(Guid apiKey, MelodeeModels.UserInfo userInfo, MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(_ => apiKey == Guid.Empty, apiKey, nameof(apiKey));
        
        var playlistResult = await GetByApiKeyAsync(userInfo, apiKey, cancellationToken).ConfigureAwait(false);
        if (!playlistResult.IsSuccess)
        {
            return new MelodeeModels.PagedResult<SongDataInfo>(["Unknown playlist"])
            {
                Data = [],
                TotalCount = 0,
                TotalPages = 0
            };
        }

        var songCount = 0;
        SongDataInfo[] songs;

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            if (playlistResult.Data!.IsDynamic)
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                var dynamicPlaylist = await libraryService.GetDynamicPlaylistAsync(apiKey, cancellationToken).ConfigureAwait(false);
                var dp = dynamicPlaylist.Data;
                if (dp == null)
                {
                    return new MelodeeModels.PagedResult<SongDataInfo>(["Unknown playlist"])
                    {
                        Data = [],
                        TotalCount = 0,
                        TotalPages = 0
                    };
                }

                var dpWhere = dp.PrepareSongSelectionWhere(userInfo);
                var dpOrderBy = dp.SongSelectionOrder ?? "RANDOM()";

                var sql = $"""
                           SELECT s."Id", s."ApiKey", s."IsLocked", s."Title", s."TitleNormalized", s."SongNumber", a."ReleaseDate",
                                  a."Name" as "AlbumName", a."ApiKey" as "AlbumApiKey", ar."Name" as "ArtistName", ar."ApiKey" as "ArtistApiKey",
                                  s."FileSize", s."Duration", s."CreatedAt", s."Tags", us."IsStarred" as "UserStarred", us."Rating" as "UserRating"
                           FROM "Songs" s
                           join "Albums" a on (s."AlbumId" = a."Id")
                           join "Artists" ar on (a."ArtistId" = ar."Id")
                           left join "UserSongs" us on (s."Id" = us."SongId")
                           where {dpWhere}
                           order by {dpOrderBy}
                           offset {pagedRequest.SkipValue} rows fetch next {pagedRequest.TakeValue} rows only;
                           """;
                songs = (await dbConn
                    .QueryAsync<SongDataInfo>(sql)
                    .ConfigureAwait(false)).ToArray();
            }
            else
            {
                var playlist = await scopedContext
                    .Playlists
                    .Include(x => x.User)
                    .Include(x => x.Songs).ThenInclude(x => x.Song).ThenInclude(x => x.Album).ThenInclude(x => x.Artist)
                    .Include(x => x.Songs).ThenInclude(x => x.Song).ThenInclude(x => x.UserSongs.Where(ua => ua.UserId == userInfo.Id))
                    .FirstOrDefaultAsync(x => x.ApiKey == apiKey, cancellationToken)
                    .ConfigureAwait(false);
                songs = playlist?
                    .Songs
                    .OrderBy(x => x.PlaylistOrder)
                    .Skip(pagedRequest.SkipValue)
                    .Take(pagedRequest.TakeValue)
                    .Select(x => x.Song.ToSongDataInfo(x.Song.UserSongs.FirstOrDefault())).ToArray() ?? [];
            }
        }
        
        return new MelodeeModels.PagedResult<SongDataInfo>
        {
            TotalCount = songCount,
            TotalPages = pagedRequest.TotalPages(songCount),
            Data = songs
        };
    }


    public async Task<MelodeeModels.OperationResult<Playlist?>> GetByApiKeyAsync(MelodeeModels.UserInfo userInfo, Guid apiKey, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(_ => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        var id = await CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                return await dbConn
                    .QuerySingleOrDefaultAsync<int?>("SELECT \"Id\" FROM \"Playlists\" WHERE \"ApiKey\" = @apiKey",
                        new { apiKey })
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        if (id == null)
        {
            // See if Dynamic playlist exists for given ApiKey. If so return it versus calling detail.
            var dynamicPlayLists = await DynamicListAsync(userInfo, new MelodeeModels.PagedRequest { PageSize = short.MaxValue }, cancellationToken).ConfigureAwait(false);
            var dynamicPlaylist = dynamicPlayLists.Data.FirstOrDefault(x => x.ApiKey == apiKey);
            if (dynamicPlaylist != null)
            {
                return new MelodeeModels.OperationResult<Playlist?>
                {
                    Data = dynamicPlaylist
                };
            }
            return new MelodeeModels.OperationResult<Playlist?>("Unknown playlist.")
            {
                Data = null
            };
        }

        return await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task<MelodeeModels.OperationResult<Playlist?>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            await using (var scopedContext =
                         await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext
                    .Playlists
                    .Include(x => x.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        return new MelodeeModels.OperationResult<Playlist?>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> DeleteAsync(int currentUserId, int[] playlistIds,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(playlistIds, nameof(playlistIds));


        bool result;
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var user = await scopedContext.Users.FirstOrDefaultAsync(x => x.Id == currentUserId, cancellationToken)
                .ConfigureAwait(false);
            if (user == null)
            {
                return new MelodeeModels.OperationResult<bool>("Unknown user.")
                {
                    Data = false
                };
            }

            foreach (var playlistId in playlistIds)
            {
                var playlist = await GetAsync(playlistId, cancellationToken).ConfigureAwait(false);
                if (!playlist.IsSuccess)
                {
                    return new MelodeeModels.OperationResult<bool>("Unknown playlist.")
                    {
                        Data = false
                    };
                }

                if (!user.CanDeletePlaylist(playlist.Data!))
                {
                    return new MelodeeModels.OperationResult<bool>("User does not have access to delete playlist.")
                    {
                        Data = false
                    };
                }

                scopedContext.Playlists.Remove(playlist.Data!);
                await ClearCacheAsync(playlistId, cancellationToken);
            }

            result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
        }


        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }
}
