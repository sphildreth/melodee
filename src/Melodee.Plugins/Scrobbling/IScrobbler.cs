using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;

namespace Melodee.Plugins.Scrobbling;

public interface IScrobbler : IPlugin
{
    Task<OperationResult<bool>> NowPlaying(User user, ScrobbleInfo scrobble, CancellationToken token = default);

    Task<OperationResult<bool>> Scrobble(User user, ScrobbleInfo scrobble, CancellationToken token = default);
}
