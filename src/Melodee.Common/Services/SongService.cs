using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Filtering;
using Melodee.Common.Models.Collection;
using Melodee.Common.Plugins.Scrobbling;
using Melodee.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Common.Services;

public class SongService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    INowPlayingRepository NowPlayingRepository)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:song:apikey:{0}";
    private const string CacheKeyDetailByTitleNormalizedTemplate = "urn:song:titlenormalized:{0}";
    private const string CacheKeyDetailTemplate = "urn:song:{0}";

    public async Task<MelodeeModels.PagedResult<SongDataInfo>> ListNowPlayingAsync(MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var songCount = 0;
        SongDataInfo[] songs = [];
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var nowPlaying = await NowPlayingRepository.GetNowPlayingAsync(cancellationToken).ConfigureAwait(false);
            if (nowPlaying.Data.Length > 0)
            {
                var nowPlayingSongIds = nowPlaying.Data.Select(x => x.Scrobble.SongId).ToArray();

                var orderBy = pagedRequest.OrderByValue();
                var dbConn = scopedContext.Database.GetDbConnection();
                songCount = nowPlayingSongIds.Length;

                if (!pagedRequest.IsTotalCountOnlyRequest)
                {
                    var sqlStartFragment = """
                                           SELECT s."Id", s."ApiKey", s."IsLocked", s."Title", s."TitleNormalized", s."SongNumber", a."ReleaseDate",
                                                  a."Name" as "AlbumName", a."ApiKey" as "AlbumApiKey", ar."Name" as "ArtistName", ar."ApiKey" as "ArtistApiKey",
                                                  s."FileSize", s."Duration", s."CreatedAt", s."Tags", us."IsStarred" as "UserStarred", us."Rating" as "UserRating"
                                           FROM "Songs" s
                                           join "Albums" a on (s."AlbumId" = a."Id")
                                           join "Artists" ar on (a."ArtistId" = ar."Id")
                                           left join "UserSongs" us on (s."Id" = us."SongId")
                                           where s."Id" in ('{0}')
                                           """.FormatSmart(string.Join(@"','", nowPlayingSongIds));

                    var listSql =
                        $"{sqlStartFragment} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
                    songs = (await dbConn
                        .QueryAsync<SongDataInfo>(listSql)
                        .ConfigureAwait(false)).ToArray();
                }
            }
        }

        return new MelodeeModels.PagedResult<SongDataInfo>
        {
            TotalCount = songCount,
            TotalPages = pagedRequest.TotalPages(songCount),
            Data = songs
        };
    }

    public async Task<MelodeeModels.PagedResult<SongDataInfo>> ListForContributorsAsync(MelodeeModels.PagedRequest pagedRequest, string contributorName, CancellationToken cancellationToken = default)
    {
        int songCount;
        SongDataInfo[] songs = [];
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();

            var sql = """
                      SELECT COUNT(s.*)
                      from "Contributors" c
                      join "Songs" s on (c."SongId" = s."Id")
                      where (c."ContributorName" ILIKE '%{0}%');
                      """.FormatSmart(contributorName);

            songCount = await dbConn.ExecuteScalarAsync<int>(sql);

            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                sql = """
                      SELECT s."Id", s."ApiKey", s."IsLocked", s."Title", s."TitleNormalized", s."SongNumber", a."ReleaseDate",
                             a."Name" as "AlbumName", a."ApiKey" as "AlbumApiKey", ar."Name" as "ArtistName", ar."ApiKey" as "ArtistApiKey",
                             s."FileSize", s."Duration", s."CreatedAt", s."Tags", us."IsStarred" as "UserStarred", us."Rating" as "UserRating"
                      FROM "Contributors" c
                      join "Songs" s on (c."SongId" = s."Id")
                      join "Albums" a on (s."AlbumId" = a."Id")
                      join "Artists" ar on (a."ArtistId" = ar."Id")
                      left join "UserSongs" us on (s."Id" = us."SongId")
                      where (c."ContributorName" ILIKE '%{0}%')
                      ORDER BY a.{1} OFFSET {2} ROWS FETCH NEXT {3} ROWS only;
                      """.FormatSmart(contributorName, pagedRequest.OrderByValue(), pagedRequest.SkipValue,
                    pagedRequest.TakeValue);
                songs = (await dbConn
                    .QueryAsync<SongDataInfo>(sql)
                    .ConfigureAwait(false)).ToArray();
            }
        }

        return new MelodeeModels.PagedResult<SongDataInfo>
        {
            TotalCount = songCount,
            TotalPages = pagedRequest.TotalPages(songCount),
            Data = songs
        };
    }

    public async Task<MelodeeModels.PagedResult<SongDataInfo>> ListAsync(MelodeeModels.PagedRequest pagedRequest,
        int userId, CancellationToken cancellationToken = default)
    {
        int songCount;
        SongDataInfo[] songs = [];
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var orderBy = pagedRequest.OrderByValue();
            var dbConn = scopedContext.Database.GetDbConnection();

            var filterByUserStarred = pagedRequest.FilterBy?.FirstOrDefault(x => x.PropertyName == "UserStarred");
            if (filterByUserStarred != null)
            {
                var nf = new FilterOperatorInfo("IsStarred", filterByUserStarred.Operator, filterByUserStarred.Value,
                    ColumnName: "us");
                var newFilterBy = pagedRequest.FilterBy!.ToList();
                newFilterBy.Remove(filterByUserStarred);
                newFilterBy.Add(nf);
                pagedRequest.FilterBy = newFilterBy.ToArray();
            }

            var filterByUserRating = pagedRequest.FilterBy?.FirstOrDefault(x => x.PropertyName == "UserRating");
            if (filterByUserRating != null)
            {
                var nf = new FilterOperatorInfo("Rating", filterByUserRating.Operator, filterByUserRating.Value,
                    ColumnName: "us");
                var newFilterBy = pagedRequest.FilterBy!.ToList();
                newFilterBy.Remove(filterByUserRating);
                newFilterBy.Add(nf);
                pagedRequest.FilterBy = newFilterBy.ToArray();
            }

            var countSqlParts = pagedRequest.FilterByParts($"""
                                                            SELECT COUNT(*) FROM "Songs" s 
                                                            left join "UserSongs" us on (s."Id" = us."SongId" and us."UserId" = {userId})
                                                            """);
            songCount = await dbConn
                .QuerySingleAsync<int>(countSqlParts.Item1, countSqlParts.Item2)
                .ConfigureAwait(false);
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                var sqlStartFragment = $"""
                                        SELECT s."Id", s."ApiKey", s."IsLocked", s."Title", s."TitleNormalized", s."SongNumber", a."ReleaseDate",
                                               a."Name" as "AlbumName", a."ApiKey" as "AlbumApiKey", ar."Name" as "ArtistName", ar."ApiKey" as "ArtistApiKey",
                                               s."FileSize", s."Duration", s."CreatedAt", s."Tags", us."IsStarred" as "UserStarred", us."Rating" as "UserRating"
                                        FROM "Songs" s
                                        join "Albums" a on (s."AlbumId" = a."Id")
                                        join "Artists" ar on (a."ArtistId" = ar."Id")
                                        left join "UserSongs" us on (s."Id" = us."SongId" and us."UserId" = {userId})
                                        """;
                var listSqlParts = pagedRequest.FilterByParts(sqlStartFragment);
                var listSql =
                    $"{listSqlParts.Item1} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
                songs = (await dbConn
                    .QueryAsync<SongDataInfo>(listSql, listSqlParts.Item2)
                    .ConfigureAwait(false)).ToArray();
            }
        }

        return new MelodeeModels.PagedResult<SongDataInfo>
        {
            TotalCount = songCount,
            TotalPages = pagedRequest.TotalPages(songCount),
            Data = songs
        };
    }

    public async Task<MelodeeModels.OperationResult<Song?>> GetAsync(int id,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            await using (var scopedContext =
                         await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext
                    .Songs
                    .Include(x => x.Contributors).ThenInclude(x => x.Artist)
                    .Include(x => x.Album).ThenInclude(x => x.Artist)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        return new MelodeeModels.OperationResult<Song?>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<Song?>> GetByApiKeyAsync(Guid apiKey,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        var id = await CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using (var scopedContext =
                         await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                return await dbConn
                    .QuerySingleOrDefaultAsync<int?>("SELECT \"Id\" FROM \"Songs\" WHERE \"ApiKey\" = @apiKey",
                        new { apiKey })
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        if (id == null)
        {
            return new MelodeeModels.OperationResult<Song?>("Unknown song.")
            {
                Data = null
            };
        }

        return await GetAsync(id.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task ClearCacheAsync(int songId, CancellationToken cancellationToken)
    {
        var song = await GetAsync(songId, cancellationToken).ConfigureAwait(false);
        if (song?.Data != null)
        {
            CacheManager.Remove(CacheKeyDetailByApiKeyTemplate.FormatSmart(song.Data.ApiKey));
            CacheManager.Remove(CacheKeyDetailByTitleNormalizedTemplate.FormatSmart(song.Data.TitleNormalized));
            CacheManager.Remove(CacheKeyDetailTemplate.FormatSmart(song.Data.Id));
        }
    }

    public Task<MelodeeModels.OperationResult<bool>> DeleteAsync(int[] toArray,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
