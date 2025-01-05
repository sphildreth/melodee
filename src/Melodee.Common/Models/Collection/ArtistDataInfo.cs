using NodaTime;

namespace Melodee.Common.Models.Collection;

public sealed record ArtistDataInfo(
    int Id,
    Guid ApiKey,
    bool IsLocked,
    string Name,
    string NameNormalized,    
    string AlternateNames,
    string Directory,
    int AlbumCount,
    int SongCount,
    Instant CreatedAt,
    string Tags);
