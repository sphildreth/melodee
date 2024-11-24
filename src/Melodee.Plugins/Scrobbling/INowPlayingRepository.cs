using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;

namespace Melodee.Plugins.Scrobbling;

public interface INowPlayingRepository
{
    Task AddOrUpdateNowPlayingAsync(NowPlayingInfo nowPlaying, CancellationToken token = default);

    Task<OperationResult<NowPlayingInfo[]>> GetNowPlayingAsync(CancellationToken token = default);
}
