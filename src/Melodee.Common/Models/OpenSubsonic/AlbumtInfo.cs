using System.Text;
using Mapster;
using Melodee.Common.Extensions;

namespace Melodee.Common.Models.OpenSubsonic;

public record AlbumInfo(
    string Id,
    string Name,
    string? SmallImageUrl = null,
    string? MediumImageUrl = null,
    string? LargeImageUrl = null,
    int? SongCount = null,
    int? AlbumCount = null,
    string? Notes = null,
    Guid? MusicBrainzArtistId = null,
    string? LastFmUrl = null) : NamedInfo(Id,
    Name,
    SmallImageUrl,
    MediumImageUrl,
    LargeImageUrl,
    SongCount,
    AlbumCount)
{
    public override string ToXml(string? nodeName = null)
    {
        var result = new StringBuilder("<albumInfo>");
        result.Append($"<notes>{ Notes.ToSafeXmlString() }</notes>");
        result.Append($"<musicBrainzId>{ MusicBrainzArtistId }</musicBrainzId>");
        result.Append($"<lastFmUrl>{ LastFmUrl }</lastFmUrl>");
        result.Append($"<smallImageUrl>{ SmallImageUrl }</smallImageUrl>");
        result.Append($"<mediumImageUrl>{ MediumImageUrl }</mediumImageUrl>");
        result.Append($"<largeImageUrl>{ LargeImageUrl }</largeImageUrl>");
        result.Append("</albumInfo>");
        return result.ToString();
    }
}
