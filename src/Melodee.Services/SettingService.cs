using Ardalis.GuardClauses;
using Melodee.Common.Configuration;
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

    public async Task<Dictionary<string, object?>> GetAllSettingsAsync(CancellationToken cancellationToken = default)
    {
        var listResult = await ListAsync(new PagedRequest { PageSize = short.MaxValue }, cancellationToken);
        if (!listResult.IsSuccess)
        {
            throw new Exception("Failed to get settings from database");
        }

        var listDictionary = listResult.Data.ToDictionary(x => x.Key, x => (object?)x.Value);
        return MelodeeConfiguration.AllSettings(listDictionary);
    }

    public async Task<PagedResult<Setting>> ListAsync(PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        int settingsCount;
        Setting[] settings = [];
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            settingsCount = await scopedContext
                .Settings
                .Where(x => pagedRequest.Filter == null || x.Key.ToUpper().Contains(pagedRequest.Filter.ToUpper()))
                .AsNoTracking()
                .CountAsync(cancellationToken)
                .ConfigureAwait(false);
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                settings = await scopedContext
                    .Settings
                    .Where(x => pagedRequest.Filter == null || x.Key.ToUpper().Contains(pagedRequest.Filter.ToUpper()))
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
                .Take(pagedRequest.PageSizeValue)
        };
    }

    public async Task<OperationResult<T?>> GetValueAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(key, nameof(key));

        var settingResult = await GetAsync(key, cancellationToken).ConfigureAwait(false);
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

    public async Task<OperationResult<Setting?>> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception e)
        {
            Logger.Error(e, "Failed to get setting [{0}]", key);
        }

        return new OperationResult<Setting?>
        {
            Data = default,
            Type = OperationResponseType.Error
        };
    }

    public async Task<IMelodeeConfiguration> GetMelodeeConfigurationAsync(CancellationToken cancellationToken = default)
        => new MelodeeConfiguration(await GetAllSettingsAsync(cancellationToken));

    public async Task<OperationResult<bool>> UpdateAsync(Setting detailToUpdate, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, detailToUpdate.Id, nameof(detailToUpdate));

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
