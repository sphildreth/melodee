using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;

namespace Melodee.Common.Plugins.Scrobbling;

public interface IScrobbler : IPlugin
{
    Task<OperationResult<bool>> NowPlaying(UserInfo user, ScrobbleInfo scrobble, CancellationToken token = default);

    Task<OperationResult<bool>> Scrobble(UserInfo user, ScrobbleInfo scrobble, CancellationToken token = default);
}
