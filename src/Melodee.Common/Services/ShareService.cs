using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Services.Caching;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using SmartFormat;
using Sqids;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Common.Services;

public class ShareService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:share:apikey:{0}";
    private const string CacheKeyDetailTemplate = "urn:share:{0}";

    public async Task<MelodeeModels.PagedResult<Share>> ListAsync(MelodeeModels.PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        int shareCount;
        Share[] shares = [];
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
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
                var listSql =
                    $"{listSqlParts.Item1} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
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

    public async Task<MelodeeModels.OperationResult<Share?>> AddAsync(Share share,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(share, nameof(share));

        share.ApiKey = Guid.NewGuid();

        var sqids = new SqidsEncoder<long>();
        share.ShareUniqueId = sqids.Encode(DateTime.UtcNow.Ticks);

        share.CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);

        var validationResult = ValidateModel(share);
        if (!validationResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<Share?>(validationResult.Data.Item2
                ?.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage!).ToArray() ?? [])
            {
                Data = null,
                Type = MelodeeModels.OperationResponseType.ValidationFailure
            };
        }

        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            scopedContext.Shares.Add(share);
            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return await GetAsync(share.Id, cancellationToken);
    }

    public async Task<MelodeeModels.OperationResult<Share?>> GetByUniqueIdAsync(string id,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(id, nameof(id));

        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var shareId = await dbConn
                .ExecuteScalarAsync<int?>("SELECT \"Id\" FROM \"Shares\" WHERE \"ShareUniqueId\" = @id;", new { id })
                .ConfigureAwait(false);

            return shareId == null
                ? new MelodeeModels.OperationResult<Share?>
                {
                    Data = null
                }
                : await GetAsync(shareId.Value, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<MelodeeModels.OperationResult<Share?>> GetAsync(int id,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            await using (var scopedContext =
                         await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext
                    .Shares
                    .Include(x => x.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        return new MelodeeModels.OperationResult<Share?>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> DeleteAsync(int currentUserId, int[] shareIds,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(shareIds, nameof(shareIds));

        bool result;

        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var user = await scopedContext.Users.FirstAsync(x => x.Id == currentUserId, cancellationToken)
                .ConfigureAwait(false);
            foreach (var shareId in shareIds)
            {
                var share = await GetAsync(shareId, cancellationToken).ConfigureAwait(false);
                if (!share.IsSuccess)
                {
                    return new MelodeeModels.OperationResult<bool>("Unknown share.")
                    {
                        Data = false
                    };
                }
            }

            foreach (var shareId in shareIds)
            {
                var share = await scopedContext
                    .Shares
                    .FirstAsync(x => x.Id == shareId, cancellationToken)
                    .ConfigureAwait(false);

                if (share.UserId != currentUserId && !user.IsAdmin)
                {
                    return new MelodeeModels.OperationResult<bool>("Non admin users cannot delete other users shares.")
                    {
                        Type = MelodeeModels.OperationResponseType.AccessDenied,
                        Data = false
                    };
                }

                scopedContext.Shares.Remove(share);
            }

            result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> UpdateAsync(Share detailToUpdate,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, detailToUpdate.Id, nameof(detailToUpdate));

        bool result;
        var validationResult = ValidateModel(detailToUpdate);
        if (!validationResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<bool>(validationResult.Data.Item2
                ?.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage!).ToArray() ?? [])
            {
                Data = false,
                Type = MelodeeModels.OperationResponseType.ValidationFailure
            };
        }

        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            // Load the detail by DetailToUpdate.Id
            var dbDetail = await scopedContext
                .Shares
                .FirstOrDefaultAsync(x => x.Id == detailToUpdate.Id, cancellationToken)
                .ConfigureAwait(false);

            if (dbDetail == null)
            {
                return new MelodeeModels.OperationResult<bool>
                {
                    Data = false,
                    Type = MelodeeModels.OperationResponseType.NotFound
                };
            }

            // Update values and save to db
            dbDetail.Description = detailToUpdate.Description;
            dbDetail.IsLocked = detailToUpdate.IsLocked;
            dbDetail.Notes = detailToUpdate.Notes;
            dbDetail.SortOrder = detailToUpdate.SortOrder;
            dbDetail.Tags = detailToUpdate.Tags;
            dbDetail.ShareType = detailToUpdate.ShareType;
            dbDetail.ShareId = detailToUpdate.ShareId;


            dbDetail.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);

            result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;

            if (result)
            {
                CacheManager.Remove(CacheKeyDetailTemplate.FormatSmart(dbDetail.Id));
                CacheManager.Remove(CacheKeyDetailByApiKeyTemplate.FormatSmart(dbDetail.ApiKey));
            }
        }


        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }
}
