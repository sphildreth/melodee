using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models.SearchEngines;
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
    short SongCount,
    double Duration,
    Instant CreatedAt,
    string? Tags,
    LocalDate ReleaseDate,
    short AlbumStatus)
{
    public bool UserStarred { get; set; }

    public int UserRating { get; set; }

    /// <summary>
    ///     This is populated when the record is created from a Media Album File.
    /// </summary>
    public string? MelodeeDataFileName { get; set; }

    /// <summary>
    ///     This is populated when the record is created from a Media Album file.
    /// </summary>
    public byte[]? ImageBytes { get; set; }

    /// <summary>
    ///     This is populated when the record is created from a Search Engine result.
    /// </summary>
    public AlbumSearchResult? AlbumSearchResult { get; init; }

    /// <summary>
    ///     This is populated when the record is created from a Search Engine result, not a Melodee database record. This is
    ///     likely an external public url.
    /// </summary>
    public string? CoverUrl { get; set; }

    public int? NeedsAttentionReasons { get; set; }

    public object? State { get; set; }

    public AlbumStatus AlbumStatusValue => SafeParser.ToEnum<AlbumStatus>(AlbumStatus);

    public AlbumNeedsAttentionReasons NeedsAttentionReasonsValue =>
        SafeParser.ToEnum<AlbumNeedsAttentionReasons>(NeedsAttentionReasons);

    public bool IsValid => NeedsAttentionReasonsValue == AlbumNeedsAttentionReasons.NotSet &&
                           AlbumStatusValue != Enums.AlbumStatus.Ok;

    public static string InfoLineTitle => "Year | Song Count | Duration";

    public string InfoLineData =>
        $"{ReleaseDate.Year} | {SongCount.ToStringPadLeft(4)} | {Duration.ToFormattedDateTimeOffset()} {(IsLocked ? " | \ud83d\udd12" : string.Empty)}";

    public Instant? LastUpdatedAt { get; set; }

    public static AlbumDataInfo BlankAlbumDataInfo =>
        new(0,
            Guid.Empty,
            false,
            string.Empty,
            string.Empty,
            string.Empty,
            Guid.Empty,
            string.Empty,
            0,
            0,
            Instant.MinValue,
            string.Empty,
            LocalDate.MinIsoValue,
            (short)Enums.AlbumStatus.Invalid);
}
