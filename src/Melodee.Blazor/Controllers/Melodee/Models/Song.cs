namespace Melodee.Blazor.Controllers.Melodee.Models;

public record Song(
    Guid Id,
    Artist Artist,
    Album Album,
    string StreamUrl,
    string ThumbnailUrl,
    string ImageUrl,
    string Title,
    double DurationMs,
    string DurationFormatted,
    bool UserStarred,
    int UserRating,
    int SongNumber,
    int Bitrate,
    int PlayCount,
    string CreatedAt,
    string UpdatedAt,
    string? Genre);
