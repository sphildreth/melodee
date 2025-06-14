using NodaTime;

namespace Melodee.Common.Models.Collection;

public record PlaylistDataInfo(
    int Id,
    Guid ApiKey,
    bool IsLocked,
    string Name,
    string? Description,
    bool IsPublic,
    UserInfo User,
    short SongCount,
    double Duration,
    Instant CreatedAt,
    Instant? LastUpdatedAt,
    string? Tags);
