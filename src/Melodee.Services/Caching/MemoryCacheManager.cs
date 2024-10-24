using Melodee.Common.Serialization;
using Melodee.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace Melodee.Services.Caching;

/// <summary>
///     MemoryCache implementation for ICacheManager.
/// </summary>
/// <param name="logger">Logger for CacheManager</param>
/// <param name="defaultTimeSpan">Default Timespan for lifetime of cached items.</param>
/// <param name="serializer">Serializer for CacheManager</param>
public sealed class MemoryCacheManager(ILogger logger, TimeSpan defaultTimeSpan, ISerializer serializer)
    : CacheManagerBase(logger, defaultTimeSpan, serializer)
{
    private MemoryCache _cache = new(new MemoryCacheOptions());

    public override void Clear()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    public override void ClearRegion(string region)
    {
        Clear();
    }

    private TOut? Get<TOut>(string key)
    {
        return _cache.Get<TOut>(key);
    }

    public override async Task<TOut> GetAsync<TOut>(string key, Func<Task<TOut>> getItem, CancellationToken token, TimeSpan? duration = null, string? region = null)
    {
        var r = Get<TOut>(key);
        if (r == null)
        {
            r = await getItem().ConfigureAwait(false);
            _cache.Set(key, r, duration ?? DefaultTimeSpan);
            Logger.Verbose($"-+> Cache Miss for Key [{key}], Region [{region}]");
        }
        else
        {
            Logger.Verbose($"-!> Cache Hit for Key [{key}], Region [{region}]");
        }

        return r;
    }

    public override bool Remove(string key)
    {
        _cache.Remove(key);
        return true;
    }

    public override bool Remove(string key, string? region)
    {
        return Remove(key);
    }
}
