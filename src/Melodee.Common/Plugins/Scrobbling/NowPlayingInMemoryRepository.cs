using System.Collections.Concurrent;
using System.Diagnostics;
using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;

namespace Melodee.Common.Plugins.Scrobbling;

public sealed class NowPlayingInMemoryRepository : INowPlayingRepository
{
    private const int MaximumMinutesAgo = 60;

    private static readonly ConcurrentDictionary<long, NowPlayingInfo> Storage = new();

    public Task RemoveNowPlayingAsync(long uniqueId, CancellationToken token = default)
    {
        if (Storage.ContainsKey(uniqueId))
        {
            Storage.TryRemove(uniqueId, out var existing);
        }

        return Task.CompletedTask;
    }

    public Task AddOrUpdateNowPlayingAsync(NowPlayingInfo nowPlaying, CancellationToken token = default)
    {
        var result = false;
        if (Storage.ContainsKey(nowPlaying.UniqueId))
        {
            Storage.TryRemove(nowPlaying.UniqueId, out var existing);
            if (existing != null)
            {
                existing.Scrobble.LastScrobbledAt = nowPlaying.Scrobble.LastScrobbledAt;
                result = Storage.TryAdd(existing.UniqueId, existing);
            }
        }
        else
        {
            result = Storage.TryAdd(nowPlaying.UniqueId, nowPlaying);
        }

        if (result)
        {
            Trace.WriteLine($"[NowPlayingInMemoryRepository] Added or updated now playing: {nowPlaying}");
        }

        return Task.CompletedTask;
    }

    public Task<OperationResult<NowPlayingInfo[]>> GetNowPlayingAsync(CancellationToken token = default)
    {
        RemoveExpiredNonScrobbledEntries();

        return Task.FromResult(new OperationResult<NowPlayingInfo[]>
        {
            Data = Storage.Values.ToArray()
        });
    }

    public Task ClearNowPlayingAsync(CancellationToken token = default)
    {
        Storage.Clear();
        return Task.CompletedTask;
    }

    private static void RemoveExpiredNonScrobbledEntries()
    {
        foreach (var nowPlaying in Storage.Values)
        {
            if (nowPlaying.Scrobble.IsExpired)
            {
                Storage.TryRemove(nowPlaying.UniqueId, out var existing);
            }
        }
    }
}
