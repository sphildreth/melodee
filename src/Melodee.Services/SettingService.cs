using Ardalis.GuardClauses;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using SmartFormat;

namespace Melodee.Services;

/// <summary>
///     Setting data domain service.
/// </summary>
public sealed class SettingService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailTemplate = "urn:setting:{0}";

    public async Task<PagedResult<Setting>> ListAsync(User currentUser, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        int settingsCount;
        Setting[] settings = [];
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            settingsCount = await scopedContext
                .Settings
                .AsNoTracking()
                .CountAsync(cancellationToken)
                .ConfigureAwait(false);
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                settings = await scopedContext
                    .Settings
                    .AsNoTracking()
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return new PagedResult<Setting>
        {
            TotalCount = settingsCount,
            TotalPages = pagedRequest.TotalPages(settingsCount),
            Data = settings
                .OrderBy(x => pagedRequest.Sort ?? nameof(Setting.Key))
                .Skip(pagedRequest.SkipValue)
                .Take(pagedRequest.TakeValue)
        };
    }

    public async Task<OperationResult<T?>> GetValueAsync<T>(User currentUser, string key, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(key, nameof(key));

        var settingResult = await GetAsync(currentUser, key, cancellationToken).ConfigureAwait(false);
        if (settingResult.Data == null || !settingResult.IsSuccess)
        {
            return new OperationResult<T?>
            {
                Data = default,
                Type = OperationResponseType.NotFound
            };
        }

        return new OperationResult<T?>
        {
            Data = settingResult.Data.Value.Convert<T>()
        };
    }

    public async Task<OperationResult<Setting?>> GetAsync(User currentUser, string key, CancellationToken cancellationToken = default)
    {
        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(key), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext
                    .Settings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Key == key, cancellationToken)
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        return new OperationResult<Setting?>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> UpdateAsync(User currentUser, Setting detailToUpdate, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, detailToUpdate?.Id ?? 0, nameof(detailToUpdate));

        var result = false;
        var validationResult = ValidateModel(detailToUpdate);
        if (!validationResult.IsSuccess)
        {
            return new OperationResult<bool>(validationResult.Data.Item2?.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage!).ToArray() ?? [])
            {
                Data = false,
                Type = OperationResponseType.ValidationFailure
            };
        }

        if (detailToUpdate != null)
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                // Load the detail by DetailToUpdate.Id
                var dbDetail = await scopedContext
                    .Settings
                    .FirstOrDefaultAsync(x => x.Id == detailToUpdate.Id, cancellationToken)
                    .ConfigureAwait(false);

                if (dbDetail == null)
                {
                    return new OperationResult<bool>
                    {
                        Data = false,
                        Type = OperationResponseType.NotFound
                    };
                }

                // Update values and save to db
                dbDetail.Category = detailToUpdate.Category;
                dbDetail.Comment = detailToUpdate.Comment;
                dbDetail.Description = detailToUpdate.Description;
                dbDetail.IsLocked = detailToUpdate.IsLocked;
                dbDetail.Key = detailToUpdate.Key;
                dbDetail.Notes = detailToUpdate.Notes;
                dbDetail.SortOrder = detailToUpdate.SortOrder;
                dbDetail.Tags = detailToUpdate.Tags;
                dbDetail.Value = detailToUpdate.Value;

                dbDetail.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);

                result = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;

                if (result)
                {
                    CacheManager.Remove(CacheKeyDetailTemplate.FormatSmart(dbDetail.Id));
                }
            }
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }
}
