using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Melodee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SmartFormat;

namespace Melodee.Services;

/// <summary>
///     Settings data domain service.
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
}
