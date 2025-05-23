using NodaTime;

namespace Melodee.Blazor.Controllers.Melodee.Models;

public record Playlist(
    Guid ApiKey,
    string ThumbnailUrl,
    string ImageUrl,    
    string Name,
    Duration Duration,
    double DurationMs,
    string DurationFormatted,
    short SongsCount);
