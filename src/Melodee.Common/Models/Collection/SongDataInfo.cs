using Melodee.Common.Extensions;
using NodaTime;

namespace Melodee.Common.Models.Collection;

public sealed record SongDataInfo(
    int Id,
    Guid ApiKey,
    bool IsLocked,
    string Title,
    string TitleNormalized,
    int SongNumber,
    string AlbumName,
    Guid AlbumApiKey,
    string ArtistName,
    Guid ArtistApiKey,
    short DiscNumber,
    long FileSize,
    double Duration,
    Instant CreatedAt,
    string Tags)
{
    public static string InfoLineTitle => $"Song Number | Duration";

    public string InfoLineData => $"{ SongNumber.ToStringPadLeft(3)} | {Duration.ToFormattedDateTimeOffset()}";    
}
