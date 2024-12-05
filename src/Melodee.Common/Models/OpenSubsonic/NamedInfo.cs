using System.Text;
using Melodee.Common.Extensions;

namespace Melodee.Common.Models.OpenSubsonic;

public record NamedInfo(
    string Id,
    string Name,
    string? SmallImageUrl = null,
    string? MediumImageUrl = null,
    string? LargeImageUrl = null,
    int? SongCount = null,
    int? AlbumCount = null) : InfoBase(SmallImageUrl, MediumImageUrl, LargeImageUrl), IOpenSubsonicToXml
{
    public virtual string ToXml(string? nodeName = null)
    {
        return $"<{nodeName ?? "musicFolder"} id=\"{Id}\" name=\"{Name.ToSafeXmlString() }\"/>";
    }
}
