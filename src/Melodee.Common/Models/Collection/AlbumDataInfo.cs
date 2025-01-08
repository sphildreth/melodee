using Melodee.Common.Extensions;
using NodaTime;

namespace Melodee.Common.Models.Collection;

public sealed record AlbumDataInfo(
    int Id,
    Guid ApiKey,
    bool IsLocked,
    string Name,
    string NameNormalized,
    string? AlternateNames,
    Guid ArtistApiKey,
    string ArtistName,
    short DiscCount,
    short SongCount,
    double Duration,
    Instant CreatedAt,
    string? Tags)
{
    public object? State { get; set; }

    public static string InfoLineTitle => $"Song Count | Duration";

    public string InfoLineData => $"{SongCount.ToStringPadLeft(4)} | {Duration.ToFormattedDateTimeOffset()}"; 
}
