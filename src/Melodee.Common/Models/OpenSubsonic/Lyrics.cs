using Melodee.Common.Extensions;

namespace Melodee.Common.Models.OpenSubsonic;

public record Lyrics : IOpenSubsonicToXml
{
    public required string Value { get; init; }

    public string Artist { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string ToXml(string? nodeName = null)
    {
        return $"<lyrics artist=\"{Artist.ToSafeXmlString()}\" title=\"{Title.ToSafeXmlString()}\">{Value.ToSafeXmlString()}</lyrics>";
    }
}
