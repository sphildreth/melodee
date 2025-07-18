using NodaTime;

namespace Melodee.Common.Models.Collection;

public sealed record UserDataInfo(
    int Id,
    Guid ApiKey,
    bool IsLocked,
    string UserName,
    string Email,
    bool IsAdmin,
    Instant? LastActivityAt,
    Instant CreatedAt,
    string? Tags,
    Instant? LastUpdatedAt,
    Instant? LastLoginAt)
{
    public override string ToString()
    {
        return $"Id [{Id}], Username [{UserName}], Email [{Email}], IsAdmin [{IsAdmin}]";
    }

    public static UserDataInfo BlankUserDataInfo =>
        new(0,
            Guid.Empty,
            false,
            string.Empty,
            string.Empty,
            false,
            null,
            Instant.MinValue,
            null,
            null,
            null);
}
