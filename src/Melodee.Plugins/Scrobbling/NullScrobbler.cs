using Melodee.Common.Data.Models;
using Melodee.Common.Models;
using Melodee.Common.Models.Scrobbling;

namespace Melodee.Plugins.Scrobbling;

public class NullScrobbler : IScrobbler
{
    public string Id => "EAA0D3F2-62EC-4553-B4BD-3DF8DC8A4547";

    public string DisplayName => nameof(NullScrobbler);

    public bool IsEnabled { get; set; } = false;

    public int SortOrder { get; } = 0;

    public bool StopProcessing { get; } = false;
    
    public Task<OperationResult<bool>> NowPlaying(User user, ScrobbleInfo scrobble, CancellationToken token = default)
    {
        return Task.FromResult(new OperationResult<bool>
        {
            Data = true
        });
    }

    public Task<OperationResult<bool>> Scrobble(User user, ScrobbleInfo scrobble, CancellationToken token = default)
    {
        return Task.FromResult(new OperationResult<bool>
        {
            Data = true
        });
    }
}