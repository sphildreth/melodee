using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Utility;
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
    string? Tags,
    LocalDate ReleaseDate,
    short AlbumStatus)
{
    public byte[]? ImageBytes { get; set; }
    
    public int? NeedsAttentionReasons { get; set; }
    
    public object? State { get; set; }
    
    public AlbumStatus AlbumStatusValue => SafeParser.ToEnum<AlbumStatus>(AlbumStatus);
    
    public AlbumNeedsAttentionReasons NeedsAttentionReasonsValue => SafeParser.ToEnum<AlbumNeedsAttentionReasons>(NeedsAttentionReasons);
    
    public bool IsValid => NeedsAttentionReasonsValue == AlbumNeedsAttentionReasons.NotSet && AlbumStatusValue != Enums.AlbumStatus.Ok;

    public static string InfoLineTitle => $"Year | Song Count | Duration";

    public string InfoLineData => $"{ReleaseDate.Year} | {SongCount.ToStringPadLeft(4)} | {Duration.ToFormattedDateTimeOffset()}"; 
}
