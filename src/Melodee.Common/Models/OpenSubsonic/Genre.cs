using System.Text.Json.Serialization;
using Melodee.Common.Extensions;

namespace Melodee.Common.Models.OpenSubsonic;

/// <summary>
///     A genre returned in list of genres for an item.
/// </summary>
public record Genre : IOpenSubsonicToXml
{
    public required string Value { get; init; }

    public int SongCount { get; init; }

    public int AlbumCount { get; init; }

    [JsonIgnore] public string ValueNormalized => Value.ToNormalizedString() ?? Value;

    public string ToXml(string? nodeName = null)
    {
        return $"<genre songCount=\"{SongCount}\" albumCount=\"{AlbumCount}\">{Value.ToSafeXmlString()}</genre>";
    }
}
