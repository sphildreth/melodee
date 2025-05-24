namespace Melodee.Blazor.Controllers.Melodee.Models;

public record Song(Guid Id, string StreamUrl, string ThumbnailUrl, string ImageUrl, string Name, double DurationMs, string FormattedDuration, bool UserStarred, int UserRating, Artist Artist, Album Album);
