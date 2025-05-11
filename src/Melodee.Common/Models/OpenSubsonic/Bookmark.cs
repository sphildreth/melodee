using Melodee.Common.Extensions;

namespace Melodee.Common.Models.OpenSubsonic;

/// <summary>
///     A bookmark.
/// </summary>
/// <param name="Position">Bookmark position in seconds</param>
/// <param name="Username">Username</param>
/// <param name="Comment">Bookmark comment</param>
/// <param name="Created">Bookmark creation date [ISO 8601]</param>
/// <param name="Change">Bookmark last updated date [ISO 8601]</param>
/// <param name="Entry">The bookmark file</param>
public sealed record Bookmark(
    long Position,
    string Username,
    string? Comment,
    string Created,
    string Change,
    Child Entry) : IOpenSubsonicToXml
{
    public string ToXml(string? nodeName = null)
    {
        return
            $"<bookmark position=\"{Position}\" username=\"{Username.ToSafeXmlString()}\" comment=\"{Comment.ToSafeXmlString()}\" created=\"{Created}\" changed=\"{Change}\">{((IOpenSubsonicToXml)Entry).ToXml("entry")}</bookmark>";
    }
}
