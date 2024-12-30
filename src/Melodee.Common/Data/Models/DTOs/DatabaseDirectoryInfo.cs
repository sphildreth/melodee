using NodaTime;

namespace Melodee.Common.Data.Models.DTOs;

/// <summary>
/// Used to populate Artist and Song index "GetIndexes" response.
/// </summary>
public record DatabaseDirectoryInfo(
    int Id,
    Guid ApiKey,
    string Index,
    string Name,
    string CoverArt,
    decimal CalculatedRating,
    int AlbumCount,
    int PlayCount,
    Instant CreatedAt,
    Instant? LastUpdatedAt,
    Instant? Played,
    string? Directory,
    Instant? UserStarred,
    int? UserRating)
{
    public int UserRatingValue => UserRating ?? 0;
    public bool Starred => UserStarred != null;
}
