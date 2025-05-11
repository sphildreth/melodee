using Melodee.Common.Extensions;

namespace Melodee.Common.Models.OpenSubsonic;

public record User(
    string Username,
    bool AdminRole,
    string Email,
    bool StreamRole,
    bool ScrobblingEnabled,
    bool DownloadRole,
    bool ShareRole,
    bool JukeboxRole,
    string AvatarLastChanged) : IOpenSubsonicToXml
{
    public string ToXml(string? nodeName = null)
    {
        return
            $"<user username=\"{Username.ToSafeXmlString()}\" email=\"{Email.ToSafeXmlString()}\" scrobblingEnabled=\"{ScrobblingEnabled.ToLowerCaseString()}\" adminRole=\"{AdminRole.ToLowerCaseString()}\" " +
            $"settingsRole=\"{AdminRole.ToLowerCaseString()}\" downloadRole=\"{DownloadRole.ToLowerCaseString()}\" " +
            $"maxBitRate=\"320\" videoConversionRole=\"false\" avatarLastChanged=\"{AvatarLastChanged}\" " +
            $"uploadRole=\"{AdminRole.ToLowerCaseString()}\" playlistRole=\"{StreamRole.ToLowerCaseString()}\" coverArtRole=\"{AdminRole.ToLowerCaseString()}\" " +
            $"commentRole=\"{AdminRole.ToLowerCaseString()}\" podcastRole=\"false\" streamRole=\"{StreamRole.ToLowerCaseString()}\" " +
            $"jukeboxRole=\"{JukeboxRole.ToLowerCaseString()}\" shareRole=\"{ShareRole.ToLowerCaseString()}\"></user>";
    }
}
