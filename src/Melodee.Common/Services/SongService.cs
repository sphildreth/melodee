using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Models.Collection;
using Melodee.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Common.Services;

public class SongService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:song:apikey:{0}";
    private const string CacheKeyDetailByTitleNormalizedTemplate = "urn:song:titlenormalized:{0}";
    private const string CacheKeyDetailTemplate = "urn:song:{0}";

    public async Task<MelodeeModels.PagedResult<SongDataInfo>> ListAsync(MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        int albumCount;
        SongDataInfo[] songs = [];
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var orderBy = pagedRequest.OrderByValue();
            var dbConn = scopedContext.Database.GetDbConnection();
            var countSqlParts = pagedRequest.FilterByParts("SELECT COUNT(*) FROM \"Songs\"");
            albumCount = await dbConn
                .QuerySingleAsync<int>(countSqlParts.Item1, countSqlParts.Item2)
                .ConfigureAwait(false);
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                var sqlStartFragment = """
                                       SELECT s."Id", s."ApiKey", s."IsLocked", s."Title", s."TitleNormalized", s."SongNumber",
                                              a."Name" as "AlbumName", a."ApiKey" as "AlbumApiKey", ar."Name" as "ArtistName", ar."ApiKey" as "ArtistApiKey",
                                              ad."DiscNumber" as "DiscNumber", s."FileSize", s."Duration", s."CreatedAt", s."Tags"
                                       FROM "Songs" s
                                       join "AlbumDiscs" ad on (s."AlbumDiscId" = ad."Id")
                                       join "Albums" a on (ad."AlbumId" = a."Id")
                                       join "Artists" ar on (a."ArtistId" = ar."Id")
                                       """;
                var listSqlParts = pagedRequest.FilterByParts(sqlStartFragment);
                var listSql = $"{listSqlParts.Item1} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
                songs = (await dbConn
                    .QueryAsync<SongDataInfo>(listSql, listSqlParts.Item2)
                    .ConfigureAwait(false)).ToArray();
            }
        }

        return new MelodeeModels.PagedResult<SongDataInfo>
        {
            TotalCount = albumCount,
            TotalPages = pagedRequest.TotalPages(albumCount),
            Data = songs
        };
    }

    public async Task<MelodeeModels.OperationResult<Song?>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext
                    .Songs
                    .Include(x => x.Contributors).ThenInclude(x => x.Artist)
                    .Include(x => x.AlbumDisc).ThenInclude(x => x.Album).ThenInclude(x => x.Artist)
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

    public async Task<MelodeeModels.OperationResult<Song?>> GetByApiKeyAsync(Guid apiKey, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        var id = await CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var dbConn = scopedContext.Database.GetDbConnection();
                return await dbConn
                    .QuerySingleOrDefaultAsync<int?>("SELECT \"Id\" FROM \"Songs\" WHERE \"ApiKey\" = @apiKey", new { apiKey })
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

    public async Task<MelodeeModels.OperationResult<bool>> DeleteAsync(int[] toArray, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
