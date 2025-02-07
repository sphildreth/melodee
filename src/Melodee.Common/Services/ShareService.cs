using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Common.Services;

public class ShareService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    public async Task<MelodeeModels.PagedResult<Share>> ListAsync(MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        int shareCount;
        Share[] shares = [];
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var orderBy = pagedRequest.OrderByValue();
            var dbConn = scopedContext.Database.GetDbConnection();
            var countSqlParts = pagedRequest.FilterByParts("SELECT COUNT(*) FROM \"Shares\"");
            shareCount = await dbConn
                .QuerySingleAsync<int>(countSqlParts.Item1, countSqlParts.Item2)
                .ConfigureAwait(false);
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                var listSqlParts = pagedRequest.FilterByParts("SELECT * FROM \"Shares\"");
                var listSql = $"{listSqlParts.Item1} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
                shares = (await dbConn
                    .QueryAsync<Share>(listSql, listSqlParts.Item2)
                    .ConfigureAwait(false)).ToArray();
            }
        }

        return new MelodeeModels.PagedResult<Share>
        {
            TotalCount = shareCount,
            TotalPages = pagedRequest.TotalPages(shareCount),
            Data = shares
        };
    }

    public Task<MelodeeModels.OperationResult<bool>> DeleteAsync(int[] shareIds, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
