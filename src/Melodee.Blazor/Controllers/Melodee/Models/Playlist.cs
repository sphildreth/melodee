using NodaTime;

namespace Melodee.Blazor.Controllers.Melodee.Models;

public record Playlist(
    Guid Id,
    string ThumbnailUrl,
    string ImageUrl,
    string Name,
    string Description,
    Duration Duration,
    double DurationMs,
    string DurationFormatted,
    short SongsCount);
