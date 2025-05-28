using Melodee.Common.Extensions;

namespace Melodee.Common.Models.OpenSubsonic;

public record LyricsListLine(string Value, long? Start) : IOpenSubsonicToXml
{
    public string ToXml(string? nodeName = null)
    {
        return $"<line start=\"{Start ?? 0}\">{Value.ToSafeXmlString()}</line>";
    }
}
