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

public class AlbumService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:album:apikey:{0}";
    private const string CacheKeyDetailByMediaUniqueIdTemplate = "urn:album:mediauniqueid:{0}";    
    private const string CacheKeyDetailTemplate = "urn:album:{0}";    
    
    public async Task<MelodeeModels.PagedResult<Album>> ListAsync(MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        int albumCount;
        Album[] albums = [];
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var orderBy = pagedRequest.OrderByValue();
            var dbConn = scopedContext.Database.GetDbConnection();            
            var countSqlParts = pagedRequest.FilterByParts("SELECT COUNT(*) FROM \"Albums\"");
            albumCount = await dbConn
                .QuerySingleAsync<int>(countSqlParts.Item1, countSqlParts.Item2)
                .ConfigureAwait(false);            
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                var listSqlParts = pagedRequest.FilterByParts("SELECT * FROM \"Albums\"");
                var listSql = $"{listSqlParts.Item1} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
                if (dbConn is SqliteConnection)
                {
                    listSql = $"{listSqlParts.Item1 } ORDER BY {orderBy} LIMIT {pagedRequest.TakeValue} OFFSET {pagedRequest.SkipValue};";
                }
                albums = (await dbConn
                    .QueryAsync<Album>(listSql, listSqlParts.Item2)
                    .ConfigureAwait(false)).ToArray();
            }
        }

        return new MelodeeModels.PagedResult<Album>
        {
            TotalCount = albumCount,
            TotalPages = pagedRequest.TotalPages(albumCount),
            Data = albums
        };
    }    
    
    public Task<MelodeeModels.OperationResult<Album?>> GetByMediaUniqueId(long mediaUniqueId, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, mediaUniqueId, nameof(mediaUniqueId));

        return CacheManager.GetAsync(CacheKeyDetailByMediaUniqueIdTemplate.FormatSmart(mediaUniqueId), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var userId = await scopedContext
                    .Albums
                    .AsNoTracking()
                    .Where(x => x.MediaUniqueId == mediaUniqueId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
                return await GetAsync(userId, cancellationToken).ConfigureAwait(false);
            }
        }, cancellationToken);
    }     
    
    public async Task<MelodeeModels.OperationResult<Album?>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext
                    .Albums
                    .Include(x => x.Discs)
                    .ThenInclude(x => x.Songs)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        return new MelodeeModels.OperationResult<Album?>
        {
            Data = result
        };
    }
    
    public Task<MelodeeModels.OperationResult<Album?>> GetByApiKeyAsync(Guid apiKey, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        return CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var userId = await scopedContext
                    .Albums
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