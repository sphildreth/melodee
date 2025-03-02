using Melodee.Common.Extensions;

namespace Melodee.Common.Models.OpenSubsonic.Searching;

public record ArtistSearchResult(string Id, string Name, string CoverArt, int AlbumCount) : IOpenSubsonicToXml
{
    public string ToXml(string? nodeName = null)
    {
        return $"<artist id=\"{Id}\" name=\"{Name.ToSafeXmlString()}\" coverArt=\"{CoverArt}\" albumCount=\"{AlbumCount}\"/>";
    }
}
