using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Common.Services;

public class RadioStationService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:radiostation:apikey:{0}";
    private const string CacheKeyDetailTemplate = "urn:radiostation:{0}";

    public async Task<MelodeeModels.PagedResult<RadioStation>> ListAsync(MelodeeModels.PagedRequest pagedRequest,
        CancellationToken cancellationToken = default)
    {
        int radioStationCount;
        RadioStation[] radioStations = [];
        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var orderBy = pagedRequest.OrderByValue();
            var dbConn = scopedContext.Database.GetDbConnection();
            var countSqlParts = pagedRequest.FilterByParts("SELECT COUNT(*) FROM \"RadioStations\"");
            radioStationCount = await dbConn
                .QuerySingleAsync<int>(countSqlParts.Item1, countSqlParts.Item2)
                .ConfigureAwait(false);
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                var listSqlParts = pagedRequest.FilterByParts("SELECT * FROM \"RadioStations\"");
                var listSql =
                    $"{listSqlParts.Item1} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
                radioStations = (await dbConn
                    .QueryAsync<RadioStation>(listSql, listSqlParts.Item2)
                    .ConfigureAwait(false)).ToArray();
            }
        }

        return new MelodeeModels.PagedResult<RadioStation>
        {
            TotalCount = radioStationCount,
            TotalPages = pagedRequest.TotalPages(radioStationCount),
            Data = radioStations
        };
    }

    public async Task<MelodeeModels.OperationResult<RadioStation?>> AddAsync(RadioStation radioStation,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(radioStation, nameof(radioStation));

        radioStation.ApiKey = Guid.NewGuid();

        radioStation.CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);

        var validationResult = ValidateModel(radioStation);
        if (!validationResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<RadioStation?>(validationResult.Data.Item2
                ?.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage!).ToArray() ?? [])
            {
                Data = null,
                Type = MelodeeModels.OperationResponseType.ValidationFailure
            };
        }

        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            scopedContext.RadioStations.Add(radioStation);
            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return await GetAsync(radioStation.Id, cancellationToken);
    }

    public async Task<MelodeeModels.OperationResult<RadioStation?>> GetAsync(int id,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            await using (var scopedContext =
                         await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext
                    .RadioStations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        return new MelodeeModels.OperationResult<RadioStation?>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> DeleteAsync(int currentUserId, int[] radioStationIds,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(radioStationIds, nameof(radioStationIds));

        bool result;

        await using (var scopedContext =
                     await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var user = await scopedContext.Users.FirstAsync(x => x.Id == currentUserId, cancellationToken)
                .ConfigureAwait(false);

            if (!user.IsAdmin)
            {
                return new MelodeeModels.OperationResult<bool>("Non admin users cannot delete RadioStations.")
                {
                    Type = MelodeeModels.OperationResponseType.AccessDenied,
                    Data = false
                };
            }


            foreach (var radioStationId in radioStationIds)
            {
                var radioStation = await GetAsync(radioStationId, cancellationToken).ConfigureAwait(false);
                if (!radioStation.IsSuccess)
                {
                    return new MelodeeModels.OperationResult<bool>("Unknown RadioStation.")
                    {
                        Data = false
                    };
                }
            }

            foreach (var radioStationId in radioStationIds)
            {
                var radioStation = await scopedContext
                    .RadioStations
                    .FirstAsync(x => x.Id == radioStationId, cancellationToken)
                    .ConfigureAwait(false);
                scopedContext.RadioStations.Remove(radioStation);
            }

            result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> UpdateAsync(RadioStation detailToUpdate,
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
            // Load the detail by detailToUpdate.Id
            var dbDetail = await scopedContext
                .RadioStations
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
            dbDetail.Name = detailToUpdate.Name;
            dbDetail.StreamUrl = detailToUpdate.StreamUrl;
            dbDetail.HomePageUrl = detailToUpdate.HomePageUrl;
            dbDetail.Description = detailToUpdate.Description;
            dbDetail.IsLocked = detailToUpdate.IsLocked;
            dbDetail.Notes = detailToUpdate.Notes;
            dbDetail.SortOrder = detailToUpdate.SortOrder;
            dbDetail.Tags = detailToUpdate.Tags;
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
