using System.Collections.Concurrent;
using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;

namespace Melodee.Plugins.Scrobbling;

public sealed class NowPlayingInMemoryRepository : INowPlayingRepository
{
    private static readonly ConcurrentDictionary<long, NowPlayingInfo> Storage = new();

    public Task AddOrUpdateNowPlayingAsync(NowPlayingInfo nowPlaying, CancellationToken token = default)
    {
        if (Storage.ContainsKey(nowPlaying.UniqueId))
        {
            Storage.TryRemove(nowPlaying.UniqueId, out var existing);
            if (existing != null)
            {
                existing.Scrobble.LastScrobbledAt = nowPlaying.Scrobble.LastScrobbledAt;
                Storage.TryAdd(existing.UniqueId, existing);
            }
        }
        else
        {
            Storage.TryAdd(nowPlaying.UniqueId, nowPlaying);
        }

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
