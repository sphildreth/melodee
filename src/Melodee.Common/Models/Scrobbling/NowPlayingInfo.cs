using Melodee.Common.Utility;

namespace Melodee.Common.Models.Scrobbling;

public record NowPlayingInfo(UserInfo User, ScrobbleInfo Scrobble)
{
    public long UniqueId = SafeParser.Hash(User.ApiKey.ToString(), Scrobble.SongApiKey.ToString());
}
