using Melodee.Common.Serialization;
using Serilog;

namespace Melodee.Common.Services.Caching;

public abstract class CacheManagerBase(ILogger logger, TimeSpan defaultTimeSpan, ISerializer serializer) : ICacheManager
{
    protected ILogger Logger { get; } = logger;
    protected TimeSpan DefaultTimeSpan { get; } = defaultTimeSpan;

    public ISerializer Serializer { get; } = serializer;
    public abstract void Clear();
    public abstract void ClearRegion(string region);

    public abstract Task<TOut> GetAsync<TOut>(string key, Func<Task<TOut>> getItem, CancellationToken token,
        TimeSpan? duration = null, string? region = null);

    public abstract bool Remove(string key);
    public abstract bool Remove(string key, string? region);
}
