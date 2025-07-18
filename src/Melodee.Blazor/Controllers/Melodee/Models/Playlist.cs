namespace Melodee.Blazor.Controllers.Melodee.Models;

public record Playlist(
    Guid Id,
    string ThumbnailUrl,
    string ImageUrl,
    string Name,
    string Description,
    double DurationMs,
    string DurationFormatted,
    short SongCount,
    bool IsPublic,
    User Owner,
    string CreatedAt,
    string UpdatedAt);
