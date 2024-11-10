using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Services.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Services;

public class ArtistService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:artist:apikey:{0}";
    private const string CacheKeyDetailByMediaUniqueIdTemplate = "urn:artist:mediauniqueid:{0}";
    private const string CacheKeyDetailTemplate = "urn:artist:{0}";    
    
    public async Task<MelodeeModels.PagedResult<Artist>> ListAsync(MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        int artistCount;
        Artist[] artists = [];
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var orderBy = pagedRequest.OrderByValue();
            var dbConn = scopedContext.Database.GetDbConnection();            
            var countSqlParts = pagedRequest.FilterByParts("SELECT COUNT(*) FROM \"Artists\"");
            artistCount = await dbConn
                .QuerySingleAsync<int>(countSqlParts.Item1, countSqlParts.Item2)
                .ConfigureAwait(false);            
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                var listSqlParts = pagedRequest.FilterByParts("SELECT * FROM \"Artists\"");
                var listSql = $"{listSqlParts.Item1} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
                if (dbConn is SqliteConnection)
                {
                    listSql = $"{listSqlParts.Item1 } ORDER BY {orderBy} LIMIT {pagedRequest.TakeValue} OFFSET {pagedRequest.SkipValue};";
                }
                artists = (await dbConn
                    .QueryAsync<Artist>(listSql, listSqlParts.Item2)
                    .ConfigureAwait(false)).ToArray();
            }
        }

        return new MelodeeModels.PagedResult<Artist>
        {
            TotalCount = artistCount,
            TotalPages = pagedRequest.TotalPages(artistCount),
            Data = artists
        };
    }    
    
    public async Task<MelodeeModels.OperationResult<Artist?>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext
                    .Artists
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        return new MelodeeModels.OperationResult<Artist?>
        {
            Data = result
        };
    }
    
    public Task<MelodeeModels.OperationResult<Artist?>> GetByMediaUniqueId(long mediaUniqueId, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, mediaUniqueId, nameof(mediaUniqueId));

        return CacheManager.GetAsync(CacheKeyDetailByMediaUniqueIdTemplate.FormatSmart(mediaUniqueId), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var userId = await scopedContext
                    .Artists
                    .AsNoTracking()
                    .Where(x => x.MediaUniqueId == mediaUniqueId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
                return await GetAsync(userId, cancellationToken).ConfigureAwait(false);
            }
        }, cancellationToken);
    } 
    
    
    public Task<MelodeeModels.OperationResult<Artist?>> GetByApiKeyAsync(Guid apiKey, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        return CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var userId = await scopedContext
                    .Artists
                    .AsNoTracking()
                    .Where(x => x.ApiKey == apiKey)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
                return await GetAsync(userId, cancellationToken).ConfigureAwait(false);
            }
        }, cancellationToken);
    }    
}
