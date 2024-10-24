using Melodee.Common.Serialization;

namespace Melodee.Services.Interfaces;

public interface ICacheManager
{
    ISerializer Serializer { get; }

    void Clear();

    void ClearRegion(string region);

    Task<TOut> GetAsync<TOut>(string key, Func<Task<TOut>> getItem, CancellationToken token, TimeSpan? duration = null, string? region = null);

    bool Remove(string key);

    bool Remove(string key, string? region);
}
