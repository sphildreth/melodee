using EasyCaching.Core;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFormat;

namespace Melodee.Services;

/// <summary>
/// Settings data domain service.
/// </summary>
public sealed class SettingService(
    ILogger<SettingService> logger, 
    IEasyCachingProviderFactory cachingProviderFactory,
    IDbContextFactory<MelodeeDbContext> contextFactory)
    : ServiceBase(logger, cachingProviderFactory, contextFactory)
{
    private const string CacheKeyDetailTemplate = "urn:setting:{0}";

    public async Task<PagedResult<Setting>> ListAsync(User currentUser, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        int settingsCount;
        Setting[] settings = [];
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            settingsCount = await scopedContext.Settings.CountAsync(cancellationToken).ConfigureAwait(false);
            if (!pagedRequest.IsTotalCountOnlyRequest)
            {
                settings = await scopedContext.Settings.ToArrayAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
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
    
    public async Task<OperationResult<T?>> GetAsync<T>(User currentUser, string key, CancellationToken cancellationToken = default)
    {
        var cacheProvider = CachingProviderFactory.GetCachingProvider(CacheName);
        var result = await cacheProvider.GetAsync<T?>(CacheKeyDetailTemplate.FormatSmart(key), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var setting = await scopedContext.Settings.FirstOrDefaultAsync(x => x.Key == key, cancellationToken).ConfigureAwait(false);
                if (setting != null)
                {
                    return (T)Convert.ChangeType(setting.Value, typeof(T));
                }
                return default;
            }
        }, DefaultCacheDuration, cancellationToken);
        return new OperationResult<T?>
        {
            Data = result.Value
        };
    }
}
