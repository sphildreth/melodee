using System.Text;

namespace Melodee.Common.Models.OpenSubsonic.Searching;

public record Search3Result(ArtistSearchResult[] Artist, AlbumSearchResult[] Album, SongSearchResult[] Song) : IOpenSubsonicToXml
{
    public string ToXml(string? nodeName = null)
    {
        var result = new StringBuilder("<searchResult3>");
        foreach (var artist in Artist)
        {
            result.Append(artist.ToXml());
        }
        foreach (var album in Album)
        {
            result.Append(album.ToXml());
        }
        foreach (var song in Song)
        {
            result.Append(song.ToXml());
        }
        result.Append("</searchResult3>");
        return result.ToString();
    }
}
