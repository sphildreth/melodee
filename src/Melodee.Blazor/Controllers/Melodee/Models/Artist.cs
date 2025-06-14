namespace Melodee.Blazor.Controllers.Melodee.Models;

public record Artist(
    Guid Id,
    string ThumbnailUrl,
    string ImageUrl,
    string Name,
    bool UserStarred,
    int UserRating,
    int AlbumCount,
    int SongCount,
    string CreatedAt,
    string UpdatedAt,
    string? Biography = null,
    string[]? Genres = null)
{
    public static Artist BlankArtist()
    {
        return new Artist(Guid.Empty,
            "",
            "",
            "",
            false,
            0,
            0,
            0,
            "",
            "");
    }
}
