namespace Melodee.Blazor.Controllers.Melodee.Models;

public record Artist(Guid ApiKey, string ThumbnailUrl, string ImageUrl, string Name, bool UserStarred, int UserRating);
