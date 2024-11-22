using System.Collections.Concurrent;
using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;

namespace Melodee.Plugins.Scrobbling;

public sealed class NowPlayingInMemoryRepository : INowPlayingRepository
{
    private static readonly ConcurrentDictionary<long, NowPlayingInfo> Storage = new ConcurrentDictionary<long, NowPlayingInfo>(); 

    public Task AddOrUpdateNowPlayingAsync(NowPlayingInfo nowPlaying, CancellationToken token = default)
    {
        if (Storage.ContainsKey(nowPlaying.UniqueId))
        {
            Storage.TryRemove(nowPlaying.UniqueId, out _);
        }
        Storage.TryAdd(nowPlaying.UniqueId, nowPlaying);
        return Task.CompletedTask;
    }

    public Task<OperationResult<NowPlayingInfo[]>> GetNowPlayingAsync(CancellationToken token = default)
    {
        return Task.FromResult(new OperationResult<NowPlayingInfo[]>
        {
            Data = Storage.Values.ToArray()
        });
    }
}
