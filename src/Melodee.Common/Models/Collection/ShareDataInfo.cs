using NodaTime;

namespace Melodee.Common.Models.Collection;

public sealed record ShareDataInfo(
    int Id,
    Guid ApiKey,
    bool IsLocked,
    string? Description,
    string ShardByUserName,
    Guid SharedByUserApiKey,
    Instant CreatedAt,
    Instant ExpiresAt,
    int VisitCount,
    AlbumDataInfo[] Albums,
    SongDataInfo[] Songs
);
