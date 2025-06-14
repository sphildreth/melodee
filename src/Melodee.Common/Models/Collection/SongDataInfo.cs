using System.Text.Json.Serialization;
using Melodee.Common.Extensions;
using NodaTime;

namespace Melodee.Common.Models.Collection;

public sealed record SongDataInfo(
    [property: JsonIgnore] int Id,
    Guid ApiKey,
    bool IsLocked,
    string Title,
    string TitleNormalized,
    int SongNumber,
    LocalDate ReleaseDate,
    string AlbumName,
    Guid AlbumApiKey,
    string ArtistName,
    Guid ArtistApiKey,
    long FileSize,
    double Duration,
    Instant CreatedAt,
    string Tags,
    bool UserStarred,
    int UserRating)
{
    public static string InfoLineTitle => "Song Number | Duration";

    public string InfoLineData => $"{SongNumber.ToStringPadLeft(3)} | {Duration.ToFormattedDateTimeOffset()}";

    public int PlayedCount { get; set; }

    public Instant? LastUpdatedAt { get; set; }

    public string? Genre { get; set; }
}
