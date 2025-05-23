namespace Melodee.Blazor.Controllers.Melodee.Models;

public record User(
    Guid Id,
    string AvatarThumbnailUrl,
    string AvatarUrl,
    string UserName,
    string Email,
    bool IsAdmin,
    bool IsEditor,
    string[] Roles,
    int SongsPlayed,
    int ArtistsLiked,
    int ArtistsDisliked,
    int AlbumsLiked,
    int AlbumsDisliked,
    int SongsLiked,
    int SongsDisliked);
