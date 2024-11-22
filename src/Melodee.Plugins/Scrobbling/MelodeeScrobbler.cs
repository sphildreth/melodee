using Melodee.Common.Configuration;
using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;

namespace Melodee.Plugins.Scrobbling;

public class MelodeeScrobbler(IMelodeeConfiguration configuration) : IScrobbler
{
    public string Id => "D8A07387-87DF-4136-8D3E-C59EABEB501F";

    public string DisplayName => nameof(MelodeeScrobbler);

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; } = 0;

    public bool StopProcessing { get; } = false;
    
    public Task<OperationResult<bool>> NowPlaying(User user, ScrobbleInfo scrobble, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<OperationResult<bool>> Scrobble(User user, ScrobbleInfo scrobble, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}
