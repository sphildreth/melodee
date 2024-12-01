namespace Melodee.Common.Models.OpenSubsonic;

/// <summary>
///     A genre returned in list of genres for an item.
/// </summary>
public record Genre
{
    public required string Value { get; init; }

    public int SongCount { get; init; }

    public int AlbumCount { get; init; }
}
