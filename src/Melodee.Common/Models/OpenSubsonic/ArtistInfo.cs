using System.Text;
using Melodee.Common.Extensions;

namespace Melodee.Common.Models.OpenSubsonic;

public record ArtistInfo(
    string Id,
    string Name,
    string? SmallImageUrl = null,
    string? MediumImageUrl = null,
    string? LargeImageUrl = null,
    int? SongCount = null,
    int? AlbumCount = null,
    string? Biography = null,
    Guid? MusicBrainzArtistId = null,
    string? LastFmUrl = null,
    Artist[]? SimilarArtist = null) : NamedInfo(Id,
    Name,
    SmallImageUrl,
    MediumImageUrl,
    LargeImageUrl,
    SongCount,
    AlbumCount)
{
    public override string ToXml(string? nodeName = null)
    {
        var result = new StringBuilder("<artistInfo>");
        result.Append($"<biography>{Biography.ToSafeXmlString()}</biography>");
        result.Append($"<musicBrainzId>{MusicBrainzArtistId}</musicBrainzId>");
        result.Append($"<lastFmUrl>{LastFmUrl}</lastFmUrl>");
        result.Append($"<smallImageUrl>{SmallImageUrl}</smallImageUrl>");
        result.Append($"<mediumImageUrl>{MediumImageUrl}</mediumImageUrl>");
        result.Append($"<largeImageUrl>{LargeImageUrl}</largeImageUrl>");
        if (SimilarArtist != null)
        {
            foreach (var artist in SimilarArtist)
            {
                result.Append($"<similarArtist id=\"{artist.Id}\" name=\" {artist.Name}\" />");
            }
        }

        result.Append("</artistInfo>");
        return result.ToString();
    }
}
