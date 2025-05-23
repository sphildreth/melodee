namespace Melodee.Blazor.Controllers.Melodee.Models;

public record Album(Guid ApiKey, string ThumbnailUrl, string ImageUrl, string Name, int ReleaseYear, bool UserStarred, int UserRating);
