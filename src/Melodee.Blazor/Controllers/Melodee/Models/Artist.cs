namespace Melodee.Blazor.Controllers.Melodee.Models;

public record Artist(Guid Id, string ThumbnailUrl, string ImageUrl, string Name, bool UserStarred, int UserRating);
