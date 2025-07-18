using Melodee.Common.Models;
using Melodee.Common.Serialization;
using Serilog;

namespace Melodee.Common.Services.Caching;

/// <summary>
///     Fake CacheManager that does nothing, primarily for testing and debugging.
/// </summary>
/// <param name="logger">Logger for CacheManager</param>
/// <param name="defaultTimeSpan">Default Timespan for lifetime of cached items.</param>
/// <param name="serializer">Serializer for CacheManager</param>
public sealed class FakeCacheManager(ILogger logger, TimeSpan defaultTimeSpan, ISerializer serializer)
    : CacheManagerBase(logger, defaultTimeSpan, serializer)
{
    public override void Clear()
    {
        // Do nothing
    }

    public override void ClearRegion(string region)
    {
        // Do nothing
    }

    public override Task<TOut> GetAsync<TOut>(string key, Func<Task<TOut>> getItem, CancellationToken token,
        TimeSpan? duration = null, string? region = null)
    {
        // Do nothing
        return getItem();
    }


    public override bool Remove(string key)
    {
        // Do nothing
        return true;
    }

    public override bool Remove(string key, string? region)
    {
        // Do nothing
        return true;
    }

    public override IEnumerable<Statistic> CacheStatistics()
    {
        // Do nothing
        return [];
    }
}
