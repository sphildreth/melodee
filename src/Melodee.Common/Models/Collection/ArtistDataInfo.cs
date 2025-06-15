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
    string Tags,
#pragma warning disable CS8907 // Parameter is unread. Did you forget to use it to initialize the property with that name?
    Instant? LastUpdatedAt)
#pragma warning restore CS8907 // Parameter is unread. Did you forget to use it to initialize the property with that name?
{
    public bool UserStarred { get; set; }

    public int UserRating { get; set; }

    public object? State { get; set; }

    /// <summary>
    ///     This is populated when the record is created from a Media Album file.
    /// </summary>
    public byte[]? ImageBytes { get; set; }

    public static string InfoLineTitle => "Album Count | Song Count";

    public string InfoLineData => $"{AlbumCount.ToStringPadLeft(4)} | {SongCount.ToStringPadLeft(5)}";

    public Instant? LastUpdatedAt { get; set; }

    public string? Biography { get; set; }

    public static ArtistDataInfo BlankArtistDataInfo =>
        new(0,
            Guid.Empty,
            false,
            0,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            0,
            0,
            Instant.MinValue,
            string.Empty,
            null);
}
