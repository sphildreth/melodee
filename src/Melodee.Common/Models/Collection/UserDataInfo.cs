using NodaTime;

namespace Melodee.Common.Models.Collection;

public sealed record UserDataInfo(
    int Id,
    Guid ApiKey,
    bool IsLocked,
    string UserName,
    string UserNameNormalized,
    string Email,
    string EmailNormalized,
    Instant LastLoginAt,
    Instant LastActivityAt,
    Instant CreatedAt,
    string Tags);
