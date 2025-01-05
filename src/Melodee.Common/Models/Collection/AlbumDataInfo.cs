using NodaTime;

namespace Melodee.Common.Models.Collection;

public sealed record AlbumDataInfo(
    int Id,
    Guid ApiKey,
    bool IsLocked,
    string Name,
    string NameNormalized,
    string AlternateNames,
    Guid ArtistApiKey,
    string ArtistName,
    short DiscCount,
    short SongCount,
    double Duration,
    Instant CreatedAt,
    string Tags);
