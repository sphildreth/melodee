using NodaTime;

namespace Melodee.Common.Models.Collection;

public sealed record ArtistDataInfo(
    int Id,
    Guid ApiKey,
    bool IsLocked,
    int LibraryId,
    string LibraryPath,
    string Name,
    string NameNormalized,    
    string AlternateNames,
    string Directory,
    int AlbumCount,
    int SongCount,
    Instant CreatedAt,
    string Tags);
