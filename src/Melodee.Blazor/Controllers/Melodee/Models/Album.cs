namespace Melodee.Blazor.Controllers.Melodee.Models;

public record Album(
    Guid Id,
    Artist Artist,
    string ThumbnailUrl,
    string ImageUrl,
    string Name,
    int ReleaseYear,
    bool UserStarred,
    int UserRating,
    int SongCount,
    double DurationMs,
    string DurationFormatted,
    string CreatedAt,
    string UpdatedAt,
    string? Description = null,
    string? Genre = null);
