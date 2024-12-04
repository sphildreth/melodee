using System.Text;

namespace Melodee.Common.Models.OpenSubsonic;

/// <summary>
///     Artist record used in list and index operations.
/// </summary>
/// <param name="Id">Artist ApiKey</param>
/// <param name="Name">Artist name</param>
/// <param name="UserRating">Artist rating [1-5]</param>
/// <param name="AverageRating">Artist average rating [1.0-5.0]</param>
/// <param name="CoverArt">A covertArt id.</param>
/// <param name="AlbumCount">Artist album count</param>
/// <param name="UserStarred">Timestamp when user starred artist</param>
/// <param name="ArtistImageUrl">Artist image url</param>
public record Artist(
    string Id,
    string Name,
    int AlbumCount,
    int? UserRating,
    decimal AverageRating,
    string CoverArt,
    string? ArtistImageUrl = null,
    string? UserStarred = null,
    AlbumList2[]? Album = null
) : IOpenSubsonicToXml
{
    /// <summary>
    ///     Sometimes its "Starred" and sometimes its "UserStarred" in the responses.
    /// </summary>
    public string? Starred => UserStarred;

    public string ToXml(string? nodeName = null)
    {
        var result = new StringBuilder("<artist id=\"5432\" name=\"AC/DC\" coverArt=\"ar-5432\" albumCount=\"15\">");
        if (Album != null)
        {
            foreach (var album in Album)
            {
                result.Append($"<album id=\"{ album.Id }\" name=\"{ album.Name }\" coverArt=\"{ album.CoverArt }\" songCount=\"{ album.SongCount }\" created=\"{ album.Created }\" duration=\"{ album.Duration }\" artist=\"{ album.Artist }\" artistId=\"{ album.ArtistId }\"/>");
            }
        }
        result.Append("</artist>");
        return result.ToString();
    }
}
