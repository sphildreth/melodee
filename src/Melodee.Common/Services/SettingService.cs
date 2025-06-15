using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Configuration;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Extensions;
using Melodee.Common.Services.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Common.Services;

/// <summary>
///     Setting data domain service, this is used to manage the settings, for getting settings for services see
///     <see cref="IMelodeeConfigurationFactory" />
/// </summary>
public class SettingService : ServiceBase
{
    private const string CacheKeyDetailTemplate = "urn:setting:{0}";
    private readonly IMelodeeConfigurationFactory _melodeeConfigurationFactory = null!;

    /// <summary>
    ///     This is required for Mocking in unit tests.
    /// </summary>
    public SettingService()
    {
    }

    /// <summary>
    ///     Setting data domain service, this is used to manage the settings, for getting settings for services see
    ///     <see cref="IMelodeeConfigurationFactory" />
    /// </summary>
    public SettingService(ILogger logger,
        ICacheManager cacheManager,
        IMelodeeConfigurationFactory melodeeConfigurationFactory,
        IDbContextFactory<MelodeeDbContext> contextFactory) : base(logger, cacheManager, contextFactory)
    {
        _melodeeConfigurationFactory = melodeeConfigurationFactory;
    }

    public virtual async Task<Dictionary<string, object?>> GetAllSettingsAsync(CancellationToken cancellationToken = default)
    {
        var listResult = await ListAsync(new MelodeeModels.PagedRequest { PageSize = short.MaxValue }, cancellationToken);
        if (!listResult.IsSuccess)
        {
            throw new Exception("Failed to get settings from database");
        }

        var listDictionary = listResult.Data.ToDictionary(x => x.Key, x => (object?)x.Value);
        return MelodeeConfiguration.AllSettings(listDictionary);
    }

    public async Task<MelodeeModels.PagedResult<Setting>> ListAsync(MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var settingsCount = 0;
        Setting[] settings = [];
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            try
            {
                var orderBy = pagedRequest.OrderByValue();
                var dbConn = scopedContext.Database.GetDbConnection();
                var countSqlParts = pagedRequest.FilterByParts("SELECT COUNT(*) FROM \"Settings\"");
                settingsCount = await dbConn
                    .QuerySingleOrDefaultAsync<int>(countSqlParts.Item1, countSqlParts.Item2)
                    .ConfigureAwait(false);
                if (!pagedRequest.IsTotalCountOnlyRequest)
                {
                    var listSqlParts = pagedRequest.FilterByParts("SELECT * FROM \"Settings\"");
                    var listSql = $"{listSqlParts.Item1} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
                    if (dbConn is SqliteConnection)
                    {
                        listSql =
                            $"{listSqlParts.Item1} ORDER BY {orderBy} LIMIT {pagedRequest.TakeValue} OFFSET {pagedRequest.SkipValue};";
                    }                    
                    settings = (await dbConn
                        .QueryAsync<Setting>(listSql, listSqlParts.Item2)
                        .ConfigureAwait(false)).ToArray();

                    foreach (var envSetSetting in MelodeeConfigurationFactory.EnvironmentVariablesSettings())
                    {
                        var kk = envSetSetting.Key.Replace("_", ".");
                        var setting = settings.FirstOrDefault(x => string.Equals(x.Key, kk, StringComparison.OrdinalIgnoreCase));
                        if (setting != null)
                        {
                            setting.Value = envSetSetting.Value?.ToString() ?? string.Empty;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to get settings from database");
            }
        }

        return new MelodeeModels.PagedResult<Setting>
        {
            TotalCount = settingsCount,
            TotalPages = pagedRequest.TotalPages(settingsCount),
            Data = settings
        };
    }

    public async Task<MelodeeModels.OperationResult<T?>> GetValueAsync<T>(string key, T? defaultValue = default,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(key, nameof(key));

        var settingResult = await GetAsync(key, cancellationToken).ConfigureAwait(false);
        if (settingResult.Data == null || !settingResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<T?>
            {
                Data = defaultValue ?? default,
                Type = MelodeeModels.OperationResponseType.NotFound
            };
        }

        return new MelodeeModels.OperationResult<T?>
        {
            Data = settingResult.Data.Value.Convert<T>()
        };
    }

    public async Task<MelodeeModels.OperationResult<bool>> SetAsync(string key, string value,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(key, nameof(key));

        var setting = await GetAsync(key, cancellationToken).ConfigureAwait(false);
        if (setting.Data != null)
        {
            setting.Data.Value = value;
            return await UpdateAsync(setting.Data, cancellationToken).ConfigureAwait(false);
        }

        return new MelodeeModels.OperationResult<bool>
        {
            Data = false
        };
    }

    public async Task<MelodeeModels.OperationResult<Setting?>> GetAsync(string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(key), async () =>
            {
                await using (var scopedContext =
                             await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
                {
                    return await scopedContext
                        .Settings
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Key == key, cancellationToken)
                        .ConfigureAwait(false);
                }
            }, cancellationToken);
            return new MelodeeModels.OperationResult<Setting?>
            {
                Data = result
            };
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to get setting [{0}]", key);
        }

        return new MelodeeModels.OperationResult<Setting?>
        {
            Data = default,
            Type = MelodeeModels.OperationResponseType.Error
        };
    }

    public async Task<MelodeeModels.OperationResult<Setting?>> AddAsync(Setting setting, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(setting, nameof(setting));

        setting.ApiKey = Guid.NewGuid();
        setting.CreatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);

        var validationResult = ValidateModel(setting);
        if (!validationResult.IsSuccess)
        {
            return new MelodeeModels.OperationResult<Setting?>(validationResult.Data.Item2
                ?.Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage!).ToArray() ?? [])
            {
                Data = null,
                Type = MelodeeModels.OperationResponseType.ValidationFailure
            };
        }

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            // Ensure the setting key is unique
            var existingSetting = await scopedContext
                .Settings
                .FirstOrDefaultAsync(x => x.Key == setting.Key, cancellationToken)
                .ConfigureAwait(false);
            if (existingSetting != null)
            {
                return new MelodeeModels.OperationResult<Setting?>([$"Setting with key '{setting.Key}' already exists."])
                {
                    Data = null,
                    Type = MelodeeModels.OperationResponseType.Error
                };
            }

            scopedContext.Settings.Add(setting);
            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return await GetAsync(setting.Key, cancellationToken);
    }

    public async Task<MelodeeModels.OperationResult<bool>> UpdateAsync(Setting detailToUpdate,
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
                .Settings
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
                _melodeeConfigurationFactory.Reset();
            }
        }


        return new MelodeeModels.OperationResult<bool>
        {
            Data = result
        };
    }
}
