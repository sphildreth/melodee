using System.Text;
using Melodee.Common.Extensions;

namespace Melodee.Common.Models.OpenSubsonic;

public record LyricsList : IOpenSubsonicToXml
{
    public required string Lang { get; init; }

    public required bool Synced { get; init; }

    public required LyricsListLine[] Line { get; init; }

    public string? DisplayArtist { get; init; }

    public string? DisplayTitle { get; init; }

    public double? Offset { get; init; }

    public string ToXml(string? nodeName = null)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append($"<structuredLyrics synced=\"{Synced.ToString().ToLower()}\" lang=\"{Lang}\" offset=\"{Offset ?? 0}\" displayArtist=\"{DisplayArtist.ToSafeXmlString()}\" displayTitle=\"{DisplayTitle.ToSafeXmlString()}\">");
        foreach (var line in Line)
        {
            stringBuilder.Append(line.ToXml());
        }

        stringBuilder.Append("</structuredLyrics>");
        return stringBuilder.ToString();
    }
}
