using Melodee.Common.Extensions;
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
    string Tags)
{
    public object? State { get; set; }
    
    public static string InfoLineTitle => $"Album Count | Song Count";

    public string InfoLineData => $"{AlbumCount.ToStringPadLeft(4)} | {SongCount.ToStringPadLeft(5)}";       
}